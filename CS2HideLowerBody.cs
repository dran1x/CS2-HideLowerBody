using System.Drawing;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;

namespace CS2HideLowerBody
{
    [MinimumApiVersion(30)]
    public class CS2HideLowerBody : BasePlugin
    {
        public override string ModuleName => "HideLowerBody";
        public override string ModuleAuthor => "DRANIX";
        public override string ModuleDescription => "Allows players to hide their first person legs model. (lower body view model)";
        public override string ModuleVersion => "1.0.0";
        private static readonly bool[] playersHide = new bool[Server.MaxPlayers];

        public override void Load(bool hotReload)
        {
            this.RegisterEventHandler<EventPlayerSpawn>(OnPlayerSpawn);
            
            this.AddCommand("legs", "Hides the lower body view model of a player.", CommandHideLowerBody);
            this.AddCommand("lowerbody", "Hides the lower body view model of a player.", CommandHideLowerBody);
            
            this.RegisterListener<Listeners.OnClientConnected>(playerSlot => { playersHide[playerSlot + 1] = false; });
            this.RegisterListener<Listeners.OnClientDisconnectPost>(playerSlot => { playersHide[playerSlot + 1] = false; });
        }

        [GameEventHandler]
        private static HookResult OnPlayerSpawn(EventPlayerSpawn @event, GameEventInfo info)
        {
            CCSPlayerController player = @event.Userid;

            if (!player.IsValid || player.IsBot || player.TeamNum <= (byte)CsTeam.Spectator) return HookResult.Continue;

            SetPawnAlphaRender(player);

            return HookResult.Continue;
        }

        [ConsoleCommand("css_legs", "Hides the lower body view model of a player.")]
        [ConsoleCommand("css_lowerbody", "Hides the lower body view model of a player.")]
        [CommandHelper(whoCanExecute: CommandUsage.CLIENT_ONLY)]
        private static void CommandHideLowerBody(CCSPlayerController? controller, CommandInfo command)
        {
            uint playerIndex = controller!.EntityIndex!.Value.Value;

            playersHide[playerIndex] = !playersHide[playerIndex];

            controller.PrintToChat($"You have {(playersHide[playerIndex] ? "disabled" : "enabled")} your lower body view, this will take effect on your next spawn.");
        }
        
        private static void SetPawnAlphaRender(CCSPlayerController controller) => controller.PlayerPawn.Value.Render = Color.FromArgb(playersHide[controller!.EntityIndex!.Value.Value] ? 254 : 255,
            controller.PlayerPawn.Value.Render.R, controller.PlayerPawn.Value.Render.G, controller.PlayerPawn.Value.Render.B);
    }
}
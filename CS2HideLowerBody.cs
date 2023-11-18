using System.Drawing;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Utils;
using MySqlConnector;
using Nexd.MySQL;

namespace CS2HideLowerBody
{
    [MinimumApiVersion(50)]
    public class CS2HideLowerBody : BasePlugin, IPluginConfig<Configuration>
    {
        public override string ModuleName => "HideLowerBody";
        public override string ModuleAuthor => "DRANIX";
        public override string ModuleDescription => "Allows players to hide their first person legs model. (lower body view model)";
        public override string ModuleVersion => "1.0.1";
        private static MySqlDb? mySql = null;
        public Configuration Config { get; set; } = new Configuration();
        private static readonly Dictionary<int, UserConfig> players = new Dictionary<int, UserConfig>();

        public override async void Load(bool hotReload)
        {
            this.AddCommand("legs", "Hides the lower body view model of a player.", CommandHideLowerBody);
            this.AddCommand("lowerbody", "Hides the lower body view model of a player.", CommandHideLowerBody);

            try
            {
                mySql = new MySqlDb(Config.DatabaseSettings.Host, Config.DatabaseSettings.Username, Config.DatabaseSettings.Password, Config.DatabaseSettings.Database, Config.DatabaseSettings.Port);
                await mySql.ExecuteQueryAsync($"CREATE TABLE IF NOT EXISTS `lower_body` (`id` INT AUTO_INCREMENT PRIMARY KEY, `account_id` int UNIQUE NOT NULL, `hide` bool DEFAULT FALSE, UNIQUE (`account_id`));");
            }
            catch (MySqlException ex)
            {
                ConsoleLog($"Failed to establish a connection to the '{Config.DatabaseSettings.Database}' database. Player data will not be saved. Error: {ex.Message}", ConsoleColor.Red);
            }
            
            this.RegisterEventHandler<EventPlayerSpawn>(OnPlayerSpawn);
            
            this.RegisterListener<Listeners.OnClientPutInServer>(OnClientPutInServer);
            this.RegisterListener<Listeners.OnClientDisconnect>(OnClientDisconnect);

            if (!hotReload) return;
            
            List<CCSPlayerController> controllers = Utilities.GetPlayers();

            foreach (var player in controllers) OnClientPutInServer((int)player.EntityIndex!.Value.Value - 1);
        }

        public override void Unload(bool hotReload)
        {
            players.Clear();
        }
        
        private async void OnClientPutInServer(int playerSlot)
        {
            CCSPlayerController controller = Utilities.GetPlayerFromSlot(playerSlot);
            
            if (!controller.IsValid || controller.IsBot) return;
            
            if (mySql != null)
            {
                try
                {
                    MySqlQueryResult result = await mySql.ExecuteQueryAsync($"SELECT hide FROM lower_body WHERE account_id = {new SteamID(controller.SteamID).SteamId32};");

                    if (result.Rows == 1)
                    {
                        players.Add(playerSlot + 1, new UserConfig { Enabled = result.Get<bool>(0, "hide") });
                        return;
                    }
                }
                catch (MySqlException ex)
                {
                    ConsoleLog($"Failed to get player data from the '{Config.DatabaseSettings.Database}' database. Error: {ex.Message}", ConsoleColor.Red);
                }
            }
                
            players.Add(playerSlot + 1, Config.UserSettings);
        }
        
        private async void OnClientDisconnect(int playerSlot)
        {
            CCSPlayerController controller = Utilities.GetPlayerFromSlot(playerSlot);

            if (!controller.IsValid || controller.IsBot) return;
            
            if (mySql != null)
            {
                try
                {
                    await mySql.ExecuteQueryAsync($"INSERT INTO lower_body (account_id, hide) VALUES ({new SteamID(controller.SteamID).SteamId32}, {players[playerSlot + 1].Enabled}) ON DUPLICATE KEY UPDATE hide = {players[playerSlot + 1].Enabled}");
                }
                catch (MySqlException ex)
                {
                    ConsoleLog($"Failed to update player data in the '{Config.DatabaseSettings.Database}' database. Error: {ex.Message}", ConsoleColor.Red);
                }
            }

            players.Remove(playerSlot + 1);
        }
        
        [GameEventHandler]
        private static HookResult OnPlayerSpawn(EventPlayerSpawn @event, GameEventInfo info)
        {
            CCSPlayerController controller = @event.Userid;
            
            if (!controller.IsValid || controller.IsBot || controller.TeamNum <= (byte)CsTeam.Spectator || !players.ContainsKey((int)controller.EntityIndex!.Value.Value)) return HookResult.Continue;
            
            SetPawnAlphaRender(controller);

            return HookResult.Continue;
        }

        [ConsoleCommand("css_legs", "Hides the lower body view model of a player.")]
        [ConsoleCommand("css_lowerbody", "Hides the lower body view model of a player.")]
        [CommandHelper(whoCanExecute: CommandUsage.CLIENT_ONLY)]
        private void CommandHideLowerBody(CCSPlayerController? controller, CommandInfo command)
        {
            int playerSlot = (int)controller!.EntityIndex!.Value.Value;
            
            players[playerSlot].Enabled ^= true;

            if (controller is { IsValid: true, PawnIsAlive: true })
            {
                SetPawnAlphaRender(controller);
                UpdatePlayer(controller);
            }
            
            controller!.PrintToChat($" {Config.ChatTag} You have {(players[playerSlot].Enabled ? "disabled" : "enabled")} your lower body view model.");
        }

        private static void SetPawnAlphaRender(CCSPlayerController controller) => controller.PlayerPawn.Value.Render = Color.FromArgb(players[(int)controller.EntityIndex!.Value.Value].Enabled ? 254 : 255,
                controller.PlayerPawn.Value.Render.R, controller.PlayerPawn.Value.Render.G, controller.PlayerPawn.Value.Render.B);

        private static void UpdatePlayer(CCSPlayerController controller)
        {
            const string classNameHealthShot = "weapon_healthshot";

            controller.GiveNamedItem(classNameHealthShot);
            
            var healthShot = controller.PlayerPawn.Value.WeaponServices!.MyWeapons.FirstOrDefault(weapon => weapon is { IsValid: true, Value: { IsValid: true, DesignerName: classNameHealthShot } });

            if (!healthShot!.IsValid) return;

            healthShot.Value.Remove();
        }
        
        private void ConsoleLog(string message, ConsoleColor color = ConsoleColor.Green)
        {
            string path = Path.Combine(this.ModuleDirectory, "logs.txt");
            string log = $"[{this.ModuleName}] {message}";

            Console.ForegroundColor = color;
            Console.WriteLine(log);
            Console.ResetColor();
            
            log = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {log}";

            try
            {
                using StreamWriter file = new StreamWriter(path, true);
                file.WriteLine(log);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[{this.ModuleName}] Error writing to log file: {ex.Message}");
                Console.ResetColor();
            }
        }

        public void OnConfigParsed(Configuration configParsed) => this.Config = configParsed;
    }
}
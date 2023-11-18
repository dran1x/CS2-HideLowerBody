using System.Text.Json.Serialization;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace CS2HideLowerBody
{
    public class Configuration : BasePluginConfig
    {
        [JsonPropertyName("Database Settings")]
        public DatabaseConfig DatabaseSettings { get; set; } = new DatabaseConfig();

        [JsonPropertyName("User Settings")] 
        public UserConfig UserSettings { get; set; } = new UserConfig();
        
        [JsonPropertyName("Chat Tag")] 
        public string ChatTag { get; set; } = $"{ChatColors.Green}[LowerBody]{ChatColors.Default}";
    }
}

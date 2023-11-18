using System.Text.Json.Serialization;

namespace CS2HideLowerBody
{
    public class DatabaseConfig
    {
        [JsonPropertyName("host")] 
        public string Host { get; set; } = "localhost";

        [JsonPropertyName("username")] 
        public string Username { get; set; } = "root";

        [JsonPropertyName("database")] 
        public string Database { get; set; } = "database";

        [JsonPropertyName("password")] 
        public string Password { get; set; } = "password";

        [JsonPropertyName("port")] 
        public int Port { get; set; } = 3306;
    }
}
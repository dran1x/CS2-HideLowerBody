using System.Text.Json.Serialization;

namespace CS2HideLowerBody
{
    public class UserConfig
    {
        public bool Enabled { get; set; }
        
        [JsonIgnore]
        public bool Changed { get; set; }
    }
}
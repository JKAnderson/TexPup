using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TexPup
{
    class GameSettings
    {
        public string GameDirectory { get; set; }

        public string PackDirectory { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public PackMode PackMode { get; set; }

        public string UnpackDirectory { get; set; }

        public bool UnpackSourceEbls { get; set; }

        public bool UnpackSourceModEngine { get; set; }

        public bool UnpackSourceUXM { get; set; }

        public static GameSettings JsonDeserialize(string text)
        {
            return JsonSerializer.Deserialize<GameSettings>(text);
        }

        public string JsonSerialize()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TexPup
{
    class UnpackGame
    {
        public GameType Type { get; }

        public GameConfig Config { get; }

        public GameSettings Settings { get; }

        public UnpackGame(GameType type, GameConfig config, GameSettings settings)
        {
            Type = type;
            Config = config;
            Settings = settings;
        }
    }
}

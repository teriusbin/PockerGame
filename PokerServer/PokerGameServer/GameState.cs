using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokerGameServer
{
    public enum GAME_STATE : byte
    {
        PLAYING, //플레이 중이다.

        ALONE, //유져가 혼자다

        NO_PALYING
    }
}

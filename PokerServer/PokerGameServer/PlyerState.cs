using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokerGameServer
{
    public enum PLAYER_STATE : byte
    {
  
        //접속자가 혼자 뿐인 상태
        USER_ALONE,

        //사용자가 방에 처음 들어간 상태
        ENTERED_ROOM,

        // 로딩을 완료한 상태.
        LOADING_COMPLETE,

        READY,

        USER_WAIT,

        BETTING_READY,

        BETTING_COMPLETE,

        NORMAL_DISTRIBUTE,

        NORMAL_DISTRIBUTE_COMPETE,

        FIRST_DISTRIBUTE_COMPLETE,

        USER_SLEECT_CARD_COMPLETE,
        
        USER_SELECT_CARD_READY,

        USER_OPEN_CARD_READY,

        USER_OPEN_CARD_COMPLETE,

        CLIENT_TURN_FINISHED
    }
}

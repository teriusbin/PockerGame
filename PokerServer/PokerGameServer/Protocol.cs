using System;

namespace PokerGameServer
{
	public enum PROTOCOL : short
	{
        BEGIN = 0,

        // 로딩을 시작해라.
        START_LOADING = 1,

        LOADING_COMPLETED = 2,

        // 게임 시작.
        GAME_START = 3,
       
        START_PLAYER_TURN = 4,

        // 클라이언트의 턴 연출이 끝났음을 알린다.
        TURN_FINISHED_REQ = 5,

        // 상대방 플레이어가 나가 방이 삭제되었다.
        ROOM_REMOVED = 6,

        //로비 입장
        ENTER_LOBBY_REQ = 7,

        //로비 
        ENTER_LOBBY_ACK = 8,

        //게임방 만들기
        MAKE_GAME_ROOM_REQ = 9,

        MAKE_GAME_ROOM_ACK = 10,

        // 게임방 입장 요청.
        ENTER_GAME_ROOM_REQ = 11,

        // 게임방 입장 응답
        ENTER_GAME_ROOM_ACK = 12,

        //게임 씬으로 변경했으니 방 정보 좀
        LOAD_GAME_SCENE_REQ = 13,

        //게임 씬에 왔으니 방 정보 전달해 줄게
        LOAD_GAME_SCENE_ACK = 14,

        //방 리스트 요청
        ROOM_LIST_REQ = 15,

        ROOM_LIST_ACK = 16,

        //채팅 요청
        CHAT_MSG_REQ = 17,

        //채팅 응답
        CHAT_MSG_ACK = 18,

        OUT_GAME_ROOM_REQ = 19,

        OUT_GAME_ROOM_ACK = 20,

        GAME_POSSIBLE_REQ = 21,

        GAME_POSSIBLE_ACK = 22,
        // 게임 종료.
        GAME_OVER = 23,

        GAME_FIRST_CARD_DISTRIBUTE_COMPLETE = 24,

        GAME_NEXT_CARD_DISTRIUBET_REQ = 25,

        GAME_NEXT_CARD_DISTRIUBET_COMPLETE = 26,

        GAME_BETTING_COMPLETE = 27,

        GAME_CARD_SELECT_REQ = 28,

        GAME_OPEN_CARD_SELECT_COMPLETE = 29,

        GAME_CARD_OPEN_REQ = 30,

        GAME_CARD_OPEN_COMPLETE = 31,

        GAME_WATCHING_REQ = 32,

        GAME_WATCHING_DISTRIBUTE = 33,

        GAME_WATCHING_DISTRIBUTE_COMPLETE = 34,

        GAME_USER_EACH_TURN_END = 35,

        GAME_DIE_REQ = 36,

        GAME_MASTER_START_REQ= 37,

        RANDOM_MATCHING_REQ = 38,

        RANDOM_MATCHING_ACK = 39,

        PING = 40,

        PONG = 41,

        END
    }
}

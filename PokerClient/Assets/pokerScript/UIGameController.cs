using UnityEngine;
using System.Collections;
using FreeNet;

[System.Serializable]


public class UIGameController : MonoBehaviour {


    UILabel DebugTest = null;
    GameRoom curGameRoom = null;

    int myPlayIndex;
    string curRoomId = null;

    void Start()
    {
        PK_NetMgr.Ins.OnMessageEvent = OnGameMessage;
        curGameRoom = new GameRoom();
        myPlayIndex = -1;

        CPacket reqMsg = CPacket.create((short)PROTOCOL.LOAD_GAME_SCENE_REQ);

        Debug.Log(curRoomId);

        reqMsg.push(curRoomId);
        PK_NetMgr.Ins.Send(reqMsg);
    }
	// Update is called once per frame
	void Update () {
        
    }


    public void OnClick_GameOut()
    {
        Debug.Log("이벤트 들어오나");
        CPacket reqMsg = CPacket.create((short)PROTOCOL.OUT_GAME_ROOM_REQ);
        Debug.Log(this.curGameRoom.roomId.ToString());
        reqMsg.push(this.curGameRoom.roomId.ToString());
        PK_NetMgr.Ins.Send(reqMsg);
    }

    public void isGamePossible()
    {
        CPacket reqMsg = CPacket.create((short)PROTOCOL.GAME_POSSIBLE_REQ);
        reqMsg.push(curRoomId);
        PK_NetMgr.Ins.Send(reqMsg);
    }

    public void changeScene()
    {
        Application.LoadLevel("Lobby");
    }


    void OnGameMessage(CPacket msg)
    {
        PROTOCOL protocolId = (PROTOCOL)msg.pop_protocol_id();

        switch (protocolId)
        {

            case PROTOCOL.LOAD_GAME_SCENE_ACK:
                this.curGameRoom.no = msg.pop_int32();
                this.curGameRoom.titleStr = msg.pop_string();
                this.curGameRoom.betMoneyStr = msg.pop_string();
                this.curGameRoom.curManCount = msg.pop_int32();
                this.curGameRoom.roomId = msg.pop_string();
                Debug.Log(this.curGameRoom.roomId);
                isGamePossible();
                //나 게임 할 수 있니 물어보자
                break;

            case PROTOCOL.GAME_POSSIBLE_ACK:
                string gameMsg = msg.pop_string();
                Debug.Log("나 게임 할 수 있어? 아니면 10 기면 1 혹은 2" +  gameMsg);
                break;

            case PROTOCOL.GAME_START:  //최초로 게임방 정보를 수신
                string test = msg.pop_string();
                Debug.Log("제발 스레드로 해보자" + test);
                break;

            case PROTOCOL.OUT_GAME_ROOM_ACK:
                changeScene();
                break;

           

        }
    }
}

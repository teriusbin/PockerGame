using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using FreeNet;
using System;
using SimpleJSON;
using System.Net.Json;

[System.Serializable]

public class GameRoom
{
    public int no = 0;
    public string titleStr = null;
    public string betMoneyStr = null;
    public int curManCount = 1;
    public int manCount = 5;
    public int curPlayingManCount = 0;
    public string roomId = null;
    public int roomMaster = -1;

}

public class UILobbyController : MonoBehaviour
{

    public GameObject itemCloneObj = null;

    public GameObject popupPanelObj = null;
    public GameObject lobbyPanelObj = null;
    public GameObject networkObj = null;
    public GameObject reConnectOjb = null;
    public GameObject freeMoneyCharge = null;

    public UIGrid uiGrid = null;
    public UIScrollView roomScrollview = null;

    public List<GameRoom> gameRoomList = new List<GameRoom>();
    public Dictionary<string, Int64> moneyList = new Dictionary<string, Int64>();
    public UILabel text = null;
    public UITextList chatTextList = null;

    private string myUserId = null;
    private string myMoney = null;

    public UILabel userId = null;
    public UILabel userMondey = null;
    public UILabel userWinrate = null;

    // Use this for initialization
    void Start()
    {
        Screen.SetResolution(1280, 720, true);

        PK_NetMgr.Ins.OnMessageEvent = OnMessage;

        //최초 Lobby 접속
        CPacket roomListReqMsg = CPacket.create();
        JsonObjectCollection roomListReqJsonObj = SetProtocol(PROTOCOL.ROOM_LIST_REQ);
        roomListReqMsg.push(roomListReqJsonObj.ToString());
        PK_NetMgr.Ins.Send(roomListReqMsg);
        CPacket.destroy(roomListReqMsg);

        //최초 Lobby 접속
        CPacket enterLobbyReqMsg = CPacket.create();
        JsonObjectCollection enterLobbyReqJsonObj = SetProtocol(PROTOCOL.ENTER_LOBBY_REQ);
        enterLobbyReqMsg.push(enterLobbyReqJsonObj.ToString());
        PK_NetMgr.Ins.Send(enterLobbyReqMsg);
        CPacket.destroy(enterLobbyReqMsg);

        this.myMoney = PK_NetMgr.Ins.userHasMoney;

        this.userId = new UILabel();
        this.userMondey = new UILabel();
        this.userWinrate = new UILabel();

        this.userId = GameObject.Find("UserId").GetComponent<UILabel>();
        this.userMondey = GameObject.Find("UserMoney").GetComponent<UILabel>();
        this.userWinrate = GameObject.Find("WinRate").GetComponent<UILabel>();

        if (Int64.Parse(PK_NetMgr.Ins.userHasMoney) == 0) //충전
        {
            this.freeMoneyCharge.SetActive(true);
            PK_NetMgr.Ins.userHasMoney = "1000000000";
        }
        this.userId.text = "ID : " + PK_NetMgr.Ins.userId;
        this.userMondey.text = "금액 :" + getMoneyString(PK_NetMgr.Ins.userHasMoney);
        this.userWinrate.text = "승률 : " + getWinRate(PK_NetMgr.Ins.userWin, PK_NetMgr.Ins.userLose);

        if (PlayerPrefs.GetString("disconect").Equals("true"))
        {
            this.reConnectOjb.SetActive(true);
            PlayerPrefs.SetString("disconect", "false");
        }
    }

    private string getWinRate(int win, int lose)
    {
        string output = null;
        float rate = ((float)win  / ((float)win + (float)lose)) * 100;
        output = rate.ToString();
        return output;
    }

    private string getMoneyString(string inputMoney)
    {
        Int64 intTotalMoney = Int64.Parse(inputMoney);
        string output = null;
        if (intTotalMoney / 10000 < 10000)
        {
            Int64 value = intTotalMoney / 10000;
            output = value.ToString() + "만원";

        }
        else if (intTotalMoney / 10000 >= 100000000)
        {
            Int64 value = intTotalMoney / 100000000;
            Int64 mok = value / 100000000;
            Int64 Spare = value % 100000000;
            if (Spare != 0)
            {
                output = mok.ToString() + "조 " + Spare.ToString() + "억원";
            }
            else
            {
                output = mok.ToString() + "조";
            }
        }
        else if (intTotalMoney / 10000 >= 10000)
        {
            Int64 value = intTotalMoney / 10000;
            Int64 mok = value / 10000;
            Int64 Spare = value % 10000;
            if (Spare != 0)
            {
                output = mok.ToString() + "억 " + Spare.ToString() + "만원";
            }
            else
            {
                output = mok.ToString() + "억원";
            }
        }
        return output;
    }
    // Update is called once per frame
    private void BuildGameRoomsList()
    {

        for (int i = 0; i < uiGrid.transform.childCount; ++i)
        {
            GameObject.Destroy(uiGrid.transform.GetChild(i).gameObject);
        }

        for (int i = 0; i < gameRoomList.Count; ++i)
        {
            GameRoom gameRoom = gameRoomList[i];

            GameObject createItemObj =
                Instantiate(itemCloneObj, Vector3.zero, Quaternion.identity) as GameObject;
            createItemObj.transform.parent = uiGrid.transform;
            createItemObj.transform.localScale = Vector3.one;

            UILabel noLabel = createItemObj.transform.FindChild("Label - No").GetComponent<UILabel>();
            UILabel titleLabel = createItemObj.transform.FindChild("Label - Title").GetComponent<UILabel>();
            UILabel bettingLabel = createItemObj.transform.FindChild("Label - BettingMoney").GetComponent<UILabel>();
            UILabel manCountLabel = createItemObj.transform.FindChild("Label - ManCount").GetComponent<UILabel>();
            UILabel roomId = createItemObj.transform.FindChild("Label - roomId").GetComponent<UILabel>();

            noLabel.text = gameRoom.no.ToString();
            titleLabel.text = gameRoom.titleStr;
            bettingLabel.text = getMoneyString(gameRoom.betMoneyStr);
            manCountLabel.text = gameRoom.curManCount.ToString() + "/" + gameRoom.manCount.ToString();
            roomId.text = gameRoom.roomId;
        }

        uiGrid.repositionNow = true;
    }

    public void OnSubmit_ChatInput()
    {

        string chatStr = UIInput.current.value;

        CPacket chatReqMsg = CPacket.create();
        JsonObjectCollection chatReqJsonObj = SetProtocol(PROTOCOL.CHAT_MSG_REQ);
        chatReqJsonObj.Add(new JsonStringValue("CHAT_MSG", "["+PK_NetMgr.Ins.userId+"]"+ " : " + chatStr));
        chatReqMsg.push(chatReqJsonObj.ToString());
        PK_NetMgr.Ins.Send(chatReqMsg);

      
        CPacket.destroy(chatReqMsg);
    }

    public void OnClick_Back()
    {
        Application.LoadLevel("pokerScene");
    }

    public void OnClick_No()
    {
        this.popupPanelObj.SetActive(false);
    }

    public void OnClick_Ok()
    {
        this.freeMoneyCharge.SetActive(false);
    }

    public void Onclick_ReConnect()
    {
        try
        {
            myUserId = PK_NetMgr.Ins.userId;

            CPacket msg = CPacket.create();
            JsonObjectCollection enterGameRoomReqObj = SetProtocol(PROTOCOL.ENTER_GAME_ROOM_REQ);
            enterGameRoomReqObj.Add(new JsonStringValue("USER_ID", myUserId));
            enterGameRoomReqObj.Add(new JsonStringValue("ROOM_ID", PlayerPrefs.GetString("curRoomId")));
            enterGameRoomReqObj.Add(new JsonStringValue("USER_MONEY", PK_NetMgr.Ins.userHasMoney.ToString()));
            msg.push(enterGameRoomReqObj.ToString());
            PK_NetMgr.Ins.Send(msg);

            CPacket.destroy(msg);

        }
        catch (System.Exception e)
        {
            Debug.Log(e.Message);  // 예외의 메시지를 출력
            Debug.Log(e.StackTrace);
        }
        
    }

    public void Onclick_NoReconnect()
    {
        this.reConnectOjb.SetActive(false);
    }

    public void OnClick_Refresh()
    {

        CPacket msg = CPacket.create();
        JsonObjectCollection refreshReqObj = SetProtocol(PROTOCOL.ROOM_LIST_REQ);
        msg.push(refreshReqObj.ToString());
        PK_NetMgr.Ins.Send(msg);

   
        CPacket.destroy(msg);

    }

    public void OnClick_RoomMake()
    {
        popupPanelObj.SetActive(true);
    }

    private void OnClick_EnterRoom(GameObject item)
    {
        try
        {
            
            item.transform.parent = uiGrid.transform;
            UILabel roomNo = item.transform.FindChild("Label - No").GetComponent<UILabel>();
            UILabel bettingMoney = item.transform.FindChild("Label - BettingMoney").GetComponent<UILabel>();
            UILabel roomId = item.transform.FindChild("Label - roomId").GetComponent<UILabel>();

            if (Int64.Parse(PK_NetMgr.Ins.userHasMoney) > this.moneyList[roomNo.text])
            {
                myUserId = PK_NetMgr.Ins.userId;

                CPacket msg = CPacket.create();
                JsonObjectCollection enterGameRoomReqObj = SetProtocol(PROTOCOL.ENTER_GAME_ROOM_REQ);
                enterGameRoomReqObj.Add(new JsonStringValue("USER_ID", myUserId));
                enterGameRoomReqObj.Add(new JsonStringValue("ROOM_ID", roomId.text.ToString()));
                enterGameRoomReqObj.Add(new JsonStringValue("USER_MONEY", PK_NetMgr.Ins.userHasMoney));
                msg.push(enterGameRoomReqObj.ToString());
                PK_NetMgr.Ins.Send(msg);

                CPacket.destroy(msg);
            }
          

        }
        catch (System.Exception e)
        {
            Debug.Log(e.Message);  // 예외의 메시지를 출력
            Debug.Log(e.StackTrace);
        }
    }

    private void RandomEnteredRoom(string roomId)
    { 
        try
        { 
            myUserId = PK_NetMgr.Ins.userId;

            CPacket msg = CPacket.create();
            JsonObjectCollection enterGameRoomReqObj = SetProtocol(PROTOCOL.ENTER_GAME_ROOM_REQ);
            enterGameRoomReqObj.Add(new JsonStringValue("USER_ID", myUserId));
            enterGameRoomReqObj.Add(new JsonStringValue("ROOM_ID", roomId));
            enterGameRoomReqObj.Add(new JsonStringValue("USER_MONEY", PK_NetMgr.Ins.userHasMoney.ToString()));
            msg.push(enterGameRoomReqObj.ToString());
            PK_NetMgr.Ins.Send(msg);

            CPacket.destroy(msg);

        }
        catch (System.Exception e)
        {
            Debug.Log(e.Message);  // 예외의 메시지를 출력
            Debug.Log(e.StackTrace);
        }
    }

    public void OnClick_RandomMatching()
    {
        try
        {
            CPacket msg = CPacket.create();
            JsonObjectCollection enterGameRoomReqObj = SetProtocol(PROTOCOL.RANDOM_MATCHING_REQ);
            msg.push(enterGameRoomReqObj.ToString());
            PK_NetMgr.Ins.Send(msg);
            CPacket.destroy(msg);

        }
        catch (System.Exception e)
        {
            Debug.Log(e.Message);  // 예외의 메시지를 출력
            Debug.Log(e.StackTrace);
        }
    }

    private void chatMsgReceive(string chatMsg)
    {
        chatTextList.Add(chatMsg);
    }

    public void setRoomInfo(string roomName, string bettingMoney)
    {
        try
        {
            this.myUserId = PK_NetMgr.Ins.userId; //PlayerPrefs.GetString("userId");
      
            CPacket msg = CPacket.create();
            JsonObjectCollection makeGameRoomReqObj = SetProtocol(PROTOCOL.MAKE_GAME_ROOM_REQ);
            makeGameRoomReqObj.Add(new JsonStringValue("ROOM_TITLE", roomName));
            makeGameRoomReqObj.Add(new JsonStringValue("ROOM_BETMONEY", bettingMoney));
            makeGameRoomReqObj.Add(new JsonStringValue("USER_ID", this.myUserId));
            makeGameRoomReqObj.Add(new JsonStringValue("USER_MONEY", this.myMoney));
            msg.push(makeGameRoomReqObj.ToString());
            PK_NetMgr.Ins.Send(msg);

            CPacket.destroy(msg);
        }
        catch (System.Exception e)
        {
            Debug.Log(e.Message);  // 예외의 메시지를 출력
            Debug.Log(e.StackTrace);
        }

    }

    private void changeScene()
    {
        Application.LoadLevel("MainScene 2");
    }

    private void savePlayerPrefsRoomId(string roomId)
    {
        //영구 저장.
        PlayerPrefs.SetString("curRoomId", roomId);
    }


    private JsonObjectCollection SetProtocol(PROTOCOL protocl)
    {
        JsonObjectCollection jsonObj = new JsonObjectCollection();
        jsonObj.Add(new JsonStringValue("PROTOCOL_ID", protocl.ToString()));
        return jsonObj;
    }

    void OnMessage(CPacket msg)
    {
        var onMsg = JSON.Parse(msg.pop_string());

        PROTOCOL PROTOCOL_ID = (PROTOCOL)Enum.Parse(typeof(PROTOCOL), onMsg["PROTOCOL_ID"].Value);

        switch (PROTOCOL_ID)
        {
            case PROTOCOL.ROOM_LIST_ACK:

                gameRoomList.Clear();
                int roomCount = int.Parse(onMsg["ROOM_COUNT"].Value);

                for (int i = 0; i < roomCount; ++i)
                {
                    GameRoom gameRoom = new GameRoom();

                    gameRoom.roomId = onMsg["ROOM_ID" + i.ToString()].Value;
                    gameRoom.no = int.Parse(onMsg["ROOM_NUMBER" + i.ToString()].Value);
                    gameRoom.titleStr = onMsg["ROOM_TITLE" + i.ToString()].Value;
                    gameRoom.betMoneyStr = onMsg["ROOM_BETMONEY" + i.ToString()].Value;
                    gameRoom.curManCount = int.Parse(onMsg["ROOM_CUR_MAN_COUNT" + i.ToString()].Value);
                    gameRoomList.Add(gameRoom);
                    this.moneyList.Add(gameRoom.no.ToString(), Int64.Parse(gameRoom.betMoneyStr));
                }

                BuildGameRoomsList();
                break;

            case PROTOCOL.CHAT_MSG_ACK:
                string chatMsg = onMsg["CHAT_MSG"].Value;
                chatMsgReceive(chatMsg);
                break;

            case PROTOCOL.ENTER_LOBBY_ACK:
                break;

            case PROTOCOL.MAKE_GAME_ROOM_ACK:
                string makeRoomId = onMsg["ROOM_ID"].Value;
                savePlayerPrefsRoomId(makeRoomId);
                changeScene();
                break;

            case PROTOCOL.ENTER_GAME_ROOM_ACK:
                string enterRoomId = onMsg["ROOM_ID"].Value;
                savePlayerPrefsRoomId(enterRoomId);
                changeScene();
                break;

            case PROTOCOL.RANDOM_MATCHING_ACK:
                string RandomEnterRoomId = onMsg["ROOM_ID"].Value;
                if (!RandomEnterRoomId.Equals("notMatch"))
                {
                    savePlayerPrefsRoomId(RandomEnterRoomId);
                    RandomEnteredRoom(RandomEnterRoomId);
                    changeScene();
                }
                break;

            case PROTOCOL.PONG:
                PK_NetMgr.lastPingTime = DateTime.Now;
                break;
        }

    }
}
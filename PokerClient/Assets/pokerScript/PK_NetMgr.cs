using UnityEngine;
using System;
using System.Collections;
using FreeNet;
using FreeNetUnity;
using SimpleJSON;
using System.Net.Json;

public class PK_NetMgr : MonoBehaviour
{
    public static CFreeNetUnityService gameserver;

    string received_msg;
    public GameObject popupPanelObj = null;
    public GameObject networkPanelObj = null;
    PK_GamePlayController componet;

    public delegate void OnMessageDelegate(CPacket msg);
    public delegate void OnNeteworkDelegateLobby();

    public OnMessageDelegate OnMessageEvent = null;
    public OnNeteworkDelegateLobby OnDisconectLobby = null;

    private static PK_NetMgr _instance = null;

    public string userId = null;
    public string userHasMoney = null;
    public int userWin = 0;
    public int userLose = 0;
    public int checkCount = 0;
    public float pingCheckTime = 5.0f;
    public static float pingCurrnetime = 0.0f;
    public static DateTime lastPingTime;
    public bool turnOn = false;

    public static PK_NetMgr Ins
    {

        get
        {
            if (_instance == null)
            {
                PlayerPrefs.SetString("disconect", "false");

                GameObject playInfoObj = GameObject.Find("/NetworkObject");
                //

                if (playInfoObj == null)
                    playInfoObj = new GameObject("NetworkObject");


                _instance = playInfoObj.GetComponent<PK_NetMgr>();
                if (_instance == null)
                    _instance = playInfoObj.AddComponent<PK_NetMgr>();

                _instance.Init();


            }


            return _instance;
        }
    }


    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        if (PK_NetMgr._instance == null)
        {
            PK_NetMgr._instance = GameObject.Find("/NetworkObject").GetComponent<PK_NetMgr>();
            PK_NetMgr._instance.Init();
        }
    }
    JsonObjectCollection SetProtocol(PROTOCOL protocl)
    {
        JsonObjectCollection jsonObj = new JsonObjectCollection();
        jsonObj.Add(new JsonStringValue("PROTOCOL_ID", protocl.ToString()));
        return jsonObj;
    }

    void Update()
    {
        if (IsConnected())
        {
            pingCurrnetime += Time.deltaTime;
            if (pingCurrnetime >= pingCheckTime) //5.0초 마다 보내기
            {
                CPacket reqMsg = CPacket.create();
                JsonObjectCollection gameStartReq = SetProtocol(PROTOCOL.PING);
                reqMsg.push(gameStartReq.ToString());
                PK_NetMgr.Ins.Send(reqMsg);

                CPacket.destroy(reqMsg);

                pingCurrnetime = 0.0f;
            }

            TimeOutCheck();
        }
    }

    void Init()
    {


        this.received_msg = "";


        gameserver = gameObject.AddComponent<CFreeNetUnityService>();


        gameserver.appcallback_on_status_changed += OnStatusChanged;

        // 패킷 수신 델리게이트 설정.
        gameserver.appcallback_on_message += OnMessage;


    }

    public void Connect()
    {
        gameserver.connect("172.30.154.7", 7979);
    }

    public bool IsConnected()
    {



        return gameserver.is_connected();


    }


    void OnStatusChanged(NETWORK_EVENT status)
    {
        switch (status)
        {
            // 접속 성공.
            case NETWORK_EVENT.connected:

                Debug.Log("on connected");
                this.received_msg += "on connected\n";
                lastPingTime = DateTime.Now;
                Application.LoadLevel("Lobby");

                break;

            // 연결 끊김.
            case NETWORK_EVENT.disconnected:
                Debug.Log("disconnected");
                this.received_msg += "disconnected\n";
                break;
        }
    }

    public void TimeOutCheck()
    {
        TimeSpan responseTime = DateTime.Now - lastPingTime;
        this.checkCount++;
        if (responseTime.TotalSeconds > 15 && this.checkCount > 1 && turnOn == false)
        {
            networkPanelObj = GameObject.Find("GamePlayContrtoller");

            componet = networkPanelObj.GetComponent<PK_GamePlayController>();

            componet.popupNetworkObj.SetActive(true);

            Debug.Log("서버와의 연결이 끊어졌습니다");

            turnOn = true;
        }


    }
    void OnMessage(CPacket msg)
    {
        if (OnMessageEvent != null)
        {
            OnMessageEvent(msg);
        }
    }

    public void Send(CPacket msg)
    {
        gameserver.send(msg);
    }

    public void Destroy()
    {
        _instance = null;
    }

}
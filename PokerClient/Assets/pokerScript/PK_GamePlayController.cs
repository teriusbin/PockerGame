using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Threading;
using FreeNet;
using DG.Tweening;
using SimpleJSON;
using System.Net.Json;
using System.Text.RegularExpressions;

[System.Serializable]
public class CardSlotPosInfo

{
    public List<Vector3> posList = new List<Vector3>();
}

public class GameObjGroup
{
    public List<GameObject> chipList = new List<GameObject>();
}

public class ChipIndex
{
    public int i = 0;
    public int j = 0;
}

public class PK_GamePlayController : MonoBehaviour
{

    const int NUM_OF_TOTAL_CARDS = 52;
    const int NUM_OF_FIRST_CARDS = 4;
    const int NUM_OF_TOTAL_USER = 5;
    const int NUM_OF_CARD = 7;

    int m_gCurGu;

    public PK_Card[] cards;
    public PK_Card[] openCards;
    public PK_Card[] backCard;

    public GameObject backCardGroupObj = null;
    public GameObject cardGroupObj = null;
    public GameObject cardDeckPosObj = null;
    public GameObject cardSlotGroupObj = null;
    public GameObject openCardSlotGroupObj = null;
    public GameObject openCardGroup = null;

    public GameObject ChipObj = null;
    public GameObject[] userChip = null;
    public GameObject[] userTurnBox = null;
    public GameObject[] userDieBox = null;
    public GameObject[] winnerBox = null;
    public GameObject[] bossBox = null;
    public GameObject[] userAvata = null;
    public GameObject[] userHand = null;
    public GameObject[] countDown = null;

    public GameObject popupPanelObj = null;
    public GameObject popupNetworkObj = null;

    public UIButton DieBtn = null;
    public UIButton CheckBtn = null;
    public UIButton CallBtn = null;
    public UIButton PPingBtn = null;
    public UIButton DDangBtn = null;
    public UIButton HalfBtn = null;
    public UIButton AllinBtn = null;
    public UIButton RoomOut = null;
    public GameObject StartBtn = null;

    public UILabel[] userName = null;
    public UILabel[] userMoney = null;
    public UILabel[] userBetting = null;
    public UILabel[] winnerInfo = null;
    public UILabel totalMoneyLable = null;
    public UILabel handValue = null;

    private List<CardSlotPosInfo> cardSlotPosInfoList = new List<CardSlotPosInfo>();
    private List<CardSlotPosInfo> openCardSlotPosInfoList = new List<CardSlotPosInfo>();
    private List<CardSlotPosInfo> chipPosInfoList = new List<CardSlotPosInfo>();

    private List<GameObjGroup> chipGroupList = new List<GameObjGroup>();
    private List<GameObject> turnBoxList = new List<GameObject>();
    private List<GameObject> dieBoxList = new List<GameObject>();
    private List<GameObject> winnerBoxList = new List<GameObject>();
    private List<GameObject> bossBoxList = new List<GameObject>();
    private List<GameObject> avataList = new List<GameObject>();
    private List<GameObject> handList = new List<GameObject>();
    private List<GameObject> countDownList = new List<GameObject>();

    private List<List<GameObjGroup>> userChipList = new List<List<GameObjGroup>>();

    private Vector3[] userChipPos = new[] { new Vector3(0.0f, -5.5f, 0),
                                            new Vector3(-9.3f, -0.5f, 0),
                                            new Vector3(-9.3f, 2.2f, 0),
                                            new Vector3(9.3f, 2.2f, 0),
                                            new Vector3(9.3f, -0.5f, 0),};

    private Vector3[] winnerChipMovePos = new[] {  new Vector3(0.0f, -4.5f, 0),
                                                   new Vector3(-7.1f, -0.5f, 0),
                                                   new Vector3(-7.1f, 2.2f, 0),
                                                   new Vector3(7.1f, 2.2f, 0),
                                                   new Vector3(7.1f, -0.5f, 0),};

    private Vector3 cardPos = new Vector3(0.0f, 5.5f, 0);
    private Vector3 backCardPos = new Vector3(0.0f, 5.5f, 0);
    private Vector3 openCardPos = new Vector3(-5.0f, -7.5f, 0);

    private List<Vector3> cardPosInfoList = new List<Vector3>();
    private List<Vector3> backCardPosInfoList = new List<Vector3>();

    private Dictionary<string, List<int>> playerCardTable = null;
    private GameRoom curGameRoom = null;

    private int[] userPos = new int[NUM_OF_TOTAL_USER];
    private int[] drawMoney = new int[11];
    private int[] throwMoney = new int[11];

    private int myPlayIndex;
    private int firstPlayerIndex;
    private int bossUserIndex;
    private int curTurnPlayer;

    private string curRoomId = null;
    private string myId = null;
    private string totalMoney = null;
    private string myMoney = null;

    private bool imPlaying = false;
    private bool lastBet = false;
    private bool checkBetFlag = false;
    private bool firstBetting = false;
    private bool sideBettingFlag = false;
    private int cardOrder;
    private int tempCount;
    private int preHand;
    private string removeCardIndex = null;
    private int BattleTime;

    private float CheckTime = 10f;
    private DateTime lastTime;
    private bool dieEventFlag = false;
    private bool timerStart = false;
    private bool roomOutCheck = false;
    private int preCountDown = 1;

    string pandonMoney = null;
    string beforeMoney = null;

    void Start()
    {
        Screen.SetResolution(1280, 720, true);

        this.curRoomId = PlayerPrefs.GetString("curRoomId");
        this.myId = PK_NetMgr.Ins.userId;
        this.myMoney = PK_NetMgr.Ins.userHasMoney;
        this.userName = new UILabel[NUM_OF_TOTAL_USER];
        this.userMoney = new UILabel[NUM_OF_TOTAL_USER];
        this.winnerInfo = new UILabel[NUM_OF_TOTAL_USER];
        this.totalMoneyLable = new UILabel();
        this.handValue = new UILabel();
        this.curGameRoom = new GameRoom();

        this.cardOrder = 1;
        this.myPlayIndex = -1;
        this.firstPlayerIndex = -1;
        this.bossUserIndex = 1;

        this.removeCardIndex = null;
        this.totalMoney = null;
        this.preHand = 0;
        this.BattleTime = 10;

        this.playerCardTable = new Dictionary<string, List<int>>();
        ResetListArray();

        PK_NetMgr.Ins.OnMessageEvent = OnGameMessage;
        PK_NetMgr.Ins.turnOn = false;

        CPacket reqMsg = CPacket.create();
        JsonObjectCollection LoadGameSceneReqJsonObj = SetProtocol(PROTOCOL.LOAD_GAME_SCENE_REQ);
        LoadGameSceneReqJsonObj.Add(new JsonStringValue("ROOM_ID", curRoomId));
        LoadGameSceneReqJsonObj.Add(new JsonStringValue("USER_ID", myId));
        reqMsg.push(LoadGameSceneReqJsonObj.ToString());
        PK_NetMgr.Ins.Send(reqMsg);
        CPacket.destroy(reqMsg);

        for (int i = 0; i < NUM_OF_TOTAL_USER; i++) userPos[i] = -1;

    }


    #region communication Module

    JsonObjectCollection SetProtocol(PROTOCOL protocl)
    {
        JsonObjectCollection jsonObj = new JsonObjectCollection();
        jsonObj.Add(new JsonStringValue("PROTOCOL_ID", protocl.ToString()));
        return jsonObj;
    }

    private void FirstDistributeCompleteMsgSendToServer()
    {
        this.popupPanelObj.SetActive(true);
        this.openCardGroup.SetActive(true);

        for (int i = 0; i < 4; i++)
        {
            int Index = this.playerCardTable[this.myPlayIndex.ToString()][i];
            this.openCards[Index].GetComponent<SpriteRenderer>().sortingOrder = 12;
            this.openCards[Index].transform.DOMove(openCardSlotPosInfoList[0].posList[i], 0.0f);

        }

        CPacket reqMsg = CPacket.create();
        JsonObjectCollection firstDisributeCompleteReq = SetProtocol(PROTOCOL.GAME_FIRST_CARD_DISTRIBUTE_COMPLETE);
        reqMsg.push(firstDisributeCompleteReq.ToString());
        PK_NetMgr.Ins.Send(reqMsg);

        CPacket.destroy(reqMsg);

    }

    private void NormalDistributeCompleteMsgSendToServer(int bettingNumber)
    {

        CPacket reqMsg = CPacket.create();
        JsonObjectCollection normalDistributeCompleteReq = SetProtocol(PROTOCOL.GAME_BETTING_COMPLETE);
        normalDistributeCompleteReq.Add(new JsonStringValue("GU_COUNT", m_gCurGu.ToString()));
        normalDistributeCompleteReq.Add(new JsonStringValue("USER_BUTTON_NAME", bettingNumber.ToString()));
        reqMsg.push(normalDistributeCompleteReq.ToString());
        PK_NetMgr.Ins.Send(reqMsg);

        CPacket.destroy(reqMsg);
    }

    private void GameStartMsgSendToServer()
    {
        CPacket reqMsg = CPacket.create();
        JsonObjectCollection gameStartReq = SetProtocol(PROTOCOL.GAME_MASTER_START_REQ);
        reqMsg.push(gameStartReq.ToString());
        PK_NetMgr.Ins.Send(reqMsg);

        CPacket.destroy(reqMsg);
    }

    private void CardOpenCompleteMsgSendToServer()
    {

        CPacket reqMsg = CPacket.create();
        JsonObjectCollection normalDistributeCompleteReq = SetProtocol(PROTOCOL.GAME_CARD_OPEN_COMPLETE);
        reqMsg.push(normalDistributeCompleteReq.ToString());
        PK_NetMgr.Ins.Send(reqMsg);
        CPacket.destroy(reqMsg);

    }

    private void WatchingUserCompleteMsg()
    {

        CPacket reqMsg = CPacket.create();
        JsonObjectCollection watchingDistributeCompleteReq = SetProtocol(PROTOCOL.GAME_WATCHING_DISTRIBUTE_COMPLETE);
        reqMsg.push(watchingDistributeCompleteReq.ToString());
        PK_NetMgr.Ins.Send(reqMsg);

        CPacket.destroy(reqMsg);

    }
    private void isGamePossibleMsgSendToServer()
    {
        CPacket reqMsg = CPacket.create();
        JsonObjectCollection isGamePossibleReqObj = SetProtocol(PROTOCOL.GAME_POSSIBLE_REQ);
        isGamePossibleReqObj.Add(new JsonStringValue("ROOM_ID", this.curGameRoom.roomId));
        reqMsg.push(isGamePossibleReqObj.ToString());
        PK_NetMgr.Ins.Send(reqMsg);

        CPacket.destroy(reqMsg);
    }

    private void ImReadyMsgSendToServer()
    {

        CPacket reqMsg = CPacket.create();
        JsonObjectCollection imReadyReqObj = SetProtocol(PROTOCOL.LOADING_COMPLETED);
        reqMsg.push(imReadyReqObj.ToString());
        PK_NetMgr.Ins.Send(reqMsg);

        CPacket.destroy(reqMsg);

    }

    private void ImWatchingMsgSendToServer()
    {
        CPacket reqMsg = CPacket.create();
        JsonObjectCollection imWatchingReqObj = SetProtocol(PROTOCOL.GAME_WATCHING_REQ);
        reqMsg.push(imWatchingReqObj.ToString());
        PK_NetMgr.Ins.Send(reqMsg);

        CPacket.destroy(reqMsg);
    }


    #endregion

    #region gameObject Setting

    private void InitCardPos()
    {
        for (int i = 0; i < NUM_OF_TOTAL_CARDS; i++)
        {
            this.cards[i].transform.DOMove(cardPos, 0.0f);
            this.backCard[i].transform.DOMove(backCardPos, 0.0f);
            this.openCards[i].transform.DOMove(openCardPos, 0.0f);
        }
    }

    private void InitDieBox()
    {
        for (int i = 0; i < NUM_OF_TOTAL_USER; i++)
        {
            this.dieBoxList[i].SetActive(false);
        }
    }

    void InitChipPos()
    {

        for (int k = 0; k < NUM_OF_TOTAL_USER; k++) //이건 사람.
        {
            for (int i = 0; i < this.ChipObj.transform.childCount; ++i)
            {
                for (int j = 0; j < 10; ++j)
                {
                    this.userChipList[k][i].chipList[j].transform.DOMove(this.userChipPos[k], 0.0f);
                }
            }
        }
    }

    private void InitListArray(int key)
    {
        List<int> userCardArray = new List<int>();

        if (!this.playerCardTable.ContainsKey(key.ToString()))
            this.playerCardTable.Add(key.ToString(), userCardArray);
    }

    private void ResetListArray()
    {

        foreach (KeyValuePair<string, List<int>> kvp in this.playerCardTable)
        {
            kvp.Value.Clear();
        }

    }

    private void CollectCardSlotPos()
    {
        Debug.Log(cardSlotGroupObj.transform.childCount);
        for (int i = 0; i < cardSlotGroupObj.transform.childCount; ++i)
        {
            CardSlotPosInfo cardSlotPosInfo = new CardSlotPosInfo();
            Transform cardSlotTrm = cardSlotGroupObj.transform.GetChild(i);

            for (int j = 0; j < cardSlotTrm.childCount; ++j)
            {
                cardSlotPosInfo.posList.Add(cardSlotTrm.GetChild(j).position);
            }

            cardSlotPosInfoList.Add(cardSlotPosInfo);
        }

        for (int i = 0; i < openCardSlotGroupObj.transform.childCount; ++i)
        {
            CardSlotPosInfo openCardSlotPosInfo = new CardSlotPosInfo();
            Transform cardSlotTrm = openCardSlotGroupObj.transform.GetChild(i);

            for (int j = 0; j < cardSlotTrm.childCount; ++j)
            {
                openCardSlotPosInfo.posList.Add(cardSlotTrm.GetChild(j).position);
            }

            openCardSlotPosInfoList.Add(openCardSlotPosInfo);
        }

        for (int i = 0; i < cardGroupObj.transform.childCount; ++i)
        {
            Vector3 cardPosInfo = new Vector3();
            Vector3 backCardPosInfo = new Vector3();

            cardPosInfo.Set(cardGroupObj.transform.GetChild(i).position.x, cardGroupObj.transform.GetChild(i).position.y, cardGroupObj.transform.GetChild(i).position.z);
            cardPosInfo.Set(backCardGroupObj.transform.GetChild(i).position.x, backCardGroupObj.transform.GetChild(i).position.y, backCardGroupObj.transform.GetChild(i).position.z);

            cardPosInfoList.Add(cardPosInfo);
            backCardPosInfoList.Add(backCardPosInfo);
        }
    }

    private void CollectCards()
    {
        this.cards = new PK_Card[52];
        this.backCard = new PK_Card[52];
        this.openCards = new PK_Card[52];

        for (int i = 0; i < cardGroupObj.transform.childCount; ++i)
        {
            cards[i] = cardGroupObj.transform.GetChild(i).GetComponent<PK_Card>();
            backCard[i] = backCardGroupObj.transform.GetChild(i).GetComponent<PK_Card>();
            openCards[i] = openCardGroup.transform.GetChild(i).GetComponent<PK_Card>();
        }
    }

    private void CollectCenterChipsPos()
    {
        for (int i = 0; i < this.ChipObj.transform.childCount; ++i)
        {
            CardSlotPosInfo centerChipPos = new CardSlotPosInfo();
            Transform userChip = this.ChipObj.transform.GetChild(i);

            for (int j = 0; j < userChip.childCount; j++)
            {
                centerChipPos.posList.Add(userChip.GetChild(j).position);
            }
            this.chipPosInfoList.Add(centerChipPos);
        }
    }

    private void CollectUserChips()
    {
        this.userChipList = new List<List<GameObjGroup>>();
        for (int k = 0; k < 5; k++)
        {
            List<GameObjGroup> eachUserChip = new List<GameObjGroup>();
            for (int i = 0; i < this.ChipObj.transform.childCount; ++i)
            {
                Transform userChip = this.userChip[k].transform.GetChild(i);
                GameObjGroup tempGroup = new GameObjGroup();

                for (int j = 0; j < userChip.childCount; ++j)
                {
                    tempGroup.chipList.Add(userChip.GetChild(j).gameObject);
                }
                eachUserChip.Add(tempGroup);
            }
            this.userChipList.Add(eachUserChip);
        }
        InitChipPos();
    }

    private void CollectBoxes()
    {
        this.turnBoxList = new List<GameObject>();
        this.dieBoxList = new List<GameObject>();
        this.winnerBoxList = new List<GameObject>();
        this.bossBoxList = new List<GameObject>();
        this.avataList = new List<GameObject>();
        this.countDownList = new List<GameObject>();

        for (int k = 0; k < NUM_OF_TOTAL_USER; k++)
        {
            GameObject userTurnBox = this.userTurnBox[k].transform.gameObject;
            GameObject userDieBox = this.userDieBox[k].transform.gameObject;
            GameObject winnerBox = this.winnerBox[k].transform.gameObject;
            GameObject bossBox = this.bossBox[k].transform.gameObject;
            GameObject avata = this.userAvata[k].transform.gameObject;
            GameObject winnerInfo = this.winnerInfo[k].transform.gameObject;
            GameObject countDown = this.countDown[k].transform.gameObject;

            userTurnBox.SetActive(false);
            userDieBox.SetActive(false);
            winnerBox.SetActive(false);
            bossBox.SetActive(false);
            avata.SetActive(false);
            countDown.SetActive(false);

            this.turnBoxList.Add(userTurnBox);
            this.dieBoxList.Add(userDieBox);
            this.winnerBoxList.Add(winnerBox);
            this.bossBoxList.Add(bossBox);
            this.avataList.Add(avata);
            this.countDownList.Add(countDown);
        }

    }

    private void CollectHand()
    {
        this.handList = new List<GameObject>();

        for (int k = 0; k < 10; k++)
        {
            GameObject userHand = this.userHand[k].transform.gameObject;
            userHand.SetActive(false);
            this.handList.Add(userHand);
        }

    }

    void CollectBettingChips()
    {
        for (int i = 0; i < this.ChipObj.transform.childCount; ++i)
        {
            Transform chipGroup = this.ChipObj.transform.GetChild(i);
            GameObjGroup tempGroup = new GameObjGroup();

            for (int j = 0; j < chipGroup.childCount; ++j)
            {
                tempGroup.chipList.Add(chipGroup.GetChild(j).gameObject);
            }
            chipGroupList.Add(tempGroup);
        }

    }

    #endregion

    #region update money
    private void SetAryZero(int[] moneyArray)
    {

        for (int i = 0; i < moneyArray.Length; i++)
        {
            moneyArray[i] = 0;
        }
    }

    private void UpdateTotalChip(string inputTotalMoney)
    {
        SetAryZero(this.drawMoney);
        this.drawMoney = MoneyStringToIntArray(inputTotalMoney);

        for (int i = 0; i < this.drawMoney.Length; ++i)
        {
            if (this.drawMoney[i] != 0)
            {
                for (int j = 0; j < this.chipGroupList[i].chipList.Count; ++j)
                {
                    if (j < this.drawMoney[i])
                        this.chipGroupList[i].chipList[j].SetActive(true);
                    else
                        this.chipGroupList[i].chipList[j].SetActive(false);
                }
            }
            else
            {
                for (int j = 0; j < chipGroupList[i].chipList.Count; ++j)
                    this.chipGroupList[i].chipList[j].SetActive(false);
            }
        }
    }

    private void myfunction(int userIndex, int i, int j)
    {

        this.userChipList[userIndex][i].chipList[j].transform.DOMove(this.userChipPos[userIndex], 0.0f);
    }

    private void ThrowChip(string inputUserMoney, int userIndex)
    {
        SetAryZero(this.throwMoney);
        this.throwMoney = MoneyStringToIntArray(inputUserMoney);

        for (int i = 0; i < this.throwMoney.Length; ++i)
        {
            if (this.throwMoney[i] != 0)
            {
                for (int j = 0; j < this.chipGroupList[i].chipList.Count; ++j)
                {
                    if (j < this.throwMoney[i])
                    {
                        int index1 = i;
                        int idnex2 = j;
                        Vector3 position = new Vector3(UnityEngine.Random.Range(-0.5f, 0.5f), UnityEngine.Random.Range(-0.2f, 0.8f), 0);
                        this.userChipList[userIndex][i].chipList[j].transform.DOMove(position, 0.5f).OnComplete(() => myfunction(userIndex, index1, idnex2));
                    }
                }
            }

        }
    }
    #endregion

    #region Card animation
    private void CardMoveToPosForWatchingUser(int playerIndex, int gCurGU, int cardIndex) //노말한 상황
    {
        try
        {
            if (gCurGU == 0 || gCurGU == 1)
            {
                backCard[cardIndex].GetComponent<SpriteRenderer>().sortingOrder = this.cardOrder;
                backCard[cardIndex].transform.DOMove(cardSlotPosInfoList[playerIndex].posList[gCurGU], 0.0f);
            }
            else
            {
                cards[cardIndex].GetComponent<SpriteRenderer>().sortingOrder = this.cardOrder;
                cards[cardIndex].transform.DOMove(cardSlotPosInfoList[playerIndex].posList[gCurGU], 0.0f);
            }

        }
        catch (System.Exception e)
        {
            Debug.Log(e.Message);
            Debug.Log(e.StackTrace);
        }
    }

    private void CardMoveToPosForPlayingUser(int playerIndex, int gCurGU, int cardIndex) //노말한 상황
    {
        try
        {
            if (gCurGU == 6)
            {
                if (myPlayIndex != userPos[playerIndex])
                {
                    backCard[cardIndex].GetComponent<SpriteRenderer>().sortingOrder = this.cardOrder;
                    backCard[cardIndex].transform.DOMove(cardSlotPosInfoList[playerIndex].posList[gCurGU], 0.5f);
                }
                else
                {
                    cards[cardIndex].GetComponent<SpriteRenderer>().sortingOrder = this.cardOrder;
                    cards[cardIndex].transform.DOMove(cardSlotPosInfoList[playerIndex].posList[gCurGU], 0.5f);
                }
            }
            else
            {
                cards[cardIndex].GetComponent<SpriteRenderer>().sortingOrder = this.cardOrder;
                cards[cardIndex].transform.DOMove(cardSlotPosInfoList[playerIndex].posList[gCurGU], 0.5f);
            }

        }
        catch (System.Exception e)
        {
            Debug.Log(e.Message);
            Debug.Log(e.StackTrace);
        }
    }

    private void CardOpenToPosForPlayingUser(int playerIndex, int playerPos, int gCurGU, int cardIndex) // 카드오픈
    {
        try
        {
            if (myPlayIndex != userPos[playerPos])
            {
                backCard[cardIndex].transform.DOMove(cardSlotPosInfoList[cardSlotGroupObj.transform.childCount - 1].posList[0], 0.0f);
                cards[cardIndex].GetComponent<SpriteRenderer>().sortingOrder = 3;
                cards[cardIndex].transform.position = cardDeckPosObj.transform.position;
                cards[cardIndex].transform.DOMove(cardSlotPosInfoList[playerPos].posList[2], 0.0f);
            }
            else
            {
                cards[cardIndex].GetComponent<SpriteRenderer>().sortingOrder = 3;
                cards[cardIndex].transform.position = cardDeckPosObj.transform.position;
                cards[cardIndex].transform.DOMove(cardSlotPosInfoList[playerPos].posList[2], 0.0f);
            }
        }
        catch (System.Exception e)
        {
            Debug.Log(e.Message);
            Debug.Log(e.StackTrace);
        }
    }

    private void CardRemovePosForPlayingUser(int playerIndex, int playerPos, int gCurGU, int cardIndex) //삭제할 카드
    {
        try
        {
            if (myPlayIndex != userPos[playerPos]) //상대방
            {
                backCard[cardIndex].transform.DOMove(cardSlotPosInfoList[cardSlotGroupObj.transform.childCount - 1].posList[0], 0.0f);
            }
            else
            {
                cards[cardIndex].transform.DOMove(cardSlotPosInfoList[cardSlotGroupObj.transform.childCount - 1].posList[0], 0.0f);
            }
        }
        catch (System.Exception e)
        {
            Debug.Log(e.Message);
            Debug.Log(e.StackTrace);
        }
    }

    private void CardMoveToFirstPosForPlayingUser(int playerIndex, int gCurGU, int cardIndex)
    {
        try
        {
            if (myPlayIndex != userPos[playerIndex]) // 상대편
            {
                backCard[cardIndex].GetComponent<SpriteRenderer>().sortingOrder = this.cardOrder;
                backCard[cardIndex].transform.DOMove(cardSlotPosInfoList[playerIndex].posList[gCurGU], 0.5f);
            }
            else //나
            {
                cards[cardIndex].GetComponent<SpriteRenderer>().sortingOrder = this.cardOrder;
                cards[cardIndex].transform.DOMove(cardSlotPosInfoList[playerIndex].posList[gCurGU], 0.5f);
            }
        }
        catch (System.Exception e)
        {
            Debug.Log(e.Message);
            Debug.Log(e.StackTrace);
        }
    }

    private void MoveToCardRePos(int playerIndex, int playerPos, int start, int openCardNumber, int removeCardNumber) // 이 경우에는 플레이어 인덱스가 다르다.
    {
        try
        {
            int guPos = 0;
            int sortingLayer = 1;
            for (int i = 0; i < NUM_OF_FIRST_CARDS; i++)
            {

                if (myPlayIndex != userPos[playerPos]) //상대편
                {

                    if (this.playerCardTable[playerIndex.ToString()][i] != openCardNumber &&
                        this.playerCardTable[playerIndex.ToString()][i] != removeCardNumber)
                    {

                        backCard[this.playerCardTable[playerIndex.ToString()][i]].GetComponent<SpriteRenderer>().sortingOrder = sortingLayer;
                        backCard[this.playerCardTable[playerIndex.ToString()][i]].transform.DOMove(cardSlotPosInfoList[playerPos].posList[guPos], 0.0f);
                        sortingLayer++;
                        guPos++;
                    }
                }
                else // 나
                {

                    if (this.playerCardTable[playerIndex.ToString()][i] != openCardNumber &&
                        this.playerCardTable[playerIndex.ToString()][i] != removeCardNumber)
                    {

                        cards[this.playerCardTable[playerIndex.ToString()][i]].GetComponent<SpriteRenderer>().sortingOrder = sortingLayer++;
                        cards[this.playerCardTable[playerIndex.ToString()][i]].transform.DOMove(cardSlotPosInfoList[playerPos].posList[guPos++], 0.0f);
                    }
                }

            }
        }
        catch (System.Exception e)
        {
            Debug.Log(e.Message);
            Debug.Log(e.StackTrace);
        }

    }
    #endregion

    #region button event

    private int MakeButtonNumber(string buttonName)
    {
        if (buttonName.Equals("DIE")) return 0;
        else if (buttonName.Equals("CHECK")) return 1;
        else if (buttonName.Equals("CALL"))
        {
            if (lastBet) return 7;
            else return 2;
        }
        else if (buttonName.Equals("PPING")) return 3;
        else if (buttonName.Equals("DDADANG")) return 4;
        else if (buttonName.Equals("HALF")) return 5;
        else if (buttonName.Equals("Allin")) return 6;
        else return -1;
    }

    public void OnClick_GameOut()
    {
        if (this.roomOutCheck == false)
        {
            UIButton.current.transform.GetChild(0).GetComponent<UILabel>().text = "예약 완료";
            this.roomOutCheck = true;
        }
        else
        {
            UIButton.current.transform.GetChild(0).GetComponent<UILabel>().text = "나가기";
            this.roomOutCheck = false;
        }
           

        Debug.Log(this.curGameRoom.roomId.ToString());

        CPacket reqMsg = CPacket.create();
        JsonObjectCollection gameOutReqJsonObj = SetProtocol(PROTOCOL.OUT_GAME_ROOM_REQ);
        gameOutReqJsonObj.Add(new JsonStringValue("ROOM_ID", this.curGameRoom.roomId));
        reqMsg.push(gameOutReqJsonObj.ToString());
        PK_NetMgr.Ins.Send(reqMsg);


        CPacket.destroy(reqMsg);

    }

    public void OnClick_GotoLogin()
    {
        PlayerPrefs.SetString("disconect", "true");
        PK_NetMgr.Ins.Destroy();
        Application.LoadLevel("pokerScene");
        this.popupNetworkObj.SetActive(false);
    }

    public void OnClick_TurnOut()
    {
        this.handList[preHand].SetActive(true);
        string buttonName = UIButton.current.transform.GetChild(0).GetComponent<UILabel>().text;

        if (buttonName.Equals("CHECK")) this.checkBetFlag = true;
        else if (buttonName.Equals("Allin")) this.sideBettingFlag = true;

        NormalDistributeCompleteMsgSendToServer(MakeButtonNumber(buttonName));
    }

    public void OnClick_GameStart()
    {
        this.StartBtn.SetActive(false);
        GameStartMsgSendToServer();
    }

    public void OnClick_SelectedCard(GameObject gameObjcet)
    {
        tempCount++;
        if (tempCount < 2)
        {
            removeCardIndex = gameObjcet.ToString().Replace("cards_", "").ToString().Replace("(UnityEngine.GameObject)", "");
            openCards[int.Parse(removeCardIndex)].GetComponent<UIButton>().isEnabled = false;
        }
        else if (tempCount == 2)
        {

            CPacket reqMsg = CPacket.create();
            JsonObjectCollection isGamePossibleReqObj = SetProtocol(PROTOCOL.GAME_OPEN_CARD_SELECT_COMPLETE);

            isGamePossibleReqObj.Add(new JsonStringValue("OPEN_CARD", gameObjcet.ToString().Replace("cards_", "").ToString().Replace("(UnityEngine.GameObject)", "")));
            isGamePossibleReqObj.Add(new JsonStringValue("REMOVE_CARD", removeCardIndex));

            reqMsg.push(isGamePossibleReqObj.ToString());
            PK_NetMgr.Ins.Send(reqMsg);
            CPacket.destroy(reqMsg);

        }
    }

    private void InitButton()
    {
        this.CheckBtn.GetComponent<UIButton>().isEnabled = false;
        this.PPingBtn.GetComponent<UIButton>().isEnabled = false;
        this.DDangBtn.GetComponent<UIButton>().isEnabled = false;
        this.CallBtn.GetComponent<UIButton>().isEnabled = false;
        this.DieBtn.GetComponent<UIButton>().isEnabled = false;
        this.HalfBtn.GetComponent<UIButton>().isEnabled = false;
        this.AllinBtn.GetComponent<UIButton>().isEnabled = false;
        return;
    }

    private void LastButtonActive(string pandonMoney, string beforeMoney, string totalMoney)
    {

        if (Int64.Parse(PK_NetMgr.Ins.userHasMoney) < Int64.Parse(beforeMoney)) //내가 이전에 배팅 했던 돈 보다 작아서 콜도 못하는 상황.
        {
            this.CheckBtn.GetComponent<UIButton>().isEnabled = false;
            this.PPingBtn.GetComponent<UIButton>().isEnabled = false;
            this.DDangBtn.GetComponent<UIButton>().isEnabled = false;
            this.CallBtn.GetComponent<UIButton>().isEnabled = false;
            this.DieBtn.GetComponent<UIButton>().isEnabled = true;
            this.HalfBtn.GetComponent<UIButton>().isEnabled = false;
            this.AllinBtn.GetComponent<UIButton>().GetComponentInChildren<UISprite>().depth = 15;
            this.AllinBtn.GetComponent<UIButton>().isEnabled = true;
        }
        else // 다이와 콜만.
        {
            this.CheckBtn.GetComponent<UIButton>().isEnabled = false;
            this.PPingBtn.GetComponent<UIButton>().isEnabled = false;
            this.DDangBtn.GetComponent<UIButton>().isEnabled = false;
            this.CallBtn.GetComponent<UIButton>().isEnabled = true;
            this.DieBtn.GetComponent<UIButton>().isEnabled = true;
            this.HalfBtn.GetComponent<UIButton>().isEnabled = false;
            this.AllinBtn.GetComponent<UIButton>().GetComponentInChildren<UISprite>().depth = -1;
            this.AllinBtn.GetComponent<UIButton>().isEnabled = false;
        }
    }

    private void ButtonActive(string pandonMoney, string beforeMoney, string totalMoney)
    {
        if (this.sideBettingFlag == true)
        {
            this.CheckBtn.GetComponent<UIButton>().isEnabled = false;
            this.PPingBtn.GetComponent<UIButton>().isEnabled = false;
            this.DDangBtn.GetComponent<UIButton>().isEnabled = false;
            this.CallBtn.GetComponent<UIButton>().isEnabled = false;
            this.DieBtn.GetComponent<UIButton>().isEnabled = true;
            this.HalfBtn.GetComponent<UIButton>().isEnabled = false;
            this.AllinBtn.GetComponent<UIButton>().GetComponentInChildren<UISprite>().depth = 15;
            this.AllinBtn.GetComponent<UIButton>().isEnabled = false;
            return;
        }

        //상황에 따른 버튼
        if (this.myPlayIndex != this.bossUserIndex) //내가 보스가 아닌경우
        {

            if (this.beforeMoney.Equals("0"))
            {
                this.CheckBtn.GetComponent<UIButton>().isEnabled = false;
                this.PPingBtn.GetComponent<UIButton>().isEnabled = false;
                this.DDangBtn.GetComponent<UIButton>().isEnabled = false;
                this.CallBtn.GetComponent<UIButton>().isEnabled = false;
                this.DieBtn.GetComponent<UIButton>().isEnabled = true;
                this.HalfBtn.GetComponent<UIButton>().isEnabled = true;
            }
            else if (Int64.Parse(PK_NetMgr.Ins.userHasMoney) < Int64.Parse(pandonMoney)) //내가 이전에 배팅 했던 돈 보다 작아서 콜도 못하는 상황.
            {
                this.CheckBtn.GetComponent<UIButton>().isEnabled = false;
                this.PPingBtn.GetComponent<UIButton>().isEnabled = false;
                this.DDangBtn.GetComponent<UIButton>().isEnabled = false;
                this.CallBtn.GetComponent<UIButton>().isEnabled = false;
                this.DieBtn.GetComponent<UIButton>().isEnabled = true;
                this.HalfBtn.GetComponent<UIButton>().isEnabled = false;
                this.AllinBtn.GetComponent<UIButton>().GetComponentInChildren<UISprite>().depth = 15;
                this.AllinBtn.GetComponent<UIButton>().isEnabled = true;
                return;
            }
            else if (Int64.Parse(PK_NetMgr.Ins.userHasMoney) < Int64.Parse(beforeMoney)) //내가 이전에 배팅 했던 돈 보다 작아서 콜도 못하는 상황.
            {
                this.CheckBtn.GetComponent<UIButton>().isEnabled = false;
                this.PPingBtn.GetComponent<UIButton>().isEnabled = false;
                this.DDangBtn.GetComponent<UIButton>().isEnabled = false;
                this.CallBtn.GetComponent<UIButton>().isEnabled = false;
                this.DieBtn.GetComponent<UIButton>().isEnabled = true;
                this.HalfBtn.GetComponent<UIButton>().isEnabled = false;
                this.AllinBtn.GetComponent<UIButton>().GetComponentInChildren<UISprite>().depth = 15;
                this.AllinBtn.GetComponent<UIButton>().isEnabled = true;
                return;
            }
            else if (Int64.Parse(PK_NetMgr.Ins.userHasMoney) < (Int64.Parse(totalMoney) / (Int64)2))
            { //내 보유 금액이 하프 금액 보다 적으면 
                this.CheckBtn.GetComponent<UIButton>().isEnabled = false;
                this.PPingBtn.GetComponent<UIButton>().isEnabled = false;
                this.DDangBtn.GetComponent<UIButton>().isEnabled = true;
                this.CallBtn.GetComponent<UIButton>().isEnabled = true;
                this.DieBtn.GetComponent<UIButton>().isEnabled = true;
                this.HalfBtn.GetComponent<UIButton>().isEnabled = false;
                this.AllinBtn.GetComponent<UIButton>().GetComponentInChildren<UISprite>().depth = 15;
                this.AllinBtn.GetComponent<UIButton>().isEnabled = true;
                return;
            }
           
            else//삥 체크 제거 하고 다 켜줌.
            {
                this.CheckBtn.GetComponent<UIButton>().isEnabled = false;
                this.PPingBtn.GetComponent<UIButton>().isEnabled = false;
                this.DDangBtn.GetComponent<UIButton>().isEnabled = true;
                this.CallBtn.GetComponent<UIButton>().isEnabled = true;
                this.DieBtn.GetComponent<UIButton>().isEnabled = true;
                this.HalfBtn.GetComponent<UIButton>().isEnabled = true;
                return;
            }
        }
        else // 보스 인경우
        {
            if (this.checkBetFlag == false) //체크를 안했다.
            {
                if (this.firstBetting == false) //첫배팅
                {
                    if (Int64.Parse(PK_NetMgr.Ins.userHasMoney) < Int64.Parse(pandonMoney)) //내가 이전에 배팅 했던 돈 보다 작아서 콜도 못하는 상황.
                    {
                        this.CheckBtn.GetComponent<UIButton>().isEnabled = false;
                        this.PPingBtn.GetComponent<UIButton>().isEnabled = false;
                        this.DDangBtn.GetComponent<UIButton>().isEnabled = false;
                        this.CallBtn.GetComponent<UIButton>().isEnabled = false;
                        this.DieBtn.GetComponent<UIButton>().isEnabled = true;
                        this.HalfBtn.GetComponent<UIButton>().isEnabled = false;
                        this.AllinBtn.GetComponent<UIButton>().GetComponentInChildren<UISprite>().depth = 15;
                        this.AllinBtn.GetComponent<UIButton>().isEnabled = true;
                        this.firstBetting = true;
                        return;
                    }
                    else if (Int64.Parse(PK_NetMgr.Ins.userHasMoney) < Int64.Parse(beforeMoney)) //내가 이전에 배팅 했던 돈 보다 작아서 콜도 못하는 상황.
                    {
                        this.CheckBtn.GetComponent<UIButton>().isEnabled = true;
                        this.PPingBtn.GetComponent<UIButton>().isEnabled = true;
                        this.DDangBtn.GetComponent<UIButton>().isEnabled = false;
                        this.CallBtn.GetComponent<UIButton>().isEnabled = false;
                        this.DieBtn.GetComponent<UIButton>().isEnabled = true;
                        this.HalfBtn.GetComponent<UIButton>().isEnabled = false;
                        this.AllinBtn.GetComponent<UIButton>().GetComponentInChildren<UISprite>().depth = 15;
                        this.AllinBtn.GetComponent<UIButton>().isEnabled = true;
                        this.firstBetting = true;
                        return;
                    }
                    else if (Int64.Parse(PK_NetMgr.Ins.userHasMoney) < (Int64.Parse(totalMoney) / (Int64)2))
                    {
                        this.CheckBtn.GetComponent<UIButton>().isEnabled = true;
                        this.PPingBtn.GetComponent<UIButton>().isEnabled = true;
                        this.DDangBtn.GetComponent<UIButton>().isEnabled = false;
                        this.CallBtn.GetComponent<UIButton>().isEnabled = false;
                        this.DieBtn.GetComponent<UIButton>().isEnabled = true;
                        this.HalfBtn.GetComponent<UIButton>().isEnabled = false;
                        this.AllinBtn.GetComponent<UIButton>().GetComponentInChildren<UISprite>().depth = 15;
                        this.AllinBtn.GetComponent<UIButton>().isEnabled = true;
                        this.firstBetting = true;
                        return;
                    }
                    else
                    {
                        this.firstBetting = true;
                        this.CheckBtn.GetComponent<UIButton>().isEnabled = true;
                        this.PPingBtn.GetComponent<UIButton>().isEnabled = true;
                        this.DDangBtn.GetComponent<UIButton>().isEnabled = false;
                        this.CallBtn.GetComponent<UIButton>().isEnabled = false;
                        this.DieBtn.GetComponent<UIButton>().isEnabled = true;
                        this.HalfBtn.GetComponent<UIButton>().isEnabled = true;
                        return;
                    }
                }
                else //첫배팅 이후
                {
                    if (Int64.Parse(PK_NetMgr.Ins.userHasMoney) < Int64.Parse(pandonMoney)) //내가 이전에 배팅 했던 돈 보다 작아서 콜도 못하는 상황.
                    {
                        this.CheckBtn.GetComponent<UIButton>().isEnabled = false;
                        this.PPingBtn.GetComponent<UIButton>().isEnabled = false;
                        this.DDangBtn.GetComponent<UIButton>().isEnabled = false;
                        this.CallBtn.GetComponent<UIButton>().isEnabled = false;
                        this.DieBtn.GetComponent<UIButton>().isEnabled = true;
                        this.HalfBtn.GetComponent<UIButton>().isEnabled = false;
                        this.AllinBtn.GetComponent<UIButton>().GetComponentInChildren<UISprite>().depth = 15;
                        this.AllinBtn.GetComponent<UIButton>().isEnabled = true;
                    }
                    else if (Int64.Parse(PK_NetMgr.Ins.userHasMoney) < Int64.Parse(beforeMoney)) //내가 이전에 배팅 했던 돈 보다 작아서 콜도 못하는 상황.
                    {
                        this.CheckBtn.GetComponent<UIButton>().isEnabled = false;
                        this.PPingBtn.GetComponent<UIButton>().isEnabled = false;
                        this.DDangBtn.GetComponent<UIButton>().isEnabled = false;
                        this.CallBtn.GetComponent<UIButton>().isEnabled = false;
                        this.DieBtn.GetComponent<UIButton>().isEnabled = true;
                        this.HalfBtn.GetComponent<UIButton>().isEnabled = false;
                        this.AllinBtn.GetComponent<UIButton>().GetComponentInChildren<UISprite>().depth = 15;
                        this.AllinBtn.GetComponent<UIButton>().isEnabled = true;

                    }
                    else if (Int64.Parse(PK_NetMgr.Ins.userHasMoney) < (Int64.Parse(totalMoney) / (Int64)2))
                    {
                        this.CheckBtn.GetComponent<UIButton>().isEnabled = false;
                        this.PPingBtn.GetComponent<UIButton>().isEnabled = false;
                        this.DDangBtn.GetComponent<UIButton>().isEnabled = false;
                        this.CallBtn.GetComponent<UIButton>().isEnabled = false;
                        this.DieBtn.GetComponent<UIButton>().isEnabled = true;
                        this.HalfBtn.GetComponent<UIButton>().isEnabled = false;
                        this.AllinBtn.GetComponent<UIButton>().GetComponentInChildren<UISprite>().depth = 15;
                        this.AllinBtn.GetComponent<UIButton>().isEnabled = true;
                    }
                    else
                    {
                        this.CheckBtn.GetComponent<UIButton>().isEnabled = true;
                        this.PPingBtn.GetComponent<UIButton>().isEnabled = true;
                        this.DDangBtn.GetComponent<UIButton>().isEnabled = true;
                        this.CallBtn.GetComponent<UIButton>().isEnabled = true;
                        this.DieBtn.GetComponent<UIButton>().isEnabled = true;
                        this.HalfBtn.GetComponent<UIButton>().isEnabled = true;
                        return;
                    }
                }
            }
            else //체크를 한 후 이면 무조건 두 번째 이상이란 소리이다.
            {
                if (Int64.Parse(PK_NetMgr.Ins.userHasMoney) < Int64.Parse(pandonMoney)) //내가 이전에 배팅 했던 돈 보다 작아서 콜도 못하는 상황.
                {
                    this.CheckBtn.GetComponent<UIButton>().isEnabled = false;
                    this.PPingBtn.GetComponent<UIButton>().isEnabled = false;
                    this.DDangBtn.GetComponent<UIButton>().isEnabled = false;
                    this.CallBtn.GetComponent<UIButton>().isEnabled = false;
                    this.DieBtn.GetComponent<UIButton>().isEnabled = true;
                    this.HalfBtn.GetComponent<UIButton>().isEnabled = false;
                    this.AllinBtn.GetComponent<UIButton>().GetComponentInChildren<UISprite>().depth = 15;
                    this.AllinBtn.GetComponent<UIButton>().isEnabled = true;
                    this.firstBetting = true;
                    return;
                }
                else if (Int64.Parse(PK_NetMgr.Ins.userHasMoney) < Int64.Parse(beforeMoney)) //내가 이전에 배팅 했던 돈 보다 작아서 콜도 못하는 상황.
                {
                    this.CheckBtn.GetComponent<UIButton>().isEnabled = false;
                    this.PPingBtn.GetComponent<UIButton>().isEnabled = false;
                    this.DDangBtn.GetComponent<UIButton>().isEnabled = false;
                    this.CallBtn.GetComponent<UIButton>().isEnabled = false;
                    this.DieBtn.GetComponent<UIButton>().isEnabled = true;
                    this.HalfBtn.GetComponent<UIButton>().isEnabled = false;
                    this.AllinBtn.GetComponent<UIButton>().GetComponentInChildren<UISprite>().depth = 15;
                    this.AllinBtn.GetComponent<UIButton>().isEnabled = true;
                    this.firstBetting = true;
                    return;
                }
                else if (Int64.Parse(PK_NetMgr.Ins.userHasMoney) < (Int64.Parse(totalMoney) / (Int64)2))
                {
                    this.CheckBtn.GetComponent<UIButton>().isEnabled = false;
                    this.PPingBtn.GetComponent<UIButton>().isEnabled = false;
                    this.DDangBtn.GetComponent<UIButton>().isEnabled = true;
                    this.CallBtn.GetComponent<UIButton>().isEnabled = true;
                    this.DieBtn.GetComponent<UIButton>().isEnabled = true;
                    this.HalfBtn.GetComponent<UIButton>().isEnabled = false;
                    this.AllinBtn.GetComponent<UIButton>().GetComponentInChildren<UISprite>().depth = 15;
                    this.AllinBtn.GetComponent<UIButton>().isEnabled = true;
                    this.firstBetting = true;
                    return;
                }
                else
                {
                    this.firstBetting = true;
                    this.CheckBtn.GetComponent<UIButton>().isEnabled = false;
                    this.PPingBtn.GetComponent<UIButton>().isEnabled = false;
                    this.DDangBtn.GetComponent<UIButton>().isEnabled = true;
                    this.CallBtn.GetComponent<UIButton>().isEnabled = true;
                    this.DieBtn.GetComponent<UIButton>().isEnabled = true;
                    this.HalfBtn.GetComponent<UIButton>().isEnabled = true;
                    return;
                }
            }
        }
    }

    #endregion

    #region Card and label Display

    private string GetHighCardIntToString(int highCard)
    {
        string output;
        if (highCard == (short)PK_Card.VALUE.JACK) output = "J";
        else if (highCard == (short)PK_Card.VALUE.QUEEN) output = "Q";
        else if (highCard == (short)PK_Card.VALUE.KING) output = "K";
        else if (highCard == (short)PK_Card.VALUE.ACE) output = "A";
        else output = highCard.ToString();
        return output;
    }

    private string GetHighShapeIntToString(int shape)
    {
        string output;
        if (shape == (short)PK_Card.SUIT.CLUBS) output = "CLUB";
        else if (shape == (short)PK_Card.SUIT.SPADES) output = "SPADE";
        else if (shape == (short)PK_Card.SUIT.HEARTS) output = "HEART";
        else output = "DIAMOND";
        return output;
    }
    private void DisplayMyHand(PK_Card.HAND myHand, int highCard, int shape)
    {
        this.handList[this.preHand].SetActive(false);
        if (myHand == PK_Card.HAND.Nothig) { this.handList[0].SetActive(true); this.preHand = 0; }
        else if (myHand == PK_Card.HAND.OnePair) { this.handList[1].SetActive(true); this.preHand = 1; }
        else if (myHand == PK_Card.HAND.TwoPairs) { this.handList[2].SetActive(true); this.preHand = 2; }
        else if (myHand == PK_Card.HAND.ThreeKind) { this.handList[3].SetActive(true); this.preHand = 3; }
        else if (myHand == PK_Card.HAND.Straight) { this.handList[4].SetActive(true); this.preHand = 4; }
        else if (myHand == PK_Card.HAND.Flush) { this.handList[5].SetActive(true); this.preHand = 5; }
        else if (myHand == PK_Card.HAND.FullHouse) { this.handList[6].SetActive(true); this.preHand = 6; }
        else if (myHand == PK_Card.HAND.FourKind) { this.handList[7].SetActive(true); this.preHand = 7; }
        else if (myHand == PK_Card.HAND.StraightFlush) { this.handList[8].SetActive(true); this.preHand = 8; }
        else if (myHand == PK_Card.HAND.RoyalStraightFlush) { this.handList[9].SetActive(true); this.preHand = 9; }

        this.handValue = GameObject.Find("HandValue").GetComponent<UILabel>();
        string handCard = GetHighShapeIntToString(shape);
        handCard += " " + GetHighCardIntToString(highCard);
        this.handValue.text = handCard;

    }
    private void DisplayUpdateEachUserMoneyAndNextUser(JSONNode msg)
    {
        int curUserIndex = FindUserDeviceRealPos(int.Parse(msg["CUR_USER_INDEX"].Value));
        string curUserBettingMoney = msg["CUR_USER_BETTING_MONEY"].Value; //이거 가지구 돈 나간 애니메이션 구현.
        this.pandonMoney = msg["PANDON"].Value;
        this.beforeMoney = msg["BEFORE_MONEY"].Value;
        string totalMoney = msg["TOTAL_MONEY"].Value;
        string curUserMoney = msg["CUR_USER_MONEY"].Value;
        int nextUser = int.Parse(msg["NEXT_USER_INDEX"].Value);
        this.lastBet = bool.Parse(msg["LAST_BET"].Value);

        this.curTurnPlayer = nextUser;

        if (int.Parse(msg["CUR_USER_INDEX"].Value) == this.myPlayIndex)
        {
            PK_NetMgr.Ins.userHasMoney = curUserMoney;
        }

        int nextUserPos = FindUserDeviceRealPos(nextUser);
        string bettingName = msg["BETTING_NAME"].Value;
        this.totalMoney = totalMoney;


        if (lastBet) LastButtonActive(this.pandonMoney, this.beforeMoney, totalMoney);
        else ButtonActive(this.pandonMoney, this.beforeMoney, totalMoney);

        ThrowChip(curUserBettingMoney, curUserIndex);

        DisplayBettingName(curUserIndex, bettingName);
        DisplayUpdateEachUserMoney(curUserIndex, curUserMoney);
        DisplayUpdateTotalMoney(totalMoney);
        UpdateTotalChip(totalMoney.ToString());

        this.handList[preHand].SetActive(true);
        DisplayCurTurnPlayer(false);
        this.firstPlayerIndex = nextUserPos;
        DisplayCurTurnPlayer(true);


        if (this.curTurnPlayer == this.myPlayIndex && this.sideBettingFlag == true)
        {
            NormalDistributeCompleteMsgSendToServer(MakeButtonNumber("8"));
        }

    }

    private void DisplayUpdateTotalMoney(string inputMoney)
    {
        totalMoneyLable = GameObject.Find("TotalMoney").GetComponent<UILabel>();
        Int64 intTotalMoney = Int64.Parse(inputMoney);
        if (intTotalMoney / 10000 < 10000)
        {
            Int64 value = intTotalMoney / 10000;
            totalMoneyLable.text = value.ToString() + "만원";

        }
        else if (intTotalMoney / 10000 >= 100000000)
        {
            Int64 value = intTotalMoney / 100000000;
            Int64 mok = value / 100000000;
            Int64 Spare = value % 100000000;
            if (Spare != 0)
            {
                totalMoneyLable.text = mok.ToString() + "조 " + Spare.ToString() + "억원";
            }
            else
            {
                totalMoneyLable.text = mok.ToString() + "조";
            }
        }
        else if (intTotalMoney / 10000 >= 10000)
        {
            Int64 value = intTotalMoney / 10000;
            Int64 mok = value / 10000;
            Int64 Spare = value % 10000;
            if (Spare != 0)
            {
                totalMoneyLable.text = mok.ToString() + "억 " + Spare.ToString() + "만원";
            }
            else
            {
                totalMoneyLable.text = mok.ToString() + "억원";
            }
        }


    }

    private void DisplayUpdateEachUserMoney(int userIndex, string updateMoney)
    {

        userMoney[userIndex] = GameObject.Find("UserMoney_" + userIndex.ToString()).GetComponent<UILabel>();
        Int64 curMoney = Int64.Parse(updateMoney);

        if (curMoney / 10000 < 10000)
        {
            Int64 value = curMoney / 10000;
            this.userMoney[userIndex].text = value.ToString() + "만원";

        }
        else if (curMoney / 10000 >= 100000000)
        {
            Int64 value = curMoney / 100000000;
            Int64 mok = value / 100000000;
            Int64 Spare = value % 100000000;
            if (Spare != 0)
            {
                this.userMoney[userIndex].text = mok.ToString() + "조 " + Spare.ToString() + "억원";
            }
            else
            {
                this.userMoney[userIndex].text = mok.ToString() + "조";
            }
        }
        else if (curMoney / 10000 >= 10000)
        {
            Int64 value = curMoney / 10000;
            Int64 mok = value / 10000;
            Int64 Spare = value % 10000;
            if (Spare != 0)
            {
                this.userMoney[userIndex].text = mok.ToString() + "억 " + Spare.ToString() + " 만원";
            }
            else
            {
                this.userMoney[userIndex].text = mok.ToString() + "억원";
            }
        }
    }
    private void DisplayBettingName(int userIndex, string bettingName)
    {
        this.userBetting[userIndex] = GameObject.Find("UserBet_" + userIndex.ToString()).GetComponent<UILabel>();
        this.userBetting[userIndex].text = bettingName;
    }
    private void InitDisplayBettingName()
    {
        for (int i = 0; i < NUM_OF_TOTAL_USER; i++)
        {
            this.userBetting[i] = GameObject.Find("UserBet_" + i.ToString()).GetComponent<UILabel>();
            this.userBetting[i].text = "";
        }
    }
    private void InitDisplayWinnerinfo(int userIndex)
    {
        this.winnerInfo[userIndex] = GameObject.Find("Winner_" + userIndex.ToString()).GetComponent<UILabel>();
        this.winnerInfo[userIndex].text = "";

    }

    private void DisplayWinner(int userIndex, PK_Card.HAND winnerInfo, int card, int shape)
    {
        this.winnerInfo[userIndex] = GameObject.Find("Winner_" + userIndex.ToString()).GetComponent<UILabel>();
        string name = Enum.GetName(typeof(PK_Card.HAND), winnerInfo);
        string output = GetHighShapeIntToString(shape);
        output += " " + GetHighCardIntToString(card);
        this.winnerInfo[userIndex].text = name + " " + output;
    }

    private void InitDisplayUserName(int userIndex)
    {
        this.userName[userIndex] = GameObject.Find("UserName_" + userIndex.ToString()).GetComponent<UILabel>();
        this.userName[userIndex].text = "";
    }

    private void InitDisplayUserMoney(int userIndex)
    {
        this.userMoney[userIndex] = GameObject.Find("UserMoney_" + userIndex.ToString()).GetComponent<UILabel>();
        this.userMoney[userIndex].text = "";
    }
    private void DisplayUserName(int userIndex, string userId)
    {
        this.userName[userIndex] = GameObject.Find("UserName_" + userIndex.ToString()).GetComponent<UILabel>();
        this.userName[userIndex].text = userId;
    }

    private void DisplayCurTurnPlayer(bool turnOn)
    {
        if (turnOn)
        {
            this.turnBoxList[this.firstPlayerIndex].SetActive(true);
        }
        else
        {
            if (this.firstPlayerIndex != -1)
            {
                this.turnBoxList[this.firstPlayerIndex].SetActive(false);
            }
        }

    }
    #endregion

    #region Card Distribute
    private IEnumerator FirstDistribute(JSONNode msg)
    {

        this.totalMoney = msg["PAN_DON"].Value;
        DisplayUpdateTotalMoney(this.totalMoney);
        UpdateTotalChip(this.totalMoney);
        Int64 enterMoney = Int64.Parse(this.totalMoney) / 3;

        for (int i = 0; i < NUM_OF_FIRST_CARDS; i++)  //구
        {
            for (int j = 0; j < this.curGameRoom.curManCount; j++) //사람
            {
                int playerIndex = int.Parse(msg["USER_INDEX" + i.ToString() + j.ToString()].Value);
                int MyCardShape = int.Parse(msg["MY_SUIT" + i.ToString() + j.ToString()].Value);
                int MyCardNumber = int.Parse(msg["MY_VALUE" + i.ToString() + j.ToString()].Value);
                string userMoney = msg["USER_MONEY" + i.ToString() + j.ToString()].Value;
                if (playerIndex == this.myPlayIndex) PK_NetMgr.Ins.userHasMoney = userMoney;

                int temp = (MyCardShape * 13) + (MyCardNumber - 2);

                this.playerCardTable[playerIndex.ToString()].Add((MyCardShape * 13) + (MyCardNumber - 2));

                int playerPos = FindUserDeviceRealPos(playerIndex);
                CardMoveToFirstPosForPlayingUser(playerPos, i, (MyCardShape * 13) + (MyCardNumber - 2));
                DisplayUpdateEachUserMoney(FindUserDeviceRealPos(playerIndex), userMoney);
          
                yield return new WaitForSeconds(0.2f);
            }
            this.cardOrder++;
        }

        FirstDistributeCompleteMsgSendToServer();
    }

    private IEnumerator NormalDistribute(JSONNode msg)
    {
        this.timerStart = true;
        this.beforeMoney = "0";

        if (FindUserDeviceRealPos(this.bossUserIndex) != -1) { this.bossBoxList[FindUserDeviceRealPos(this.bossUserIndex)].SetActive(false); }

        int playingUserCount = int.Parse(msg["PLAYING_USER_COUNT"].Value);
        InitDisplayBettingName();
        DisplayCurTurnPlayer(false);

        this.bossUserIndex = int.Parse(msg["FIRST_USER"].Value);
        this.curTurnPlayer = this.bossUserIndex;
        this.firstPlayerIndex = FindUserDeviceRealPos(this.bossUserIndex);
        this.bossBoxList[FindUserDeviceRealPos(this.bossUserIndex)].SetActive(true);

        //DisplayCurTurnPlayer(true);
        ButtonActive(this.curGameRoom.betMoneyStr, "0", this.totalMoney);
 

        for (int i = 0; i < playingUserCount; i++)
        {
            int playerIndex = int.Parse(msg["USER_INDEX" + i.ToString()].Value);
            int MyCardShape = int.Parse(msg["MY_SUIT" + i.ToString()].Value);
            int MyCardNumber = int.Parse(msg["MY_VALUE" + i.ToString()].Value);
            this.m_gCurGu = int.Parse(msg["GU_COUNT" + i.ToString()].Value);

            PK_Card.HAND handValue = (PK_Card.HAND)Enum.Parse(typeof(PK_Card.HAND), msg["MY_HAND" + i.ToString()].Value);
            int highCard = int.Parse(msg["MY_HIGH_CARD" + i.ToString()].Value);
            int Shape = int.Parse(msg["MY_HIGH_SHAPE" + i.ToString()].Value);

            int playerPos = FindUserDeviceRealPos(playerIndex);

            this.playerCardTable[playerIndex.ToString()].Add((MyCardShape * 13) + (MyCardNumber - 2));
            CardMoveToPosForPlayingUser(playerPos, m_gCurGu, (MyCardShape * 13) + (MyCardNumber - 2));

            if (playerIndex == this.myPlayIndex)
            {
                DisplayMyHand(handValue, highCard, Shape);
            }
            yield return new WaitForSeconds(0.5f);
        }
        this.cardOrder++;
        DisplayCurTurnPlayer(true);

        if (this.curTurnPlayer == this.myPlayIndex && this.sideBettingFlag == true)
        {
            NormalDistributeCompleteMsgSendToServer(MakeButtonNumber("8"));
        }

    }

    private void WatchingGame(JSONNode msg)
    {

        int playingUserCount = int.Parse(msg["PLAYING_USER_COUNT"].Value);
        for (int i = 0; i < playingUserCount; i++)
        {
            //index
            this.cardOrder = 1;
            int playerIndex = int.Parse(msg["USER_INDEX" + i.ToString()].Value);
            int cardCount = int.Parse(msg["CARD_COUNT" + i.ToString()].Value);
            for (int j = 0; j < cardCount; j++)
            {
                int MyCardShape = int.Parse(msg["MY_SUIT" + i.ToString() + j.ToString()].Value);
                int MyCardNumber = int.Parse(msg["MY_VALUE" + i.ToString() + j.ToString()].Value);
                this.m_gCurGu = int.Parse(msg["GU_COUNT" + i.ToString() + j.ToString()].Value);

                int playerPos = FindUserDeviceRealPos(playerIndex);
                this.playerCardTable[playerIndex.ToString()].Add((MyCardShape * 13) + (MyCardNumber - 2));

                CardMoveToPosForWatchingUser(playerPos, this.m_gCurGu, (MyCardShape * 13) + (MyCardNumber - 2));
                this.cardOrder++;
            }
        }
    }

    private void SwapInts(List<int> array, int position1, int position2)
    {
        try
        {
            int temp = array[position1];
            array[position1] = array[position2];
            array[position2] = temp;
            Debug.Log("INDEX 1 " + position1);
            Debug.Log("INDEX 2 " + position2);
        }
        catch (SystemException e)
        {
            Debug.Log(e.Message);
            Debug.Log(e.StackTrace);
        }
    }

    private void RemoveCardArray(int key, int start, int removeCardNumber, int openCardNumber)
    {
        if (this.playerCardTable[key.ToString()].Contains(removeCardNumber))
            this.playerCardTable[key.ToString()].Remove(removeCardNumber);

        int swapIndex = this.playerCardTable[key.ToString()].IndexOf(openCardNumber);
        if(swapIndex == - 1) {
            Debug.Log("cardNumber " + openCardNumber);
            swapIndex = 0;
        }
        SwapInts(this.playerCardTable[key.ToString()], 2, swapIndex);
    }

    private void CardOpenEachUser(JSONNode msg)
    {
        this.openCardGroup.SetActive(false);
        this.popupPanelObj.SetActive(false);
        openCards[int.Parse(removeCardIndex)].GetComponent<UIButton>().isEnabled = true;

        int playingUserCount = int.Parse(msg["PLAYING_USER_COUNT"].Value);
        this.cardOrder--;
        for (int i = 0; i < playingUserCount; i++)
        {
            int playerIndex = int.Parse(msg["USER_INDEX" + i.ToString()].Value);
            int openCardNumber = int.Parse(msg["OPEN_CARD_INDEX" + i.ToString()].Value);
            int removeCardNumber = int.Parse(msg["REMOVE_CARD_INDEX" + i.ToString()].Value);
            int removeGu = int.Parse(msg["REMOVE_GU_INDEX" + i.ToString()].Value);

            int playerPos = FindUserDeviceRealPos(playerIndex);

            CardOpenToPosForPlayingUser(playerIndex, playerPos, m_gCurGu, openCardNumber);
            MoveToCardRePos(playerIndex, playerPos, removeGu, openCardNumber, removeCardNumber);
            CardRemovePosForPlayingUser(playerIndex, playerPos, m_gCurGu, removeCardNumber);

            RemoveCardArray(playerIndex, removeGu, removeCardNumber, openCardNumber);

        }
        CardOpenCompleteMsgSendToServer();
    }


    #endregion
    public void Awake()
    {
        CollectCenterChipsPos();
        CollectCardSlotPos();
        CollectCards();
        CollectBettingChips();
        CollectUserChips();
        CollectBoxes();
        CollectHand();
        InitCardPos();
        InitButton();

    }

    void Update()
    {
        if (this.imPlaying && this.timerStart) //게임 중이고.
        {
            TimeSpan responseTime = DateTime.Now - lastTime;
            if (responseTime.TotalSeconds >= CheckTime &&
                this.myPlayIndex == this.curTurnPlayer &&
                this.dieEventFlag == false)
            {
                this.countDownList[preCountDown].SetActive(false);
                NormalDistributeCompleteMsgSendToServer(0);
                this.dieEventFlag = true;
            }
            int countDown = -((int)responseTime.TotalSeconds - 10);
            if (countDown <= 5 && countDown > 0 && preCountDown != countDown)
            {
                this.countDownList[preCountDown - 1].SetActive(false);
                this.countDownList[countDown - 1].SetActive(true);
                this.preCountDown = countDown;
            }
        }
    }

    private bool IsClientExist(int playerIndex)
    {
        for (int i = 0; i < NUM_OF_TOTAL_USER; i++)
        {
            if (userPos[i] == playerIndex) return true;
        }
        return false;
    }


    private void ResetGameData()
    {
        if (this.curGameRoom.roomMaster == this.myPlayIndex && this.curGameRoom.curManCount >1) this.StartBtn.SetActive(true);
        else this.StartBtn.SetActive(false);

        for(int i = 0; i<NUM_OF_TOTAL_USER; i ++)
        {
            this.bossBoxList[i].SetActive(false);
            this.winnerBoxList[i].SetActive(false);
            this.turnBoxList[i].SetActive(false);
        }
       
        this.handList[this.preHand].SetActive(false);

        this.cardOrder = 1;

        if ((this.sideBettingFlag == true && Int64.Parse(PK_NetMgr.Ins.userHasMoney) <= 0) ||
            Int64.Parse(PK_NetMgr.Ins.userHasMoney) < Int64.Parse(curGameRoom.betMoneyStr))
        {
            Debug.Log(this.curGameRoom.roomId.ToString());

            CPacket reqMsg = CPacket.create();
            JsonObjectCollection gameOutReqJsonObj = SetProtocol(PROTOCOL.OUT_GAME_ROOM_REQ);
            gameOutReqJsonObj.Add(new JsonStringValue("ROOM_ID", this.curGameRoom.roomId));
            reqMsg.push(gameOutReqJsonObj.ToString());
            PK_NetMgr.Ins.Send(reqMsg);


            CPacket.destroy(reqMsg);
        }

        this.sideBettingFlag = false;
        this.checkBetFlag = false;
        this.dieEventFlag = false;
        totalMoneyLable.text = " ";

        InitCardPos();
        InitDieBox();
        DisplayCurTurnPlayer(false);
        ResetListArray();
    }

    private void RecieveRoomInfoFromServer(JSONNode msg)
    {
        this.curGameRoom.no = int.Parse(msg["ROOM_NUMBER"].Value);
        this.curGameRoom.titleStr = msg["ROOM_TITLE"].Value;
        this.curGameRoom.betMoneyStr = msg["ROOM_BETMONEY"].Value;
        this.curGameRoom.curManCount = int.Parse(msg["ROOM_CUR_MAN_COUNT"]);
        this.curGameRoom.roomId = msg["ROOM_ID"].Value;
        this.curGameRoom.curPlayingManCount = int.Parse(msg["ROOM_CUR_PLAYING_MAN_COUNT"]);
        this.totalMoney = msg["TOTAL_MONEY"];
        this.curGameRoom.roomMaster = int.Parse(msg["ROOM_MASTER"]);

        DisplayUpdateTotalMoney(this.totalMoney);

        if (!imPlaying) UpdateTotalChip("0");
        else UpdateTotalChip(this.totalMoney);

        for (int i = 0; i < this.curGameRoom.curManCount; i++)
        {
            string key = msg["USER_ID" + i.ToString()].Value;
            int value = int.Parse(msg["USER_INDEX" + i.ToString()].Value);
            string eachUserMoney = msg["USER_MONEY" + i.ToString()].Value;

            InitListArray(value);

            if (key == myId)
            {
                this.myPlayIndex = value;
                userPos[0] = myPlayIndex;

                DisplayUserName(0, key);
                DisplayUpdateEachUserMoney(0, PK_NetMgr.Ins.userHasMoney);
                this.avataList[0].SetActive(true);
            }
            else
            {
                if (!IsClientExist(value))
                {
                    for (int j = 1; j < 5; j++)
                    {
                        if (userPos[j] == -1 && userPos[j] != value)
                        {
                            userPos[j] = value;

                            DisplayUserName(j, key);
                            DisplayUpdateEachUserMoney(j, eachUserMoney);
                            this.avataList[j].SetActive(true);
                            break;
                        }

                    }
                }
            }
        }
        if (this.imPlaying == false && this.curGameRoom.roomMaster == this.myPlayIndex)
        {
            if (this.curGameRoom.curManCount > 1) this.StartBtn.SetActive(true);
            else this.StartBtn.SetActive(false);
        }
        else this.StartBtn.SetActive(false);
    }

    private int FindUserDeviceRealPos(int playerIndex)
    {
        for (int i = 0; i < NUM_OF_TOTAL_USER; i++)
        {
            if (userPos[i] == playerIndex)
            {
                return i;
            }
        }
        return 0;
    }

    private void RemoveUserPos(int playerIndex)
    {
        for (int i = 0; i < NUM_OF_TOTAL_USER; i++)
        {

            if (userPos[i] == playerIndex)
            {
                userPos[i] = -1;
            }
        }

    }
    private void ReadyOrWatchingMsgToServer(JSONNode msg)
    {
        string possibleFlag = msg["POSSIBLE_FLAG"].Value;

        if (possibleFlag.Equals("1") && this.imPlaying == false)
        {
            ImReadyMsgSendToServer();
        }
        else if (possibleFlag.Equals("-1") && this.imPlaying == false)
        {
            ImWatchingMsgSendToServer();
        }
        else {
            Debug.Log("혼자 있는 상태");
        }

    }

    private void DieUserProcess(JSONNode onMsg)
    {
        int dieUser = int.Parse(onMsg["DIE_USER_INDEX"].Value);
        int nextUser = int.Parse(onMsg["NEXT_USER_INDEX"].Value);
        int userPos = FindUserDeviceRealPos(dieUser);
        this.curTurnPlayer = nextUser;

        int sortingLayer = 1;

        for (int i = 0; i < this.playerCardTable[dieUser.ToString()].Count; i++)
        {
            if (playerCardTable[dieUser.ToString()][i] != -1)
            {
                backCard[this.playerCardTable[dieUser.ToString()][i]].GetComponent<SpriteRenderer>().sortingOrder = sortingLayer;
                backCard[this.playerCardTable[dieUser.ToString()][i]].transform.DOMove(cardSlotPosInfoList[userPos].posList[i], 0.0f);
                sortingLayer++;
            }
            else
            {
                break;
            }
        }

        DisplayCurTurnPlayer(false);
        this.firstPlayerIndex = FindUserDeviceRealPos(this.curTurnPlayer);
        DisplayCurTurnPlayer(true);

        this.dieBoxList[userPos].GetComponent<SpriteRenderer>().sortingOrder = 8;
        this.dieBoxList[userPos].SetActive(true);

        if (dieUser == this.myPlayIndex)
        {
            this.DDangBtn.GetComponent<UIButton>().isEnabled = false;
            this.CallBtn.GetComponent<UIButton>().isEnabled = false;
            this.CheckBtn.GetComponent<UIButton>().isEnabled = false;
            this.DieBtn.GetComponent<UIButton>().isEnabled = false;
            this.HalfBtn.GetComponent<UIButton>().isEnabled = false;
            this.PPingBtn.GetComponent<UIButton>().isEnabled = false;

        }
        else
        {
            if (lastBet) LastButtonActive(this.pandonMoney, this.beforeMoney, this.totalMoney);
            else ButtonActive(this.pandonMoney, this.beforeMoney, this.totalMoney);
        }
    }

    private void RoomOutUserProcess(JSONNode onMsg)
    {
        string possibleFlag = onMsg["ROOM_OUT_MSG"].Value;
        int userIndex = int.Parse(onMsg["USER_INDEX"].Value);
        this.curGameRoom.roomMaster = int.Parse(onMsg["ROOM_MASTER"].Value);
        int nextTurnUser = int.Parse(onMsg["CUR_TURN_PLAYER"].Value);
        this.curTurnPlayer = nextTurnUser;

        if (this.imPlaying)
        {
            DisplayCurTurnPlayer(false);
            this.firstPlayerIndex = FindUserDeviceRealPos(this.curTurnPlayer);
            DisplayCurTurnPlayer(true);
        }

        if (possibleFlag.Equals("1"))
        {
            this.curGameRoom.curManCount--;
            if (userIndex == this.myPlayIndex)
            {
                changeScene();
            }
            else
            {
                this.avataList[FindUserDeviceRealPos(userIndex)].SetActive(false);
                this.dieBoxList[FindUserDeviceRealPos(userIndex)].SetActive(false);
                this.bossBoxList[FindUserDeviceRealPos(userIndex)].SetActive(false);
                this.turnBoxList[FindUserDeviceRealPos(userIndex)].SetActive(false);
                InitDisplayUserMoney(FindUserDeviceRealPos(userIndex));
                InitDisplayUserName(FindUserDeviceRealPos(userIndex));

                for (int i = 0; i < this.playerCardTable[userIndex.ToString()].Count; i++)
                {
                    int cardIndex = this.playerCardTable[userIndex.ToString()][i];

                    backCard[cardIndex].transform.DOMove(this.backCardPos, 0.0f);
                    cards[cardIndex].transform.DOMove(this.cardPos, 0.0f);
                }

                RemoveUserPos(userIndex);

                if (this.curGameRoom.roomMaster == this.myPlayIndex && this.curGameRoom.curManCount > 1 && this.imPlaying == false) this.StartBtn.SetActive(true);
                else this.StartBtn.SetActive(false);

            }

            this.playerCardTable.Remove(userIndex.ToString());
        }
    }
    private void ReturnChipPos()
    {
        for (int i = 0; i < this.drawMoney.Length; ++i)
        {
            for (int j = 0; j < this.chipGroupList[i].chipList.Count; ++j)
            {
                if (this.chipGroupList[i].chipList[j].activeSelf == true)
                {
                    this.chipGroupList[i].chipList[j].SetActive(false);
                    this.chipGroupList[i].chipList[j].transform.DOMove(this.chipPosInfoList[i].posList[j], 0.0f);
                }
            }
        }
    }

    private int[] MoneyStringToIntArray(string inputMoney)
    {
        Int64 totalMoney = Int64.Parse(inputMoney);
        totalMoney /= 100000;
        int[] moneyArray = new int[11];
        SetAryZero(moneyArray);
        long tmpDiv = 1;
        for (int i = 0; i < moneyArray.Length; i++)
        {
            moneyArray[i] = (int)((totalMoney / tmpDiv) % 10L);
            tmpDiv *= 10L;
        }

        return moneyArray;

    }


    private void DistributeMoveChip(int winnerIndex, int secondWinnerIndex, string winnerMoney, string sideMoney)
    {
        int[] winnerMoneyArray = new int[11];
        int[] secondWinerMoneyArray = new int[11];

        SetAryZero(winnerMoneyArray);
        SetAryZero(secondWinerMoneyArray);

        winnerMoneyArray = MoneyStringToIntArray(winnerMoney);
        secondWinerMoneyArray = MoneyStringToIntArray(sideMoney);

        ReturnChipPos();

        for (int i = 0; i < this.drawMoney.Length; ++i)
        {
            for (int j = 0; j < this.chipGroupList[i].chipList.Count; ++j)
            {
                if (winnerMoneyArray[i] != 0)
                {
                    if (j < winnerMoneyArray[i])
                    {
                        this.chipGroupList[i].chipList[j].SetActive(true);
                        Vector3 curPos = this.chipGroupList[i].chipList[j].transform.position;
                        this.chipGroupList[i].chipList[j].transform.DOMove(curPos + this.winnerChipMovePos[winnerIndex], 2.0f).OnComplete(ReturnChipPos);
                    }
                }
            }
        }

        for (int i = 0; i < this.drawMoney.Length; ++i)
        {
            for (int j = 0; j < this.chipGroupList[i].chipList.Count; ++j)
            {
                if (secondWinerMoneyArray[i] != 0)
                {
                    if (j < secondWinerMoneyArray[i])
                    {
                        j = j + winnerMoneyArray[i];
                        this.chipGroupList[i].chipList[j].SetActive(true);
                        Vector3 curPos = this.chipGroupList[i].chipList[j].transform.position;
                        this.chipGroupList[i].chipList[j].transform.DOMove(curPos + this.winnerChipMovePos[secondWinnerIndex], 2.0f).OnComplete(ReturnChipPos);
                    }
                }
            }
        }
    }

    private void moveChip(int winnerIndex)
    {
        GameObject group = new GameObject();
        for (int i = 0; i < this.drawMoney.Length; ++i)
        {
            for (int j = 0; j < this.chipGroupList[i].chipList.Count; ++j)
            {
                if (this.chipGroupList[i].chipList[j].activeSelf == true)
                {
                    Vector3 curPos = this.chipGroupList[i].chipList[j].transform.position;

                    this.chipGroupList[i].chipList[j].transform.DOMove(curPos + this.winnerChipMovePos[winnerIndex], 2.0f).OnComplete(ReturnChipPos);
                }
            }
        }

    }

    IEnumerator MyMethod(int winnerIndex)
    {

        yield return new WaitForSeconds(10);
        ResetGameData();
        this.winnerBoxList[FindUserDeviceRealPos(winnerIndex)].SetActive(false);
        this.handValue.text = " ";
        InitDisplayWinnerinfo(FindUserDeviceRealPos(winnerIndex));
        this.handList[preHand].SetActive(false);
        this.totalMoney = null;
    }

    private void AllCardOpen()
    {
        Vector3 backCardPos = new Vector3(0.0f, 5.5f, 0);
        try
        {
            foreach (KeyValuePair<string, List<int>> kvp in this.playerCardTable)
            {
                int playerPos = FindUserDeviceRealPos(int.Parse(kvp.Key));
                int order = 0;
                for (int i = 0; i < kvp.Value.Count; i++)
                {
                    if (kvp.Value[i] != -1)
                    {
                        cards[kvp.Value[i]].GetComponent<SpriteRenderer>().sortingOrder = ++order;
                        backCard[kvp.Value[i]].transform.DOMove(backCardPos, 0.0f);
                        cards[kvp.Value[i]].transform.DOMove(cardSlotPosInfoList[playerPos].posList[i], 0.0f);
                    }
                }
            } // 남은 카드 오픈

        }
        catch (SystemException e)
        {
            Debug.Log(e.Message);
            Debug.Log(e.StackTrace);
        }
    }

    private void GameOver(JSONNode onMsg)
    {
        this.imPlaying = false;
        this.timerStart = false;

        int playingUserCount = int.Parse(onMsg["PLAYING_USER_COUNT"].Value);
        this.totalMoney = onMsg["TOTAL_MONEY"].Value;
        int winnerIndex = int.Parse(onMsg["WINNER_INDEX"].Value);


        PK_Card.HAND winnerHandValue = (PK_Card.HAND)Enum.Parse(typeof(PK_Card.HAND), onMsg["WINNER_HAND"].Value);
        int winnerMaxCard = int.Parse(onMsg["WINNER_MAXCARD"].Value);
        int winnerMaxShape = int.Parse(onMsg["WINNER_MAXSHAPE"].Value);

        this.curGameRoom.roomMaster = winnerIndex;
        for (int i = 0; i < playingUserCount; i++)
        {

            int playerIndex = int.Parse(onMsg["USER_INDEX" + i.ToString()].Value);
            string userMoney = onMsg["USER_MONEY" + i.ToString()].Value;
            int userWin = int.Parse(onMsg["USER_WIN" + i.ToString()].Value);
            int userLose = int.Parse(onMsg["USER_LOSE" + i.ToString()].Value);

            if (winnerIndex == this.myPlayIndex &&
                playerIndex == this.myPlayIndex)
            {
                PK_NetMgr.Ins.userHasMoney = userMoney;
                PK_NetMgr.Ins.userWin = userWin;
                PK_NetMgr.Ins.userLose = userLose;
            }

            int playerPos = FindUserDeviceRealPos(playerIndex);
            DisplayUpdateEachUserMoney(playerPos, userMoney);
        }

        DisplayWinner(FindUserDeviceRealPos(winnerIndex), winnerHandValue, winnerMaxCard, winnerMaxShape);

        for (int i = 0; i < 5; i++) { this.countDownList[i].SetActive(false); }

        if (bool.Parse(onMsg["SIDE_BETTING"].Value))
        {
            int secondWinner = int.Parse(onMsg["SECOND_WINNER"].Value);
            string sideMoney = onMsg["SIDE_MONEY"].Value;
            Int64 tempTotalMoney = Int64.Parse(this.totalMoney);
            tempTotalMoney -= Int64.Parse(sideMoney);

            DistributeMoveChip(FindUserDeviceRealPos(winnerIndex), FindUserDeviceRealPos(secondWinner), tempTotalMoney.ToString(), sideMoney);
        }
        else moveChip(FindUserDeviceRealPos(winnerIndex));

        this.bossBoxList[FindUserDeviceRealPos(this.bossUserIndex)].SetActive(false);
        this.turnBoxList[this.firstPlayerIndex].SetActive(false);

        this.winnerBoxList[FindUserDeviceRealPos(winnerIndex)].GetComponent<SpriteRenderer>().sortingOrder = 9;
        this.winnerBoxList[FindUserDeviceRealPos(winnerIndex)].SetActive(true);
        this.handList[preHand].GetComponent<SpriteRenderer>().sortingOrder = 9;
        this.handList[preHand].SetActive(true);
        AllCardOpen();

        //게임 오버 했으니 
        InitButton();
        InitDisplayBettingName();
        StartCoroutine(MyMethod(winnerIndex));
    }

    private void changeScene()
    {

        Application.LoadLevel("Lobby");

    }

    IEnumerator BattleTimer()
    {
        for (; BattleTime > 0; BattleTime--) // Count down;
        {
            Debug.Log(BattleTime);
            yield return new WaitForSeconds(1.0f);
        }
    }

    public void OnGameMessage(CPacket msg)
    {
        var onMsg = JSON.Parse(msg.pop_string());

        PROTOCOL PROTOCOL_ID = (PROTOCOL)Enum.Parse(typeof(PROTOCOL), onMsg["PROTOCOL_ID"].Value);

        switch (PROTOCOL_ID)
        {
            case PROTOCOL.LOAD_GAME_SCENE_ACK:
                RecieveRoomInfoFromServer(onMsg);
                isGamePossibleMsgSendToServer();
                break;

            case PROTOCOL.GAME_POSSIBLE_ACK:
                ReadyOrWatchingMsgToServer(onMsg);
                break;

            case PROTOCOL.GAME_START:
                this.imPlaying = true;
                this.cardOrder = 1;
                this.StartBtn.SetActive(false);
                StartCoroutine(FirstDistribute(onMsg));
                break;

            case PROTOCOL.GAME_CARD_SELECT_REQ:
                tempCount = 0;
                break;

            case PROTOCOL.GAME_CARD_OPEN_REQ:
                CardOpenEachUser(onMsg);
                break;

            case PROTOCOL.GAME_NEXT_CARD_DISTRIUBET_REQ:
                this.checkBetFlag = false;
                this.firstBetting = false;
                this.lastBet = false;
                this.countDownList[preCountDown - 1].SetActive(false);
                StartCoroutine(NormalDistribute(onMsg));
                lastTime = DateTime.Now;
                break;

            case PROTOCOL.GAME_WATCHING_DISTRIBUTE:
                WatchingGame(onMsg);
                break;

            case PROTOCOL.OUT_GAME_ROOM_ACK:
                RoomOutUserProcess(onMsg);
                break;

            case PROTOCOL.GAME_USER_EACH_TURN_END:
                this.countDownList[preCountDown - 1].SetActive(false);
                DisplayUpdateEachUserMoneyAndNextUser(onMsg);
                lastTime = DateTime.Now;
                break;

            case PROTOCOL.GAME_DIE_REQ:
                this.countDownList[preCountDown - 1].SetActive(false);
                lastTime = DateTime.Now;
                DieUserProcess(onMsg);
                break;

            case PROTOCOL.GAME_OVER:
                GameOver(onMsg);
                break;

            case PROTOCOL.PONG:
                Debug.Log("pong");
                PK_NetMgr.lastPingTime = DateTime.Now;
                break;
        }
    }

}

using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using SimpleJSON;
using FreeNet;
using MySql.Data.MySqlClient;
using System.Data;

namespace PokerGameServer
{

    public class Room
    {
        private const int NUM_OF_FIRST_CARDS = 4;
        private const int NUM_OF_CARDS = 7;
        private const int NUM_OF_MAXIMUM_USER_COUNT = 5;

        public int no;
        public int curManCount;
        public int currentGu;
        public String titleStr;
        public String betMoneyStr;
        public String roomId;

        public List<Gamer> players;

        private object playerLock;

        private int currentTurnPlayer;
        private int bossIndex;
        private int winnerIndex;
        public int roomMaster;

        private Dictionary<int, PLAYER_STATE> playerStateList;
        public Dictionary<string, GAME_STATE> gameStateList;

        private Dictionary<int, string> openCardList;
        private Dictionary<int, string> removeCardList;
        private Dictionary<int, string> removeGuList;
        private Dictionary<int, bool> reservedUserOutList;
        private Dictionary<int, string> reconnectGamer;
        private Dictionary<int, string> unNormalDieUser;
        private Dictionary<int, UserInfo> userHandInfo;
        private CircularLinkedList<int> userTurnOrder = new CircularLinkedList<int>();

        private PK_Dealer pokerGameObject;
        public PK_Bet pokerBetObject;

        private HAND maxBossHand;
        private int maxBossHighCard;
        private int maxBossHighShape;

        private HAND maxWinnerHand;
        private int maxWinnerHighCard;
        private int maxWinnerHighShape;

        private int[] userIndex;
        private int[] bettingRule;
        private int bettingCount;
        private int userTotalIndex;
        private string bettingName = null;

        private int secondWinnerIndex = 0;
        private bool sideBetterWin = false;
        private bool wifiConValue = false;
        private Int64 sideMoney = 0;

        private delegate void DataBaseUpdateComplete(EventArgs e);
        private event DataBaseUpdateComplete dbUpdateComplete;

        private delegate object[] AasyncDelegateCaller(Gamer player, bool protocol, out int threadId); //async Select Callback

        MySqlConnection dbUpdateConnectionForcedRemoveUser = null;
        MySqlConnection dbUpdateConnection = null;
        public Room()
        {
            this.players = new List<Gamer>();
            this.currentTurnPlayer = 0;
            this.playerStateList = new Dictionary<int, PLAYER_STATE>();
            this.gameStateList = new Dictionary<string, GAME_STATE>();
            this.userHandInfo = new Dictionary<int, UserInfo>();
            this.reservedUserOutList = new Dictionary<int, bool>();
            this.reconnectGamer = new Dictionary<int, string>();
            this.unNormalDieUser = new Dictionary<int, string>();

            this.playerLock = new object();

            this.no = 0;
            this.titleStr = null;
            this.betMoneyStr = null;
            this.curManCount = 0;
            this.roomId = null;
            this.currentGu = 0;

            this.currentTurnPlayer = 0;
            this.pokerGameObject = null;
            this.pokerBetObject = null;

            this.maxBossHand = HAND.Nothig;
            this.maxBossHighCard = -1;
            this.maxBossHighShape = -1;

            this.maxWinnerHand = HAND.Nothig;
            this.maxWinnerHighCard = -1;
            this.maxWinnerHighShape = -1;

            this.roomMaster = -1;
            this.bettingName = "NO";
            this.userIndex = new int[NUM_OF_MAXIMUM_USER_COUNT];

            for (int i = 0; i < NUM_OF_MAXIMUM_USER_COUNT; i++) this.userIndex[i] = -1;

            this.userTotalIndex = 0;
            this.bettingRule = new int[NUM_OF_CARDS] { 0, 0, 0, 1, 1, 2, 3 };
            this.bettingCount = 0;

            this.dbUpdateComplete += new DataBaseUpdateComplete(DataBaseUpdateProcessCompleteCallback);

            String strConn = "Server=localhost;Database=poker;Uid=root;";
            dbUpdateConnection = new MySqlConnection(strConn);
            dbUpdateConnectionForcedRemoveUser = new MySqlConnection(strConn);
        }

        public int GetPlayingUserCount()
        {
            int count = 0;
            for (int i = 0; i < players.Count; i++)
            {
                if (players[i].playingFlag == true) count++;
            }
            return count;
        }

        public int GetUserIndex(int index)
        {
            for (int i = 0; i < NUM_OF_MAXIMUM_USER_COUNT; i++)
            {
                if (userIndex[i] == -1) //빈자리
                {
                    userIndex[i] = index;
                    return userIndex[i];
                }
            }
            return -1;
        }

        private int FindUserIndex(int key)
        {
            for (int i = 0; i < this.players.Count; i++)
            {
                if (this.players[i].player_index == key) return i;
            }
            return -1;
        }

        public void RemovePlayerIndex(int playerIndex)
        {
            for (int i = 0; i < NUM_OF_MAXIMUM_USER_COUNT; i++)
            {
                if (this.userIndex[i] == playerIndex)
                {
                    for (int j = i; j < NUM_OF_MAXIMUM_USER_COUNT - 1; j++)
                        this.userIndex[j] = this.userIndex[j + 1];
                }
            }
        }

        public int ChangeRoomMaster(int playerIndex)
        {
            if (this.userTurnOrder.Count != 0)
                return this.userTurnOrder.Find(playerIndex).Next.Value;
            else
                return -1;
        }

        private Gamer GetReconnectGamer(User user)
        {
            for (int i = 0; i < players.Count; i++)
            {
                if (this.players[i].userId == user.GetUserId)
                {
                    Gamer temp = players[i];
                    this.players.Remove(players[i]);
                    //temp.owner = user;
                    return temp;
                }
            }
            return null;
        }


        public JObject SetProtocol(PROTOCOL protocl)
        {
            JObject jsonObj = new JObject();
            jsonObj.Add("PROTOCOL_ID", protocl.ToString());
            return jsonObj;
        }

        public void CreateGameroom(User user)
        {
            Gamer masterGamer = new Gamer(user, GetUserIndex(++this.userTotalIndex));
            masterGamer.SetUserHasMoney(user.GetUserMoney);

            this.pokerBetObject = new PK_Bet(Int64.Parse(this.betMoneyStr));

            DataBaseThreadStart(masterGamer, true);

            this.players.Clear();
            this.players.Add(masterGamer);

            this.userTurnOrder.AddLast(masterGamer.player_index);
            this.playerStateList.Clear();
            this.gameStateList.Clear();
            this.roomMaster = masterGamer.player_index;

            ChangeRoomCurrentState(this.roomId, GAME_STATE.NO_PALYING);
            ChangeUserCurrentState(masterGamer, PLAYER_STATE.USER_ALONE);

            masterGamer.playingFlag = true;
            user.EnterRoom(masterGamer, this);

        }


        public void EnterGameRoom(User user)
        {
            for (int i = 0; i < this.players.Count; i++)  //와이파이 끊겼을 경우 재 접속자 처리.
            {
                Gamer reConnectGamer = GetReconnectGamer(user);
                if (reConnectGamer != null)
                {
                    this.reconnectGamer.Add(reConnectGamer.player_index, reConnectGamer.userId);
                    this.wifiConValue = false;
                    this.players.Add(reConnectGamer);
                    user.EnterRoom(reConnectGamer, this);
                    reConnectGamer.playingFlag = false;
                    reConnectGamer.owner.reConnectionFlag = false;
                    ChangeUserCurrentState(reConnectGamer, PLAYER_STATE.USER_WAIT);

                    CPacket enterRoomMsg = CPacket.create();
                    JObject jsonObj2 = SetProtocol(PROTOCOL.ENTER_GAME_ROOM_ACK);
                    jsonObj2.Add("ROOM_ID", this.roomId);
                    enterRoomMsg.push(jsonObj2.ToString());
                    user.send(enterRoomMsg);

                    CPacket.destroy(enterRoomMsg);

                    return;
                }
            }

            //일반적인 접속자
            Gamer enteredGamer = new Gamer(user, GetUserIndex(++this.userTotalIndex));

            DataBaseThreadStart(enteredGamer, false);

            string roomId = this.roomId;
            this.players.Add(enteredGamer);
            this.userTurnOrder.AddLast(enteredGamer.player_index);

            enteredGamer.playingFlag = false;
            user.EnterRoom(enteredGamer, this);
            ChangeUserCurrentState(enteredGamer, PLAYER_STATE.USER_WAIT);

            CPacket msg = CPacket.create();
            JObject jsonObj = SetProtocol(PROTOCOL.ENTER_GAME_ROOM_ACK);
            jsonObj.Add("ROOM_ID", this.roomId);
            msg.push(jsonObj.ToString());
            user.send(msg);

            CPacket.destroy(msg);

        }

        public void IsPossibleGamePlay(User user)
        {
            if (this.curManCount == 1) //홀로 있는경우
            {
                ChangeUserCurrentState(user.player, PLAYER_STATE.USER_WAIT);
                user.player.playingFlag = true;

                CPacket msg = CPacket.create();
                JObject jsonObj = SetProtocol(PROTOCOL.GAME_POSSIBLE_ACK);
                jsonObj.Add("POSSIBLE_FLAG", "-2");
                msg.push(jsonObj.ToString());
                user.player.send(msg);
                CPacket.destroy(msg);
                return;

            }

            if (IsGamePlaying(roomId)) //게임 중인 경우
            {
                if (user.player.playingFlag == false)
                {
                    this.userTurnOrder.Remove(user.player.player_index);
                    ChangeUserCurrentState(user.player, PLAYER_STATE.USER_WAIT);
                    user.player.playingFlag = false;

                    CPacket msg = CPacket.create();
                    JObject jsonObj = SetProtocol(PROTOCOL.GAME_POSSIBLE_ACK);
                    jsonObj.Add("POSSIBLE_FLAG", "-1"); //관전자
                    msg.push(jsonObj.ToString());
                    user.player.send(msg);
                    CPacket.destroy(msg);
                }

            }
            else  //게임 대기 중
            {
                ChangeUserCurrentState(user.player, PLAYER_STATE.READY);
                user.player.playingFlag = true;

                CPacket msg = CPacket.create();
                JObject jsonObj = SetProtocol(PROTOCOL.GAME_POSSIBLE_ACK);
                jsonObj.Add("POSSIBLE_FLAG", "1");
                msg.push(jsonObj.ToString());
                user.player.send(msg);
                CPacket.destroy(msg);

            }

        }

        public void SendRemoveCardSelectMsg()
        {
            CPacket msg = CPacket.create();
            JObject jsonObj = SetProtocol(PROTOCOL.GAME_CARD_SELECT_REQ);
            msg.push(jsonObj.ToString());
            this.players.ForEach(player => player.send_for_broadcast(msg));
            CPacket.destroy(msg);
        }

        public void CardSelectedComplete(Gamer player, string removeCardIndex, string openCardIndex)
        {
            this.removeCardList.Add(player.player_index, removeCardIndex);
            PK_Card removeCard = this.pokerGameObject.getCard(int.Parse(removeCardIndex));
            PK_Card openCard = this.pokerGameObject.getCard(int.Parse(openCardIndex));
            int removeGuIndex = this.pokerGameObject.RemoveTempHand(player.player_index, removeCard);

            if (removeGuIndex == -1) Console.WriteLine("wrong value");
            else this.removeGuList.Add(player.player_index, removeGuIndex.ToString());

            this.currentGu = 3;

            //카드 복사 실제 사용자의 손에 주워짐.(실제로 연산할 카드 복사)
            this.pokerGameObject.CopyTempHandToPlyerTotalHand(player.player_index);
            this.pokerGameObject.CopyTempHandToPlyerOpenHand(player.player_index, openCard);
            this.openCardList.Add(player.player_index, openCardIndex);

            //현재까지의 보스가 누구고, 현재 까지 위너가 누구인지.
            UserInfo temp = new UserInfo();
            this.userHandInfo.Add(player.player_index, temp);

            CurWhoIsWinner(player);
            CurWhoIsBoss(player);

            ChangeUserCurrentState(player, PLAYER_STATE.USER_SLEECT_CARD_COMPLETE);
            if (!AllplayerSameState(PLAYER_STATE.USER_SLEECT_CARD_COMPLETE)) return;

            ChangeUserCurrentState(player, PLAYER_STATE.USER_OPEN_CARD_READY);
            OpenCardAndRemoveCardSendToClient();

        }

        public void CardOpenComplete(Gamer player)
        {
            ChangeUserCurrentState(player, PLAYER_STATE.USER_OPEN_CARD_COMPLETE);

            if (!AllplayerSameState(PLAYER_STATE.USER_OPEN_CARD_COMPLETE))
            {
                return;
            }
            NextDistriubute();
        }

        public void FirstDistributeComplete(Gamer player)
        {
            ChangeUserCurrentState(player, PLAYER_STATE.FIRST_DISTRIBUTE_COMPLETE);

            if (!AllplayerSameState(PLAYER_STATE.FIRST_DISTRIBUTE_COMPLETE)) return;

            ChangeAllUserCurrentState(PLAYER_STATE.USER_SELECT_CARD_READY);

            SendRemoveCardSelectMsg();

        }

        public void WatchingGame(Gamer player)
        {
            player.playingFlag = false;

            if (PlayingUserCheckState(PLAYER_STATE.FIRST_DISTRIBUTE_COMPLETE) ||
                PlayingUserCheckState(PLAYER_STATE.USER_SELECT_CARD_READY) ||
                PlayingUserCheckState(PLAYER_STATE.USER_OPEN_CARD_READY))
            {
                FirstDistributeForWatchingUser(player);
            }
            else
            {
                NextDistributeForWatchingUser(player);
            }

        }

        public void LoadingComplete(Gamer player)
        {
            player.playingFlag = true;

            ChangeUserCurrentState(player, PLAYER_STATE.LOADING_COMPLETE);

            if (!AllplayerSameState(PLAYER_STATE.LOADING_COMPLETE)) return;
        }

        public void GameStartMsgRecieveFromClient(Gamer player)
        {
            if (!AllplayerSameState(PLAYER_STATE.LOADING_COMPLETE)) return;
            if (player.player_index == this.roomMaster) GameStart();
        }

        private void WhoIsBoss(CardInfo userCardInfo, int playerIndex)
        {
            Console.Write("사용자" + playerIndex + "의 오픈카드 족보는 " + userCardInfo.maxHand +
               " 높은 숫자는 ");

            this.userHandInfo[playerIndex].maxOpenHand = userCardInfo.maxHand;
            this.userHandInfo[playerIndex].maxOpenHighCard = userCardInfo.maxHighCard;
            this.userHandInfo[playerIndex].maxOpenHighShape = userCardInfo.maxHighShape;

            this.pokerGameObject.DebugDisplayCard((short)userCardInfo.maxHighShape, (short)userCardInfo.maxHighCard);

            if (userCardInfo.maxHand > this.maxBossHand) //족보가 다른 경우
            {
                this.maxBossHand = userCardInfo.maxHand;
                this.maxBossHighCard = userCardInfo.maxHighCard;
                this.maxBossHighShape = userCardInfo.maxHighShape;
                this.bossIndex = playerIndex;
            }
            else if (userCardInfo.maxHand == this.maxBossHand)//족보가 같은 경우
            {
                if (userCardInfo.maxHighCard > this.maxBossHighCard) //높은 카드
                {
                    this.maxBossHighCard = userCardInfo.maxHighCard;
                    this.maxBossHighShape = userCardInfo.maxHighShape;
                    this.bossIndex = playerIndex;
                }
                else if (userCardInfo.maxHighCard == this.maxBossHighCard)
                {
                    if (userCardInfo.maxHighShape > this.maxBossHighShape) //높은 모양
                    {
                        this.maxBossHighShape = userCardInfo.maxHighShape;
                        this.bossIndex = playerIndex;
                    }
                }
            }
            Console.WriteLine("--------------------------------------");

        }

        private void WhoIsWinner(CardInfo userCardInfo, int playerIndex)
        {
            Console.Write("사용자" + playerIndex + "의 모든카드 족보는 " + userCardInfo.maxHand + " 높은 숫자는 ");

            this.userHandInfo[playerIndex].maxTotalHand = userCardInfo.maxHand;
            this.userHandInfo[playerIndex].maxTotalHighCard = userCardInfo.maxHighCard;
            this.userHandInfo[playerIndex].maxTotalHighShape = userCardInfo.maxHighShape;

            this.pokerGameObject.DebugDisplayCard((short)userCardInfo.maxHighShape, (short)userCardInfo.maxHighCard);

            if (userCardInfo.maxHand > this.maxWinnerHand) //족보가 다른 경우
            {
                this.maxWinnerHand = userCardInfo.maxHand;
                this.maxWinnerHighCard = userCardInfo.maxHighCard;
                this.maxWinnerHighShape = userCardInfo.maxHighShape;
                this.winnerIndex = playerIndex;
            }
            else if (userCardInfo.maxHand == this.maxWinnerHand)//족보가 같은 경우
            {
                if (userCardInfo.maxHighCard > this.maxWinnerHighCard) //높은 카드
                {
                    this.maxWinnerHighCard = userCardInfo.maxHighCard;
                    this.maxWinnerHighShape = userCardInfo.maxHighShape;
                    this.winnerIndex = playerIndex;
                }
                else if (userCardInfo.maxHighCard == this.maxWinnerHighCard)
                {
                    if (userCardInfo.maxHighShape > this.maxWinnerHighShape) //높은 모양
                    {
                        this.maxWinnerHighShape = userCardInfo.maxHighShape;
                        this.winnerIndex = playerIndex;
                    }
                }
            }
        }

        private int SecondWinnerHand(int winnerIndex)
        {
            UserInfo secondWinner = new UserInfo();
            secondWinner.maxTotalHand = HAND.Nothig;
            secondWinner.maxTotalHighCard = -1;
            secondWinner.maxTotalHighShape = -1;
            int secondWinnerIndex = 0;
            foreach (KeyValuePair<int, UserInfo> kvp in this.userHandInfo.ToList())
            {
                if (!(kvp.Key == winnerIndex))
                {
                    if (kvp.Value.maxTotalHand > secondWinner.maxTotalHand) //족보가 다른 경우
                    {
                        secondWinner.maxTotalHand = kvp.Value.maxTotalHand;
                        secondWinner.maxTotalHighCard = kvp.Value.maxTotalHighCard;
                        secondWinner.maxTotalHighShape = kvp.Value.maxTotalHighShape;
                        secondWinnerIndex = kvp.Key;
                    }
                    else if (kvp.Value.maxTotalHand == secondWinner.maxTotalHand)//족보가 같은 경우
                    {
                        if (kvp.Value.maxTotalHighCard > secondWinner.maxTotalHighCard) //높은 카드
                        {
                            secondWinner.maxTotalHighCard = kvp.Value.maxTotalHighCard;
                            secondWinner.maxTotalHighShape = kvp.Value.maxTotalHighShape;
                            secondWinnerIndex = kvp.Key;
                        }
                        else if (kvp.Value.maxTotalHighCard == secondWinner.maxTotalHighCard)
                        {
                            if (kvp.Value.maxTotalHighShape > secondWinner.maxTotalHighShape) //높은 모양
                            {
                                secondWinner.maxTotalHighShape = kvp.Value.maxTotalHighShape;
                                secondWinnerIndex = kvp.Key;
                            }
                        }
                    }
                }
            }
            return secondWinnerIndex;
        }

        private void UpdateOpenUserHand(UserInfo userCardInfo, int playerIndex)
        {
            if (userCardInfo.maxOpenHand > this.maxBossHand) //족보가 다른 경우
            {
                this.maxBossHand = userCardInfo.maxOpenHand;
                this.maxBossHighCard = userCardInfo.maxOpenHighCard;
                this.maxBossHighShape = userCardInfo.maxOpenHighShape;
                this.bossIndex = playerIndex;
            }
            else if (userCardInfo.maxOpenHand == this.maxBossHand)//족보가 같은 경우
            {
                if (userCardInfo.maxOpenHighCard > this.maxBossHighCard) //높은 카드
                {
                    this.maxBossHighCard = userCardInfo.maxOpenHighCard;
                    this.maxBossHighShape = userCardInfo.maxOpenHighShape;
                    this.bossIndex = playerIndex;
                }
                else if (userCardInfo.maxOpenHighCard == this.maxBossHighCard)
                {
                    if (userCardInfo.maxOpenHighShape > this.maxBossHighShape) //높은 모양
                    {
                        this.maxBossHighShape = userCardInfo.maxOpenHighShape;
                        this.bossIndex = playerIndex;
                    }
                }
            }
        }

        private void UpdateTotalUserHand(UserInfo userCardInfo, int playerIndex)
        {
            if (userCardInfo.maxTotalHand > this.maxWinnerHand) //족보가 다른 경우
            {
                this.maxWinnerHand = userCardInfo.maxTotalHand;
                this.maxWinnerHighCard = userCardInfo.maxTotalHighCard;
                this.maxWinnerHighShape = userCardInfo.maxTotalHighShape;
                this.winnerIndex = playerIndex;
            }
            else if (userCardInfo.maxTotalHand == this.maxWinnerHand)//족보가 같은 경우
            {
                if (userCardInfo.maxTotalHighCard > this.maxWinnerHighCard) //높은 카드
                {
                    this.maxWinnerHighCard = userCardInfo.maxOpenHighCard;
                    this.maxWinnerHighShape = userCardInfo.maxOpenHighShape;
                    this.winnerIndex = playerIndex;
                }
                else if (userCardInfo.maxTotalHighCard == this.maxWinnerHighCard)
                {
                    if (userCardInfo.maxTotalHighShape > this.maxWinnerHighShape) //높은 모양
                    {
                        this.maxWinnerHighShape = userCardInfo.maxTotalHighShape;
                        this.winnerIndex = playerIndex;
                    }
                }
            }
        }

        private void CurWhoIsBoss(Gamer player)
        {
            CardInfo userOpenCardInfo = new CardInfo();
            for (int j = 0; j < this.currentGu - 2; j++) //최초에는 1장 이지. // 근데 토탈은 세장이야.
            {
                userOpenCardInfo = this.pokerGameObject.GetEachUserOpenHandInfo(player.player_index, j);
                WhoIsBoss(userOpenCardInfo, player.player_index);
            }

        }

        private void CurWhoIsWinner(Gamer player)
        {
            CardInfo userTotalCardInfo = new CardInfo();
            for (int j = 0; j < this.currentGu; j++)
            {
                userTotalCardInfo = this.pokerGameObject.GetEachUserTotalHandInfo(player.player_index, j);
                WhoIsWinner(userTotalCardInfo, player.player_index);
            }
        }

        private void ClusteringMsg(CPacket msg)
        {
            for (int i = 0; i < this.userTurnOrder.Count; i++)
            {
                int playerIndex = this.userTurnOrder.ElementAt(i);
                for (int j = 0; j < this.players.Count; j++)
                {
                    if (this.players[j].player_index == playerIndex) this.players[j].send(msg);
                }
            }

        }
        private void NextCardEachUserDistribute()
        {
            CPacket newmsg = CPacket.create();
            JObject jsonObj = SetProtocol(PROTOCOL.GAME_NEXT_CARD_DISTRIUBET_REQ);

            jsonObj.Add("PLAYING_USER_COUNT", GetPlayingUserCount().ToString());

            int packetIndex = 0;
            for (int i = 0; i < players.Count(); i++)
            {
                if (this.players[i].playingFlag == true) //실제 게임에 참여하는 사람 것 만 패킷을 만듬.
                {
                    jsonObj.Add("USER_INDEX" + packetIndex.ToString(), players[i].player_index.ToString());

                    PK_Card popCard = this.pokerGameObject.GetHand();

                    this.pokerGameObject.SetTotalHand(this.players[i].player_index, this.currentGu);
                    this.pokerGameObject.SetOpenHand(this.players[i].player_index, this.currentGu - 2);

                    CardInfo userTotalCardInfo = this.pokerGameObject.GetEachUserTotalHandInfo(this.players[i].player_index, this.currentGu);
                    CardInfo userOpenCardInfo = this.pokerGameObject.GetEachUserOpenHandInfo(this.players[i].player_index, this.currentGu - 2);

                    WhoIsWinner(userTotalCardInfo, this.players[i].player_index);
                    WhoIsBoss(userOpenCardInfo, this.players[i].player_index);

                    jsonObj.Add("MY_HAND" + packetIndex.ToString(), userTotalCardInfo.maxHand.ToString());
                    jsonObj.Add("MY_HIGH_CARD" + packetIndex.ToString(), userTotalCardInfo.maxHighCard.ToString());
                    jsonObj.Add("MY_HIGH_SHAPE" + packetIndex.ToString(), userTotalCardInfo.maxHighShape.ToString());
                    jsonObj.Add("MY_SUIT" + packetIndex.ToString(), popCard.MyShape.ToString());
                    jsonObj.Add("MY_VALUE" + packetIndex.ToString(), popCard.MyValue.ToString());
                    jsonObj.Add("GU_COUNT" + packetIndex.ToString(), currentGu.ToString());

                    ChangeUserCurrentState(players[i], PLAYER_STATE.BETTING_READY);

                    packetIndex++;
                }
            }
            this.currentTurnPlayer = this.bossIndex;
            jsonObj.Add("FIRST_USER", this.bossIndex);

            newmsg.push(jsonObj.ToString());

            if (this.wifiConValue) ClusteringMsg(newmsg);
            else this.players.ForEach(player => player.send_for_broadcast(newmsg));
  
           
            CPacket.destroy(newmsg);
        }

        private void FirstDistribute()
        {
            CPacket newmsg = CPacket.create();
            JObject jsonObj = SetProtocol(PROTOCOL.GAME_START);
            jsonObj.Add("PAN_DON", this.pokerBetObject.getTotalMoney.ToString());

            for (int i = 0; i < NUM_OF_FIRST_CARDS; i++) //몇 구인지.
            {
                for (int j = 0; j < players.Count(); j++)
                {
                    if (this.players[j].playingFlag == true) //실제 게임에 참여하는 사람 것 만 패킷을 만듬.
                    {
                        PK_Card temp = this.pokerGameObject.GetHand();
                        this.pokerGameObject.GetTempEachUserHandList(this.players[j].player_index).Add(temp);

                        jsonObj.Add("USER_INDEX" + i.ToString() + j.ToString(), players[j].player_index.ToString());
                        jsonObj.Add("MY_SUIT" + i.ToString() + j.ToString(), temp.MyShape.ToString());
                        jsonObj.Add("MY_VALUE" + i.ToString() + j.ToString(), temp.MyValue.ToString());
                        jsonObj.Add("USER_MONEY" + i.ToString() + j.ToString(), this.pokerBetObject.eachUserMoney[players[j].player_index]);
                    }

                }
            }

            this.pokerGameObject.SetTempHand();

            newmsg.push(jsonObj.ToString());

            if (this.wifiConValue)
            {
                for (int i = 0; i < this.userTurnOrder.Count; i++)
                {
                    int playerIndex = this.userTurnOrder.ElementAt(i);
                    for (int j = 0; j < this.players.Count; j++)
                    {
                        if (this.players[j].player_index == playerIndex) this.players[j].send(newmsg);
                    }
                }
            }
            else
            {
                this.players.ForEach(player => player.send_for_broadcast(newmsg));
            }

            CPacket.destroy(newmsg);
        }

        private void FirstDistributeForWatchingUser(Gamer player)
        {
            CPacket newmsg = CPacket.create();
            JObject jsonObj = SetProtocol(PROTOCOL.GAME_WATCHING_DISTRIBUTE);

            jsonObj.Add("PLAYING_USER_COUNT", GetPlayingUserCount().ToString());

            int packetIndex = 0;
            for (int i = 0; i < players.Count(); i++)
            {
                if (this.players[i].playingFlag == true) //실제 게임에 참여하는 사람 것 만 패킷을 만듬.
                {
                    jsonObj.Add("USER_INDEX" + packetIndex.ToString(), players[i].player_index.ToString());
                    jsonObj.Add("CARD_COUNT" + packetIndex.ToString(), this.pokerGameObject.GetTempEachUserHandList(this.players[i].player_index).Count().ToString());

                    for (int j = 0; j < this.pokerGameObject.GetTempEachUserHandList(this.players[i].player_index).Count(); j++)
                    {
                        jsonObj.Add("MY_SUIT" + packetIndex.ToString() + j.ToString(), this.pokerGameObject.GetTempEachUserHandList(this.players[i].player_index)[j].MyShape.ToString());
                        jsonObj.Add("MY_VALUE" + packetIndex.ToString() + j.ToString(), this.pokerGameObject.GetTempEachUserHandList(this.players[i].player_index)[j].MyValue.ToString());
                        jsonObj.Add("GU_COUNT" + packetIndex.ToString() + j.ToString(), j.ToString());
                    }
                    packetIndex++;
                }
            }
            newmsg.push(jsonObj.ToString());

            player.send(newmsg);

            CPacket.destroy(newmsg);

        }

        private void NextDistributeForWatchingUser(Gamer player)
        {

            CPacket newmsg = CPacket.create();
            JObject jsonObj = SetProtocol(PROTOCOL.GAME_WATCHING_DISTRIBUTE);

            jsonObj.Add("PLAYING_USER_COUNT", GetPlayingUserCount().ToString());
            int packetIndex = 0;
            for (int i = 0; i < players.Count(); i++)
            {

                if (this.players[i].playingFlag == true)
                {
                    jsonObj.Add("USER_INDEX" + packetIndex.ToString(), players[i].player_index.ToString());
                    jsonObj.Add("CARD_COUNT" + packetIndex.ToString(), this.currentGu.ToString());
                    //만약 after 배열이 비어 있을경우는 temp를 보내장.(예외처리)
                    for (int j = 0; j < this.currentGu; j++)
                    {
                        jsonObj.Add("MY_SUIT" + packetIndex.ToString() + j.ToString(), this.pokerGameObject.GetAfterSelectedPlayerHandList(this.players[i].player_index)[j].MyShape.ToString());
                        jsonObj.Add("MY_VALUE" + packetIndex.ToString() + j.ToString(), this.pokerGameObject.GetAfterSelectedPlayerHandList(this.players[i].player_index)[j].MyValue.ToString());
                        jsonObj.Add("GU_COUNT" + packetIndex.ToString() + j.ToString(), j.ToString());
                    }
                    packetIndex++;
                }
            }
            newmsg.push(jsonObj.ToString());

            player.send(newmsg);

            CPacket.destroy(newmsg);

        }

        private void OpenCardAndRemoveCardSendToClient()
        {
            CPacket msg = CPacket.create();
            JObject jsonObj = SetProtocol(PROTOCOL.GAME_CARD_OPEN_REQ);

            jsonObj.Add("PLAYING_USER_COUNT", GetPlayingUserCount().ToString());
            int packetIndex = 0;

            for (int i = 0; i < openCardList.Count; i++)
            {
                string openCard;
                string removeCardNumber;
                string removeCardGuIndex;

                bool isExistOpenCard = this.openCardList.TryGetValue(this.players[i].player_index, out openCard);
                bool isExistRemoveCard = this.removeCardList.TryGetValue(this.players[i].player_index, out removeCardNumber);
                bool isExistRemoveGu = this.removeGuList.TryGetValue(this.players[i].player_index, out removeCardGuIndex);

                if (isExistOpenCard && isExistRemoveCard && isExistRemoveGu)
                {
                    if (this.players[i].playingFlag == true) //실제 게임에 참여하는 사람 것 만 패킷을 만듬.
                    {
                        jsonObj.Add("USER_INDEX" + packetIndex.ToString(), this.players[i].player_index.ToString());
                        jsonObj.Add("OPEN_CARD_INDEX" + packetIndex.ToString(), openCard);
                        jsonObj.Add("REMOVE_CARD_INDEX" + packetIndex.ToString(), removeCardNumber);
                        jsonObj.Add("REMOVE_GU_INDEX" + packetIndex.ToString(), removeCardGuIndex);
                        packetIndex++;
                    }
                }
                else return;
            }

            msg.push(jsonObj.ToString());
            this.players.ForEach(eachPlayer => eachPlayer.send_for_broadcast(msg));
            CPacket.destroy(msg);

        }

        private void ResetGamedata()
        {
            if (this.openCardList == null) this.openCardList = new Dictionary<int, string>();
            if (this.removeCardList == null) this.removeCardList = new Dictionary<int, string>();
            if (this.removeGuList == null) this.removeGuList = new Dictionary<int, string>();
            if (this.userHandInfo == null) this.userHandInfo = new Dictionary<int, UserInfo>();
            if (this.reservedUserOutList == null) this.reservedUserOutList = new Dictionary<int, bool>();
     
            this.maxBossHand = HAND.Nothig;
            this.maxBossHighCard = -1;
            this.maxBossHighShape = -1;

            this.maxWinnerHand = HAND.Nothig;
            this.maxWinnerHighCard = -1;
            this.maxWinnerHighShape = -1;

            this.secondWinnerIndex = 0;
            this.sideBetterWin = false;
            this.wifiConValue = false;

            this.pokerGameObject = new PK_Dealer();

            for (int i = 0; i < this.curManCount; i++)
                this.pokerGameObject.InitEstimateObj(this.players[i].player_index);

            this.pokerGameObject.setUpDeck();

            for (int i = 0; i < players.Count; i++)
            {
                if (!this.pokerBetObject.eachUserAccumulateMoney.ContainsKey(players[i].player_index))
                    this.pokerBetObject.eachUserAccumulateMoney.Add(players[i].player_index, 0);
                else
                    this.pokerBetObject.eachUserAccumulateMoney[players[i].player_index] = 0;
            }


        }

        private void InitGameData()
        {
            this.bossIndex = 1;
            this.winnerIndex = 1;

            if (this.pokerGameObject != null) this.pokerGameObject.Destroy();
            if (this.openCardList != null) this.openCardList.Clear();
            if (this.removeCardList != null) this.removeCardList.Clear();
            if (this.removeGuList != null) this.removeGuList.Clear();
            if (this.userHandInfo != null) this.userHandInfo.Clear();
            if (this.reservedUserOutList != null) this.reservedUserOutList.Clear();
            if (this.unNormalDieUser != null) this.unNormalDieUser.Clear();
            if (this.reconnectGamer != null) this.reconnectGamer.Clear();
        }

        bool UserIsAccessThisRoom()
        {
            if (this.reconnectGamer.Count > 0) return true;
            else return false;
          
        }

        void GameStart()
        {
            InitGameData();
            ResetGamedata();

            ChangeRoomCurrentState(this.roomId, GAME_STATE.PLAYING);

            for (int i = 0; i < players.Count; i++)
            {
                if (players[i].playingFlag == false)
                {
                    players[i].playingFlag = true;
                    this.userTurnOrder.AddLast(players[i].player_index);
                }
            }

            this.pokerBetObject.SetPandonMoney(GetPlayingUserCount());

            FirstDistribute();
        }

        void NextDistriubute()
        {
            this.pokerBetObject.InitAccumlateMoney();
            if (this.currentGu < 7)
            {
                NextCardEachUserDistribute();
                this.currentGu++;
                this.pokerBetObject.getBeforeMoney = 0;
                this.pokerBetObject.getCallMoney = 0;
            }
            else
            {
                Console.WriteLine(winnerIndex - 1 + " 가 승리했고 " + "족보는 " + maxWinnerHand);
                this.pokerGameObject.DebugDisplayCard((short)maxWinnerHighShape, (short)maxWinnerHighCard);

                GameOver();
            }

        }


        private void UnNormalDieUserProcess(Gamer user, bool wifiConnection)
        {
            if (!wifiConnection) //강제적으로 앱을 종료 한경우
            {
                this.players.Remove(user);
                this.playerStateList.Remove(user.player_index);
            }

            if (this.currentTurnPlayer == user.player_index) // 나간 사용자가 현재 턴이면
            {
                this.currentTurnPlayer = this.userTurnOrder.Find(this.currentTurnPlayer).Next.Value;
            }

            this.pokerBetObject.eachUserAccumulateMoney.Remove(user.player_index);
            this.playerStateList.Remove(user.player_index);
            int nextIndex = this.userTurnOrder.Find(user.player_index).Next.Value;
            this.userTurnOrder.Remove(user.player_index);
            user.playingFlag = false;
            this.bettingName = "다이";

            if (this.roomMaster == user.player_index) // 나간 사용자가 방장이면
            {
                this.roomMaster = nextIndex;
            }

            if (this.currentTurnPlayer == user.player_index) //나간 사용자의 턴이면.
            {
                this.currentTurnPlayer = nextIndex;
            }

            if (this.bossIndex == user.player_index) // 나간 사용자가 보스이면
            {
                this.bossIndex = -1;
                this.maxBossHand = HAND.no;
                this.maxBossHighCard = 0;
                this.maxBossHighShape = -1;


                for (int i = 0; i < this.userTurnOrder.Count; i++)
                {
                    UpdateOpenUserHand(this.userHandInfo[this.userTurnOrder[i].Value], this.userTurnOrder[i].Value);
                }

            }
            if (this.winnerIndex == user.player_index) // 나간 사용자가 지금까지 승자였으면
            {

                this.winnerIndex = -1;
                this.maxWinnerHand = HAND.no;
                this.maxWinnerHighCard = 0;
                this.maxWinnerHighShape = -1;

                for (int i = 0; i < this.userTurnOrder.Count; i++)
                {
                    UpdateTotalUserHand(this.userHandInfo[this.userTurnOrder[i].Value], this.userTurnOrder[i].Value);
                }
            }

            EachUserDieMsgToClient(user, wifiConnection);

            if (this.userTurnOrder.Count == 1) //한명만 남으면.
            {
                this.winnerIndex = nextIndex;

                GameOver();

                ChangeAllUserCurrentState(PLAYER_STATE.LOADING_COMPLETE);

            }
            if (!wifiConnection) // 강제 종료(순서가 중요)
            {

                ForcedRemoveUser(user.owner);

                //DB관련
                lock (PokerServer.threadCount)
                {
                    int theradCount = int.Parse(PokerServer.threadCount.ToString()) + 1;
                    theradCount %= 5;
                    PokerServer.threadCount = (object)theradCount;
                }

                ThreadPool.QueueUserWorkItem(new WaitCallback(DataBaseUpdateForForcedRemoveUser), (object)(new object[] { user.player_index, PokerServer.threadCount }));
            }

        }

        private void DieUserProcess(Gamer user) //일반적인 경우는 다이 프로세스
        {
            this.pokerBetObject.eachUserAccumulateMoney.Remove(user.player_index);
            this.playerStateList.Remove(user.player_index);
            int nextIndex = this.userTurnOrder.Find(user.player_index).Next.Value;
            user.playingFlag = false;
            this.bettingName = "다이";

            if (this.bossIndex == user.player_index) //다이 한 사람이 보스였으면 보스에 대한 정보를 갱신해야지.
            {
                this.bossIndex = -1;
                this.maxBossHand = HAND.no;
                this.maxBossHighCard = 0;
                this.maxBossHighShape = -1;

                for (int i = 0; i < this.userTurnOrder.Count; i++)
                {
                    if (user.player_index != this.userTurnOrder[i].Value)
                    {
                        UpdateOpenUserHand(this.userHandInfo[this.userTurnOrder[i].Value], this.userTurnOrder[i].Value);
                    }
                }

            }

            if (this.winnerIndex == user.player_index)
            {

                this.winnerIndex = -1;
                this.maxWinnerHand = HAND.no;
                this.maxWinnerHighCard = 0;
                this.maxWinnerHighShape = -1;

                for (int i = 0; i < this.userTurnOrder.Count; i++)
                {
                    if (user.player_index != this.userTurnOrder[i].Value)
                    {
                        UpdateTotalUserHand(this.userHandInfo[this.userTurnOrder[i].Value], this.userTurnOrder[i].Value);
                    }
                }

            }

            if (this.roomMaster == user.player_index)
            {
                this.roomMaster = nextIndex;
            }

            this.currentTurnPlayer = nextIndex;

            EachUserDieMsgToClient(user, false);

            if (this.userTurnOrder.Count == 2)
            {
                this.winnerIndex = nextIndex;

                GameOver();

                ChangeAllUserCurrentState(PLAYER_STATE.LOADING_COMPLETE);

            }
        }

        public void SetBettingName(int BettingEvent)
        {
            if (BettingEvent.Equals((int)BET.CALL)) this.bettingName = "콜";
            else if (BettingEvent.Equals((int)BET.LASTCALL)) this.bettingName = "콜";
            else if (BettingEvent.Equals((int)BET.CHECK)) this.bettingName = "체크";
            else if (BettingEvent.Equals((int)BET.DDADANG)) this.bettingName = "따당";
            else if (BettingEvent.Equals((int)BET.DIE)) this.bettingName = "다이";
            else if (BettingEvent.Equals((int)BET.HALF)) this.bettingName = "하프";
            else if (BettingEvent.Equals((int)BET.PPING)) this.bettingName = "삥";
            else if (BettingEvent.Equals((int)BET.ALLIN)) this.bettingName = "올인";
            else this.bettingName = "사이드배팅";
        }

        public void BettingProcess(Gamer user, int BettingEvent, bool unNormal, bool wifiConnection)
        {

            if (BettingEvent.Equals((int)BET.CALL))
                this.pokerBetObject.CallEvent(user.player_index);
            else if (BettingEvent.Equals((int)BET.LASTCALL))
                this.pokerBetObject.LastCallEvent(user.player_index);
            else if (BettingEvent.Equals((int)BET.CHECK))
                this.pokerBetObject.CheckEvent(user.player_index);
            else if (BettingEvent.Equals((int)BET.DDADANG))
                this.pokerBetObject.DDadangEvent(user.player_index);
            else if (BettingEvent.Equals((int)BET.DIE))
            {
                this.pokerBetObject.DieEvent(user.player_index);
                if (unNormal) { if (this.userTurnOrder.Contains(user.player_index)) UnNormalDieUserProcess(user, wifiConnection); } //현재 죽은 사용자가 아니면 
                else DieUserProcess(user);
            }
            else if (BettingEvent.Equals((int)BET.HALF))
                this.pokerBetObject.HalfEvent(user.player_index);
            else if (BettingEvent.Equals((int)BET.PPING))
                this.pokerBetObject.PPingEvent(user.player_index);
            else if (BettingEvent.Equals((int)BET.ALLIN))
            {
                this.pokerBetObject.AllinEvent(user.player_index);
                if (!this.pokerBetObject.sideMoney.ContainsKey(user.player_index))
                    this.pokerBetObject.sideMoney.Add(user.player_index, -this.pokerBetObject.getAllinMoney);
                if (this.pokerBetObject.eachUserAccumulateMoney.ContainsKey(user.player_index))
                    this.playerStateList.Remove(user.player_index);
            }
            else
                this.pokerBetObject.SideBettingEvent(user.player_index);

        }

        private void EachUserDieMsgToClient(Gamer user, bool wifiConnection)
        {
            CPacket turnEndMsg = CPacket.create();
            JObject jsonObj = SetProtocol(PROTOCOL.GAME_DIE_REQ);

            jsonObj.Add("DIE_USER_INDEX", user.player_index.ToString());
            jsonObj.Add("NEXT_USER_INDEX", this.currentTurnPlayer.ToString());
            turnEndMsg.push(jsonObj.ToString());

            this.wifiConValue = wifiConnection;
            if (wifiConnection)
            {
                this.unNormalDieUser.Add(user.player_index, user.userId);
                for (int i = 0; i < this.players.Count; i++)
                    if (this.players[i] != user) this.players[i].send(turnEndMsg);
            }
            else
                this.players.ForEach(player => player.send_for_broadcast(turnEndMsg));

            CPacket.destroy(turnEndMsg);

        }

        public void EachUserOutMsgToClient(Gamer user, int flag)
        {
            CPacket outMsg = CPacket.create();
            JObject roomOutAck = SetProtocol(PROTOCOL.OUT_GAME_ROOM_ACK);

            roomOutAck.Add("USER_INDEX", user.player_index.ToString());
            roomOutAck.Add("CUR_TURN_PLAYER", this.currentTurnPlayer);
            roomOutAck.Add("ROOM_MASTER", this.roomMaster.ToString());
            roomOutAck.Add("ROOM_OUT_MSG", flag.ToString());

            outMsg.push(roomOutAck.ToString());
            if (this.wifiConValue)
            {
                for (int i = 0; i < this.players.Count; i++)
                    if (this.players[i] != user) this.players[i].send(outMsg);
            }
            else
                this.players.ForEach(player => player.send_for_broadcast(outMsg));
            CPacket.destroy(outMsg);

        }

        private void EachUserTurnEndMsgToClient(Gamer user, bool lastBet)
        {
            CPacket turnEndMsg = CPacket.create();
            JObject jsonObj = SetProtocol(PROTOCOL.GAME_USER_EACH_TURN_END);

            jsonObj.Add("PANDON", this.pokerBetObject.getPandonMoney.ToString());
            jsonObj.Add("BEFORE_MONEY", this.pokerBetObject.getBeforeMoney.ToString());
            jsonObj.Add("CUR_USER_INDEX", user.player_index.ToString());
            jsonObj.Add("CUR_USER_BETTING_MONEY", this.pokerBetObject.getCallMoney.ToString());
            jsonObj.Add("TOTAL_MONEY", this.pokerBetObject.getTotalMoney.ToString());
            jsonObj.Add("CUR_USER_MONEY", this.pokerBetObject.GetEachUserMoney(user.player_index));
            jsonObj.Add("NEXT_USER_INDEX", this.currentTurnPlayer.ToString());
            jsonObj.Add("BETTING_NAME", this.bettingName);
            jsonObj.Add("LAST_BET", lastBet.ToString());

            turnEndMsg.push(jsonObj.ToString());

            if (this.wifiConValue) ClusteringMsg(turnEndMsg);
            else this.players.ForEach(player => player.send_for_broadcast(turnEndMsg));

            CPacket.destroy(turnEndMsg);

        }

        private void EachUserGameOverMsgToClient()
        {
            CPacket newmsg = CPacket.create();
            JObject jsonObj = SetProtocol(PROTOCOL.GAME_OVER);

            jsonObj.Add("PLAYING_USER_COUNT", GetPlayingUserCount().ToString());
            jsonObj.Add("TOTAL_MONEY", this.pokerBetObject.getTotalMoney.ToString());
            jsonObj.Add("WINNER_INDEX", this.winnerIndex);
            jsonObj.Add("WINNER_HAND", this.maxWinnerHand.ToString());
            jsonObj.Add("WINNER_MAXCARD", this.maxWinnerHighCard.ToString());
            jsonObj.Add("WINNER_MAXSHAPE", this.maxWinnerHighShape.ToString());
            jsonObj.Add("SIDE_BETTING", this.sideBetterWin.ToString());
            jsonObj.Add("SECOND_WINNER", this.secondWinnerIndex.ToString());
            jsonObj.Add("SIDE_MONEY", this.sideMoney.ToString());

            int packetIndex = 0;
            for (int i = 0; i < players.Count(); i++)
            {
                if (this.players[i].playingFlag == true) //실제 게임에 참여하는 사람 것 만 패킷을 만듬.
                {
                    jsonObj.Add("USER_INDEX" + packetIndex.ToString(), players[i].player_index.ToString());
                    jsonObj.Add("USER_MONEY" + packetIndex.ToString(), this.pokerBetObject.GetEachUserMoney(players[i].player_index));
                    jsonObj.Add("USER_WIN" + packetIndex.ToString(), this.pokerBetObject.GetEachUserWin(players[i].player_index).ToString());
                    jsonObj.Add("USER_LOSE" + packetIndex.ToString(), this.pokerBetObject.GetEachUserLose(players[i].player_index).ToString());

                    packetIndex++;
                }
            }

            newmsg.push(jsonObj.ToString());

            this.players.ForEach(player => player.send_for_broadcast(newmsg));

            CPacket.destroy(newmsg);
        }

        private bool IsPlayerCurruentPlaying(int key)
        {
            for (int i = 0; i < players.Count; i++)
            {
                if (this.players[i].player_index == key && this.players[i].playingFlag == true)
                    return true;
            }
            return false;
        }

        private bool IsExistSideBetter(int key)
        {
            foreach (KeyValuePair<int, Int64> kvp in this.pokerBetObject.sideMoney.ToList())
            {
                if (kvp.Key == key) return true;
            }
            return false;
        }

        private Int64 GetPivotMoney()
        {
            Int64 pivotMoney = 0;
            bool flag = false;
            if (this.pokerBetObject.sideMoney.Count > 0) //사이드 배팅자가 존재하면. 기준 머니를 사이드 배팅자가 아닌 놈으로
            {
                foreach (KeyValuePair<int, Int64> sidekvp in this.pokerBetObject.sideMoney.ToList())
                {
                    foreach (KeyValuePair<int, Int64> kvp in this.pokerBetObject.eachUserAccumulateMoney.ToList())
                    {
                        if (sidekvp.Key != kvp.Key && IsPlayerCurruentPlaying(kvp.Key))
                        {
                            pivotMoney = kvp.Value;
                            flag = true;
                            break;
                        }
                    }
                    if (flag) break;
                }
            }
            else
            {
                pivotMoney = this.pokerBetObject.eachUserAccumulateMoney.Values.First(); //이거 문제 무조건 첫번째 안됨. 첫번쨰가 사이드배터 일수도
            }

            return pivotMoney;
        }

        private bool SearchBettingMoney()
        {
            Int64 pivotMoney = GetPivotMoney();

            foreach (KeyValuePair<int, Int64> kvp in this.pokerBetObject.eachUserAccumulateMoney.ToList())
            {
                if (!IsExistSideBetter(kvp.Key))
                {
                    if (kvp.Value != pivotMoney && IsPlayerCurruentPlaying(kvp.Key)) return false;
                }
            }
            return true;
        }

        private bool IsFirstBetting()
        {
            foreach (KeyValuePair<int, Int64> kvp in this.pokerBetObject.eachUserAccumulateMoney.ToList())
            {
                if (kvp.Value == 0) return true;
            }
            return false;
        }

        private void SetSideMoney()
        {
            foreach (KeyValuePair<int, Int64> sidekvp in this.pokerBetObject.sideMoney.ToList()) //사이드 머니 누적. 남보다 덜 낸 금액
            {
                Int64 sideMoney = sidekvp.Value;
                foreach (KeyValuePair<int, Int64> kvp in this.pokerBetObject.eachUserAccumulateMoney.ToList()) // 사이드 머니 한사람 재외 하고 차액을 누적
                {
                    if (sidekvp.Key != kvp.Key)
                    {
                        int playingUser = this.pokerBetObject.eachUserAccumulateMoney.Count - this.pokerBetObject.sideMoney.Count;
                        sideMoney += (kvp.Value * playingUser);
                        this.pokerBetObject.sideMoney[sidekvp.Key] = sideMoney;
                        break;
                    }
                }

            }
        }

        public void TurnEnd(Gamer user, int BettingEvent)
        {
           
            if (this.currentTurnPlayer != user.player_index)
            {
                return;
            }

            ChangeUserCurrentState(user, PLAYER_STATE.BETTING_COMPLETE);
            SetBettingName(BettingEvent);

            try
            {
                if (this.bettingCount == 0) this.pokerBetObject.SetBeforeMoney(0); //첫번째 배팅일 경우

                if (AllplayerSameState(PLAYER_STATE.BETTING_COMPLETE))
                {
                    ChangeAllUserCurrentState(PLAYER_STATE.BETTING_READY);
                    this.bettingCount++;
                }

                BettingProcess(user, BettingEvent, false, false); // 배팅 작업

                if (SearchBettingMoney()) //배팅 금액이 같을 경우
                {
                    if (BettingEvent == (int)BET.DIE)
                    {
                        this.userTurnOrder.Remove(user.player_index);
                        return;
                    }
                    else if (BettingEvent == (int)BET.CHECK)
                    {
                        this.currentTurnPlayer = this.userTurnOrder.Find(this.currentTurnPlayer).Next.Value;
                        EachUserTurnEndMsgToClient(user, false);
                        return;
                    }

                    SetSideMoney(); //사이드 머니
                    EachUserTurnEndMsgToClient(user, false);
                    ChangeAllUserCurrentState(PLAYER_STATE.BETTING_READY);
                    NextDistriubute();
                    this.bettingCount = 0;

                }
                else //배팅 금액이 다를 경우
                {
                    if (this.bettingRule[this.currentGu - 1] > this.bettingCount) //아직 계속 레이즈 할 수 있을 경우
                    {
                        if (BettingEvent == (int)BET.DIE)
                        {
                            this.userTurnOrder.Remove(user.player_index);
                            return;
                        }
                        else if (BettingEvent == (int)BET.CHECK)
                        {
                            this.currentTurnPlayer = this.userTurnOrder.Find(this.currentTurnPlayer).Next.Value;
                            EachUserTurnEndMsgToClient(user, false);
                            return;
                        }
                        this.currentTurnPlayer = this.userTurnOrder.Find(this.currentTurnPlayer).Next.Value;
                        EachUserTurnEndMsgToClient(user, false);

                    }
                    else //레이즈 안되는 경우 라스트 콜 하라고 시키자
                    {
                        if (BettingEvent == (int)BET.DIE)
                        {
                            this.userTurnOrder.Remove(user.player_index);
                            return;
                        }
                        else if (BettingEvent == (int)BET.CHECK)
                        {
                            this.currentTurnPlayer = this.userTurnOrder.Find(this.currentTurnPlayer).Next.Value;
                            EachUserTurnEndMsgToClient(user, false);
                            return;
                        }
                        this.currentTurnPlayer = this.userTurnOrder.Find(this.currentTurnPlayer).Next.Value;
                        EachUserTurnEndMsgToClient(user, true);
                    }
                }
            }
            catch (System.Exception e)
            {
                Console.WriteLine(e.Message);  // 예외의 메시지를 출력
                Console.WriteLine(e.StackTrace);
            }

        }

        bool IsGamePlaying(string roomId)
        {
            GAME_STATE tempValue;
            this.gameStateList.TryGetValue(roomId, out tempValue);

            if (tempValue.Equals(GAME_STATE.PLAYING)) return true;
            else return false;
        }

        void ChangeAllUserCurrentState(PLAYER_STATE state)
        {
            foreach (KeyValuePair<int, PLAYER_STATE> kvp in this.playerStateList.ToList())
            {
                int index = FindUserIndex(kvp.Key);
                if (this.playerStateList.ContainsKey(kvp.Key))
                {
                    if (this.players[index].playingFlag == true) //현재 플레이에 참여하는 놈들 중
                        this.playerStateList[kvp.Key] = state;
                }
                else
                {
                    this.playerStateList.Add(kvp.Key, state);
                }
            }
        }

        void ChangeAllUserCurrentStateExcludeLastUser(PLAYER_STATE state, int lastUser)
        {
            foreach (KeyValuePair<int, PLAYER_STATE> kvp in this.playerStateList.ToList())
            {
                if (lastUser != kvp.Key)
                {
                    int index = FindUserIndex(kvp.Key);
                    if (this.playerStateList.ContainsKey(kvp.Key))
                    {
                        if (this.players[index].playingFlag == true)
                            this.playerStateList[kvp.Key] = state;
                    }
                    else
                    {
                        this.playerStateList.Add(kvp.Key, state);
                    }
                }

            }
        }

        private bool PlayingUserCheckState(PLAYER_STATE state)
        {
            foreach (KeyValuePair<int, PLAYER_STATE> kvp in this.playerStateList)
            {
                int index = FindUserIndex(kvp.Key);
                if (this.players[index].playingFlag == true) if (kvp.Value == state) return true;
            }
            return false;
        }

        private bool AllplayerSameState(PLAYER_STATE state)
        {
            foreach (KeyValuePair<int, PLAYER_STATE> kvp in this.playerStateList)
            {
                int index = FindUserIndex(kvp.Key);
                if (this.players[index].playingFlag == true) if (kvp.Value != state) return false;
            }
            return true;
        }

        void ChangeUserCurrentState(Gamer player, PLAYER_STATE state)
        {
            if (this.playerStateList.ContainsKey(player.player_index))
                this.playerStateList[player.player_index] = state;
            else
                this.playerStateList.Add(player.player_index, state);
        }

        void ChangeRoomCurrentState(string roomId, GAME_STATE state)
        {
            if (this.gameStateList.ContainsKey(roomId))
                this.gameStateList[roomId] = state;
            else
                this.gameStateList.Add(roomId, state);
        }

        Int64 SideBetterIsWin(Int64 resultMoney)
        {
            foreach (KeyValuePair<int, Int64> kvp in this.pokerBetObject.sideMoney.ToList())
            {
                if (kvp.Key == winnerIndex) //사이드 배팅 자가 승리할 경우
                {
                    resultMoney = this.pokerBetObject.getTotalMoney - kvp.Value;
                    this.pokerBetObject.eachUserMoney[this.winnerIndex] = resultMoney.ToString();
                    this.sideMoney = kvp.Value;
                    sideBetterWin = true;
                    break;
                }

            }
            return resultMoney;
        }

        void GameOver()
        {
            this.roomMaster = this.winnerIndex;
            this.pokerBetObject.UpDateWinLoseRate(this.winnerIndex);
            Int64 resultMoney = Int64.Parse(this.pokerBetObject.eachUserMoney[this.winnerIndex]);
            this.sideMoney = 0;
            this.secondWinnerIndex = 0;
            this.sideBetterWin = false;

            resultMoney = SideBetterIsWin(resultMoney);

            if (sideBetterWin)// 사이드 배팅자가 우승하면 돈 나눠줘야함.
            {
                secondWinnerIndex = SecondWinnerHand(winnerIndex);
                Int64 temp = Int64.Parse(this.pokerBetObject.eachUserMoney[secondWinnerIndex]);
                temp += this.sideMoney;
                this.pokerBetObject.eachUserMoney[secondWinnerIndex] = temp.ToString();
            }
            else
            {
                resultMoney += this.pokerBetObject.getTotalMoney;
                this.pokerBetObject.eachUserMoney[this.winnerIndex] = resultMoney.ToString();
            }

            //DB관련
            lock (PokerServer.threadCount)
            {
                int theradCount = int.Parse(PokerServer.threadCount.ToString()) + 1;
                theradCount %= 5;
                PokerServer.threadCount = (object)theradCount;
            }

            ThreadPool.QueueUserWorkItem(new WaitCallback(ResultDataBaseUpdate), (object)(PokerServer.threadCount));
        }

        public object[] AasyncDelegate(Gamer player, bool protocol, out int threadId)
        {
            Console.WriteLine("비동기 DB스레드 시작");

            threadId = Thread.CurrentThread.ManagedThreadId;

            try
            {
                HiPerfTimer temp = new HiPerfTimer();
                temp.Start();
                if (!this.pokerBetObject.eachUserMoney.ContainsKey(player.player_index))
                {
                    DataSet ds = new DataSet();
                    string sql = "SELECT money,win,lose FROM user WHERE id=" + "'" + player.userId + "'";
                    MySqlDataAdapter adpt = new MySqlDataAdapter(sql, dbUpdateConnection);
                    adpt.Fill(ds, "user");

                    if (ds.Tables.Count > 0)
                    {
                        foreach (DataRow r in ds.Tables[0].Rows)
                        {
                            this.pokerBetObject.eachUserMoney.Add(player.player_index, r["money"].ToString()); //
                            this.pokerBetObject.eachUserAccumulateMoney.Add(player.player_index, 0);
                            this.pokerBetObject.eachUserWin.Add(player.player_index, int.Parse(r["win"].ToString()));
                            this.pokerBetObject.eachUserLose.Add(player.player_index, int.Parse(r["lose"].ToString()));
                        }
                    }
                    this.pokerBetObject.eachUserId.Add(player.player_index, player.userId);
                }
                temp.Stop();
                Console.WriteLine("~!DB  " + temp.Duration);
            }

            catch (System.Data.SqlClient.SqlException E)
            {
                Console.WriteLine("db error");
                Console.WriteLine(E.ToString());
            }
            return new object[] { player, protocol };

        }

        public void DataBaseThreadStart(Gamer gamer, bool master)
        {
            int threadId;

            AasyncDelegateCaller caller = new AasyncDelegateCaller(AasyncDelegate);
            // 비 동기 시작
            IAsyncResult result = caller.BeginInvoke(gamer, master, out threadId, new AsyncCallback(AsyncDelegateCallback), caller);

            Console.WriteLine("로직 쓰레드 {0} 에서 작업.", Thread.CurrentThread.ManagedThreadId);

            Console.WriteLine("완료");
        }

        public void AsyncDelegateCallback(IAsyncResult result)
        {
            AasyncDelegateCaller caller = (AasyncDelegateCaller)result.AsyncState;

            int threadId;

            object[] returnValue = caller.EndInvoke(out threadId, result);

            Console.WriteLine("DB작업 완료");

            Gamer masterGamer = (Gamer)returnValue[0];
            bool flag = (bool)returnValue[1];
            CPacket msg = CPacket.create();

            JObject jsonObj;
            if (flag) { jsonObj = SetProtocol(PROTOCOL.MAKE_GAME_ROOM_ACK); }
            else { jsonObj = SetProtocol(PROTOCOL.ENTER_GAME_ROOM_ACK); }

            jsonObj.Add("ROOM_ID", this.roomId);
            msg.push(jsonObj.ToString());
            masterGamer.send(msg);

            CPacket.destroy(msg);
        }

        public void DataBaseUpdateProcessCompleteCallback(EventArgs e)
        {
            ChangeRoomCurrentState(this.roomId, GAME_STATE.NO_PALYING);

            EachUserGameOverMsgToClient();


            if (UserIsAccessThisRoom() == false) //재접속한 사람이 없거나, 리스트에 아무것도 없을때
            {
                foreach (KeyValuePair<int, string> kvp in this.unNormalDieUser.ToList())
                {
                    this.players[FindUserIndex(kvp.Key)].playingFlag = false;
                    RemoveUser(this.players[FindUserIndex(kvp.Key)].owner);
                }
            }

            foreach (KeyValuePair<int, bool> kvp in this.reservedUserOutList.ToList()) //나가기 예약.
            {
                if (kvp.Value == true)
                {
                    int userIndex = FindUserIndex(kvp.Key);
                    this.players[userIndex].playingFlag = false;
                    RemoveUser(this.players[userIndex].owner);
                }
            }

            ChangeAllUserCurrentState(PLAYER_STATE.LOADING_COMPLETE);

        }

        public void DataBaseUpdateForForcedRemoveUser(object o)
        {
            object[] array = o as object[];
            int Key = (int)array[0];
            int index = (int)array[1];
            try
            {
                if (dbUpdateConnectionForcedRemoveUser.State != ConnectionState.Open) { dbUpdateConnectionForcedRemoveUser.Open(); }

                MySqlCommand updateCommand = new MySqlCommand();
                updateCommand.Connection = dbUpdateConnectionForcedRemoveUser;
                updateCommand.CommandText = "UPDATE user SET money=@MONEY, win=@WIN, lose=@LOSE WHERE id=@NAME";

                updateCommand.Parameters.Add("@NAME", MySqlDbType.VarChar, 20);
                updateCommand.Parameters.Add("@MONEY", MySqlDbType.Int64);
                updateCommand.Parameters.Add("@WIN", MySqlDbType.Int32);
                updateCommand.Parameters.Add("@LOSE", MySqlDbType.Int32);

                updateCommand.Parameters[0].Value = this.pokerBetObject.eachUserId[Key].ToString();
                if (this.pokerBetObject.eachUserMoney[Key] == 0.ToString()) updateCommand.Parameters[1].Value = 1000000000;
                else updateCommand.Parameters[1].Value = Int64.Parse(this.pokerBetObject.eachUserMoney[Key]);
                updateCommand.Parameters[2].Value = this.pokerBetObject.eachUserWin[Key];
                updateCommand.Parameters[3].Value = this.pokerBetObject.eachUserLose[Key];

                int affected = updateCommand.ExecuteNonQuery();
                Console.WriteLine("# of affected row: " + affected);

                dbUpdateConnectionForcedRemoveUser.Close();
            }
            catch (System.Data.SqlClient.SqlException E)
            {
                Console.WriteLine("db error");
                Console.WriteLine(E.ToString());
            }
            finally
            {
                dbUpdateConnectionForcedRemoveUser.Close();
            }

            //PokerServer.resetEvent[index].Set();
        }

        public void ResultDataBaseUpdate(object o)
        {
            int index = (int)o;
            try
            {
                if (dbUpdateConnection.State != ConnectionState.Open) { dbUpdateConnection.Open(); }

                foreach (KeyValuePair<int, string> kvp in this.pokerBetObject.eachUserMoney.ToList())
                {
                    MySqlCommand updateCommand = new MySqlCommand();
                    updateCommand.Connection = dbUpdateConnection;
                    updateCommand.CommandText = "UPDATE user SET money=@MONEY, win=@WIN, lose=@LOSE WHERE id=@NAME";

                    updateCommand.Parameters.Add("@NAME", MySqlDbType.VarChar, 20);
                    updateCommand.Parameters.Add("@MONEY", MySqlDbType.Int64);
                    updateCommand.Parameters.Add("@WIN", MySqlDbType.Int32);
                    updateCommand.Parameters.Add("@LOSE", MySqlDbType.Int32);

                    updateCommand.Parameters[0].Value = this.pokerBetObject.eachUserId[kvp.Key].ToString();
                    if (this.pokerBetObject.eachUserMoney[kvp.Key] == 0.ToString()) updateCommand.Parameters[1].Value = 1000000000;
                    else updateCommand.Parameters[1].Value = Int64.Parse(this.pokerBetObject.eachUserMoney[kvp.Key]);
                    updateCommand.Parameters[2].Value = this.pokerBetObject.eachUserWin[kvp.Key];
                    updateCommand.Parameters[3].Value = this.pokerBetObject.eachUserLose[kvp.Key];

                    int affected = updateCommand.ExecuteNonQuery();
                    Console.WriteLine("# of affected row: " + affected);
                }

                dbUpdateConnection.Close();
            }
            catch (System.Data.SqlClient.SqlException E)
            {
                Console.WriteLine("db error");
                Console.WriteLine(E.ToString());
            }
            finally
            {
                dbUpdateConnection.Close();
            }

            //PokerServer.resetEvent[index].Set();
            this.dbUpdateComplete(null);
        }

        public bool RemoveUser(User user)
        {
            if (IsGamePlaying(this.roomId) && user.player.playingFlag == true) // 나가기 예약
            {
                if (this.reservedUserOutList.ContainsKey(user.player.player_index))
                    this.reservedUserOutList[user.player.player_index] = !this.reservedUserOutList[user.player.player_index];
                else
                    this.reservedUserOutList.Add(user.player.player_index, true);

                EachUserOutMsgToClient(user.player, 0);

                return false;
            }
            else
            {
                if (user.player.player_index == this.roomMaster) this.roomMaster = ChangeRoomMaster(user.player.player_index);

                EachUserOutMsgToClient(user.player, 1);

                lock (this.playerLock)
                {
                    this.pokerBetObject.removeItem(user.player.player_index);
                    this.players.Remove(user.player);
                    this.playerStateList.Remove(user.player.player_index);
                    this.userTurnOrder.Remove(user.player.player_index);
                    this.pokerBetObject.eachUserAccumulateMoney.Remove(user.player.player_index);
                    this.curManCount--;
                    RemovePlayerIndex(user.player.player_index);

                }
                return true;

            }
        }

        public void ForcedRemoveUser(User user)
        {
            if (user.player.player_index == this.roomMaster) this.roomMaster = ChangeRoomMaster(user.player.player_index);

            lock (this.playerLock)
            {
                this.playerStateList.Remove(user.player.player_index);
                this.userTurnOrder.Remove(user.player.player_index);
                this.curManCount--;
                RemovePlayerIndex(user.player.player_index);
            }

            EachUserOutMsgToClient(user.player, 1);
        }

        public void Destroy()
        {
            GC.SuppressFinalize(this);
        }
    }
}








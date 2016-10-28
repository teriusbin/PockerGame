using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Json;
using Newtonsoft.Json;
using System.Threading;
using System.Net;
using Newtonsoft.Json.Linq;
using FreeNet;
using SimpleJSON;

namespace PokerGameServer
{

    public class User : IPeer
    {
        public CUserToken token;
        public DateTime lastPingTime;
        public Room play_room { get; private set; }
        public Gamer player { get; private set; }
        public bool reConnectionFlag = false; 

        private String roomId;
        private String userId;
        private String chatMsg;
        private String titleStr;
        private String betMoneyStr;
        private String userMoney;
        private int curentGu;

        public String GetRoomId { get { return this.roomId; } }
        public String GetUserId { get { return this.userId; } }
        public String GetTitleStr { get { return this.titleStr; } }
        public String GetBetMoneyStr { get { return this.betMoneyStr; } }
        public String GetChatMsg { get { return this.chatMsg; } }
        public String GetUserMoney { get { return this.userMoney; } }
        public int GetCurrentGu { get { return this.curentGu; } }

        public User(CUserToken token)
        {
            this.token = token;
            this.token.set_peer(this);

            this.roomId = null;
            this.userId = null;
            this.chatMsg = null;
            this.titleStr = null;
            this.betMoneyStr = null;
            this.userMoney = null;
            this.lastPingTime = DateTime.Now;
            this.curentGu = 0;
        }

        void IPeer.on_message(Const<byte[]> buffer)
        {

            byte[] clone = new byte[buffer.Value.Length];
            Array.Copy(buffer.Value, clone, buffer.Value.Length);
            CPacket msg = new CPacket(clone, this);
            MainServer.pokerServerMain.EnqueuePacket(msg, this);
        }

        void IPeer.on_removed()
        {
            MainServer.NormalRemoveUser(this);
        }

        public void send(CPacket msg)
        {
            this.token.send(msg);
        }

        private JObject SetProtocol(PROTOCOL protocl)
        {
            JObject jsonObj = new JObject();
            jsonObj.Add("PROTOCOL_ID", protocl.ToString());
            return jsonObj;
        }

        void IPeer.disconnect()
        {
            this.token.socket.Disconnect(false);
        }

        private void EnterLobby(JSONNode onMsg)
        {
            MainServer.pokerServerMain.EnteredLobbyReq(this);

            CPacket ackmsg = CPacket.create();
            JObject enterLobbyAckObj = SetProtocol(PROTOCOL.ENTER_LOBBY_ACK);
            ackmsg.push(enterLobbyAckObj.ToString());
            this.send(ackmsg);

            CPacket.destroy(ackmsg);
        }

        private void MakeGameRoom(JSONNode onMsg)
        {
            this.titleStr = onMsg["ROOM_TITLE"].Value;
            this.betMoneyStr = onMsg["ROOM_BETMONEY"].Value;
            this.userId = onMsg["USER_ID"].Value;
            this.userMoney = onMsg["USER_MONEY"].Value;

            MainServer.pokerServerMain.CreateRoomReq(this);
        }

        private void EnterGameRoom(JSONNode onMsg)
        {
            this.userId = onMsg["USER_ID"].Value;
            this.roomId = onMsg["ROOM_ID"].Value;
            this.userMoney = onMsg["USER_MONEY"].Value;

            MainServer.pokerServerMain.EnteredRoomReq(this);
        }

        private void RandomRoom(JSONNode onMsg)
        {
             MainServer.pokerServerMain.randomGetRoomId(this);
        }

        private void OutGameRoom(JSONNode onMsg)
        {
            MainServer.pokerServerMain.RoomOutReq(this);
        }

        private void UserLoadGameSceneComplete(JSONNode onMsg)
        {
            this.roomId = onMsg["ROOM_ID"].Value;
            this.userId = onMsg["USER_ID"].Value;
            MainServer.pokerServerMain.SendRoomInfoReq(this);
        }

        private void UserCardSelectComplete(JSONNode onMsg)
        {
            string openCardIndex = onMsg["OPEN_CARD"].Value;
            string removeCardIndex = onMsg["REMOVE_CARD"].Value;
            this.play_room.CardSelectedComplete(this.player, removeCardIndex, openCardIndex);
        }

        private void EachUserBettingComplete(JSONNode onMsg)
        {
            this.curentGu = int.Parse(onMsg["GU_COUNT"].Value);
            int bettingNumber = int.Parse(onMsg["USER_BUTTON_NAME"].Value);
            this.play_room.TurnEnd(this.player, bettingNumber);
        }

        private void PongMsgToClient(JSONNode onMsg)
        {
            this.lastPingTime = DateTime.Now;
            CPacket pongMsg = CPacket.create();
            JObject pongObj = SetProtocol(PROTOCOL.PONG);
            pongMsg.push(pongObj.ToString());
            this.send(pongMsg);

            CPacket.destroy(pongMsg);
        }

        void IPeer.process_user_operation(CPacket msg)
        {

            var onMsg = JSON.Parse(msg.pop_string());

            PROTOCOL PROTOCOL_ID=  (PROTOCOL) Enum.Parse(typeof(PROTOCOL), onMsg["PROTOCOL_ID"].Value);

            switch (PROTOCOL_ID)
            {
                case PROTOCOL.ENTER_LOBBY_REQ:
                    EnterLobby(onMsg);
                    break;

                case PROTOCOL.MAKE_GAME_ROOM_REQ:
                    MakeGameRoom(onMsg);
                    break;

                case PROTOCOL.ENTER_GAME_ROOM_REQ:
                    EnterGameRoom(onMsg);
                    break;

                case PROTOCOL.RANDOM_MATCHING_REQ:
                    RandomRoom(onMsg);
                    break;

                case PROTOCOL.CHAT_MSG_REQ:
                    this.chatMsg = onMsg["CHAT_MSG"].Value;
                    MainServer.pokerServerMain.ChatMsgBroadCastLobbyUsers(this);
                    break;

                case PROTOCOL.ROOM_LIST_REQ:
                    MainServer.pokerServerMain.RoomListReq(this);
                    break;

                case PROTOCOL.LOAD_GAME_SCENE_REQ:
                    UserLoadGameSceneComplete(onMsg);
                    break;

                case PROTOCOL.GAME_POSSIBLE_REQ:
                    this.roomId = onMsg["ROOM_ID"].Value;
                    MainServer.pokerServerMain.IsGamePlayPossibleReq(this);
                    break;

                case PROTOCOL.OUT_GAME_ROOM_REQ:
                    OutGameRoom(onMsg);
                    break;

                case PROTOCOL.LOADING_COMPLETED:
                    this.play_room.LoadingComplete(this.player);
                    break;

                case PROTOCOL.GAME_FIRST_CARD_DISTRIBUTE_COMPLETE:
                    this.play_room.FirstDistributeComplete(this.player);
                    break;

                case PROTOCOL.GAME_OPEN_CARD_SELECT_COMPLETE:
                    UserCardSelectComplete(onMsg);
                    break;

                case PROTOCOL.GAME_CARD_OPEN_COMPLETE:
                    this.play_room.CardOpenComplete(this.player);
                    break;

                case PROTOCOL.GAME_BETTING_COMPLETE:
                    EachUserBettingComplete(onMsg);
                    break;

                case PROTOCOL.GAME_WATCHING_REQ:
                    this.play_room.WatchingGame(this.player);
                    break;

                case PROTOCOL.GAME_WATCHING_DISTRIBUTE_COMPLETE:
                    this.play_room.TurnEnd(this.player, -1);
                    break;

                case PROTOCOL.GAME_MASTER_START_REQ:
                    this.play_room.GameStartMsgRecieveFromClient(this.player);
                    break;

                case PROTOCOL.PING:
                    PongMsgToClient(onMsg);
                    break;
            }
        }

        public void EnterRoom(Gamer player, Room room)
        {
            try
            {
                this.player = player;
                this.play_room = room;
            }
            catch (System.Exception e)
            {
                Console.WriteLine(e.Message);  // 예외의 메시지를 출력
                Console.WriteLine(e.StackTrace);

            }
        }
    }
}

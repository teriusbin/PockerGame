using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using FreeNet;
using SimpleJSON;

namespace PokerGameServer
{
	using FreeNet;
	using System.Threading;

	public class PokerServer
	{
		object operationLock;
		Queue<CPacket> userOperationQueue;

		// 로직은 하나의 스레드로만 처리한다.
		Thread logicThread;
		AutoResetEvent loopEvent;

        public RoomController roomManager { get; private set; }
        List<User> lobbyUsers;

        //public static ManualResetEvent[] resetEvent;
        public static int THREAD_MAX_COUNT = 5;
        public static object threadCount = 0;

        public PokerServer()
		{
            this.operationLock = new object();
            this.loopEvent = new AutoResetEvent(false);
            this.userOperationQueue = new Queue<CPacket>();

            this.roomManager = new RoomController();
          
            this.lobbyUsers = new List<User>();

            this.logicThread = new Thread(MsgLoop);
            this.logicThread.Start();

            //resetEvent = new ManualResetEvent[THREAD_MAX_COUNT];

            Thread.Sleep(100);
            //for (int i = 0; i < THREAD_MAX_COUNT; i++)
            //{
            //    resetEvent[i] = new ManualResetEvent(false);
            //}

        }

		
		void MsgLoop()
		{
			while (true)
			{

				CPacket packet = null;
				lock (this.operationLock)
				{    
					if (this.userOperationQueue.Count > 0)
					{
                        packet = this.userOperationQueue.Dequeue();
					}
				}

				if (packet != null)
				{
					// 패킷 처리.
					packetReceive(packet);
				}

                MainServer.keepAlive();

                if (this.userOperationQueue.Count <= 0)
                {
                    this.loopEvent.WaitOne();
                }
            }
		}

		public void EnqueuePacket(CPacket packet, User user)
		{
			lock (this.operationLock)
			{
				this.userOperationQueue.Enqueue(packet);
				this.loopEvent.Set();
			}
		}

		void packetReceive(CPacket msg)
        { 
			msg.owner.process_user_operation(msg);
		}


        public void ChatMsgBroadCastLobbyUsers(User user)
        {
            CPacket msg = CPacket.create();
            JObject jsonObj = this.roomManager.SetProtocol(PROTOCOL.CHAT_MSG_ACK);
            jsonObj.Add("CHAT_MSG", user.GetChatMsg);
            msg.push(jsonObj.ToString());
            this.lobbyUsers.ForEach(lobbyUsers => lobbyUsers.send(msg));

          
            CPacket.destroy(msg);
        }

        public void EnteredLobbyReq(User user)
        {
          
            if (this.lobbyUsers.Contains(user))
            {
                return;
            }
       
            this.lobbyUsers.Add(user);
          
        }

        public void EnteredRoomReq(User user)
        {

            if (!this.roomManager.IsRoomExsist(user.GetRoomId)) 
            {
                Console.WriteLine("방이 없다");
                return;
            }

          
            if (this.roomManager.GetCurManCount(user.GetRoomId) < 5)  //다섯명 이하일 경우에만
            {
                // 게임 방 입장
                Room battleroom = this.roomManager.GetGameRoomObj(user.GetRoomId);
            
                if (battleroom != null)
                {
                    this.roomManager.EnterRoom(user, battleroom);
                    this.lobbyUsers.Remove(user);
                }
                else
                {
                    Console.WriteLine("방이 없다");
                }

            }
            else //접속 못함.
            {

                Console.WriteLine("다섯명 이상이라 안된다");
            }

        }
        public void randomGetRoomId(User user)
        {
            string roomId = this.roomManager.GetRandomRoom();

            CPacket msg = CPacket.create();
            JObject jsonObj = this.roomManager.SetProtocol(PROTOCOL.RANDOM_MATCHING_ACK);
            jsonObj.Add("ROOM_ID", roomId);
            msg.push(jsonObj.ToString());
            user.send(msg);

        }
        public void CreateRoomReq(User user)
        {
            // 게임 방 생성.
            this.roomManager.CreateRoom(user);
            this.lobbyUsers.Remove(user);
        }

        public void IsGamePlayPossibleReq(User user)  
        {

            this.roomManager.IsPossibleGamePlay(user);
 
        }

        public void SendRoomInfoReq(User user)
        {
            CPacket msg = this.roomManager.CreateRoomInfoPacket(user);
         
            Room curRoom = user.play_room; 
       
            curRoom.players.ForEach(player => player.send_for_broadcast(msg));

            CPacket.destroy(msg);
            
        }

        public void RoomOutReq(User user) 
        {
            if (this.roomManager.RoomOut(user))
            {
                EnteredLobbyReq(user);
               
            }
        }

        public void RoomListReq(User user)
        { 
            CPacket msg = this.roomManager.CreateRoomListPacket();
            user.send(msg);
        }

        public void UserDisconnected(User user)
		{
            if(this.lobbyUsers.Contains(user))
            {
                this.lobbyUsers.Remove(user);
            }

        }
	}
}

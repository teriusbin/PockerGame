using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using FreeNet;

namespace PokerGameServer
{
	public class MainServer
	{
        public static List<User> userlist = new List<User>();
        public static List<User> removeUsers = new List<User>(100);
        public static PokerServer pokerServerMain = new PokerServer();

        public static CNetworkService service;

        static void Main(string[] args)
		{
            CPacketBufferManager.initialize(2000);
			
		    service = new CNetworkService();
			// 콜백 매소드 설정.
			service.seesionCreatedCallback += OnSessionCreated;
			// 초기화.
			service.initialize();
			service.listen("172.30.154.7", 7979, 100);


			Console.WriteLine("Started!");
			while (true)
			{
				string input = Console.ReadLine();
				
				System.Threading.Thread.Sleep(1000);
			}

			Console.ReadKey();
		}

		
		static void OnSessionCreated(CUserToken token)
		{
			User user = new User(token);
			lock (userlist)
			{
				userlist.Add(user);
			}
           
		}

     
        public static void NormalRemoveUser(User user)
		{
			lock (userlist)
            {
                if (userlist.Contains(user))
                {
                    userlist.Remove(user);
                    if (user.play_room != null)
                    {
                        if (MainServer.pokerServerMain.roomManager.IsRoomExsist(user.play_room.roomId) &&
                            user.play_room.gameStateList[user.play_room.roomId] == GAME_STATE.PLAYING
                             && user.reConnectionFlag == false)
                        {
                            if(user.player.playingFlag == true)
                            {
                                user.play_room.BettingProcess(user.player, 0, true, false);
                            }
                           
                        }

                        pokerServerMain.UserDisconnected(user);

                        if (user.play_room.gameStateList[user.play_room.roomId].Equals(GAME_STATE.NO_PALYING))
                        {
                            user.play_room.curManCount--;
                            if(user.play_room.roomMaster == user.player.player_index) { user.play_room.ChangeRoomMaster(user.player.player_index); }
                            user.play_room.EachUserOutMsgToClient(user.player, 1);
                        }
              

                        if (user.play_room != null
                            && user.play_room.curManCount < 1)
                        {
                            pokerServerMain.roomManager.RemoveRoom(user.play_room);

                        }
                    }
                }
			}
		}

        public static void UnNormalRemoveUser(User user) //wifi끊겼을때
        {
            lock (userlist)
            {
                if (user.play_room != null)
                {
                    if (MainServer.pokerServerMain.roomManager.IsRoomExsist(user.play_room.roomId) &&
                       user.play_room.gameStateList[user.play_room.roomId] == GAME_STATE.PLAYING &&
                        user.reConnectionFlag == false)
                    {
                        if (user.player.playingFlag == true)
                        {
                            user.reConnectionFlag = true;
                            user.play_room.BettingProcess(user.player, 0, true, true);
                        }
                    }

                    pokerServerMain.UserDisconnected(user);

                    if (user.play_room.gameStateList[user.play_room.roomId].Equals(GAME_STATE.NO_PALYING))
                    {
                        user.play_room.curManCount--;
                        if (user.play_room.roomMaster == user.player.player_index) { user.play_room.ChangeRoomMaster(user.player.player_index); }
                        user.play_room.EachUserOutMsgToClient(user.player, 1);
                    }

                    if (user.play_room != null && 
                        user.play_room.curManCount < 1)
                    {
                        pokerServerMain.roomManager.RemoveRoom(user.play_room);
                    }
                }
            }

        }

        public static void keepAlive()
        {
            lock (userlist)
            {
                removeUsers.Clear();
                for (int i = 0; i < MainServer.userlist.Count; i++)
                {
                    TimeSpan timespan = DateTime.Now - MainServer.userlist[i].lastPingTime;
                    if (timespan.TotalSeconds > 15)
                    {
                        Console.WriteLine("user가 비정상으로 끊겼다." + MainServer.userlist[i].token.socket.Handle);
                        removeUsers.Add(MainServer.userlist[i]);
                        userlist.Remove(MainServer.userlist[i]);
                    }
                }

                for (int i = 0; i < removeUsers.Count; i++)
                {
                    MainServer.UnNormalRemoveUser(removeUsers[i]);
                }
               
            }
        }
    }
}

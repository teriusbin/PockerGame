using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using SimpleJSON;
using FreeNet;

namespace PokerGameServer
{
    public class RoomController
    {
      
        Dictionary<string, Room> roomDictionary;

        public RoomController()
        {
            this.roomDictionary = new Dictionary<string, Room>();
        }

        public CPacket CreateRoomListPacket()
        {

            CPacket msg = CPacket.create();
            JObject jsonObj = SetProtocol(PROTOCOL.ROOM_LIST_ACK);
            jsonObj.Add("ROOM_COUNT", this.roomDictionary.Count.ToString());
            int i = 0;
            foreach (var pair in roomDictionary)
            {
                jsonObj.Add("ROOM_ID"+i.ToString(), pair.Key.ToString());
                jsonObj.Add("ROOM_NUMBER" + i.ToString(), pair.Value.no.ToString());
                jsonObj.Add("ROOM_TITLE" + i.ToString(), pair.Value.titleStr.ToString());
                jsonObj.Add("ROOM_BETMONEY" + i.ToString(), pair.Value.betMoneyStr.ToString());
                jsonObj.Add("ROOM_CUR_MAN_COUNT" + i.ToString(), pair.Value.curManCount.ToString());
                i++;
            }
            msg.push(jsonObj.ToString());
           
            return msg;
        }
      
        public CPacket CreateRoomInfoPacket(User user)
        {
            Room tempObj;
            bool hasValue = roomDictionary.TryGetValue(user.GetRoomId, out tempObj);
           
            if (hasValue)
            {
                CPacket msg = CPacket.create();
                JObject jsonObj = SetProtocol(PROTOCOL.LOAD_GAME_SCENE_ACK);
               
                jsonObj.Add("ROOM_ID", tempObj.roomId.ToString());
                jsonObj.Add("ROOM_NUMBER", tempObj.no.ToString());
                jsonObj.Add("ROOM_TITLE", tempObj.titleStr.ToString());
                jsonObj.Add("ROOM_BETMONEY", tempObj.betMoneyStr.ToString());
                jsonObj.Add("ROOM_CUR_MAN_COUNT", tempObj.curManCount.ToString());
                jsonObj.Add("ROOM_CUR_PLAYING_MAN_COUNT", tempObj.GetPlayingUserCount().ToString());
                jsonObj.Add("TOTAL_MONEY", user.play_room.pokerBetObject.getTotalMoney.ToString());
                jsonObj.Add("ROOM_MASTER", user.play_room.roomMaster.ToString());
                
                for (int i = 0; i < user.play_room.players.Count; i++)
                {
                    jsonObj.Add("USER_ID"+i.ToString(), user.play_room.players[i].userId);
                    jsonObj.Add("USER_INDEX"+ i.ToString(), user.play_room.players[i].player_index);
                    jsonObj.Add("USER_MONEY" + i.ToString(), user.play_room.pokerBetObject.GetEachUserMoney(user.play_room.players[i].player_index));
                   
                }

                msg.push(jsonObj.ToString());

                return msg;
            }
            else {

                return null;
            }
        }

        public string GetRandomRoom()
        {
            string roomId = null;
            foreach (var pair in this.roomDictionary)
            {
                if(pair.Value.curManCount < 5 )
                {
                    return pair.Key;
                  
                }
            }
            roomId = "notMatch";
            return roomId;
        }

        public Boolean IsRoomExsist(string roomId)
        {

            return roomDictionary.ContainsKey(roomId);

        }

        public Room GetGameRoomObj(string roomId)
        {
            Room tempObj;
            bool hasValue = roomDictionary.TryGetValue(roomId, out tempObj);
            if (hasValue)
            {
                return tempObj;
            }
            else
            {
                return null;
            }
        }

        public int GetCurManCount(string roomId)
        {
            Room tempObj;
            bool hasValue = roomDictionary.TryGetValue(roomId, out tempObj);
            if (hasValue)
            {
                return tempObj.curManCount;
            }
            else {

                return -1;
            }
        }

        public void UpdateRoomCurManCountInfo(string roomId)
        {
            Room tempObj;
            bool hasValue = this.roomDictionary.TryGetValue(roomId, out tempObj);
            if (hasValue)
            {
                tempObj.curManCount = (tempObj.players.Count) % 5; //d이것도 지워야함
            }
            else {

                return;
            }
        }

        public void IsPossibleGamePlay(User user)
        {
         
            bool hasValue = this.roomDictionary.ContainsValue(user.play_room);
            if (hasValue)
            {
                user.play_room.IsPossibleGamePlay(user);
            }
            else {
                return;
            }
        }

        public void CreateRoom(User user)
        {
            Room room = new Room();

            Guid guid = Guid.NewGuid(); ;
       
            room.no = this.roomDictionary.Count + 1;
            room.titleStr = user.GetTitleStr;
            room.betMoneyStr = user.GetBetMoneyStr;
            room.curManCount = 1;
            room.roomId = guid.ToString();
            room.CreateGameroom(user);
            
            this.roomDictionary.Add(guid.ToString(), room);
        }

      
        public void EnterRoom(User user, Room battleRoom)
        {
            battleRoom.EnterGameRoom(user);
            UpdateRoomCurManCountInfo(user.GetRoomId);
            
        }

        public bool RoomOut(User user) //로비 유저에 추가, 해당 룸 플레이어에서 제거
        {
            if (user.play_room.RemoveUser(user))
            {
                if (user.play_room.curManCount < 1)
                {
                    RemoveRoom(user.play_room);
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        public void RemoveRoom(Room room)
        { 
            this.roomDictionary.Remove(room.roomId);
            room.Destroy();
        }

        public JObject SetProtocol(PROTOCOL protocl)
        {
            JObject jsonObj = new JObject();
            jsonObj.Add("PROTOCOL_ID", protocl.ToString());
            return jsonObj;
        }

    }
}

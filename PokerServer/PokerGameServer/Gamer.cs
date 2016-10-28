using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokerGameServer
{
	using FreeNet;

	public class Gamer
	{
		public User owner;
        public string userId;
        public int myGu;
        public int myIndex;
        public int player_index { get; private set; }
        public bool playingFlag;
        private string hasMoney;

        public String GetGamerMoney { get { return this.hasMoney; } }


        public Gamer(User user, int player_index)
		{
			this.owner = user;
            this.userId = user.GetUserId;
            this.myIndex = player_index;
            this.myGu = user.GetCurrentGu;
            this.player_index = player_index;
            this.hasMoney = null;
            this.playingFlag = false;
		}
       
     
        public void SetUserHasMoney(string inputMoney)
        {
            this.hasMoney = inputMoney;
        }

		public void send(CPacket msg)
		{
			this.owner.send(msg);
			CPacket.destroy(msg);
		}

		public void send_for_broadcast(CPacket msg)
		{
			this.owner.send(msg);
		}

		
	}
}

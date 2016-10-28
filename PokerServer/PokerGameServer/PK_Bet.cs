using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokerGameServer
{
    public enum BET : int
    {
        DIE = 0,
        CHECK = 1,
        CALL = 2,
        PPING = 3,
        DDADANG = 4,
        HALF = 5,
        ALLIN = 6,
        LASTCALL = 7,
        SIDE = 8,
    }


    public class PK_Bet
    {

        private Int64 totalMoney = 0;
        private Int64 pandonMoney = 0;
        private Int64 bettingMoney = 0;
        private Int64 beforeMoney = 0;
        private Int64 allInMoney = 0;

        public short MyBet { get; set; }
        public Int64 getTotalMoney { get { return this.totalMoney; } }
        public Int64 getCallMoney { get { return this.bettingMoney; } set { this.bettingMoney = value; } }
        public Int64 getBeforeMoney { get { return this.beforeMoney; } set { this.beforeMoney = value; } }
        public Int64 getPandonMoney { get { return this.pandonMoney; } }
        public Int64 getAllinMoney { get { return this.allInMoney; } }

        public Dictionary<int, string> eachUserMoney;
        public Dictionary<int, string> eachUserId;
        public Dictionary<int, int> eachUserWin;
        public Dictionary<int, int> eachUserLose;
        public Dictionary<int, Int64> eachUserAccumulateMoney;
        public Dictionary<int, Int64> sideMoney;

        public PK_Bet(Int64 pandon)
        {
            this.pandonMoney = pandon;
            this.eachUserMoney = new Dictionary<int, string>();
            this.eachUserId = new Dictionary<int, string>();
            this.eachUserWin = new Dictionary<int, int>();
            this.eachUserLose = new Dictionary<int, int>();
            this.eachUserAccumulateMoney = new Dictionary<int, Int64>();
            this.sideMoney = new Dictionary<int, Int64>();

            this.beforeMoney = this.pandonMoney; //이전 사용자가 콜 했던 금액. -> 초기에는 판돈 머니를 넣는게 맞지.
            this.bettingMoney = this.pandonMoney; //사용자마다 배팅할 금액.

        }
        public string GetEachUserMoney(int key)
        {
            if (!this.eachUserMoney.ContainsKey(key))
            {
                return null;
            }
            return this.eachUserMoney[key];
        }

        public int GetEachUserWin(int key)
        {
            return this.eachUserWin[key];
        }
        public int GetEachUserLose(int key)
        {
            return this.eachUserLose[key];
        }
        public void SetBeforeMoney(Int64 inputMoney)
        {
            this.beforeMoney = inputMoney;
        }

        public void SetPandonMoney(Int64 playingUserCount)
        {
            this.totalMoney = pandonMoney * playingUserCount;

            foreach (KeyValuePair<int, string> kvp in this.eachUserMoney.ToList())
            {
                Int64 updateMoney = Int64.Parse(this.eachUserMoney[kvp.Key]) - pandonMoney;
                this.eachUserMoney[kvp.Key] = updateMoney.ToString();
            }
        }

        public void CallEvent(int key)
        {

            string output;
            this.eachUserMoney.TryGetValue(key, out output);
            this.bettingMoney = (this.eachUserAccumulateMoney.Values.Max() - this.eachUserAccumulateMoney[key]);
            this.totalMoney += bettingMoney;
            this.eachUserAccumulateMoney[key] += this.bettingMoney;
            SetBeforeMoney(this.eachUserAccumulateMoney[key]);
            this.eachUserMoney[key] = (Int64.Parse(output) - this.bettingMoney).ToString();

        }

        public void LastCallEvent(int key)
        {
            string output;
            this.eachUserMoney.TryGetValue(key, out output);
            Int64 money = this.eachUserAccumulateMoney.Values.Max() - this.eachUserAccumulateMoney[key];
            this.eachUserMoney[key] = (Int64.Parse(output) - money).ToString();
            this.bettingMoney = money;
            this.eachUserAccumulateMoney[key] += money;
            this.totalMoney += money;
            SetBeforeMoney(money);
        }

        public void HalfEvent(int key)
        {
            string output;
            this.eachUserMoney.TryGetValue(key, out output);
            this.bettingMoney = (this.beforeMoney - this.eachUserAccumulateMoney[key]) + (this.totalMoney / 2);
            this.totalMoney += bettingMoney;
            this.eachUserAccumulateMoney[key] += this.bettingMoney;
            SetBeforeMoney(this.eachUserAccumulateMoney[key]);
            this.eachUserMoney[key] = (Int64.Parse(output) - this.bettingMoney).ToString();

        }

        public void DDadangEvent(int key)
        {

            string output;
            this.eachUserMoney.TryGetValue(key, out output);
            this.bettingMoney = (this.beforeMoney - this.eachUserAccumulateMoney[key]) + (this.bettingMoney * 2);
            this.totalMoney += bettingMoney;
            this.eachUserAccumulateMoney[key] += this.bettingMoney;
            SetBeforeMoney(this.eachUserAccumulateMoney[key]);
            this.eachUserMoney[key] = (Int64.Parse(output) - this.bettingMoney).ToString();

        }

        public void CheckEvent(int key)
        {
            this.totalMoney += 0;
        }

        public void DieEvent(int key)
        {
            this.totalMoney += 0;
        }

        public void PPingEvent(int key) //무조건 선만.
        {
            string output;
            this.eachUserMoney.TryGetValue(key, out output);
            this.bettingMoney = this.pandonMoney;
            this.totalMoney += this.bettingMoney;
            this.eachUserAccumulateMoney[key] += this.bettingMoney;
            SetBeforeMoney(this.eachUserAccumulateMoney[key]);
            this.eachUserMoney[key] = (Int64.Parse(output) - this.bettingMoney).ToString();
        }

        public void SideBettingEvent(int key)
        {
            this.totalMoney += 0;
        }

        public void AllinEvent(int key)
        {
            string output;
            Int64 spareMoney = int.Parse(this.eachUserMoney[key]);
            this.eachUserMoney.TryGetValue(key, out output);
            this.eachUserMoney[key] = (Int64.Parse(output) - Int64.Parse(output)).ToString();
            this.allInMoney = spareMoney;
            this.totalMoney += spareMoney;
            this.eachUserAccumulateMoney[key] += spareMoney;
            SetBeforeMoney(this.bettingMoney);
        }

        public void InitAccumlateMoney()
        {
            foreach (KeyValuePair<int, Int64> kvp in this.eachUserAccumulateMoney.ToList())
            {
                this.eachUserAccumulateMoney[kvp.Key] = 0;
            }
        }
        public bool IsBettingPossible(int user)
        {
            string output;
            bool hasBool = eachUserMoney.TryGetValue(user, out output);
            if (hasBool)
            {
                if (int.Parse(output) < 0)
                {
                    return false;
                }
                else
                {
                    return true;
                }

            }
            return false;

        }

        public void UpDateWinLoseRate(int winnerIndex)
        {
            foreach (KeyValuePair<int, int> kvp in this.eachUserWin.ToList())
            {
                if (kvp.Key == winnerIndex)
                {
                    this.eachUserWin[winnerIndex] += 1;
                }
            }
            foreach (KeyValuePair<int, int> kvp in this.eachUserLose.ToList())
            {
                if (kvp.Key != winnerIndex)
                {
                    this.eachUserLose[kvp.Key] += 1;
                }
            }
        }

        public void removeItem(int key)
        {
            this.eachUserMoney.Remove(key);
            this.eachUserId.Remove(key);
            this.eachUserWin.Remove(key);
            this.eachUserLose.Remove(key);
        }
        public void Destroy()
        {

            GC.SuppressFinalize(this);
        }
    }
}

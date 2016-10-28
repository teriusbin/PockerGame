using System;
using System.Collections.Generic;
using System.Collections;

using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokerGameServer
{
    public class PK_Card
    {

        // 모양은 <♣, ♥, ◆, ♠>의 순
        public enum SHAPE : short
        {
            SPADES = 3,
            DIAMONDS = 2,
            HEARTS = 1,
            CLUBS = 0
        }

        public enum VALUE : short
        {
            TWO = 2,
            THREE = 3,
            FOUR = 4,
            FIVE =5,
            SIX = 6,
            SEVEN = 7,
            EIGHT = 8,
            NINE = 9,
            TEN = 10,
            JACK = 11,
            QUEEN = 12,
            KING = 13,
            ACE =14
        }

      

        public short MyShape { get; set; }
        public short MyValue { get; set; }

        public bool isBack;
        public bool isMade;


    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokerGameServer
{
    public class PK_Deck : PK_Card
    {

        const int NUM_OF_TOTAL_CARDS = 52;
        const int NUM_OF_SHAPE = 4;
        const int NUM_OF_CARDS = 15;

        private PK_Card[] deck = null;
        private PK_Card[] initDeck = null;

        public PK_Deck()
        {
            deck = new PK_Card[NUM_OF_TOTAL_CARDS];
            initDeck = new PK_Card[NUM_OF_TOTAL_CARDS];
        }

        public PK_Card[] getDeck { get { return deck; } } //getg curren Deck

     
        public void setUpDeck()
        { 
            try
            {
                for (int i = 0; i < NUM_OF_SHAPE; i++)
                {
                    for (int j = 2; j < NUM_OF_CARDS; j++)
                    {
                        deck[i * (NUM_OF_CARDS - 2) + (j - 2)] = new PK_Card { MyShape = (short)i, MyValue = (short)j };
                        initDeck[i * (NUM_OF_CARDS - 2) + (j - 2)] = new PK_Card { MyShape = (short)i, MyValue = (short)j };
                    }
                }
            }
            catch (System.Exception e)
            {
                Console.WriteLine(e.Message);  // 예외의 메시지를 출력
                Console.WriteLine(e.StackTrace);
            }
            ShuffleCads();
        }

        public PK_Card getCard(int index)
        {
            PK_Card Card = initDeck[index];
            return Card;
        }
       
        public void ShuffleCads()
        {
            System.Random rand = new System.Random();

            PK_Card temp;

            for (int i = deck.Length - 1; i > 0; i--)
            {
                int cardIndex = rand.Next(51);
                temp = deck[cardIndex];
                deck[cardIndex] = deck[i];
                deck[i] = temp;
            }


        }
    }
}

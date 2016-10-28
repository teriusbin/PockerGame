using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace PokerGameServer
{
    public class PK_Dealer : PK_Deck
    {
        const int MAX_USER_COUNT = 5;

        private int deckIndex;

        private Dictionary<int, List<PK_Card>> beforeSelectedHandList;
        private Dictionary<int, List<PK_Card>> beforeSelectedEachUserHandList;

        private Dictionary<int, List<PK_Card>> afterSelectedPlayerTotalHandList = new Dictionary<int, List<PK_Card>>();
        private Dictionary<int, List<PK_Card>> afterSelectedPlayerOpenHandList = new Dictionary<int, List<PK_Card>>();
 
        public PK_CardEstimate playerHandEvaluator;
        public PK_CardEstimate playerTotalHandEvaluator;


        public List<PK_Card> GetTempEachUserHandList(int key)
        {
     
            if (beforeSelectedEachUserHandList.ContainsKey(key)) //key 가 있으면
            {
                return this.beforeSelectedEachUserHandList[key];
            }
            else //키가 없으면.
            {
                List<PK_Card> temp = new List<PK_Card>();
                this.beforeSelectedEachUserHandList.Add(key, temp);
                return this.beforeSelectedEachUserHandList[key];
            }
            
        }

        public List<PK_Card> GetAfterSelectedPlayerHandList(int key)
        {
            if (afterSelectedPlayerTotalHandList.ContainsKey(key)) //key 가 있으면
            {
                return this.afterSelectedPlayerTotalHandList[key];
            }
            else //키가 없으면.
            {
                List<PK_Card> temp = new List<PK_Card>();
                this.afterSelectedPlayerTotalHandList.Add(key, temp);
                return this.afterSelectedPlayerTotalHandList[key];
            }
        }

        public PK_Dealer()
        {
            this.deckIndex = 0;

            this.beforeSelectedHandList = new Dictionary<int, List<PK_Card>>();
            this.beforeSelectedEachUserHandList = new Dictionary<int, List<PK_Card>>();

            this.playerHandEvaluator = new PK_CardEstimate();
            this.playerTotalHandEvaluator = new PK_CardEstimate();
        }

       
        public PK_Card GetHand()
        {
            return getDeck[deckIndex++];
        }

        public int RemoveTempHand(int key, PK_Card card)
        {

            for (int i = 0; i < beforeSelectedHandList[key].Count; i++)
            {
                if (beforeSelectedHandList[key][i].MyShape == card.MyShape &&
                    beforeSelectedHandList[key][i].MyValue == card.MyValue)
                {
                    this.beforeSelectedHandList[key].RemoveAt(i);
                    return i;
                }

            }
            return -1;
        }

        public void CopyTempHandToPlyerTotalHand(int key)
        {
            try
            {
                if (!this.afterSelectedPlayerTotalHandList.ContainsKey(key)) //key 가 있으면
                {
                    List<PK_Card> tempCard = new List<PK_Card>();
                    this.afterSelectedPlayerTotalHandList.Add(key, tempCard);
            
                    for (int i = 0; i < 3; i++)
                        this.afterSelectedPlayerTotalHandList[key].Add(this.beforeSelectedHandList[key][i]);
                }

            }
            catch (System.Exception e)
            {
                Console.WriteLine(e.Message);  // 예외의 메시지를 출력
                Console.WriteLine(e.StackTrace);
            }
        }

        public void CopyTempHandToPlyerOpenHand(int key, PK_Card openCard)
        {
            try
            {
                if (this.afterSelectedPlayerOpenHandList.ContainsKey(key)) //key 가 있으면
                {
                    this.afterSelectedPlayerOpenHandList[key].Add(openCard);
                    //return this.beforeSelectedEachUserHandList[key];
                }
                else //키가 없으면.
                {
                    List<PK_Card> temp = new List<PK_Card>();
                    this.afterSelectedPlayerOpenHandList.Add(key, temp);
                    this.afterSelectedPlayerOpenHandList[key].Add(openCard);
                }
            }
            catch (System.Exception e)
            {
                Console.WriteLine(e.Message);  // 예외의 메시지를 출력
                Console.WriteLine(e.StackTrace);
            }
        }


        public void SetTempHand() //
        {
            try
            {
                foreach (KeyValuePair<int, List<PK_Card>> kvp in this.beforeSelectedEachUserHandList.ToList())
                {
                    this.beforeSelectedHandList.Add(kvp.Key, kvp.Value);
                }

            }
            catch (System.Exception e)
            {
                Console.WriteLine(e.Message);  // 예외의 메시지를 출력
                Console.WriteLine(e.StackTrace);
            }

        }

        public void SetTotalHand(int key, int gu)
        {
            try
            {
                this.afterSelectedPlayerTotalHandList[key].Add(getDeck[deckIndex - 1]);
            }
            catch (System.Exception e)
            {
                Console.WriteLine(e.Message);  // 예외의 메시지를 출력
                Console.WriteLine(e.StackTrace);
            }

        }

        public void SetOpenHand(int key, int gu)
        {
            try
            {
                this.afterSelectedPlayerOpenHandList[key].Add(getDeck[deckIndex - 1]);
            }
            catch (System.Exception e)
            {
                Console.WriteLine(e.Message);  // 예외의 메시지를 출력
                Console.WriteLine(e.StackTrace);
            }

        }
        public void InitEstimateObj(int playerIndex)
        {
            this.playerHandEvaluator.InitPK_HandEstimates(playerIndex);
            this.playerTotalHandEvaluator.InitPK_HandEstimates(playerIndex);
        }

        public CardInfo GetEachUserTotalHandInfo(int key, int gu)
        {

            if (this.afterSelectedPlayerTotalHandList.ContainsKey(key)) //key 가 있으면
            {
                PK_Card popCard = this.afterSelectedPlayerTotalHandList[key][gu];

                CardInfo temp = this.playerTotalHandEvaluator.CheckMyHandOpenCard(key, popCard, gu);

                return temp;
            }
            else //키가 없으면.
            {
                return null;
                
            }
    
        }
        public CardInfo GetEachUserOpenHandInfo(int key, int gu)
        {

            PK_Card popCard = this.afterSelectedPlayerOpenHandList[key][gu];

            CardInfo temp = this.playerHandEvaluator.CheckMyHandOpenCard(key, popCard, gu);

            return temp;

        }

        public void DebugDisplayCard(short shape, short num)
        {

            if (shape == (short)SHAPE.CLUBS)
            {
                Console.Write("♣" + num + "  ");
            }
            else if (shape == (short)SHAPE.SPADES)
            {
                Console.Write("♠" + num + "  ");
            }
            else if (shape == (short)SHAPE.HEARTS)
            {
                Console.Write("♥" + num + "  ");
            }
            else if (shape == (short)SHAPE.DIAMONDS)
            {
                Console.Write("◆" + num + "  ");
            }
            Console.WriteLine("");
        }

        public void DisplayCard()
        {

            for (int i = 0; i < afterSelectedPlayerTotalHandList.Count; i++)
            {
                Console.Write("user " + i + "는 ");
             
                for (int j = 0; j < afterSelectedPlayerTotalHandList[i].Count; j++)
                {
                    if (afterSelectedPlayerTotalHandList[i][j].MyShape == (short)SHAPE.CLUBS)
                    {
                        Console.Write("♣" + afterSelectedPlayerTotalHandList[i][j].MyValue + "  ");
                    }
                    else if (afterSelectedPlayerTotalHandList[i][j].MyShape == (short)SHAPE.SPADES)
                    {
                        Console.Write("♠" + afterSelectedPlayerTotalHandList[i][j].MyValue + "  ");
                    }
                    else if (afterSelectedPlayerTotalHandList[i][j].MyShape == (short)SHAPE.HEARTS)
                    {
                        Console.Write("♥" + afterSelectedPlayerTotalHandList[i][j].MyValue + "  ");
                    }
                    else if (afterSelectedPlayerTotalHandList[i][j].MyShape == (short)SHAPE.DIAMONDS)
                    {
                        Console.Write("◆" + afterSelectedPlayerTotalHandList[i][j].MyValue + "  ");
                    }

                }
                Console.WriteLine("");
            }

        }
        public void Destroy()
        {
            GC.SuppressFinalize(this);
        }

        /*기존 족보판별*/

        //public void sortCards()
        //{
        //    for (int i = 0; i < afterSelectedPlayerTotalHandList.Count; i++)
        //    {
        //        var suitPlayer = from hand in afterSelectedPlayerTotalHandList[i]
        //                         orderby hand.MyShape
        //                         select hand;

        //        var index = 0;
        //        foreach (var element in suitPlayer.ToList())
        //        {
        //            sortedPlayerHandList[i][index] = element;
        //            index++;
        //        }

        //        var valuePlayer = from hand in afterSelectedPlayerTotalHandList[i]
        //                          orderby hand.MyValue
        //                          select hand;

        //        index = 0;
        //        foreach (var element in valuePlayer.ToList())
        //        {
        //            sortedPlayerHandList[i][index] = element;
        //            index++;
        //        }
        //    }
        //}

        //private int evaluateWinner()
        //{
        //    int winnerIndex = 0;
        //    int loserIndex = 0;
        //    Console.WriteLine("");
        //    for (int i = 1; i < afterSelectedPlayerTotalHandList.Count; i++)
        //    {

        //        if (playerResultBook[winnerIndex].getHand > playerResultBook[i].getHand)  //족보 만으로 비교
        //        {
        //            loserIndex = i;
        //            Console.WriteLine("winner is " + winnerIndex + " " +
        //                playerResultBook[winnerIndex].getHand + " loser is " +
        //                playerResultBook[loserIndex].getHand);

        //            break;

        //        }
        //        else if (playerResultBook[winnerIndex].getHand < playerResultBook[i].getHand)  //족보 만으로 비교
        //        {
        //            loserIndex = winnerIndex;
        //            winnerIndex = i;
        //            Console.WriteLine("winner is " + winnerIndex + " " +
        //                playerResultBook[winnerIndex].getHand + " loser is " +
        //                playerResultBook[loserIndex].getHand);

        //            break;
        //        }
        //        else //족보가 같은 경우
        //        {

        //            if (playerResultBook[winnerIndex].HandValues.HighCard > playerResultBook[i].HandValues.HighCard) //높은 카드
        //            {
        //                loserIndex = i;
        //                Console.WriteLine("sameHand " + playerResultBook[winnerIndex].getHand +
        //                    " winner is " + winnerIndex + " " + playerResultBook[winnerIndex].HandValues.HighCard +
        //                    " loser is " + playerResultBook[loserIndex].HandValues.HighCard);
        //                break;
        //            }

        //            else if (playerResultBook[winnerIndex].HandValues.HighCard < playerResultBook[i].HandValues.HighCard) //높은 카드
        //            {
        //                loserIndex = winnerIndex;
        //                winnerIndex = i;
        //                Console.WriteLine("sameHand " + playerResultBook[winnerIndex].getHand +
        //                    " winner is " + winnerIndex + " " + playerResultBook[winnerIndex].HandValues.HighCard +
        //                    " loser is " + playerResultBook[loserIndex].HandValues.HighCard);
        //                break;
        //            }

        //            else// 높은카드도 같은 경우
        //            {

        //                if (playerResultBook[winnerIndex].HandValues.HighShape > playerResultBook[i].HandValues.HighShape)
        //                {

        //                    loserIndex = i;
        //                    Console.WriteLine("sameHand " + playerResultBook[winnerIndex].getHand +
        //                        " winner is " + winnerIndex + " " + playerResultBook[winnerIndex].HandValues.HighShape +
        //                        " loser is " + playerResultBook[loserIndex].HandValues.HighShape);
        //                    break;
        //                }
        //                else if (playerResultBook[winnerIndex].HandValues.HighShape < playerResultBook[i].HandValues.HighShape)
        //                {
        //                    loserIndex = winnerIndex;
        //                    winnerIndex = i;
        //                    Console.WriteLine("sameHand " + playerResultBook[winnerIndex].getHand +
        //                        " winner is " + winnerIndex + " " + playerResultBook[winnerIndex].HandValues.HighShape +
        //                        " loser is " + playerResultBook[loserIndex].HandValues.HighShape);
        //                    break;
        //                }
        //                else
        //                {
        //                    Console.WriteLine("이런 상황은 안생겨야 한다");
        //                }
        //            }

        //        }

        //    }
        //    return winnerIndex;
        //}

        //    private void CompareMyCards(HAND playerHand, PK_CardEstimate playerHandEvaluator)
        //    {
        //        if (playerHand > maxHand) //족보가 다른 경우
        //        {
        //            maxHand = playerHand;
        //            maxHighCard = playerHandEvaluator.HandValues.HighCard;
        //            maxHighShape = playerHandEvaluator.HandValues.HighShape;
        //        }
        //        else //족보가 같은 경우
        //        {
        //            if (playerHandEvaluator.HandValues.HighCard > maxHighCard) //높은 카드
        //            {
        //                maxHighCard = playerHandEvaluator.HandValues.HighCard;
        //            }
        //            else
        //            {
        //                if (playerHandEvaluator.HandValues.HighShape > maxHighShape) //높은 모양
        //                {
        //                    maxHighShape = playerHandEvaluator.HandValues.HighShape;
        //                }
        //            }
        //        }

        //    }
        //    public int EvaluateHands()
        //    {
        //        try
        //        {
        //            for (int i = 0; i < afterSelectedPlayerTotalHandList.Count; i++) //각가의 사용자를 비교
        //            {
        //                PK_CardEstimate playerHandEvaluator = new PK_CardEstimate(sortedPlayerHandList[i]);
        //                HAND playerHand = HAND.Nothig;
        //                maxInfo();

        //                for (int start = 0; start < 3; start++)  //사용자의 패를 비교
        //                {
        //                    playerHandEvaluator.ResetHandInfo();

        //                    playerHandEvaluator.CopyTo5Cards(start);
        //                    playerHand = playerHandEvaluator.EvaluateHand();

        //                    CompareMyCards(playerHand, playerHandEvaluator);

        //                }

        //                playerHandEvaluator.GetHighCard = maxHighCard;
        //                playerHandEvaluator.GetHighShape = maxHighShape;
        //                playerHand = maxHand;

        //                playerResultBook[i] = playerHandEvaluator;
        //                playerResultBook[i].getHand = playerHand;
        //            }
        //        }
        //        catch (System.Exception e)
        //        {
        //            Console.WriteLine(e.Message);  // 예외의 메시지를 출력
        //            Console.WriteLine(e.StackTrace);
        //        }

        //        return evaluateWinner();
        //    }

    }
}

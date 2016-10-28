using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokerGameServer
{
    public enum HAND
    {
        no,
        Nothig,
        OnePair,
        TwoPairs,
        ThreeKind,
        Straight,
        Flush,
        FullHouse,
        FourKind,
        StraightFlush,
        RoyalStraightFlush

    }
    public class UserInfo
    {
        public HAND maxOpenHand { get; set; }
        public int maxOpenHighCard { get; set; }
        public int maxOpenHighShape { get; set; }

        public HAND maxTotalHand { get; set; }
        public int maxTotalHighCard { get; set; }
        public int maxTotalHighShape { get; set; }
    }

    public struct HandValue
    {
        public int Total { get; set; }
        public int HighCard { get; set; }
        public PK_Card.SHAPE HighShape { get; set; }
    }

    public class CardInfo
    {

        public int heartsSum { get; set; }
        public int diamondsSum { get; set; }
        public int clubSum { get; set; }
        public int spadesSum { get; set; }

        public HAND maxHand { get; set; }
        public int maxHighCard { get; set; }
        public int maxHighShape { get; set; }

        public HAND beforeMaxHand { get; set; }
        public int beforeMaxHighCard { get; set; }
        public int beforeMaxHighShape { get; set; }


        public int[] handCardNumber;
        public int[] handCardShape;

        public bool onePairFlag;
        public bool twoPairFlag;
        public bool threePairFlag;

    };

    public class PK_CardEstimate : PK_Card
    {

        const int MAX_CARD_NUMBER = 7;
        const int CONFIRM_CARD_NUMBER = 5;

        private Dictionary<int, CardInfo> curHandInfo;
        public int maxCard;

        public PK_CardEstimate()
        {
            this.curHandInfo = new Dictionary<int, CardInfo>();
        }

        public void InitPK_HandEstimates(int playerIndex)
        {
            CardInfo temp = new CardInfo();

            temp.heartsSum = 0;
            temp.diamondsSum = 0;
            temp.clubSum = 0;
            temp.spadesSum = 0;

            temp.maxHand = HAND.Nothig;
            temp.maxHighCard = 0;
            temp.maxHighShape = 0;

            temp.beforeMaxHand = HAND.Nothig;
            temp.beforeMaxHighCard = 0;
            temp.beforeMaxHighShape = 0;

            temp.handCardNumber = new int[15];
            temp.handCardShape = new int[15];
            

            for (int i = 0; i < 15; i++)
            {
                temp.handCardShape[i] = -1;
              
            }

            temp.onePairFlag = false;
            temp.twoPairFlag = false;
            temp.threePairFlag = false;

            this.maxCard = 0;
            this.curHandInfo.Add(playerIndex, temp);

        }


        public int CompareShapeOpenCard(int playerIndex, int cardNum, int cardShape)
        { 
            if (this.curHandInfo[playerIndex].handCardShape[cardNum] < cardShape)
            {
                this.curHandInfo[playerIndex].handCardShape[cardNum] = cardShape;
                return cardShape;
            }

            return this.curHandInfo[playerIndex].handCardShape[cardNum];
        }


        public void CompareCurCardToBeforeCardInOpenCard(int playerIndex, int cardNum, int cardShape, HAND curHand) //새로 들어온 카드 들어왔을 때 어떻게 바뀐느지.
        {

            if (this.curHandInfo[playerIndex].maxHand < curHand)
            {
                this.curHandInfo[playerIndex].beforeMaxHand = this.curHandInfo[playerIndex].maxHand;
                this.curHandInfo[playerIndex].maxHand = curHand;  
               
                if (this.curHandInfo[playerIndex].maxHighCard < cardNum) 
                {
                    this.curHandInfo[playerIndex].beforeMaxHighCard = this.curHandInfo[playerIndex].maxHighCard;
                    this.curHandInfo[playerIndex].maxHighCard = cardNum;

                    this.curHandInfo[playerIndex].beforeMaxHighShape = this.curHandInfo[playerIndex].maxHighShape;
                    CompareShapeOpenCard(playerIndex, cardNum, cardShape);
                    this.curHandInfo[playerIndex].maxHighShape =
                    this.curHandInfo[playerIndex].handCardShape[this.curHandInfo[playerIndex].maxHighCard];
                }
                else if (this.curHandInfo[playerIndex].maxHighCard == cardNum)
                {
                    this.curHandInfo[playerIndex].beforeMaxHighShape = this.curHandInfo[playerIndex].maxHighShape;
                    CompareShapeOpenCard(playerIndex, cardNum, cardShape);
                    this.curHandInfo[playerIndex].maxHighShape =
                    this.curHandInfo[playerIndex].handCardShape[this.curHandInfo[playerIndex].maxHighCard];
                }
                else 
                {
                    this.curHandInfo[playerIndex].beforeMaxHighCard = this.curHandInfo[playerIndex].maxHighCard;
                    this.curHandInfo[playerIndex].maxHighCard = cardNum;
                    CompareShapeOpenCard(playerIndex, cardNum, cardShape);
                    this.curHandInfo[playerIndex].maxHighShape =
                    this.curHandInfo[playerIndex].handCardShape[this.curHandInfo[playerIndex].maxHighCard];

                    if (this.curHandInfo[playerIndex].beforeMaxHand == HAND.OnePair && curHand == HAND.TwoPairs
                        && this.curHandInfo[playerIndex].beforeMaxHighCard > cardNum)
                    {
                        this.curHandInfo[playerIndex].maxHighCard = this.curHandInfo[playerIndex].beforeMaxHighCard;
                        CompareShapeOpenCard(playerIndex, cardNum, cardShape);
                        this.curHandInfo[playerIndex].maxHighShape =
                        this.curHandInfo[playerIndex].handCardShape[this.curHandInfo[playerIndex].maxHighCard];
                    }
             
                }

            }
            else if (this.curHandInfo[playerIndex].maxHand == curHand)  
            {
                if (this.curHandInfo[playerIndex].maxHighCard < cardNum)
                {
                    this.curHandInfo[playerIndex].maxHighCard = cardNum;

                    CompareShapeOpenCard(playerIndex, cardNum, cardShape);
                    this.curHandInfo[playerIndex].maxHighShape =
                    this.curHandInfo[playerIndex].handCardShape[this.curHandInfo[playerIndex].maxHighCard];
                }
                else if(this.curHandInfo[playerIndex].maxHighCard == cardNum)
                {
                    CompareShapeOpenCard(playerIndex, cardNum, cardShape);
                    this.curHandInfo[playerIndex].maxHighShape =
                    this.curHandInfo[playerIndex].handCardShape[this.curHandInfo[playerIndex].maxHighCard];
                }
                
            }
            else  //족보가 작으면 그냥 모냥만 넣어주자.
            {
                CompareShapeOpenCard(playerIndex, cardNum, cardShape);
                this.curHandInfo[playerIndex].maxHighShape =
                this.curHandInfo[playerIndex].handCardShape[this.curHandInfo[playerIndex].maxHighCard];
            }
        }

        public CardInfo CountShapeNumber(CardInfo myCardInfo, PK_Card popCard)
        {

            if (popCard.MyShape == (short)PK_Card.SHAPE.HEARTS)
                myCardInfo.heartsSum++;
            else if (popCard.MyShape == (short)PK_Card.SHAPE.DIAMONDS)
                myCardInfo.diamondsSum++;
            else if (popCard.MyShape == (short)PK_Card.SHAPE.CLUBS)
                myCardInfo.clubSum++;
            else if (popCard.MyShape == (short)PK_Card.SHAPE.SPADES)
                myCardInfo.spadesSum++;

            return myCardInfo;
        }

        private bool Flush(int playerIndex)
        {
            if (this.curHandInfo[playerIndex].heartsSum >= 5)
            {
                this.curHandInfo[playerIndex].maxHighShape = (int)PK_Card.SHAPE.HEARTS;

                return true;
            }
            else if (this.curHandInfo[playerIndex].diamondsSum >= 5)
            {
                this.curHandInfo[playerIndex].maxHighShape = (int)PK_Card.SHAPE.DIAMONDS;

                return true;
            }
            else if (this.curHandInfo[playerIndex].clubSum >= 5)
            {
                this.curHandInfo[playerIndex].maxHighShape = (int)PK_Card.SHAPE.CLUBS;

                return true;
            }
            else if (this.curHandInfo[playerIndex].spadesSum >= 5)
            {
                this.curHandInfo[playerIndex].maxHighShape = (int)PK_Card.SHAPE.SPADES;
                return true;
            }
            return false;
        }

        private bool Straight(int playerIndex)
        {
            for (int i = 2; i + 5 < 15; i++)
            {
                if (this.curHandInfo[playerIndex].handCardNumber[i + 0] == 1 &&
                    this.curHandInfo[playerIndex].handCardNumber[i + 1] == 1 &&
                    this.curHandInfo[playerIndex].handCardNumber[i + 2] == 1 &&
                    this.curHandInfo[playerIndex].handCardNumber[i + 3] == 1 &&
                    this.curHandInfo[playerIndex].handCardNumber[i + 4] == 1)
                {
                    if (this.maxCard < i + 4)
                    {
                        this.maxCard = i + 4;
                        return true;
                    }
                }

            }

            return false;

        }

        private bool StraightFlush(int playerIndex)
        {
            for (int i = 2; i + 5 < 15; i++)
            {
                if (this.curHandInfo[playerIndex].handCardNumber[i + 0] == 1 &&  //숫자
                    this.curHandInfo[playerIndex].handCardNumber[i + 1] == 1 &&
                    this.curHandInfo[playerIndex].handCardNumber[i + 2] == 1 &&
                    this.curHandInfo[playerIndex].handCardNumber[i + 3] == 1 &&
                    this.curHandInfo[playerIndex].handCardNumber[i + 4] == 1 &&

                    this.curHandInfo[playerIndex].handCardShape[i + 0] ==  //모양
                    this.curHandInfo[playerIndex].handCardShape[i + 1] &&
                    this.curHandInfo[playerIndex].handCardShape[i + 1] ==
                    this.curHandInfo[playerIndex].handCardShape[i + 2] &&
                    this.curHandInfo[playerIndex].handCardShape[i + 2] ==
                    this.curHandInfo[playerIndex].handCardShape[i + 3] &&
                    this.curHandInfo[playerIndex].handCardShape[i + 3] ==
                    this.curHandInfo[playerIndex].handCardShape[i + 4])
                {
                    if (this.maxCard < i + 4)
                    {
                        this.maxCard = i + 4;
                        return true;
                    }
                }
            }
            return false;
        }

        private bool RoyalStraightFlush(int playerIndex)
        {
            if (Flush(playerIndex))
            {
                if (this.curHandInfo[playerIndex].handCardNumber[10] >= 1 && //숫자
                    this.curHandInfo[playerIndex].handCardNumber[11] >= 1 &&
                    this.curHandInfo[playerIndex].handCardNumber[12] >= 1 &&
                    this.curHandInfo[playerIndex].handCardNumber[13] >= 1 &&
                    this.curHandInfo[playerIndex].handCardNumber[14] >= 1 &&

                    this.curHandInfo[playerIndex].handCardShape[10] ==  //모양
                    this.curHandInfo[playerIndex].handCardShape[11] &&
                    this.curHandInfo[playerIndex].handCardShape[11] ==
                    this.curHandInfo[playerIndex].handCardShape[12] &&
                    this.curHandInfo[playerIndex].handCardShape[12] ==
                    this.curHandInfo[playerIndex].handCardShape[13] &&
                    this.curHandInfo[playerIndex].handCardShape[14] ==
                    this.curHandInfo[playerIndex].handCardShape[15])
                {
                    this.maxCard = 14;
                    return true;
                }

            }
            return false;

        }
        public int SearchIndexForFlushSituation(int playerIndex)
        {
            int max = -1;
            for (int i = 2; i < 15; i++)
            {
                if (this.curHandInfo[playerIndex].handCardShape[i] == this.curHandInfo[playerIndex].maxHighShape)
                {
                    if (max < i) max = i;
                }
            }
            return max;
        }


        public CardInfo CheckMyHandOpenCard(int playerIndex, PK_Card popCard, int curGu)
        {
            this.curHandInfo[playerIndex] = CountShapeNumber(this.curHandInfo[playerIndex], popCard);

            if (this.curHandInfo[playerIndex].handCardNumber[popCard.MyValue] == 0) //초기
            {
                this.curHandInfo[playerIndex].handCardNumber[popCard.MyValue] = 1;
                CompareCurCardToBeforeCardInOpenCard(playerIndex, popCard.MyValue, popCard.MyShape, HAND.Nothig);

                if (curGu > 3) // 5구부터 검사함. 계산량 줄이기.
                {
                    if (RoyalStraightFlush(playerIndex))
                    {
                        int hightShape = this.curHandInfo[playerIndex].handCardShape[this.maxCard];
                        CompareCurCardToBeforeCardInOpenCard(playerIndex, this.maxCard, hightShape, HAND.RoyalStraightFlush);

                    }
                    else if (StraightFlush(playerIndex))
                    {
                        int hightShape = this.curHandInfo[playerIndex].handCardShape[this.maxCard];
                        CompareCurCardToBeforeCardInOpenCard(playerIndex, this.maxCard, hightShape, HAND.StraightFlush);

                    }
                    else if (Flush(playerIndex))
                    {
                        this.curHandInfo[playerIndex].maxHighCard = SearchIndexForFlushSituation(playerIndex);
                        int hightShape = this.curHandInfo[playerIndex].handCardShape[this.curHandInfo[playerIndex].maxHighCard];
                        CompareCurCardToBeforeCardInOpenCard(playerIndex, this.curHandInfo[playerIndex].maxHighCard, hightShape, HAND.Flush);

                    }
                    else if (Straight(playerIndex))
                    {
                        int hightShape = this.curHandInfo[playerIndex].handCardShape[this.maxCard];
                        CompareCurCardToBeforeCardInOpenCard(playerIndex, this.maxCard, hightShape, HAND.Straight);

                    }
                }

            }

            else if (this.curHandInfo[playerIndex].handCardNumber[popCard.MyValue] == 1 && curGu > 0)// 원페어가 되는 경우
            {
                if (this.curHandInfo[playerIndex].onePairFlag) // 원페어기 때문에 투페어가 될 확률이 있다.
                {
                    this.curHandInfo[playerIndex].handCardNumber[popCard.MyValue] = 2; // 투페어가 되는경우
                    CompareCurCardToBeforeCardInOpenCard(playerIndex, popCard.MyValue, popCard.MyShape, HAND.TwoPairs);
                    this.curHandInfo[playerIndex].twoPairFlag = true;
                }
                else
                {
                    this.curHandInfo[playerIndex].handCardNumber[popCard.MyValue] = 2;
                    CompareCurCardToBeforeCardInOpenCard(playerIndex, popCard.MyValue, popCard.MyShape, HAND.OnePair);
                    this.curHandInfo[playerIndex].onePairFlag = true;

                    if (curGu > 3)
                    {
                        if (RoyalStraightFlush(playerIndex))
                        {
                            int hightShape = this.curHandInfo[playerIndex].handCardShape[this.maxCard];
                            CompareCurCardToBeforeCardInOpenCard(playerIndex, this.maxCard, hightShape, HAND.RoyalStraightFlush);

                        }
                        else if (StraightFlush(playerIndex))
                        {
                            int hightShape = this.curHandInfo[playerIndex].handCardShape[this.maxCard];
                            CompareCurCardToBeforeCardInOpenCard(playerIndex, this.maxCard, hightShape, HAND.StraightFlush);

                        }
                        else if (Flush(playerIndex))
                        {
                            this.curHandInfo[playerIndex].maxHighCard = SearchIndexForFlushSituation(playerIndex);
                            int hightShape = this.curHandInfo[playerIndex].handCardShape[this.curHandInfo[playerIndex].maxHighCard];
                            CompareCurCardToBeforeCardInOpenCard(playerIndex, this.curHandInfo[playerIndex].maxHighCard, hightShape, HAND.Flush);
                        }
                        else if (Straight(playerIndex))
                        {
                            int hightShape = this.curHandInfo[playerIndex].handCardShape[this.maxCard];
                            CompareCurCardToBeforeCardInOpenCard(playerIndex, this.maxCard, hightShape, HAND.Straight);


                        }
                    }
                }

            }

            else if (this.curHandInfo[playerIndex].handCardNumber[popCard.MyValue] == 2 && curGu > 1) //트리플이 되는 경우
            {
                if (this.curHandInfo[playerIndex].twoPairFlag) // 투페어가 있기때문에 풀 하우스
                {
                    this.curHandInfo[playerIndex].handCardNumber[popCard.MyValue] = 3;
                    CompareCurCardToBeforeCardInOpenCard(playerIndex, popCard.MyValue, popCard.MyShape, HAND.FullHouse);

                }
                else if (this.curHandInfo[playerIndex].onePairFlag) //원페어가 있기 때문에 쓰리페어
                {
                    this.curHandInfo[playerIndex].handCardNumber[popCard.MyValue] = 3;
                    CompareCurCardToBeforeCardInOpenCard(playerIndex, popCard.MyValue, popCard.MyShape, HAND.ThreeKind);
                    this.curHandInfo[playerIndex].threePairFlag = true;
                }

            }
            else if (this.curHandInfo[playerIndex].handCardNumber[popCard.MyValue] == 3 && curGu > 2)//포카드 되는 경우
            {
                if (this.curHandInfo[playerIndex].threePairFlag) {  //쓰리카드가 있기 때문에
                    this.curHandInfo[playerIndex].handCardNumber[popCard.MyValue] = 4;
                 CompareCurCardToBeforeCardInOpenCard(playerIndex, popCard.MyValue, popCard.MyShape, HAND.FourKind);
            }
        }

            return this.curHandInfo[playerIndex];
        }


        /*기존 족보 판별*/
        //public PK_CardEstimate(PK_Card[] sortedHand)
        //{
        //    this.heartsSum = 0;
        //    this.diamondsSum = 0;
        //    this.clubSum = 0;
        //    this.spadesSum = 0;

        //    this.cards = new PK_Card[CONFIRM_CARD_NUMBER];
        //    this.SevenCards = new PK_Card[MAX_CARD_NUMBER];
        //    this.Cards = sortedHand;
        //    this.handValue = new HandValue();

        //}
        //public void CopyTo5Cards(int start)
        //{
        //    for (int i = 0; i < CONFIRM_CARD_NUMBER; i++)
        //    {
        //        this.cards[i] = SevenCards[start];
        //        start++;
        //    }
        //}

        //public void ResetHandInfo()
        //{
        //    this.heartsSum = 0;
        //    this.diamondsSum = 0;
        //    this.clubSum = 0;
        //    this.spadesSum = 0;
        //}
        //public HandValue HandValues
        //{
        //    get { return handValue; }
        //    set { handValue = value; }
        //}

        //public HAND getHand
        //{
        //    get { return myHand; }
        //    set { myHand = value; }
        //}

        //public int GetHighCard
        //{
        //    get { return this.handValue.HighCard; }
        //    set { this.handValue.HighCard = value; }
        //}

        //public SHAPE GetHighShape
        //{
        //    get { return this.handValue.HighShape; }
        //    set { this.handValue.HighShape = value; }
        //}

        //public PK_Card[] GetFiveCards
        //{
        //    get { return cards; }
        //}

        //public PK_Card[] Cards
        //{
        //    get { return SevenCards; }
        //    set
        //    {
        //        try
        //        {
        //            this.SevenCards[0] = value[0];
        //            this.SevenCards[1] = value[1];
        //            this.SevenCards[2] = value[2];
        //            this.SevenCards[3] = value[3];
        //            this.SevenCards[4] = value[4];
        //            this.SevenCards[5] = value[5];
        //            this.SevenCards[6] = value[6];
        //        }
        //        catch (System.Exception e)
        //        {
        //            Console.WriteLine(e.Message);  // 예외의 메시지를 출력
        //            Console.WriteLine(e.StackTrace);
        //        }
        //    }
        //}

        //public HAND EvaluateHand()
        //{
        //    GetNumberOfSuit();
        //    if (RoyalStraightFlush())
        //        return HAND.RoyalStraightFlush;
        //    else if (StraightFlush())
        //        return HAND.StraightFlush;
        //    else if (FourOfKind())
        //        return HAND.FourKind;
        //    else if (FullHouse())
        //        return HAND.FullHouse;
        //    else if (Flush())
        //        return HAND.Flush;
        //    else if (straigth())
        //        return HAND.Straight;
        //    else if (Triple())
        //        return HAND.ThreeKind;
        //    else if (TwoPairs())
        //        return HAND.TwoPairs;
        //    else if (OnePair())
        //        return HAND.OnePair;


        //    handValue.HighCard = (int)cards[4].MyValue;
        //    handValue.HighShape = (PK_Card.SHAPE)cards[4].MyShape;

        //    return HAND.Nothig;
        //}

        //private void GetNumberOfSuit()
        //{

        //    for (int i = 0; i < cards.Length; i++)
        //    {
        //        if (this.cards[i].MyShape == (short)PK_Card.SHAPE.HEARTS)
        //            this.heartsSum++;
        //        else if (this.cards[i].MyShape == (short)PK_Card.SHAPE.DIAMONDS)
        //            this.diamondsSum++;
        //        else if (this.cards[i].MyShape == (short)PK_Card.SHAPE.CLUBS)
        //            this.clubSum++;
        //        else if (this.cards[i].MyShape == (short)PK_Card.SHAPE.SPADES)
        //            this.spadesSum++;
        //    }

        //}

        //private bool RoyalStraightFlush()
        //{
        //    if (this.heartsSum == 5 || this.diamondsSum == 5 || this.clubSum == 5 || this.spadesSum == 5)
        //    {
        //        if (this.cards[0].MyValue == 10 &&
        //            this.cards[1].MyValue == 11 &&
        //            this.cards[2].MyValue == 12 &&
        //            this.cards[3].MyValue == 13 &&
        //            this.cards[4].MyValue == 14)
        //        {
        //            this.handValue.HighShape = (PK_Card.SHAPE)this.cards[4].MyShape;
        //            this.handValue.HighCard = this.cards[4].MyValue;
        //            return true;
        //        }
        //    }
        //    return false;
        //}


        //private bool StraightFlush()
        //{
        //    if (this.heartsSum == 5 || this.diamondsSum == 5 || this.clubSum == 5 || this.spadesSum == 5)
        //    {
        //        if (this.cards[0].MyValue + 1 == this.cards[1].MyValue &&
        //            this.cards[1].MyValue + 1 == this.cards[2].MyValue &&
        //            this.cards[2].MyValue + 1 == this.cards[3].MyValue &&
        //            this.cards[3].MyValue + 1 == this.cards[4].MyValue)
        //        {
        //            this.handValue.HighShape = (PK_Card.SHAPE)this.cards[4].MyShape;
        //            this.handValue.HighCard = this.cards[4].MyValue;
        //            return true;
        //        }
        //    }
        //    return false;
        //}

        //private bool FourOfKind()
        //{

        //    if (this.cards[0].MyValue == this.cards[1].MyValue &&
        //        this.cards[0].MyValue == this.cards[2].MyValue &&
        //        this.cards[0].MyValue == this.cards[3].MyValue)
        //    {
        //        this.handValue.HighCard = (int)this.cards[3].MyValue;
        //        this.handValue.HighShape = (PK_Card.SHAPE)this.cards[3].MyShape;
        //        return true;
        //    }
        //    else if (this.cards[1].MyValue == this.cards[2].MyValue &&
        //             this.cards[1].MyValue == this.cards[3].MyValue &&
        //             this.cards[1].MyValue == this.cards[4].MyValue)
        //    {
        //        this.handValue.HighCard = (int)this.cards[4].MyValue;
        //        this.handValue.HighShape = (PK_Card.SHAPE)this.cards[4].MyShape;
        //        return true;
        //    }

        //    return false;
        //}

        //private bool FullHouse()
        //{


        //    if (this.cards[0].MyValue == this.cards[1].MyValue &&
        //        this.cards[0].MyValue == this.cards[2].MyValue &&
        //        this.cards[3].MyValue == this.cards[4].MyValue)
        //    {
        //        this.handValue.HighCard = this.cards[2].MyValue;
        //        this.handValue.HighShape = (PK_Card.SHAPE)this.cards[2].MyShape;
        //    }

        //    else if (this.cards[0].MyValue == this.cards[1].MyValue &&
        //            this.cards[2].MyValue == this.cards[3].MyValue &&
        //            this.cards[2].MyValue == this.cards[4].MyValue)
        //    {
        //        this.handValue.HighCard = this.cards[4].MyValue;
        //        this.handValue.HighShape = (PK_Card.SHAPE)this.cards[4].MyShape;
        //        //handValue.HighCard
        //        return true;
        //    }
        //    return false;
        //}

        //private bool Flush()
        //{

        //    if (this.heartsSum == 5 || this.diamondsSum == 5 || this.clubSum == 5 || this.spadesSum == 5)
        //    {

        //        this.handValue.HighCard = (int)this.cards[4].MyValue;
        //        this.handValue.HighShape = (PK_Card.SHAPE)this.cards[4].MyShape;

        //        return true;
        //    }

        //    return false;
        //}

        //private bool straigth()
        //{


        //    if (this.cards[0].MyValue + 1 == this.cards[1].MyValue &&
        //        this.cards[1].MyValue + 1 == this.cards[2].MyValue &&
        //        this.cards[2].MyValue + 1 == this.cards[3].MyValue &&
        //        this.cards[3].MyValue + 1 == this.cards[4].MyValue)
        //    {
        //        this.handValue.HighCard = (int)this.cards[4].MyValue;
        //        this.handValue.HighShape = (PK_Card.SHAPE)this.cards[4].MyShape;
        //        return true;
        //    }
        //    return false;
        //}

        //private bool Triple()
        //{
        //    if ((this.cards[0].MyValue == this.cards[1].MyValue &&
        //        this.cards[0].MyValue == this.cards[2].MyValue))
        //    {

        //        this.handValue.HighCard = (int)this.cards[2].MyValue;
        //        this.handValue.HighShape = (PK_Card.SHAPE)this.cards[2].MyShape;
        //        return true;
        //    }

        //    else if (this.cards[1].MyValue == this.cards[2].MyValue &&
        //        this.cards[1].MyValue == this.cards[3].MyValue)
        //    {
        //        this.handValue.HighCard = (int)this.cards[3].MyValue;
        //        this.handValue.HighShape = (PK_Card.SHAPE)this.cards[3].MyShape;
        //        return true;
        //    }

        //    else if ((this.cards[2].MyValue == this.cards[3].MyValue &&
        //             this.cards[2].MyValue == this.cards[4].MyValue))
        //    {
        //        this.handValue.HighCard = (int)this.cards[4].MyValue;
        //        this.handValue.HighShape = (PK_Card.SHAPE)this.cards[4].MyShape;
        //        return true;

        //    }
        //    return false;
        //}

        //private bool TwoPairs()
        //{

        //    if (this.cards[0].MyValue == this.cards[1].MyValue &&
        //        this.cards[2].MyValue == this.cards[3].MyValue)
        //    {
        //        this.handValue.HighCard = (int)this.cards[3].MyValue;
        //        this.handValue.HighShape = (PK_Card.SHAPE)this.cards[3].MyShape;
        //        return true;
        //    }

        //    else if (this.cards[0].MyValue == this.cards[1].MyValue &&
        //             this.cards[3].MyValue == this.cards[4].MyValue)
        //    {
        //        this.handValue.HighCard = (int)this.cards[4].MyValue;
        //        this.handValue.HighShape = (PK_Card.SHAPE)this.cards[4].MyShape;
        //        return true;
        //    }

        //    else if (this.cards[1].MyValue == this.cards[2].MyValue &&
        //             this.cards[3].MyValue == this.cards[4].MyValue)
        //    {
        //        this.handValue.HighCard = (int)this.cards[4].MyValue;
        //        this.handValue.HighShape = (PK_Card.SHAPE)this.cards[4].MyShape;
        //        return true;
        //    }

        //    return false;
        //}

        //private bool OnePair()
        //{

        //    if (this.cards[0].MyValue == this.cards[1].MyValue)
        //    {
        //        this.handValue.HighCard = (int)this.cards[1].MyValue;
        //        this.handValue.HighShape = (PK_Card.SHAPE)this.cards[1].MyShape;
        //        return true;
        //    }
        //    else if (this.cards[1].MyValue == this.cards[2].MyValue)
        //    {
        //        this.handValue.HighCard = (int)this.cards[2].MyValue;
        //        this.handValue.HighShape = (PK_Card.SHAPE)this.cards[2].MyShape;
        //        return true;
        //    }
        //    else if (this.cards[2].MyValue == this.cards[3].MyValue)
        //    {
        //        this.handValue.HighCard = (int)this.cards[3].MyValue;
        //        this.handValue.HighShape = (PK_Card.SHAPE)this.cards[3].MyShape;
        //        return true;
        //    }
        //    else if (this.cards[3].MyValue == this.cards[4].MyValue)
        //    {
        //        this.handValue.HighCard = (int)this.cards[4].MyValue;
        //        this.handValue.HighShape = (PK_Card.SHAPE)this.cards[4].MyShape;
        //        return true;
        //    }
        //    return false;
        //}
    }
}

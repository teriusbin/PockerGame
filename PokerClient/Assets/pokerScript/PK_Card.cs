using UnityEngine;
using System.Collections;

public class PK_Card : MonoBehaviour
{
   
    public static  sbyte CS_CLOVER = 0;
    public static  sbyte CS_HEART = 1;
    public static  sbyte CS_DIAMOND = 2;
    public static  sbyte CS_SPADE = 3;

    public enum HAND
    {
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

    public enum SUIT
    {
        SPADES = 3,
        DIAMONDS = 2,
        HEARTS =1,
        CLUBS = 0
    }

    public enum VALUE
    {
        TWO = 2,
        THREE = 3,
        FOUR = 4,
        FIVE = 5,
        SIX = 6,
        SEVEN = 7,
        EIGHT = 8,
        NINE = 9,
        TEN = 10,
        JACK = 11,
        QUEEN = 12,
        KING = 13,
        ACE = 14
    }

    public SUIT MySuit { get; set; }
    public VALUE MyValue { get; set; }


    public sbyte no;
    public sbyte shape;

    int y;
    //boolean visible;
    public bool isBack;
    public bool isMade;

 
}
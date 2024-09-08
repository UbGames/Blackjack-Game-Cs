using System;
using System.Collections.Generic;

namespace BlackJack.CardGameFramework
{
    public class Deck
    {
        // Creates a list of cards
        protected List<Card> cards = new List<Card>();

        // Returns the card at the given position
        public Card this[int position] { get { return (Card)cards[position]; } }

        /// <summary>
        /// One complete deck with every face value and suit
        /// </summary>
        public Deck()
        {
            foreach (Suit suit in Enum.GetValues(typeof(Suit)))
            {
                foreach (FaceValue faceVal in Enum.GetValues(typeof(FaceValue)))
                {
                    cards.Add(new Card(suit, faceVal, true));
                }
            }
        }

        /// <summary>
        /// Draws one card and removes it from the deck
        /// </summary>
        /// <returns></returns>
        public Card Draw()
        {
            Card card = cards[0];
            cards.RemoveAt(0);
            return card;
        }

        //public struct PlayingCard
        //{
        //    public int rank;
        //    public int suit;            //face-up?
        //    public bool revealed;
        //    public Image face;
        //    public Image rear;

        //    public short FormIndex;     //hold location on form index
        //    public short CardIndex;     //hold location of card
        //    public string CardVal;      //hold card value
        //    public bool CardFlag;       //true id matched
        //}
        ////used to hold deck of cards for dealer
        //private PlayingCard[] shuffled = new PlayingCard[MAXCARDS];
        //private PlayingCard[] unshuffled = new PlayingCard[MAXCARDS];

        //shuffle the deck
        //shuffled = InsideOutShuffle(unshuffled);

        //private PlayingCard[] InsideOutShuffle(PlayingCard[] deck)
        //{
        //    //Fisher-Yates shuffle algorithm, shuffle the deck of cards
        //    int i = 0;
        //    int j = 0;
        //    PlayingCard[] newShuffled = new PlayingCard[MAXCARDS];
        //    newShuffled[0] = deck[0];
        //    Random sortRandom = new Random();
        //    for (i = 1; i < MAXCARDS; i++)
        //    {
        //        j = Convert.ToInt32(sortRandom.Next(0, i + 1));
        //        newShuffled[i] = newShuffled[j];
        //        newShuffled[j] = deck[i];
        //    }
        //    return newShuffled;
        //}

        public void ShuffleCards()
        {
            //Fisher-Yates shuffle algorithm
            int i = 0;
            int j = 0;
            int MAXCARDS = 52;

            Random sortRandom = new Random();

            for (i = 0; i <= (MAXCARDS - 1); i++)
            {
                j = Convert.ToInt32(sortRandom.Next(0, i + 1));
                var deck = cards[i];
                cards[i] = cards[j];
                cards[j] = deck;
            }
        }


        /// <summary>
        /// Shuffles the cards in the deck
        /// </summary>
        public void Shuffle()
        {
            Random random = new Random();
            for (int i = 0; i < cards.Count; i++)
            {
                int index1 = i;
                int index2 = random.Next(cards.Count);
                SwapCard(index1, index2);
            }
        }

        /// <summary>
        /// Swaps the placement of two cards
        /// </summary>
        /// <param name="index1"></param>
        /// <param name="index2"></param>
        private void SwapCard(int index1, int index2)
        {
            Card card = cards[index1];
            cards[index1] = cards[index2];
            cards[index2] = card;
        }
    }
}

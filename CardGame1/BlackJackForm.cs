using System;
using System.Text;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using BlackJack.CardGameFramework;
using System.Media;

namespace BlackJack
{
    partial class BlackJackForm : Form
    {
        //to expand regions, highlight all then use keys ctrl + M to expand
        //Tools -> Options -> Text Editor -> C# -> Advanced

        #region Fields
        //Creates a new blackjack game with one player and an inital balance set through the settings designer
        private readonly BlackJackGame game = new BlackJackGame(Properties.Settings.Default.InitBalance);
        
        private PictureBox[] playerCards;
        private PictureBox[] dealerCards;
        private bool firstTurn;

        //set value of animation interval, the lower the number the faster the dealing cards
        private readonly int AnimationTimerInterval = 5;
        private bool AnimationDealerDone = false;
        private bool AnimationPlayerDone = false;

        private int PlayerCardLocationX, PlayerCardLocationY;
        private int DealerCardLocationX, DealerCardLocationY;
        private int DeckCardPBLocationX, DeckCardPBLocationY;

        public int MAXCARDS = 52;
        public int CardsInDeck;
        public int CardsInPlayersHand;
        public int CardsInDealersHand;

        private int PlayerHits;
        private int DealerHits;
        //player first 2 cards dealt
        private bool PlayerInitcards;
        //dealer first 2 cards dealt
        private bool DealerInitcards;

        //initial numbers of cards in initial hand
        private readonly int NumberOfCards = 2;
        //how fast to deal cards
        private readonly int DealSpeed = 50;

        //*************************
        //set to true to show hand totals for the player and dealer
        private readonly bool tShowPlayerHandTotals = true;
        //set to false to hide hand totals for dealer when going live.
        private readonly bool tShowDealerHandTotals = false;

        //used for testing card game, displays the number of cards left in deck, number of cards dealt to dealer and player
        //set to true to show labels, set to false to hide labels
        //set to false to hide labels when going live.
        private readonly bool tFlag = false;
        //*************************

        #endregion

        #region Player Card Animation Field(s)
        // Fields needed to control the player card dealing animation
        private Point _startPoint;
        private int _PlayerCardIndex;
        private int _DealerCardIndex;
        private int _a, _b;
        private int _x, _y;
        private int _Increment;
        #endregion

        // the @ which will allow us the use of single slashes instead of using double slashes. 
        // the path is set to the sound folder under the bin folder.
        public const string SoundFileFolder = @"Sounds\";

        //the location to the sound file
        public static object SoundFileLocation;

        //turn sound on/off, 1,0, default off
        private short NSound = 1;

        #region Main Constructor

        /// <summary>
        /// Main constructor for the BlackJackForm.  Initializes components, loads the card skin images, and sets up the new game
        /// </summary>
        public BlackJackForm()
        {
            InitializeComponent();
            LoadCardSkinImages();
            SetUpNewGame();
        }

        #endregion

        #region Game Event Handlers

        private void BlackJackForm_Load(object sender, EventArgs e)
        {
            DoubleBuffered = true;

            AnimationTimerPlayer.Interval = AnimationTimerInterval;
            AnimationTimerPlayer.Tick += new EventHandler(AnimatePlayerCardTimer_Tick);

            AnimationTimerDealer.Interval = AnimationTimerInterval;
            AnimationTimerDealer.Tick += new EventHandler(AnimateDealerCardTimer_Tick);

            DeckCardPB.Location = deckCard3PictureBox.Location;

            // Save the original location of the DeckCardPB pictureBox

            _startPoint = DeckCardPB.Location;
            _x = DeckCardPB.Location.X;
            _y = DeckCardPB.Location.Y;
            _Increment = 0;

            CardsInDeck = MAXCARDS;

            framePictureBox.Hide();

            //cards have not been dealt
            PlayerInitcards = true;
            DealerInitcards = true;

            //NumberOfCards - initial numbers of cards in initial hand
            PlayerHits = NumberOfCards;
            DealerHits = NumberOfCards;

            if (tShowPlayerHandTotals)
            {
                //Show player card totals
                playerTotalLabel.Show();
            }

            if (tShowDealerHandTotals)
            {
                //Show dealer card totals
                dealerTotalLabel.Show();
            }

            //hide labels at start of game
            //Hide player card totals
            playerTotalLabel.Hide();
            //Hide dealer card totals
            dealerTotalLabel.Hide();


            if (tFlag)
            {
                lblNew.Show();
                //lblNew.Text = "";

                lblDeck.Show();
                lblDealer.Show();
                lblPlayer.Show();
                lblCardsInDeck.Show();
                lblCardsInPlayersHand.Show();
                lblCardsInDealersHand.Show();
            }
            else
            {
                lblNew.Hide();
                //lblNew.Text = "";

                lblDeck.Hide();
                lblDealer.Hide();
                lblPlayer.Hide();
                lblCardsInDeck.Hide();
                lblCardsInPlayersHand.Hide();
                lblCardsInDealersHand.Hide();
            }

            lblNew.Text = "";

            //StartCollectingInk((int)Handle); // <=== Added
        }

        /// <summary>
        /// Invoked when the deal button is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DealBtn_Click(object sender, EventArgs e)
        {
            try
            {
                if (tShowPlayerHandTotals)
                {
                    //Show player card totals
                    playerTotalLabel.Show();
                }
                if (tShowDealerHandTotals)
                {
                    //Show dealer card totals
                    dealerTotalLabel.Show();
                }

                //Creates a new blackjack game with one player and an inital balance set through the settings designer
                //private readonly BlackJackGame game = new BlackJackGame(Properties.Settings.Default.InitBalance);
                //game is a readonly variable of the class BlackJackGame, created in the #region Fields above

                // If the current bet is equal to 0, ask the player to place a bet
                if ((game.CurrentPlayer.Bet == 0) && (game.CurrentPlayer.Balance > 0))
                {
                    //Hide player card totals
                    playerTotalLabel.Hide();
                    MessageBox.Show("You must place a bet before the dealer deals.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    // Place the bet, BlackJackGame.CurrentPlayer { get { return player; } }
                    game.CurrentPlayer.PlaceBet();
                    ShowBankValue();

                    // Clear the table, set up the UI for playing a game, and deal a new game
                    ClearTable();
                    SetUpGameInPlay();

                    //MessageBox.Show("Cards Count: " + CardsInDeck.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    //game.CurrentDec.

                    //call to Shuffle Cards, DealNewGame is in BlackJackGames.cs
                    if (CardsInDeck <= 10 || CardsInDeck == MAXCARDS)
                    {
                        PlaySoundFiles("shuffling_cards");

                        //card delay of 1.0 sec
                        Delay(750);

                        //refers to BlackJackGame.NewCardShuffle()
                        game.NewCardShuffle();


                        CardsInDeck = MAXCARDS;
                        lblCardsInDeck.Text = CardsInDeck.ToString();

                        CardsInPlayersHand = 0;
                        lblCardsInPlayersHand.Text = CardsInPlayersHand.ToString();

                        CardsInDealersHand = 0;
                        lblCardsInDealersHand.Text = CardsInDealersHand.ToString();

                        if (tFlag)
                        {
                            lblNew.Show();
                            lblNew.Text = "New Deck";
                            lblNew.ForeColor = Color.Gold;
                        }
                    }
                    else
                    {
                        lblNew.Hide();
                    }

                    game.DealNewGame();

                    DealerInitcards = true;
                    PlayerInitcards = true;

                    //send over number of cards in initial hand, ie 2
                    for (int i = 1; i <= NumberOfCards; i++)
                    {
                        UpdateUIPlayerCards(i);
                        UpdateUIDealerCardsInt(i);
                    }

                    //second card is dealt
                    DealerInitcards = false;
                    PlayerInitcards = false;

                    // Check see if the current player has blackjack
                    if (game.CurrentPlayer.HasBlackJack())
                    {
                        // calls EndResult GetGameResult() in this form code
                        EndGame(EndResult.PlayerBlackJack);
                    }
                }
            }
            catch (Exception NotEnoughMoneyException)
            {
                MessageBox.Show(NotEnoughMoneyException.Message);
            }
        }

        /// <summary>
        /// Invoked when the player has finished their turn and clicked the stand button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StandBtn_Click(object sender, EventArgs e)
        {
            // Dealer should finish playing and the UI should be updated

            //DealerHits will start at 3, the number of initial cards in hand (2) + 1 if a hit is needed
            DealerHits = 0;
            DealerHits = game.DealerPlay(DealerHits);
            //MessageBox.Show("1DealerHits Stand " + DealerHits.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Information);

            DealerInitcards = false;
            UpdateUIDealerCards(DealerHits);

            // Check who won the game
            EndGame(GetGameResult());
        }

        /// <summary>
        /// Invoked when the hit button is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HitBtn_Click(object sender, EventArgs e)
        {
            // It is no longer the first turn, set this to false so that the cards will all be facing upwards
            firstTurn = false;

            // Hit once and update UI cards
            game.CurrentPlayer.Hit();

            //start at 2
            PlayerHits++;

            //MessageBox.Show("1PlayerHits Hit " + PlayerHits.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        //maybe need to send over number of cards in this function, ie 1
            UpdateUIPlayerCards(PlayerHits);

            // Check to see if player has bust
            if (game.CurrentPlayer.HasBust())
            {
                EndGame(EndResult.PlayerBust);
            }
        }

        /// <summary>
        /// Invoked when the double down button is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DblDwnBtn_Click(object sender, EventArgs e)
        {

            //The double down in blackjack is when you double your bet in the middle of a hand, after which you only receive one more card. 
            //player will receive one card

            try
            {
                // Double the player's bet amount
                game.CurrentPlayer.DoubleDown();

                //start at 2
                PlayerHits++;

                //need to send over number of cards in initial hand + 1 hit, ie 3
                UpdateUIPlayerCards(PlayerHits);

                //MessageBox.Show("2PlayerHits DblDown " + PlayerHits.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Information);

                ShowBankValue();

                //Make sure that the player didn't bust
                if (game.CurrentPlayer.HasBust())
                {
                    EndGame(EndResult.PlayerBust);
                }
                else
                {
                    // Otherwise, let the dealer finish playing

                    DealerHits = 0;
                    DealerHits = game.DealerPlay(DealerHits);
                    //MessageBox.Show("1DealerHits DblDown " + DealerHits.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    //used for testing sending and receiving parameters in function
                    //DealerHits = game.AddTwoNumbers(3, 2);
                    //MessageBox.Show("2DealerHits " + DealerHits.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    DealerInitcards = false;

                    UpdateUIDealerCards(DealerHits);
                    EndGame(GetGameResult());
                }
            }
            catch (Exception NotEnoughMoneyException)
            {
                MessageBox.Show(NotEnoughMoneyException.Message);
            }
        }

        /// <summary>
        /// Refresh the UI to update the initial dealer cards of 2
        /// </summary>
        private void UpdateUIDealerCardsInt(int cardNum)
        {
            //this function will be called twice for the two initial cards

            // Save the original location of the PicBoxDealerCard (card to animate)
            _startPoint = DeckCardPB.Location;

            List<Card> dcards = game.Dealer.Hand.Cards;

            //MessageBox.Show("1DealerCards Count: " + dcards.Count.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Information);
            //MessageBox.Show("1Dealer cardNum " + cardNum, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

            int i = cardNum - 1;

            LoadCard(dealerCards[i], dcards[i]);

            //DealerCard
            DealerCardLocationX = dealerCards[i].Location.X;
            DealerCardLocationY = dealerCards[i].Location.Y;
            //DeckCard
            DeckCardPBLocationX = DeckCardPB.Location.X;
            DeckCardPBLocationY = DeckCardPB.Location.Y;

            AnimationDealerDone = false;
            //MessageBox.Show("Dealer cardNum, i, AnimationDealerDone, DealerInitcards " + cardNum + ", " + i + ", " + AnimationDealerDone + ", " + DealerInitcards, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

            if (AnimationDealerDone == false && i <= 1 && DealerInitcards == true)
            {
                AnimateDealerCard(i);
                //MessageBox.Show("Dealer cardNum, i, AnimationDealerDone " + cardNum + ", " + i + ", " + AnimationDealerDone, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }

            if (tShowDealerHandTotals)
            {
                // Update the value of the hand
                dealerTotalLabel.Text = game.Dealer.Hand.GetSumOfHand().ToString();
            }
        }

        /// <summary>
        /// Refresh the UI to update the dealer cards
        /// </summary>
        private void UpdateUIDealerCards(int cardNum)
        {
            //this function is called form the stand button click and dbldown button click, increment from 2 thru 6

            // Save the original location of the PicBoxDealerCard (card to animate)
            _startPoint = DeckCardPB.Location;

            List<Card> dcards = game.Dealer.Hand.Cards;

            for (int i = 0; i < dcards.Count; i++)
            {
                LoadCard(dealerCards[i], dcards[i]);

                //MessageBox.Show("dealer card x and y " + dealerCards[i].Location.X + " " + dealerCards[i].Location.Y, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                //MessageBox.Show("start card x and y " + DeckCardPB.Location.X + " " + DeckCardPB.Location.Y, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

                //DealerCard
                DealerCardLocationX = dealerCards[i].Location.X;
                DealerCardLocationY = dealerCards[i].Location.Y;
                //DeckCard
                DeckCardPBLocationX = DeckCardPB.Location.X;
                DeckCardPBLocationY = DeckCardPB.Location.Y;

                //MessageBox.Show("Dealer cardNum and i " + cardNum + " " + i, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

                AnimationDealerDone = false;
                //MessageBox.Show("Dealer cardNum, i, AnimationDealerDone, DealerInitcards " + cardNum + ", " + i + ", " + AnimationDealerDone + ", " + DealerInitcards, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

                if (AnimationDealerDone == false && i >= 2 && DealerInitcards == false)
                {
                    //MessageBox.Show("Dealer cardNum, i, AnimationDealerDone " + cardNum + ", " + i + ", " + AnimationDealerDone, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

                    AnimateDealerCard(i);
                    //MessageBox.Show("Dealer cardNum, i, AnimationDealerDone " + cardNum + ", " + i + ", " + AnimationDealerDone, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }

                if (tShowDealerHandTotals)
                {
                    // Update the value of the hand
                    dealerTotalLabel.Text = game.Dealer.Hand.GetSumOfHand().ToString();
                }
            }
        }

        /// <summary>
        ///   Calls inherited class for power info, then takes necessary actions.
        /// </summary>
        /// <remarks></remarks>
        private void AnimateDealerCard(int cardIndex)
        {
            CardsInDeck--;
            CardsInDealersHand++;

            lblCardsInDeck.Text = CardsInDeck.ToString();
            lblCardsInDealersHand.Text = CardsInDealersHand.ToString();

            //MessageBox.Show("1Cards Count: " + CardsInDeck.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Information);

            PlaySoundFiles("card_drop");

            //dealerCards[cardIndex].Hide();
            DeckCardPB.BringToFront();
            DeckCardPB.Location = _startPoint;
            _DealerCardIndex = cardIndex;
            AnimationTimerDealer.Enabled = true;
            while (AnimationTimerDealer.Enabled == true)

                // Wait for animation to finish
                Application.DoEvents();
        }

        /// <summary>
        /// Move card one frame per timer tick
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks></remarks>
        private void AnimateDealerCardTimer_Tick(object sender, EventArgs e)
        {
            if (_Increment <= 500)
            {
                // Player is dealt the card, so use as endpoints player card locations.
                _a = DealerCardLocationX;
                _b = DealerCardLocationY;
            }
            _x = (_a - _startPoint.X) * _Increment / 500 + _startPoint.X;
            _y = (_b - _startPoint.Y) * _Increment / 500 + _startPoint.Y;

            //MessageBox.Show("_startPoint x and y " + _startPoint.X + " " + _startPoint.Y, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            //MessageBox.Show("1_x and _y and _Increment " + _x + " " + _y + " " + _Increment, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

            DeckCardPB.Location = new Point(_x, _y);
            DeckCardPB.Show();
            DeckCardPB.BringToFront();

            if (_x <= (_a + 1) && AnimationDealerDone == false)
            {
                //end animation before top of card is reached
                AnimationDealerDone = true;
                //MessageBox.Show("2_x and _a and _Increment " + _x + " " + _a + " " + _Increment, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }

            if (_Increment == 500)
            {
                DeckCardPB.Location = _startPoint;
            }

            if (_Increment > 500 || AnimationDealerDone == true)
            {
                AnimationTimerDealer.Enabled = false;
                if (_DealerCardIndex >= 0)
                {
                    dealerCards[_DealerCardIndex].Show();
                    dealerCards[_DealerCardIndex].BringToFront();
                }

                DeckCardPB.Location = _startPoint;
                _Increment = 0;
            }

            //how fast to deal Dealer cards, 75
            _Increment += DealSpeed;
        }

        /// <summary>
        /// Refresh the UI to update the player cards
        /// </summary>
        private void UpdateUIPlayerCards(int cardNum)
        {
            // Save the original location of the PicBoxDealerCard (card to animate)
            _startPoint = DeckCardPB.Location;

            //MessageBox.Show("_startPoint x and y " + _startPoint.X + " " + _startPoint.Y, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

            List<Card> pcards = game.CurrentPlayer.Hand.Cards;

            //MessageBox.Show("2PlayerCards Count: " + pcards.Count.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Information);
            //MessageBox.Show("2Player cardNum " + cardNum, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

            //AnimationPlayerDone = false;

            for (int i = 0; i < pcards.Count; i++)
            {
                // Load each card from file
                LoadCard(playerCards[i], pcards[i]);

                //PlayerCard
                PlayerCardLocationX = playerCards[i].Location.X;
                PlayerCardLocationY = playerCards[i].Location.Y;
                //DeckCard
                DeckCardPBLocationX = DeckCardPB.Location.X;
                DeckCardPBLocationY = DeckCardPB.Location.Y;

                AnimationPlayerDone = false;

                //MessageBox.Show("Player cardNum, i, AnimationPlayerDone, PlayerInitcards " + cardNum + ", " + i + ", " + AnimationPlayerDone + ", " + PlayerInitcards, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

                if (AnimationPlayerDone == false && i == cardNum - 1)
                {
                    //MessageBox.Show("Dealer cardNum, i, AnimationPlayerDone, PlayerInitcards " + cardNum + ", " + i + ", " + AnimationPlayerDone + ", " + PlayerInitcards, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    AnimatePlayerCard(i);
                    //MessageBox.Show("Dealer cardNum, i, AnimationPlayerDone, PlayerInitcards " + cardNum + ", " + i + ", " + AnimationPlayerDone + ", " + PlayerInitcards, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }

                if (tShowPlayerHandTotals)
                {
                    playerTotalLabel.Show();
                    // Update the value of the hand
                    playerTotalLabel.Text = game.CurrentPlayer.Hand.GetSumOfHand().ToString();
                }
            }
        }

        /// <summary>
        ///   Calls inherited class for power info, then takes necessary actions.
        /// </summary>
        /// <remarks></remarks>
        private void AnimatePlayerCard(int cardIndex)
        {
            CardsInDeck--;
            CardsInPlayersHand++;

            lblCardsInDeck.Text = CardsInDeck.ToString();
            lblCardsInPlayersHand.Text = CardsInPlayersHand.ToString();

            //MessageBox.Show("2Cards Count: " + CardsInDeck.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Information);

            PlaySoundFiles("card_drop");

            //playerCards[cardIndex].Hide();
            DeckCardPB.BringToFront();
            DeckCardPB.Location = _startPoint;
            _PlayerCardIndex = cardIndex;
            AnimationTimerPlayer.Enabled = true;
            while (AnimationTimerPlayer.Enabled == true)

                // Wait for animation to finish
                Application.DoEvents();
        }

        /// <summary>
        /// Move card one frame per timer tick
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks></remarks>
        private void AnimatePlayerCardTimer_Tick(object sender, EventArgs e)
        {
            if (_Increment <= 500)
            {
                // Player is dealt the card, so use as endpoints player card locations.
                _a = PlayerCardLocationX;
                _b = PlayerCardLocationY;
            }
            _x = (_a - _startPoint.X) * _Increment / 500 + _startPoint.X;
            _y = (_b - _startPoint.Y) * _Increment / 500 + _startPoint.Y;

            //MessageBox.Show("_startPoint x and y " + _startPoint.X + " " + _startPoint.Y, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            //MessageBox.Show("1_x and _y and _Increment " + _x + " " + _y + " " + _Increment, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

            if (_Increment == 0)
            {
                //MessageBox.Show("1_x and _y and _Increment " + _x + " " + _y + " " + _Increment, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }

            DeckCardPB.Location = new Point(_x, _y);
            DeckCardPB.Show();
            DeckCardPB.BringToFront();

            if (_x <= (_a + 1) && AnimationPlayerDone == false)
            {
                //end animation before top of card is reached
                AnimationPlayerDone = true;
                //MessageBox.Show("2_x and _a and _Increment " + _x + " " + _a + " " + _Increment, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }

            if (_Increment == 500)
            {
                DeckCardPB.Location = _startPoint;
            }

            if (_Increment > 500 || AnimationPlayerDone == true)
            {
                AnimationTimerPlayer.Enabled = false;
                if (_PlayerCardIndex >= 0)
                {
                    playerCards[_PlayerCardIndex].Show();
                    playerCards[_PlayerCardIndex].BringToFront();
                }

                DeckCardPB.Location = _startPoint;
                _Increment = 0;
            }

            //how fast to deal Player cards, 75
            _Increment += DealSpeed;
        }

        /// <summary>
        /// Exits the game
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExitBtn_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Place a bet for 10 dollars
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TenBtn_Click(object sender, EventArgs e)
        {
            Bet(10);
        }

        /// <summary>
        /// Place a bet for 25 dollars
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TwentyFiveBtn_Click(object sender, EventArgs e)
        {
            Bet(25);
        }

        /// <summary>
        /// Place a bet for 50 dollars
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FiftyBtn_Click(object sender, EventArgs e)
        {
            Bet(50);
        }

        /// <summary>
        /// Place a bet for 100 dollars
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HundredBtn_Click(object sender, EventArgs e)
        {
            Bet(100);
        }

        /// <summary>
        /// Clear the bet
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClearBetBtn_Click(object sender, EventArgs e)
        {
            //Clear the bet amount
            game.CurrentPlayer.ClearBet();
            ShowBankValue();
        }

        #endregion

        #region Game Methods

        /// <summary>
        /// This method updates the current bet by a specified bet amount
        /// </summary>
        /// <param name="betValue"></param>
        private void Bet(decimal betValue)
        {
            try
            {
                // Update the bet amount
                game.CurrentPlayer.IncreaseBet(betValue);
                // Update the "My Bet" and "My Account" values
                ShowBankValue();
            }
            catch (Exception NotEnoughMoneyException)
            {
                MessageBox.Show(NotEnoughMoneyException.Message);
            }
        }

        /// <summary>
        /// Set the "My Account" value in the UI
        /// </summary>
        private void ShowBankValue()
        {
            // Update the "My Account" value
            myAccountTextBox.Text = "$" + game.CurrentPlayer.Balance.ToString();
            myBetTextBox.Text = "$" + game.CurrentPlayer.Bet.ToString();
        }

        /// <summary>
        /// Clear the dealer and player cards
        /// </summary>
        private void ClearTable()
        {
            for (int i = 0; i < 6; i++)
            {
                dealerCards[i].Image = null;
                dealerCards[i].Visible = false;

                playerCards[i].Image = null;
                playerCards[i].Visible = false;
            }
        }

        /// <summary>
        /// Get the game result.  This returns an EndResult value
        /// </summary>
        /// <returns></returns>
        private EndResult GetGameResult()
        {
            EndResult endState;
            // Check for blackjack
            if (game.Dealer.Hand.NumCards == 2 && game.Dealer.HasBlackJack())
            {
                endState = EndResult.DealerBlackJack;
            }
            // Check if the dealer has bust
            else if (game.Dealer.HasBust())
            {
                endState = EndResult.DealerBust;
            }
            else if (game.Dealer.Hand.CompareFaceValue(game.CurrentPlayer.Hand) > 0)
            {
                //dealer wins
                endState = EndResult.DealerWin;
            }
            else if (game.Dealer.Hand.CompareFaceValue(game.CurrentPlayer.Hand) == 0)
            {
                // push
                endState = EndResult.Push;
            }
            else
            {
                // player wins
                endState = EndResult.PlayerWin;
            }
            return endState;
        }

        private void Delay(int miliseconds_to_sleep)
        {
            //do we need a delay at this time
            System.Threading.Thread.Sleep(miliseconds_to_sleep);
            //Application.DoEvents();
        }

        // PlaySoundFiles
        private void PlaySoundFiles(string tString)
        {
            //play sound files
            if (NSound == 1)
            {
                //used if sound files are in resources - SoundPlayer player = new SoundPlayer(Properties.Resources.hooray);
                //sounds used in game - shuffling_cards, card_drop, cord 
                switch (tString)
                {
                    case "shuffling_cards":
                        //if using sound files in resoures
                        //SoundPlayer player1 = new SoundPlayer(Properties.Resources.chord);
                        //player1.Play();
                        //player1.Dispose();

                        // Set location of the .wav file
                        SoundFileLocation = SoundFileFolder + "shuffling_cards.wav";
                        SoundPlayer player1 = new SoundPlayer(SoundFileLocation.ToString());
                        player1.Play();
                        player1.Dispose();
                        break;
                    case "deal":
                        //player2
                        SoundFileLocation = SoundFileFolder + "deal.wav";
                        SoundPlayer player2 = new SoundPlayer(SoundFileLocation.ToString());
                        player2.Play();
                        player2.Dispose();
                        break;
                    case "cardflip":
                        //player3
                        SoundFileLocation = SoundFileFolder + "cardflip.wav";
                        SoundPlayer player3 = new SoundPlayer(SoundFileLocation.ToString());
                        player3.Play();
                        player3.Dispose();
                        break;
                    case "defeat":
                        //player4
                        SoundFileLocation = SoundFileFolder + "defeat.wav";
                        SoundPlayer player4 = new SoundPlayer(SoundFileLocation.ToString());
                        player4.Play();
                        player4.Dispose();
                        break;
                    case "hooray":
                        //player5
                        SoundFileLocation = SoundFileFolder + "hooray.wav";
                        SoundPlayer player5 = new SoundPlayer(SoundFileLocation.ToString());
                        player5.Play();
                        player5.Dispose();
                        break;
                    case "chord":
                        SoundFileLocation = SoundFileFolder + "chord.wav";
                        SoundPlayer player6 = new SoundPlayer(SoundFileLocation.ToString());
                        player6.Play();
                        player6.Dispose();
                        break;
                    case "card_drop":
                        //player7
                        SoundFileLocation = SoundFileFolder + "card_drop.wav";
                        SoundPlayer player7 = new SoundPlayer(SoundFileLocation.ToString());
                        player7.Play();
                        player7.Dispose();
                        break;
                    default:
                        //    SoundPlayer player = new SoundPlayer(Properties.Resources.card_drop);
                        //    player.Play();
                        //    player.Dispose();
                            break;
                }   //end switch
            }   //end if NSound
        }   //end PlaySoundFiles


        /// <summary>
        /// Takes an EndResult value and shows the resulting game ending in the UI
        /// </summary>
        /// <param name="endState"></param>
        private void EndGame(EndResult endState)
        {

            switch (endState)
            {
                case EndResult.DealerBust:
                    gameOverTextBox.Text = "Dealer Bust!";
                    game.PlayerWin();
                    PlaySoundFiles("chord");
                    break;
                case EndResult.DealerBlackJack:
                    gameOverTextBox.Text = "Dealer BlackJack!";
                    game.PlayerLose();
                    break;
                case EndResult.DealerWin:
                    gameOverTextBox.Text = "Dealer Won!";
                    game.PlayerLose();
                    break;
                case EndResult.PlayerBlackJack:
                    gameOverTextBox.Text = "BlackJack!";
                    game.CurrentPlayer.Balance += (game.CurrentPlayer.Bet * (decimal)2.5);
                    game.CurrentPlayer.Wins += 1;
                    break;
                case EndResult.PlayerBust:
                    gameOverTextBox.Text = "You Bust!";
                    game.PlayerLose();
                    break;
                case EndResult.PlayerWin:
                    gameOverTextBox.Text = "You Won!";
                    game.PlayerWin();
                    break;
                case EndResult.Push:
                    gameOverTextBox.Text = "Push";
                    game.CurrentPlayer.Push += 1;
                    game.CurrentPlayer.Balance += game.CurrentPlayer.Bet;
                    break;
            }
            // Update the "My Record" values
            winTextBox.Text = game.CurrentPlayer.Wins.ToString();
            lossTextBox.Text = game.CurrentPlayer.Losses.ToString();
            tiesTextBox.Text = game.CurrentPlayer.Push.ToString();
            SetUpNewGame();
            ShowBankValue();
            gameOverTextBox.Show();
            // Check if the current player is out of money
            if (game.CurrentPlayer.Balance == 0)
            {
                MessageBox.Show("Out of Money.  Please create a new game to play again.");
                this.Close();
            }
        }

        #endregion

        #region Game UI Methods

        /// <summary>
        /// Load the Deck Card Images
        /// </summary>
        private void LoadCardSkinImages()
        {
            try
            {
                // Load the card skin image from file
                Image cardSkin = Image.FromFile(Properties.Settings.Default.CardGameImageSkinPath);
                // Set the three cards on the UI to the card skin image
                deckCard1PictureBox.Image = cardSkin;
                //deckCard1PictureBox.BackColor = Color.SeaGreen;
                deckCard2PictureBox.Image = cardSkin;
                //deckCard2PictureBox.BackColor = Color.SeaGreen;
                deckCard3PictureBox.Image = cardSkin;
                //deckCard3PictureBox.BackColor = Color.SeaGreen;

                DeckCardPB.Image = cardSkin;
                //DeckCardPB.BackColor = Color.SeaGreen;
            }
            catch (OutOfMemoryException)
            {
                MessageBox.Show("Card skin images are not loading correctly.  Make sure the card skin images are in the correct location.");
            }

            playerCards = new PictureBox[] { playerCard1, playerCard2, playerCard3, playerCard4, playerCard5, playerCard6 };
            dealerCards = new PictureBox[] { dealerCard1PictureBox, dealerCard2PictureBox, dealerCard3PictureBox, dealerCard4PictureBox, dealerCard5PictureBox, dealerCard6PictureBox };
        }

        /// <summary>
        /// Set up the UI for when the game is in play after the player has hit deal game
        /// </summary>
        private void SetUpGameInPlay()
        {
            tenButton.Enabled = false;
            twentyFiveButton.Enabled = false;
            fiftyButton.Enabled = false;
            hundredButton.Enabled = false;
            clearBetButton.Enabled = false;
            standButton.Enabled = true;
            hitButton.Enabled = true;
            gameOverTextBox.Hide();

            if (tShowPlayerHandTotals)
            {
                //show player card totals
                playerTotalLabel.Show();
            }
            if (tShowDealerHandTotals)
            {
                //Show dealer card totals
                dealerTotalLabel.Show();
            }

            dealButton.Enabled = false;

            if (firstTurn)
                doubleDownButton.Enabled = true;
        }

        /// <summary>
        /// Set up the UI for a new game
        /// </summary>
        private void SetUpNewGame()
        {
            //photoPictureBox.ImageLocation = Properties.Settings.Default.PlayerImage;
            photoPictureBox.Image = Image.FromFile(Properties.Settings.Default.PlayerImage);

            photoPictureBox.Visible = true;
            playerNameLabel.Text = Properties.Settings.Default.PlayerName;
            dealButton.Enabled = true;
            doubleDownButton.Enabled = false;
            standButton.Enabled = false;
            hitButton.Enabled = false;
            clearBetButton.Enabled = true;
            tenButton.Enabled = true;
            twentyFiveButton.Enabled = true;
            fiftyButton.Enabled = true;
            hundredButton.Enabled = true;
            gameOverTextBox.Hide();

            if (tShowPlayerHandTotals)
            {
                //show player card totals
                playerTotalLabel.Show();
            }
            if (tShowDealerHandTotals)
            {
                //Show dealer card totals
                dealerTotalLabel.Show();
            }

            firstTurn = true;
            ShowBankValue();

            //NumberOfCards - initial numbers of cards in initial hand
            PlayerHits = NumberOfCards;
            DealerHits = NumberOfCards;

            //cards have not been dealt
            PlayerInitcards = true;
            DealerInitcards = true;
        }

        /// <summary>
        /// Takes the card value and loads the corresponding card image from file
        /// </summary>
        /// <param name="pb"></param>
        /// <param name="c"></param>
        private void LoadCard(PictureBox pb, Card c)
        {
            try
            {
                StringBuilder image = new StringBuilder();

                switch (c.Suit)
                {
                    case Suit.Diamonds:
                        image.Append("di");
                        break;
                    case Suit.Hearts:
                        image.Append("he");
                        break;
                    case Suit.Spades:
                        image.Append("sp");
                        break;
                    case Suit.Clubs:
                        image.Append("cl");
                        break;
                }

                switch (c.FaceVal)
                {
                    case FaceValue.Ace:
                        image.Append("1");
                        break;
                    case FaceValue.King:
                        image.Append("k");
                        break;
                    case FaceValue.Queen:
                        image.Append("q");
                        break;
                    case FaceValue.Jack:
                        image.Append("j");
                        break;
                    case FaceValue.Ten:
                        image.Append("10");
                        break;
                    case FaceValue.Nine:
                        image.Append("9");
                        break;
                    case FaceValue.Eight:
                        image.Append("8");
                        break;
                    case FaceValue.Seven:
                        image.Append("7");
                        break;
                    case FaceValue.Six:
                        image.Append("6");
                        break;
                    case FaceValue.Five:
                        image.Append("5");
                        break;
                    case FaceValue.Four:
                        image.Append("4");
                        break;
                    case FaceValue.Three:
                        image.Append("3");
                        break;
                    case FaceValue.Two:
                        image.Append("2");
                        break;
                }

                image.Append(Properties.Settings.Default.CardGameImageExtension);
                string cardGameImagePath = Properties.Settings.Default.CardGameImagePath;
                string cardGameImageSkinPath = Properties.Settings.Default.CardGameImageSkinPath;
                image.Insert(0, cardGameImagePath);
                //check to see if the card should be faced down or up;
                if (!c.IsCardUp)
                    image.Replace(image.ToString(), cardGameImageSkinPath);

                pb.Image = new Bitmap(image.ToString());
            }
            catch (ArgumentOutOfRangeException)
            {
                MessageBox.Show("Card images are not loading correctly.  Make sure all card images are in the right location.");
            }
        }
        #endregion
    }
}
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BlackJack.Models;
using Microsoft.AspNetCore.Http;
using System.Text;

namespace BlackJack.Controllers
{
    public class HomeController : Controller
    {
        static BlackJackGame game;
        bool firstTurn;
        string imageLocation;
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Deal()
        {
            try
            {
                // If the current bet is equal to 0, ask the player to place a bet
                if ((game.CurrentPlayer.Bet == 0) && (game.CurrentPlayer.Balance > 0))
                {
                    TempData["alertMessage"] = "You must place a bet before the dealer deals.";
                }
                else
                {
                    // Place the bet
                    game.CurrentPlayer.PlaceBet();
                    ShowBankValue();

                    // Clear the table, set up the UI for playing a game, and deal a new game
                    ClearTable();
                    //SetUpGameInPlay();
                    game.DealNewGame();
                    UpdateUIPlayerCards();

                    // Check see if the current player has blackjack
                    if (game.CurrentPlayer.HasBlackJack())
                    {
                        EndGame(EndResult.PlayerBlackJack);
                    }
                }
            }
            catch (Exception NotEnoughMoneyException)
            {
                ViewBag.WarningMessage = NotEnoughMoneyException.Message;
            }
            return RedirectToAction("index");
        }
        public IActionResult NewGame()
        {
            //Creates a new blackjack game with one player and an inital balance set through the settings designer
            game = new BlackJackGame(100);
            firstTurn = true;
            ShowBankValue();
            return RedirectToAction("index");
        }
        public IActionResult Hit()
        {
            // It is no longer the first turn, set this to false so that the cards will all be facing upwards
            firstTurn = false;
            // Hit once and update UI cards
            game.CurrentPlayer.Hit();
            UpdateUIPlayerCards();

            // Check to see if player has bust
            if (game.CurrentPlayer.HasBust())
            {
                EndGame(EndResult.PlayerBust);
            }
            return RedirectToAction("index");
        }
        public IActionResult Stand()
        {
            // Dealer should finish playing and the UI should be updated
            game.DealerPlay();
            UpdateUIPlayerCards();

            // Check who won the game
            EndGame(GetGameResult());
            return RedirectToAction("index");
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
        public IActionResult Bet5()
        {
            Bet(5);
            return RedirectToAction("index");
        }
        /// <summary>
        /// Refresh the UI to update the player cards
        /// </summary>
        private void UpdateUIPlayerCards()
        {
            // Update the value of the hand
            //playerTotalLabel.Text = game.CurrentPlayer.Hand.GetSumOfHand().ToString();

            List<Card> pcards = game.CurrentPlayer.Hand.Cards;
            for (int i = 0; i < pcards.Count; i++)
            {
                // Load each card from file
                LoadCard(pcards[i]);
                string myImageInView = "mc" + (i + 1);
                TempData[myImageInView] = imageLocation;
                imageLocation = "";
            }

            List<Card> dcards = game.Dealer.Hand.Cards;
            for (int i = 0; i < dcards.Count; i++)
            {
                LoadCard(dcards[i]);
                string dealerImageInView = "dc" + (i + 1);
                TempData[dealerImageInView] = imageLocation;
                imageLocation = "";
            }
        }
        /// <summary>
        /// Takes the card value and loads the corresponding card image from file
        /// </summary>
        /// <param name="c"></param>
        private void LoadCard(Card c)
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
                image.Append(".gif");
                image.Insert(0, "/images/");

                //check to see if the card should be faced down or up;
                if (!c.IsCardUp)
                    image.Replace(image.ToString(), "/images/cardSkin.gif");
                imageLocation = image.ToString();
            }
            catch (ArgumentOutOfRangeException)
            {
                ViewBag.WarningMessage = "Card images are not loading correctly.  Make sure all card images are in the right location.";

            }
        }

        /// <summary>
        /// Clear the dealer and player cards
        /// </summary>
        private void ClearTable()
        {
            TempData["mc1"] = "";
            TempData["mc2"] = "";
            TempData["mc3"] = "";
            TempData["mc4"] = "";
            TempData["mc5"] = "";
            TempData["mc6"] = "";

            TempData["dc1"] = "";
            TempData["dc2"] = "";
            TempData["dc3"] = "";
            TempData["dc4"] = "";
            TempData["dc5"] = "";
            TempData["dc6"] = "";
        }
        /// <summary>
        /// Set the "My Account" value in the UI
        /// </summary>
        private void ShowBankValue()
        {
            // Update the "My Account" value
            TempData["myAccount"] = game.CurrentPlayer.Balance.ToString();
            TempData["myBet"] = game.CurrentPlayer.Bet.ToString();
        }
        /// <summary>
        /// This method updates the current bet by a specified bet amount
        /// </summary>
        /// <param name="betValue"></param>
        private void Bet(decimal betValue)
        {
            // Update the bet amount
            game.CurrentPlayer.IncreaseBet(betValue);

            // Update the "My Bet" and "My Account" values
            ShowBankValue();
        }
        /// <summary>
        /// Takes an EndResult value and shows the resulting game ending in the UI
        /// </summary>
        /// <param name="endState"></param>
        private void EndGame(EndResult endState)
        {
            switch (endState)
            {
                case EndResult.DealerBust:
                    TempData["resultMessage"] = "Dealer Bust!";
                    game.PlayerWin();
                    break;
                case EndResult.DealerBlackJack:
                    TempData["resultMessage"] = "Dealer BlackJack!";
                    game.PlayerLose();
                    break;
                case EndResult.DealerWin:
                    TempData["resultMessage"] = "Dealer Won!";
                    game.PlayerLose();
                    break;
                case EndResult.PlayerBlackJack:
                    TempData["resultMessage"] = "BlackJack!";
                    game.CurrentPlayer.Balance += (game.CurrentPlayer.Bet * (decimal)2.5);
                    game.CurrentPlayer.Wins += 1;
                    break;
                case EndResult.PlayerBust:
                    TempData["resultMessage"] = "You Bust!";
                    game.PlayerLose();
                    break;
                case EndResult.PlayerWin:
                    TempData["resultMessage"] = "You Won!";
                    game.PlayerWin();
                    break;
                case EndResult.Push:
                    TempData["resultMessage"] = "Push";
                    game.CurrentPlayer.Push += 1;
                    game.CurrentPlayer.Balance += game.CurrentPlayer.Bet;
                    break;
            }

            firstTurn = true;
            ShowBankValue();
            // Check if the current player is out of money
            if (game.CurrentPlayer.Balance == 0)
            {
                TempData["resultMessage"] = "Out of Money. Please create a new game to play again.";
            }
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

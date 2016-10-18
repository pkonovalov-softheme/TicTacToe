using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace RosettaTicTacToe
{
    internal class Program
    {

        /*================================================================
         *Pieces (players and board)
         *================================================================*/

        private static string[][] _players = new string[][] {
      new string[] { "COMPUTER", "X" }, // computer player
      new string[] { "HUMAN", "O" }     // human player
    };

        private const int Unplayed = -1;
        private const int Computer = 0;
        private const int Human = 1;
        private const double E = 0.3; //epsilon
        private const double Alpha = 0.1; //value update factor
        private const double Beta = 0.2; // How much learn is reduced between states
        private static ulong aiWon = 0;
        private static ulong totalGames = 0;

        // GameBoard holds index into Players[] (0 or 1) or Unplayed (-1) if location not yet taken
        private static readonly int[] GameBoard = new int[9];
        private static readonly Dictionary<int[], double> Values = new Dictionary<int[], double>();
        private static Stack<int[]> _lastStates;

        private static int[] _corners = new int[] { 0, 2, 6, 8 };

        private static int[][] _wins = new int[][] {
          new int[] { 0, 1, 2 }, new int[] { 3, 4, 5 }, new int[] { 6, 7, 8 },
          new int[] { 0, 3, 6 }, new int[] { 1, 4, 7 }, new int[] { 2, 5, 8 },
          new int[] { 0, 4, 8 }, new int[] { 2, 4, 6 } };


        /*================================================================
         *Main Game Loop (this is what runs/controls the game)
         *================================================================*/

        private static void Main(string[] args)
        {


               // Console.Clear();
               // Console.WriteLine("Welcome to Rosetta Code Tic-Tac-Toe for C#.");
                
              //  DisplayGameBoard();
                int currentPlayer = _rnd.Next(0, 2);  // current player represented by Players[] index of 0 or 1
             //   Console.WriteLine("The first move goes to {0} who is playing {1}s.\n", PlayerName(currentPlayer), PlayerToken(currentPlayer));

                _lastStates = new Stack<int[]>();

                for (ulong y = 0; y < 100000; y++)
                {
                    InitializeGameBoard();
                    if (y > 0 && y % 1000 == 0)
                    {
                        Console.WriteLine("Perfomance: {0}", (double)aiWon / totalGames);
                    }

                    PlayGame(currentPlayer);
                    currentPlayer = GetNextPlayer(currentPlayer);
                }
                //if (!PlayAgain())
                //    return;
        }

        private static void PlayGame(int currentPlayer)
        {
            while (true)
            {
                int thisMove = GetMoveFor(currentPlayer);
                if (thisMove == Unplayed)
                {
                    Console.WriteLine("{0}, you've quit the game ... am I that good?", PlayerName(currentPlayer));
                    throw new InvalidOperationException();
                }

                PlayMove(thisMove, currentPlayer);
                //DisplayGameBoard();
                if (IsGameWon())
                {
                    if (currentPlayer == Human)
                    {
                        UpdateValues(-1);
                    }
                    else
                    {
                        aiWon++;
                        UpdateValues(1);
                    }

                    totalGames++;

                    // Console.WriteLine("{0} has won the game!", PlayerName(currentPlayer));
                    break;
                }
                else if (IsGameTied())
                {
                    UpdateValues(0.5);
                    // Console.WriteLine("Cat game ... we have a tie.");
                    break;
                }
            }
        }

        private static void UpdateValues(double reward)
        {
            Debug.Assert(reward >= -1 && reward <= 1 );

            double prevValue = reward;

            while (_lastStates.Count > 0)
            {
                int[] state = _lastStates.Pop();
                if (!Values.ContainsKey(state))
                {
                    Values.Add(state, 0.5);
                }

                double curValue = Values[state];
                double newValue = curValue + Alpha * (prevValue - curValue);
                prevValue = Beta * newValue; //?
            }
        }

        /*================================================================
         *Move Logic
         *================================================================*/

        private static int GetMoveFor(int player)
        {
            if (player == Human)
            {
                //return getManualMove(player);
                return GetBestMove(player);
            }
            else
            {
                //int selectedMove = getManualMove(player);
                //int selectedMove = getRandomMove(player);
                int selectedMove = GetSemiRandomMove(player);
                //int selectedMove = GetBestMove(player);
               // Console.WriteLine("{0} selects position {1}.", PlayerName(player), selectedMove + 1);
                Debug.Assert(selectedMove != Unplayed);
                return selectedMove;
            }
        }

        private static int GetManualMove(int player)
        {
            while (true)
            {
                Console.Write("{0}, enter you move (number): ", PlayerName(player));
                ConsoleKeyInfo keyInfo = Console.ReadKey();
                Console.WriteLine();  // keep the display pretty
                if (keyInfo.Key == ConsoleKey.Escape)
                    return Unplayed;
                if (keyInfo.Key >= ConsoleKey.D1 && keyInfo.Key <= ConsoleKey.D9)
                {
                    int move = keyInfo.KeyChar - '1';  // convert to between 0..8, a GameBoard index position.
                    if (GameBoard[move] == Unplayed)
                        return move;
                    else
                        Console.WriteLine("Spot {0} is already taken, please select again.", move + 1);
                }
                else
                    Console.WriteLine("Illegal move, please select again.\n");
            }
        }

        // Check is it random? - Pkonoval
        private static int GetRandomMove(int player)
        {
            int movesLeft = GameBoard.Count(position => position == Unplayed);
            int x = _rnd.Next(0, movesLeft);
            for (int i = 0; i < GameBoard.Length; i++)  // walk board ...
            {
                if (GameBoard[i] == Unplayed && x <= 0)    // until we reach the unplayed move.
                    return i;
                x--;
            }
            return Unplayed;
        }

        // plays random if no winning move or needed block.
        private static int GetSemiRandomMove(int player)
        {
            int posToPlay;
            if (CheckForWinningMove(player, out posToPlay))
                return posToPlay;
            if (CheckForBlockingMove(player, out posToPlay))
                return posToPlay;
            return GetRandomMove(player);
        }

        //----------------
        // Smart logic

        //private static int[] GetSwapedValue()
        //{
        //    var gameBoardSwap = new int[9];

        //    for (int i = 0; i < GameBoard.Length; i++)
        //    {
        //        if (GameBoard[i] == 0)
        //        {
        //            gameBoardSwap[i] = 1;
        //        }
        //        else if (GameBoard[i] == 1)
        //        {
        //            gameBoardSwap[i] = 0;
        //        }
        //    }

        //    return gameBoardSwap;
        //}


        //static bool IsValueExists()
        //{
        //    if (Values.ContainsKey(GameBoard))
        //    {
        //        return true;
        //    }

          

        //    return Values.ContainsKey(GameBoardSwap);
        //}


        // purposely not implemented (this is the thinking part).
        private static int GetBestMove(int player)
        {
             _lastStates.Push(GameBoard);

            double res = _rnd.NextDouble();
            if (res < E)
            {
                return GetRandomMove(player);
            }

            return GetBestPosition(player);
        }

        private static int GetBestPosition(int player)
        {
            KeyValuePair<double, int> winner = new KeyValuePair<double, int>(0, 0);

            for (int i = 0; i < GameBoard.Length; i++)
            {
                if (GameBoard[i] == Unplayed)
                {
                    GameBoard[i] = player;
                    double res;
                    bool contain = Values.TryGetValue(GameBoard, out res);
                    if (!contain)
                    {
                        Values.Add(GameBoard, 0.5);
                    }

                    if (winner.Key < res)
                    {
                        winner = new KeyValuePair<double, int>(res, i);
                    }

                    //positions.Add(res, i);
                    GameBoard[i] = Unplayed;
                }
            }

            return winner.Value;
        }


        private static bool CheckForWinningMove(int player, out int posToPlay)
        {
            posToPlay = Unplayed;
            foreach (var line in _wins)
                if (TwoOfThreeMatchPlayer(player, line, out posToPlay))
                    return true;
            return false;
        }

        private static bool CheckForBlockingMove(int player, out int posToPlay)
        {
            posToPlay = Unplayed;
            foreach (var line in _wins)
                if (TwoOfThreeMatchPlayer(GetNextPlayer(player), line, out posToPlay))
                    return true;
            return false;
        }

        private static bool TwoOfThreeMatchPlayer(int player, int[] line, out int posToPlay)
        {
            int cnt = 0;
            posToPlay = int.MinValue;
            foreach (int pos in line)
            {
                if (GameBoard[pos] == player)
                    cnt++;
                else if (GameBoard[pos] == Unplayed)
                    posToPlay = pos;
            }
            return cnt == 2 && posToPlay >= 0;
        }

        private static void PlayMove(int boardPosition, int player)
        {
            GameBoard[boardPosition] = player;
        }

        private static bool IsGameWon()
        {
            return _wins.Any(line => TakenBySamePlayer(line[0], line[1], line[2]));
        }

        private static bool TakenBySamePlayer(int a, int b, int c)
        {
            return GameBoard[a] != Unplayed && GameBoard[a] == GameBoard[b] && GameBoard[a] == GameBoard[c];
        }

        private static bool IsGameTied()
        {
            return !GameBoard.Any(spot => spot == Unplayed);
        }

        /*================================================================
         *Misc Methods
         *================================================================*/
        private static Random _rnd = new Random();

        private static void InitializeGameBoard()
        {
            for (int i = 0; i < GameBoard.Length; i++)
                GameBoard[i] = Unplayed;
        }

        private static string PlayerName(int player)
        {
            return _players[player][0];
        }

        private static string PlayerToken(int player)
        {
            return _players[player][1];
        }

        private static int GetNextPlayer(int player)
        {
            return (player + 1) % 2;
        }

        private static void DisplayGameBoard()
        {
            Console.WriteLine(" {0} | {1} | {2}", PieceAt(0), PieceAt(1), PieceAt(2));
            Console.WriteLine("---|---|---");
            Console.WriteLine(" {0} | {1} | {2}", PieceAt(3), PieceAt(4), PieceAt(5));
            Console.WriteLine("---|---|---");
            Console.WriteLine(" {0} | {1} | {2}", PieceAt(6), PieceAt(7), PieceAt(8));
            Console.WriteLine();
        }

        private static string PieceAt(int boardPosition)
        {
            if (GameBoard[boardPosition] == Unplayed)
                return (boardPosition + 1).ToString();  // display 1..9 on board rather than 0..8
            return PlayerToken(GameBoard[boardPosition]);
        }

        private static bool PlayAgain()
        {
            Console.WriteLine("\nDo you want to play again?");
            return Console.ReadKey(false).Key == ConsoleKey.Y;
        }
    }

}
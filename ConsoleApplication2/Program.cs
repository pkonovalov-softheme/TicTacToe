using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace RosettaTicTacToe
{
    public class ArraysEqualityComparer : IEqualityComparer<int[]>
    {
        public bool Equals(int[] x, int[] y)
        {
            if (x.Length != y.Length)
            {
                return false;
            }
            for (int i = 0; i < x.Length; i++)
            {
                if (x[i] != y[i])
                {
                    return false;
                }
            }
            return true;
        }

        public int GetHashCode(int[] obj)
        {
            int result = 17;
            for (int i = 0; i < obj.Length; i++)
            {
                unchecked
                {
                    result = result * 23 + obj[i];
                }
            }
            return result;
        }
    }

    enum PlayerType
    {
        RLbot,
        Alg
    }

    internal class Program
    {

        /*================================================================
         *Pieces (players and board)
         *================================================================*/

        private static string[][] _players = new string[][] {
          new string[] { "COMPUTER", "X" }, // computer player
          new string[] { "HUMAN", "O" }     // human player
        };

        private static PlayerType _currentPlayer;

        private const int Unplayed = -1;
        private const int Computer = 0;
        private const int Human = 1;
        private const double E = 0.05; //epsilon
        private const double Alpha = 0.2; //value update factor
        private const double Gamma = 1; // Discount factor
        private static ulong aiWon = 0;
        private static ulong aiLoose = 0;
        private static ulong draws = 0;

        // GameBoard holds index into Players[] (0 or 1) or Unplayed (-1) if location not yet taken
        private static readonly int[] GameBoard = new int[9];
        private static readonly Dictionary<int[], double> Values = new Dictionary<int[], double>(new ArraysEqualityComparer());
        private static Stack<int[]> _lastStates;
        private static ArraysEqualityComparer _comparer = new ArraysEqualityComparer();

        private static int[] _corners = new int[] { 0, 2, 6, 8 };

        private static readonly int[][] Wins = new int[][] {
          new int[] { 0, 1, 2 }, new int[] { 3, 4, 5 }, new int[] { 6, 7, 8 },
          new int[] { 0, 3, 6 }, new int[] { 1, 4, 7 }, new int[] { 2, 5, 8 },
          new int[] { 0, 4, 8 }, new int[] { 2, 4, 6 } };


        /*================================================================
         *Main Game Loop (this is what runs/controls the game)
         *================================================================*/

        private static void Main(string[] args)
        {



           // TestRnd();
           // UpdateProc();
               // Console.Clear();
               // Console.WriteLine("Welcome to Rosetta Code Tic-Tac-Toe for C#.");

            //  DisplayGameBoard();


            //   Console.WriteLine("The first move goes to {0} who is playing {1}s.\n", PlayerName(CurrentPlayer), PlayerToken(CurrentPlayer));

            _lastStates = new Stack<int[]>();
            

                for (ulong y = 0; y < 1000000; y++)
                {
                    _currentPlayer = PlayerType.RLbot;
                    //_currentPlayer = (PlayerType)_rnd.Next(0, 2);  // current player represented by Players[] index of 0 or 1
                    InitializeGameBoard();
                    if (y > 0 && y % 300000 == 0)
                    {
                        ManMove = true;
                        //UpdateProc();
                        ulong totalGames = aiLoose + aiWon + draws;
                        Console.WriteLine("Wons: {0}", (double)aiWon / totalGames);
                        Console.WriteLine("Draws perfomance: {0}", (double)draws / totalGames);
                        Console.WriteLine("Loses: {0}", (double)aiLoose / totalGames);
                        Console.WriteLine();
                        aiLoose = 0;
                        aiWon = 0;
                        draws = 0;
                    }

                    PlayGame();
                }
                //if (!PlayAgain())
                //    return;
        }

        private static void PlayGame()
        {
            while (true)
            {
                int thisMove = ChoseMove();
                if (thisMove == Unplayed)
                {
                    throw new InvalidOperationException();
                }

                if (PlayMove(thisMove))
                {
                    break;
                }

                NextCurrentPlayer();

                thisMove = ChoseMove();
                if (thisMove == Unplayed)
                {
                    throw new InvalidOperationException();
                }

                if (PlayMove(thisMove))
                {
                    break;
                }

                NextCurrentPlayer();
            }
        }

        private static void PlayStates()
        {
            Console.WriteLine("States:");
            foreach (int[] state in _lastStates)
            {
                DisplayGameBoard(state);
            }
            Console.WriteLine("-------------------");
        }
        private static void UpdateValues(double reward)
        {
            Debug.Assert(reward >= -1 && reward <= 1 );

            double prevValue = reward;

           // int[] first = (int[])_lastStates.First().Clone();

         //   bool firstTime = true;
            while (_lastStates.Count > 0)
            {
                //if (firstTime)
                //{
                //    var st = _lastStates.Peek();
                //    Debug.Assert(_comparer.Equals(st, first));
                //    firstTime = false;
                //}


                int[] state = _lastStates.Pop();

                if (!Values.ContainsKey(state))
                {
                    Values.Add(state, 0.5);
                }

                double curValue = Values[state];
                double newValue = curValue + Alpha * (prevValue - curValue);
                Values[state] = newValue;
                prevValue = Gamma * newValue; //?
            }
        }

        /*================================================================
         *Move Logic
         *================================================================*/

        static bool ManMove = false;

        private static int ChoseMove()
        {
            if (_currentPlayer ==  PlayerType.RLbot)
            {
                //return getManualMove(player);
                return GetBestMove();
              //  return GetSemiRandomMove();
            }
            else
            {
                int selectedMove;

                if (!ManMove)
                {
                    selectedMove = GetSemiRandomMove();
                }
                else
                {
                    selectedMove = GetManualMove((int)_currentPlayer);
                }

                //int selectedMove = getManualMove(player);
                //int selectedMove = getRandomMove(player);
                
                //int selectedMove = GetBestMove(player);
               // Console.WriteLine("{0} selects position {1}.", PlayerName(player), selectedMove + 1);
                Debug.Assert(selectedMove != Unplayed);
                return selectedMove;
            }
        }

        private static int GetManualMove(int player)
        {
            DisplayGameBoard(GameBoard);
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

        private static int[] _stats = new int[9];
        private static ulong count = 0;
        private static double[] _ProcStats = new double[9];

        private static void UpdateProc()
        {
            for(int i = 0; i < 9; i++)
            {
                _ProcStats[i] = (double)_stats[i]/count;
            }
        }

        private static void TestRnd()
        {
            for (int i = 0; i < 100000; i++)
            {
                InitializeGameBoard();
                for (int j = 0; j < GameBoard.Length; j++)
                {
                    int x = _rnd.Next(0, GameBoard.Length);
                    
                    if (_rnd.NextDouble() < 0.3)
                    {
                        GameBoard[x] = 1;
                    }
                }

                GetRandomMove();
            }
        }
        // Check is it random? - Pkonoval
        private static int GetRandomMove()
        {
            int movesLeft = GameBoard.Count(position => position == Unplayed);
            int x = _rnd.Next(0, movesLeft);
            for (int i = 0; i < GameBoard.Length; i++)  // walk board ...
            {
                if (GameBoard[i] == Unplayed) // until we reach the unplayed move.
                {
                    if (x == 0)
                    {
                      //  _stats[i]++;
                      //  count++;
                        return i;
                    }

                    x--;
                }

            }
            return Unplayed;
        }

        // plays random if no winning move or needed block.
        private static int GetSemiRandomMove()
        {
            int player = (int)_currentPlayer;
            int posToPlay;
            if (CheckForWinningMove(player, out posToPlay))
                return posToPlay;
            if (CheckForBlockingMove(player, out posToPlay))
                return posToPlay;
            return GetRandomMove();
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
        private static int GetBestMove()
        {
            int player = (int) _currentPlayer;

            double res = _rnd.NextDouble();
            if (res < E)
            {
                return GetRandomMove();
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
                        Values.Add((int[])GameBoard.Clone(), 0.5);
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
            foreach (int[] line in Wins)
                if (TwoOfThreeMatchPlayer(player, line, out posToPlay))
                    return true;
            return false;
        }

        private static bool CheckForBlockingMove(int player, out int posToPlay)
        {
            posToPlay = Unplayed;
            foreach (int[] line in Wins)
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

        /// <summary>
        /// Update board
        /// </summary>
        /// <param name="boardPosition"></param>
        /// <returns>True when game is over</returns>
        private static bool PlayMove(int boardPosition)
        {
            GameBoard[boardPosition] = (int)_currentPlayer;
            _lastStates.Push((int[])GameBoard.Clone());
            Debug.Assert((new ArraysEqualityComparer()).Equals(_lastStates.First(), GameBoard));

            //DisplayGameBoard();
            if (IsGameWon())
            {
                if (_currentPlayer == PlayerType.RLbot)
                {
                    if(ManMove)
                    {
                        Console.WriteLine("RLbot win");
                    }
                    Debug.Assert(_comparer.Equals(_lastStates.First(), GameBoard));
                    aiWon++;
                    UpdateValues(1);

                }
                else
                {
                    if (ManMove)
                    {
                        Console.WriteLine("Human win");
                    }
                    Debug.Assert(_comparer.Equals(_lastStates.First(), GameBoard));
                    aiLoose++;
                    UpdateValues(0);
                }

                // Console.WriteLine("{0} has won the game!", PlayerName(CurrentPlayer));
                return true;
            }

            if (IsGameTied())
            {
                if (ManMove)
                {
                    Console.WriteLine("draws");
                }
                draws++;
                UpdateValues(0.5);
                // Console.WriteLine("Cat game ... we have a tie.");
                return true;
            }

            return false;
        }

        private static bool IsGameWon()
        {
          //  return Wins.Any(line => TakenBySamePlayer(line[0], line[1], line[2]));
            foreach (int[] line in Wins)
            {
                if (TakenBySamePlayer(line[0], line[1], line[2]))
                {
                    return true;
                }
            }

            return false;
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

        private static void NextCurrentPlayer()
        {
            _currentPlayer = (PlayerType)(GetNextPlayer((int)_currentPlayer));
        }

        private static int GetNextPlayer(int player)
        {
            return (player + 1) % 2;
        }

        private static void DisplayGameBoard(int[] board)
        {
            Console.WriteLine(" {0} | {1} | {2}", PieceAt(0, board), PieceAt(1, board), PieceAt(2, board));
            Console.WriteLine("---|---|---");
            Console.WriteLine(" {0} | {1} | {2}", PieceAt(3, board), PieceAt(4, board), PieceAt(5, board));
            Console.WriteLine("---|---|---");
            Console.WriteLine(" {0} | {1} | {2}", PieceAt(6, board), PieceAt(7, board), PieceAt(8, board));
            Console.WriteLine();
        }

        //private static string PieceAt(int boardPosition)
        //{
        //    if (GameBoard[boardPosition] == Unplayed)
        //        return (boardPosition + 1).ToString();  // display 1..9 on board rather than 0..8
        //    return PlayerToken(GameBoard[boardPosition]);
        //}


        private static string PieceAt(int boardPosition, int[] board)
        {
            if (board[boardPosition] == Unplayed)
                return (boardPosition + 1).ToString();  // display 1..9 on board rather than 0..8
            return PlayerToken(board[boardPosition]);
        }

        private static bool PlayAgain()
        {
            Console.WriteLine("\nDo you want to play again?");
            return Console.ReadKey(false).Key == ConsoleKey.Y;
        }
    }

}
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace RookWorks.Stockfish
{
    public class StockfishInterface
    {
        public event Action<string> OnStockfishOutput;
        private readonly Process _stockfishProcess;
        private readonly StreamWriter _stockfishInput;
        private readonly StreamReader _stockfishOutput;
        private readonly StockfishSettings _stockfishSettings;
        
        // Define a delegate for the Unity callback
        public delegate void UnityCallback(string message);

        // Import the native functions from the dylib
        [DllImport("libstockfish")] // Exclude the ".dylib" extension
        private static extern void Initialize(UnityCallback callback);

        [DllImport("libstockfish")]
        private static extern void ExecuteCommand(string cmd);

        [DllImport("libstockfish")]
        private static extern string ProcessEventsFromNative();

        public StockfishInterface()
        {
            /*var stockfishPath = Application.dataPath + "/External/stockfish";
            _stockfishProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = stockfishPath,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
    
            _stockfishProcess.Start();
            _stockfishInput = _stockfishProcess.StandardInput;
            _stockfishOutput = _stockfishProcess.StandardOutput;*/
            
            
            _stockfishSettings = Resources.Load<StockfishSettings>("StockfishSettings");
            // Initialize Stockfish with the UCI protocol
            Initialize(OnMessageReceived);
            //ReadResponseAsync();
            SendCommand("uci");
            SendCommand("isready");
            Read();

        }

        async void Read()
        {
            while (true)
            {
                await Task.Delay(3000);
                Debug.Log("reading");
                string result = ProcessEventsFromNative();
                Debug.Log(result);
            }
        }
        
        /*private string GetMessageFromNative() {
            IntPtr ptr = ProcessEventsFromNative();
            if (ptr == IntPtr.Zero) {
                return null; // No message in the queue
            }
            

            // Convert the pointer to a managed string
            string message = Marshal.PtrToStringAuto(ptr);
            if (string.IsNullOrEmpty(message)) {
                return "No message"; // Fallback for empty strings
            }
            return message;
        }*/

        private void OnMessageReceived(string message)
        {
            Debug.Log($"Message from Stockfish: {message}");
            HandleStockfishResponse(message);
        }

        private void StockfishIsReady()
        {
            ApplySettings();
            _stockfishSettings.ValueChanged += ApplySettings;
        }

        private async void ReadResponseAsync()
        {
            if (_stockfishProcess == null || _stockfishProcess.HasExited)
            {
                Debug.LogError("Stockfish process is not running!");
                return;
            }

            while (!_stockfishProcess.HasExited)
            {
                string line = await _stockfishOutput.ReadLineAsync();
                HandleStockfishResponse(line);
            }
            Debug.Log("stop reading");
        }

        void HandleStockfishResponse(string line)
        {
            if (line == null)
            {
                return;
            }

            if (line == "readyok")
            {
                StockfishIsReady();
            }

            Debug.Log(line);
            OnStockfishOutput?.Invoke(line);
        }

        private void ApplySettings()
        {
            if (_stockfishSettings.Threads != StockfishSettings.DefaultThreads)
                SendOption("Threads", _stockfishSettings.Threads.ToString());
            if (_stockfishSettings.Hash != StockfishSettings.DefaultHash)
                SendOption("Hash", _stockfishSettings.Hash.ToString());
            if (_stockfishSettings.Multipv != StockfishSettings.DefaultMultipv)
                SendOption("Multipv", _stockfishSettings.Multipv.ToString());
            if (_stockfishSettings.NumaPolicy != StockfishSettings.DefaultNumaPolicy)
                SendOption("NumaPolicy", _stockfishSettings.NumaPolicy);
            if (_stockfishSettings.Ponder != StockfishSettings.DefaultPonder)
                SendOption("Ponder", _stockfishSettings.Ponder.ToString());
            if (_stockfishSettings.EvalFile != StockfishSettings.DefaultEvalFile)
                SendOption("EvalFile", _stockfishSettings.EvalFile);
            if (_stockfishSettings.EvalFileSmall != StockfishSettings.DefaultEvalFileSmall)
                SendOption("EvalFileSmall", _stockfishSettings.EvalFileSmall);
            if (_stockfishSettings.Chess960 != StockfishSettings.DefaultChess960)
                SendOption("Chess960", _stockfishSettings.Chess960.ToString());
            if (_stockfishSettings.ShowWDL != StockfishSettings.DefaultShowWDL)
                SendOption("ShowWDL", _stockfishSettings.ShowWDL.ToString());
            if (_stockfishSettings.LimitStrength != StockfishSettings.DefaultLimitStrength)
                SendOption("LimitStrength", _stockfishSettings.LimitStrength.ToString());
            if (_stockfishSettings.LimitStrength && _stockfishSettings.Elo != StockfishSettings.DefaultElo)
                SendOption("Elo", _stockfishSettings.Elo.ToString());
            if (_stockfishSettings.SkillLevel != StockfishSettings.DefaultSkillLevel)
                SendOption("SkillLevel", _stockfishSettings.SkillLevel.ToString());
            if (_stockfishSettings.SyzygyPath != StockfishSettings.DefaultSyzygyPath)
                SendOption("SyzygyPath", _stockfishSettings.SyzygyPath);
            if (_stockfishSettings.SyzygyProbeDepth != StockfishSettings.DefaultSyzygyProbeDepth)
                SendOption("SyzygyProbeDepth", _stockfishSettings.SyzygyProbeDepth.ToString());
            if (_stockfishSettings.Syzygy50MoveRule != StockfishSettings.DefaultSyzygy50MoveRule)
                SendOption("Syzygy50MoveRule", _stockfishSettings.Syzygy50MoveRule.ToString());
            if (_stockfishSettings.SyzygyProbeLimit != StockfishSettings.DefaultSyzygyProbeLimit)
                SendOption("SyzygyProbeLimit", _stockfishSettings.SyzygyProbeLimit.ToString());
            if (_stockfishSettings.MoveOverhead != StockfishSettings.DefaultMoveOverhead)
                SendOption("MoveOverhead", _stockfishSettings.MoveOverhead.ToString());
        }

        private void SendOption(string optionName, string optionValue)
        {
            var command = $"setoption name {optionName} value {optionValue}";
            SendCommand(command);
        }

        /// <summary>
        /// Sends any command
        /// </summary>
        /// <param name="command">command</param>
        public void SendCommand(string command)
        {
            /*if (_stockfishProcess == null || _stockfishProcess.HasExited)
            {
                Debug.LogError("Stockfish process is not running!");
                return;
            }
            Debug.LogWarning($"Sending command to stockfish {command}");
            _stockfishInput.WriteLine(command);
            _stockfishInput.Flush();*/
            ExecuteCommand(command);
        }
        
        /// <summary>
        /// Sets initial standard position
        /// </summary>
        public void SetInitialPosition()
        {
            SendCommand($"position startpos");
        }
        
        /// <summary>
        /// Sets initial given position in fen format
        /// </summary>
        /// <param name="fen">position in fen format</param>
        public void SetInitialPosition(string fen)
        {
            SendCommand($"position {fen}");
        }

        /// <summary>
        /// Sets a position and applies moves to it.
        /// </summary>
        /// <param name="fen">initial position, if null or empty, it is standard position</param>
        /// <param name="moves">moves</param>
        public void SetPosition(string fen, List<string> moves)
        {
            string stringMoves = string.Join(' ', moves);
            SetPosition(fen, stringMoves);
        }

        /// <summary>
        /// Sets a position and applies moves to it.
        /// </summary>
        /// <param name="fen">initial position, if null or empty, it is standard position</param>
        /// <param name="moves">moves</param>
        public void SetPosition(string fen, string moves)
        {
            if (string.IsNullOrEmpty(fen))
            {
                SendCommand($"position startpos moves {moves}");
            }
            else
            {
                SendCommand($"position {fen} moves {moves}");
            }
        }

        /// <summary>
        /// Sets initial standard position and applies moves to it.
        /// </summary>
        /// <param name="moves">moves</param>
        public void SetPosition(List<string> moves)
        {
            string stringMoves = string.Join(' ', moves);
            SetPosition(stringMoves);
        }

        /// <summary>
        /// Sets initial standard position and applies moves to it.
        /// </summary>
        /// <param name="moves"></param>
        public void SetPosition(string moves)
        {
            SendCommand($"position startpos moves {moves}");
        }
        
        /// <summary>
        /// Starts calculating on the current position set up with the position command, go depth 245 will be executed
        /// </summary>
        public void Go()
        {
            SendCommand("go");
        }

        /// <summary>
        ///  Starts calculating on the current position set up with the position command limited by depth
        /// </summary>
        /// <param name="depth">Specifies how many moves to search ahead.</param>
        public void GoDepth(int depth)
        {
            SendCommand($"go depth {depth}");
        }
        
        /// <summary>
        /// Starts calculating on the current position set up with the position command limited by time
        /// </summary>
        /// <param name="timeMove">Specifies time in milliseconds to search.</param>
        public void GoTimeMove(int timeMove)
        {
            SendCommand($"go movetime {timeMove}");
        }
        
        /// <summary>
        /// Starts calculating on the current position set up with the position command limited by specific number of nodes
        /// </summary>
        /// <param name="nodes">Specifies individual positions Stockfish evaluates during its search.</param>
        public void GoNodes(int nodes)
        {
            SendCommand($"go nodes {nodes}");
        }

        /// <summary>
        /// Starts calculating on the current position set up with the position command limiting the search to only moves that can lead to checkmate.
        /// </summary>
        /// <param name="mate">Specifies number of moves required to checkmate.</param>
        public void GoMate(int mate)
        {
            SendCommand($"go mate {mate}");
        }
        
        /// <summary>
        /// Starts calculating on the current position set up with the position command simulation time control during a game,
        /// affecting how much time Stockfish spends on its next move.
        /// </summary>
        /// <param name="wTime">White time in milliseconds</param>
        /// <param name="bTime">Black time in milliseconds</param>
        public void GoWTimeBTime(int wTime, int bTime)
        {
            SendCommand($"go wtime {wTime} btime {bTime}");
        }
        
        /// <summary>
        /// Starts calculating on the current position set up with the position command simulation time control during a game,
        /// affecting how much time Stockfish spends on its next move.
        /// </summary>
        /// <param name="wTime">White time in milliseconds</param>
        /// <param name="bTime">Black time in milliseconds</param>
        /// <param name="wInc">White increment in milliseconds</param>
        /// <param name="bInc">Black increment in milliseconds</param>
        public void GoWTimeBTimeInc(int wTime,  int bTime, int wInc, int bInc)
        {
            SendCommand($"go wtime {wTime} btime {bTime} winc {wInc} binc {bInc}");
        }

        public void Stop()
        {
            SendCommand("stop");
        }

        public void Kill()
        {
            if (_stockfishProcess != null && !_stockfishProcess.HasExited)
            {
                SendCommand("quit");
                _stockfishProcess.Kill();
                _stockfishProcess.Close();
            }
        }
    }
}


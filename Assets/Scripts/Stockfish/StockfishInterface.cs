using System.Diagnostics;
using System.IO;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace RookWorks.Stockfish
{
    public class StockfishInterface 
    {
        private readonly Process _stockfishProcess;
        private readonly StreamWriter _stockfishInput;
        private readonly StreamReader _stockfishOutput;
    
        public StockfishInterface()
        {
            var stockfishPath = Application.dataPath + "/External/stockfish";
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
            _stockfishOutput = _stockfishProcess.StandardOutput;
    
            // Initialize Stockfish with the UCI protocol
            ReadResponseAsync();
            SendCommand("uci");
        }
    
        public void SendCommand(string command)
        {
            if (_stockfishProcess == null || _stockfishProcess.HasExited)
            {
                Debug.LogError("Stockfish process is not running!");
                return;
            }
    
            _stockfishInput.WriteLine(command);
            _stockfishInput.Flush();
        }
    
        public async void ReadResponseAsync()
        {
            if (_stockfishProcess == null || _stockfishProcess.HasExited)
            {
                Debug.LogError("Stockfish process is not running!");
                return;
            }
    
            while (!_stockfishProcess.HasExited)
            {
                string line = await _stockfishOutput.ReadLineAsync();
                if (line == null)
                {
                    break;
                }
                Debug.Log(line);
            }
            Debug.Log("stop reading");
        }
    
        public void Kill()
        {
            if (_stockfishProcess != null && !_stockfishProcess.HasExited)
            {
                _stockfishProcess.Kill();
                _stockfishProcess.Close();
            }
        }
    }
}


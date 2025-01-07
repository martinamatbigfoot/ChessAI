using System.Diagnostics;
using System.IO;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class StockfishInterface : MonoBehaviour
{
    private Process stockfishProcess;
    private StreamWriter stockfishInput;
    private StreamReader stockfishOutput;

    // Path to the Stockfish binary
    public string stockfishPath;

    void Start()
    {
        // Update this with the relative or absolute path to your Stockfish binary
        stockfishPath = Application.dataPath + "/External/stockfish";
        StartStockfish();
    }

    public void StartStockfish()
    {
        stockfishProcess = new Process
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

        stockfishProcess.Start();
        stockfishInput = stockfishProcess.StandardInput;
        stockfishOutput = stockfishProcess.StandardOutput;

        // Initialize Stockfish with the UCI protocol
        ReadResponseAsync();
        SendCommand("uci");
    }

    public void SendCommand(string command)
    {
        if (stockfishProcess == null || stockfishProcess.HasExited)
        {
            Debug.LogError("Stockfish process is not running!");
            return;
        }

        stockfishInput.WriteLine(command);
        stockfishInput.Flush();
    }

    public async void ReadResponseAsync()
    {
        if (stockfishProcess == null || stockfishProcess.HasExited)
        {
            Debug.LogError("Stockfish process is not running!");
            return;
        }

        while (!stockfishProcess.HasExited)
        {
            string line = await stockfishOutput.ReadLineAsync();
            if (line == null)
            {
                break;
            }
            Debug.Log(line);
        }
        Debug.Log("stop reading");
    }

    private void OnDestroy()
    {
        if (stockfishProcess != null && !stockfishProcess.HasExited)
        {
            stockfishProcess.Kill();
        }
    }
}

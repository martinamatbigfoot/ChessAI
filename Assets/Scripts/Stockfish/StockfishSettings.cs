using System;
using UnityEngine;

namespace RookWorks.Stockfish
{
    [CreateAssetMenu(fileName = "StockfishSettings", menuName = "RookWorks/StockfishSettings",order = 1)]
    public class StockfishSettings : ScriptableObject
    {
        public const int DefaultThreads = 1;
        public const int DefaultHash = 16;
        public const int DefaultMultipv = 1;
        public const string DefaultNumaPolicy = "auto";
        public const bool DefaultPonder = false;
        public const string DefaultEvalFile = "";
        public const string DefaultEvalFileSmall = "";
        public const bool DefaultChess960 = false;
        public const bool DefaultShowWDL = false;
        public const bool DefaultLimitStrength = false;
        public const int DefaultElo = 1320;
        public const int DefaultSkillLevel = 20;
        public const string DefaultSyzygyPath = "";
        public const int DefaultSyzygyProbeDepth = 1;
        public const bool DefaultSyzygy50MoveRule = true;
        public const int DefaultSyzygyProbeLimit = 7;
        public const int DefaultMoveOverhead = 10;
        
        
        public event Action ValueChanged;
        
        // Set the number of threads (CPU cores) Stockfish should use
        public int Threads = DefaultThreads;
        
        // Set the size of the hash table (in MB)
        public int Hash = DefaultHash;
        
        // Set the number of top moves Stockfish should return
        public int Multipv = DefaultMultipv;

        // Set the NUMA policy (specific to multi-CPU or multi-NUMA node systems)
        public string NumaPolicy = DefaultNumaPolicy;
        
        // Enable or disable "Pondering" (engine considers moves while waiting for the opponent's turn)
        public bool Ponder = DefaultPonder;

        // Set the EvalFiles for NNUE evaluation
        public string EvalFile = DefaultEvalFile;

        public string EvalFileSmall = DefaultEvalFileSmall;

        // Set the "UCI_Chess960" option (Chess960 mode, also known as Fischer Random Chess)
        public bool Chess960 = DefaultChess960;

        // Set the "UCI_ShowWDL" option (show wins, draws, and losses)
        public bool ShowWDL = DefaultShowWDL;

        // Set the "UCI_LimitStrength" option (whether to limit engine strength for training or testing)
        public bool LimitStrength = DefaultLimitStrength;

        // If Set "UCI_LimitStrength is enabled, it aims for an engine strength of the given Elo (between 1320 and 3190)
        public int Elo = DefaultElo;

        // Set the skill level (0-20)
        public int SkillLevel = DefaultSkillLevel;

        // Set the Syzygy tablebase path (if available)
        public string SyzygyPath = DefaultSyzygyPath;

        // Set the Syzygy probe depth (how deep to probe the tablebase)
        public int SyzygyProbeDepth = DefaultSyzygyProbeDepth;

        // Enable the 50-move rule for Syzygy tablebases
        public bool Syzygy50MoveRule = DefaultSyzygy50MoveRule;

        // Set the limit for Syzygy tablebase probes
        public int SyzygyProbeLimit = DefaultSyzygyProbeLimit;
        
        // Assume a time delay of x ms due to network and GUI overheads
        public int MoveOverhead = DefaultMoveOverhead;
        
        private void OnValidate()
        {
            // This method is called when any property is changed in the Inspector
            Debug.Log("StockfishSettings values changed in the Inspector.");

            ValueChanged?.Invoke();
        }
    }
}
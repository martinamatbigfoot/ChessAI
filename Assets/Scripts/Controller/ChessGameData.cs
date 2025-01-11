using System.Collections.Generic;

namespace RookWorks.Controller
{
    public class ChessGameData
    {
        public Dictionary<string, string> Metadata { get; set; }
        public List<string> Moves { get; set; }
    }
}

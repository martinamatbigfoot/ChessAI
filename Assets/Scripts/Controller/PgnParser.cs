using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace RookWorks.Controller
{
    public static class PGNParser
    {
        public static List<ChessGameData> ParsePGNFile(string fileContent)
        {
            var games = new List<ChessGameData>();

            // Regex to split PGN games
            var gamesSplit = Regex.Split(fileContent.Trim(), @"\r?\n\r?\n\r?\n");

            foreach (var game in gamesSplit)
            {
                if (string.IsNullOrWhiteSpace(game)) continue;

                var lines = game.Split('\n');
                var metadata = new Dictionary<string, string>();
                var moves = new List<string>();

                // Parse metadata
                foreach (var line in lines)
                {
                    if (line.StartsWith("["))
                    {
                        var match = Regex.Match(line, @"\[(\w+)\s+""([^""]+)""\]");
                        if (match.Success)
                        {
                            metadata[match.Groups[1].Value] = match.Groups[2].Value;
                        }
                    }
                    else
                    {
                        // Parse moves
                        moves.AddRange(ParseMoves(line));
                    }
                }

                games.Add(new ChessGameData
                {
                    Metadata = metadata,
                    Moves = moves
                });
            }

            return games;
        }

        private static IEnumerable<string> ParseMoves(string moveText)
        {
            // Remove move numbers and result indicators (e.g., "1-0", "1/2-1/2")
            var cleaned = Regex.Replace(moveText, @"\d+\.", "").Trim();
            cleaned = Regex.Replace(cleaned, @"(1-0|0-1|1/2-1/2|\*)", "").Trim();

            return cleaned.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}

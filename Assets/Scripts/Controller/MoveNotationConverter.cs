using System;
using System.Collections.Generic;
using System.Linq;
using RookWorks.Gameplay;

namespace RookWorks.Controller
{
    public static class MoveNotationConverter
    {
        public static string ConvertToUCI(string algebraicMove, Board board)
        {
            var squares = board.Squares;

            // Handle castling
            if (algebraicMove == "O-O") return board.IsWhiteTurn ? "e1g1" : "e8g8";
            if (algebraicMove == "O-O-O") return board.IsWhiteTurn ? "e1c1" : "e8c8";

            // Remove check (`+`) or checkmate (`#`) indicators
            if (algebraicMove.EndsWith("+") || algebraicMove.EndsWith("#"))
            {
                algebraicMove = algebraicMove[..^1];
            }

            // Handle promotions
            string promotion = null;
            if (algebraicMove.Contains("="))
            {
                int promotionIndex = algebraicMove.IndexOf('=');
                promotion = algebraicMove[(promotionIndex + 1)..];
                algebraicMove = algebraicMove[..promotionIndex];
            }

            // Extract destination square
            string destination = algebraicMove[^2..];
            int toX = destination[0] - 'a';
            int toY = destination[1] - '1';

            // Determine the piece type
            string pieceType = char.IsUpper(algebraicMove[0]) && !char.IsDigit(algebraicMove[1])
                ? algebraicMove[0].ToString().ToLower()
                : "p";

            if (char.IsUpper(algebraicMove[0]) && !char.IsDigit(algebraicMove[1]))
            {
                algebraicMove = algebraicMove[1..];
            }

            // Disambiguation
            int? disambiguateX = null;
            int? disambiguateY = null;
            if (algebraicMove.Length > 2)
            {
                char disambiguator = algebraicMove[0];
                if (char.IsLetter(disambiguator))
                    disambiguateX = disambiguator - 'a';
                else if (char.IsDigit(disambiguator))
                    disambiguateY = disambiguator - '1';

                algebraicMove = algebraicMove[1..];
            }

            // Find source squares
            var possibleSources = FindPossibleSources(pieceType, toX, toY, board);

            if (possibleSources.Count == 0)
                throw new Exception($"No valid source found for move: {algebraicMove}");

            (int fromX, int fromY) = possibleSources.Count == 1
                ? possibleSources[0]
                : DisambiguateSource(possibleSources, disambiguateX, disambiguateY);

            // Construct UCI
            return $"{(char)(fromX + 'a')}{fromY + 1}{(char)(toX + 'a')}{toY + 1}{promotion ?? ""}";
        }

        private static List<(int x, int y)> FindPossibleSources(string pieceType, int toX, int toY, Board board)
        {
            var sources = new List<(int x, int y)>();
            var squares = board.Squares;

            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    string piece = squares[y, x];
                    if (!string.IsNullOrEmpty(piece) && piece.ToLower() == pieceType && IsPlayerPiece(piece, board))
                    {
                        // Construct the move and check if it's legal
                        string move = $"{(char)(x + 'a')}{y + 1}{(char)(toX + 'a')}{toY + 1}";
                        if (board.IsLegalMove(move))
                        {
                            sources.Add((x, y));
                        }
                    }
                }
            }

            return sources;
        }

        private static (int x, int y) DisambiguateSource(List<(int x, int y)> sources, int? disambiguateX, int? disambiguateY)
        {
            if (disambiguateX.HasValue)
                sources = sources.Where(s => s.x == disambiguateX.Value).ToList();

            if (disambiguateY.HasValue)
                sources = sources.Where(s => s.y == disambiguateY.Value).ToList();

            if (sources.Count == 0)
                throw new Exception("No valid source found after disambiguation.");
            if (sources.Count > 1)
                throw new Exception("Ambiguous move and disambiguation failed.");

            return sources[0];
        }

        private static bool IsPlayerPiece(string piece, Board board)
        {
            return board.IsWhiteTurn ? char.IsUpper(piece[0]) : char.IsLower(piece[0]);
        }
    }
}
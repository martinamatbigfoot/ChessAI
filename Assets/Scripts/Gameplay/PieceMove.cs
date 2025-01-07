using System.Collections.Generic;

namespace RookWorks
{
    public class PieceMove 
    {
        public static List<(int, int)> GetLegalMoves(string piece, int x, int y, string[,] board, (int, int)? enPassantTarget)
        {
            var legalMoves = new List<(int, int)>();

            if (string.IsNullOrEmpty(piece)) return legalMoves;

            bool isWhite = char.IsUpper(piece[0]);
            piece = piece.ToLower();

            switch (piece)
            {
                case "p": // Pawn
                    legalMoves.AddRange(GetPawnMoves(x, y, isWhite, board, enPassantTarget));
                    break;
                case "r": // Rook
                    legalMoves.AddRange(GetSlidingMoves(x, y, board, new[] { (0, 1), (1, 0), (0, -1), (-1, 0) }));
                    break;
                case "n": // Knight
                    legalMoves.AddRange(GetKnightMoves(x, y, board, isWhite));
                    break;
                case "b": // Bishop
                    legalMoves.AddRange(GetSlidingMoves(x, y, board, new[] { (1, 1), (1, -1), (-1, 1), (-1, -1) }));
                    break;
                case "q": // Queen
                    legalMoves.AddRange(GetSlidingMoves(x, y, board, new[] { (0, 1), (1, 0), (0, -1), (-1, 0), (1, 1), (1, -1), (-1, 1), (-1, -1) }));
                    break;
                case "k": // King
                    legalMoves.AddRange(GetKingMoves(x, y, board, isWhite));
                    break;
            }

            return legalMoves;
        }

        public static List<(int, int)> GetPawnMoves(int x, int y, bool isWhite, string[,] board, (int, int)? enPassantTarget)
        {
            var legalMoves = new List<(int, int)>();
            int direction = isWhite ? 1 : -1;

            // Regular forward moves
            if (board[y + direction, x] == "") legalMoves.Add((x, y + direction));

            // Diagonal captures
            if (x > 0 && board[y + direction, x - 1] != "" && isWhite != char.IsUpper(board[y + direction, x - 1][0]))
                legalMoves.Add((x - 1, y + direction));

            if (x < 7 && board[y + direction, x + 1] != "" && isWhite != char.IsUpper(board[y + direction, x + 1][0]))
                legalMoves.Add((x + 1, y + direction));

            // En passant
            if (enPassantTarget != null)
            {
                (int enPassantX, int enPassantY) = enPassantTarget.Value;
                if (y + direction == enPassantY && (x - 1 == enPassantX || x + 1 == enPassantX))
                    legalMoves.Add((enPassantX, enPassantY));
            }

            return legalMoves;
        }

        private static List<(int, int)> GetKnightMoves(int x, int y, string[,] board, bool isWhite)
        {
            var moves = new List<(int, int)>();
            var knightOffsets = new[]
            {
                (2, 1), (1, 2), (-1, 2), (-2, 1), (-2, -1), (-1, -2), (1, -2), (2, -1)
            };

            foreach (var (dx, dy) in knightOffsets)
            {
                int nx = x + dx, ny = y + dy;
                if (IsInBounds(nx, ny) && (string.IsNullOrEmpty(board[ny, nx]) || char.IsUpper(board[ny, nx][0]) != isWhite))
                {
                    moves.Add((nx, ny));
                }
            }

            return moves;
        }

        private static List<(int, int)> GetSlidingMoves(int x, int y, string[,] board, (int, int)[] directions)
        {
            var moves = new List<(int, int)>();

            foreach (var (dx, dy) in directions)
            {
                int nx = x, ny = y;

                while (true)
                {
                    nx += dx;
                    ny += dy;

                    if (!IsInBounds(nx, ny)) break;

                    if (string.IsNullOrEmpty(board[ny, nx]))
                    {
                        moves.Add((nx, ny));
                    }
                    else
                    {
                        if (char.IsUpper(board[ny, nx][0]) != char.IsUpper(board[y, x][0]))
                        {
                            moves.Add((nx, ny)); // Capture
                        }
                        break;
                    }
                }
            }

            return moves;
        }

        private static List<(int, int)> GetKingMoves(int x, int y, string[,] board, bool isWhite)
        {
            var moves = new List<(int, int)>();
            var kingOffsets = new[]
            {
                (0, 1), (1, 0), (0, -1), (-1, 0), (1, 1), (1, -1), (-1, 1), (-1, -1)
            };

            foreach (var (dx, dy) in kingOffsets)
            {
                int nx = x + dx, ny = y + dy;
                if (IsInBounds(nx, ny) && (string.IsNullOrEmpty(board[ny, nx]) || char.IsUpper(board[ny, nx][0]) != isWhite))
                {
                    moves.Add((nx, ny));
                }
            }

            return moves;
        }

        private static bool IsInBounds(int x, int y)
        {
            return x >= 0 && x < 8 && y >= 0 && y < 8;
        }
    }
}

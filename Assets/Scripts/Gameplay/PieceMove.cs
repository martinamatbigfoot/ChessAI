using System.Collections.Generic;

namespace RookWorks.Gameplay
{
    public class PieceMove 
    {
        public static List<(int, int)> GetLegalMoves(string piece, int x, int y, string[,] board, (int, int)? enPassantTarget,
            bool isWhite, bool canCastleKingside, bool canCastleQeenside)
        {
            var legalMoves = new List<(int, int)>();

            if (string.IsNullOrEmpty(piece)) return legalMoves;
            piece = piece.ToLower();

            switch (piece)
            {
                case Piece.Pawn: // Pawn
                    legalMoves.AddRange(GetPawnMoves(x, y, isWhite, board, enPassantTarget));
                    break;
                case Piece.Rook: // Rook
                    legalMoves.AddRange(GetSlidingMoves(x, y, board, new[] { (0, 1), (1, 0), (0, -1), (-1, 0) }));
                    break;
                case Piece.Knight: // Knight
                    legalMoves.AddRange(GetKnightMoves(x, y, board, isWhite));
                    break;
                case Piece.Bishop: // Bishop
                    legalMoves.AddRange(GetSlidingMoves(x, y, board, new[] { (1, 1), (1, -1), (-1, 1), (-1, -1) }));
                    break;
                case Piece.Queen: // Queen
                    legalMoves.AddRange(GetSlidingMoves(x, y, board, new[] { (0, 1), (1, 0), (0, -1), (-1, 0), (1, 1), (1, -1), (-1, 1), (-1, -1) }));
                    break;
                case Piece.King: // King
                    legalMoves.AddRange(GetKingMoves(x, y, board, isWhite, canCastleKingside, canCastleQeenside));
                    break;
            }

            return legalMoves;
        }

        public static List<(int, int)> GetPawnMoves(int x, int y, bool isWhite, string[,] board, (int, int)? enPassantTarget)
        {
            var legalMoves = new List<(int, int)>();
            int direction = isWhite ? 1 : -1;

            // Regular forward moves
            if (board[y + direction, x] == "")
            {
                legalMoves.Add((x, y + direction));
                int startingRank = isWhite ? 1 : 6;
                // Double move from starting rank
                if (y == startingRank && board[y + 2 * direction, x] == "")
                {
                    legalMoves.Add((x, y + 2 * direction));
                }
            }

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

        private static List<(int, int)> GetKingMoves(int x, int y, string[,] board, bool isWhite, bool canCastleKingSide, bool canCastleQueenSide)
        {
            var moves = new List<(int, int)>();
            var kingOffsets = new[]
            {
                (0, 1), (1, 0), (0, -1), (-1, 0), (1, 1), (1, -1), (-1, 1), (-1, -1)
            };

            // Standard King moves
            foreach (var (dx, dy) in kingOffsets)
            {
                int nx = x + dx, ny = y + dy;
                if (IsInBounds(nx, ny) && (string.IsNullOrEmpty(board[ny, nx]) || char.IsUpper(board[ny, nx][0]) != isWhite))
                {
                    moves.Add((nx, ny));
                }
            }

            // Castling logic
            if (canCastleKingSide)
            {
                // Check if squares between the king and rook are empty and not under attack
                if (IsInBounds(x + 1, y) && string.IsNullOrEmpty(board[y, x + 1]) &&
                    IsInBounds(x + 2, y) && string.IsNullOrEmpty(board[y, x + 2]) &&
                    !IsSquareUnderAttack(x, y, board, !isWhite) && // King's current square
                    !IsSquareUnderAttack(x + 1, y, board, !isWhite) && // Square king moves through
                    !IsSquareUnderAttack(x + 2, y, board, !isWhite)) // Square king lands on
                {
                    moves.Add((x + 2, y)); // Add the castling move
                }
            }

            if (canCastleQueenSide)
            {
                // Check if squares between the king and rook are empty and not under attack
                if (IsInBounds(x - 1, y) && string.IsNullOrEmpty(board[y, x - 1]) &&
                    IsInBounds(x - 2, y) && string.IsNullOrEmpty(board[y, x - 2]) &&
                    IsInBounds(x - 3, y) && string.IsNullOrEmpty(board[y, x - 3]) &&
                    !IsSquareUnderAttack(x, y, board, !isWhite) && // King's current square
                    !IsSquareUnderAttack(x - 1, y, board, !isWhite) && // Square king moves through
                    !IsSquareUnderAttack(x - 2, y, board, !isWhite)) // Square king lands on
                {
                    moves.Add((x - 2, y)); // Add the castling move
                }
            }

            return moves;
        }
        
        // Utility to check if a square is under attack
        private static bool IsSquareUnderAttack(int x, int y, string[,] board, bool byWhite)
        {
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (!string.IsNullOrEmpty(board[j, i]) && char.IsUpper(board[j, i][0]) == byWhite)
                    {
                        var attackerMoves = GetLegalMoves(board[j, i], i, j, board, null, byWhite, false, false);
                        if (attackerMoves.Contains((x, y)))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private static bool IsInBounds(int x, int y)
        {
            return x >= 0 && x < 8 && y >= 0 && y < 8;
        }
    }
}

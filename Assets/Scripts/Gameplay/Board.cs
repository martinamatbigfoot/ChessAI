using System;
using System.Text;
using UnityEngine;

namespace RookWorks
{
    public class Board 
    {
        private string[,] _board = new string[8, 8];
        private bool _whiteKingsideCastle = true;
        private bool _whiteQueensideCastle = true;
        private bool _blackKingsideCastle = true;
        private bool _blackQueensideCastle = true;
        private (int, int) _whiteKingPosition = (4, 0);
        private (int, int) _blackKingPosition = (4, 7);
        private (int, int)? _enPassantTarget;
        
        public Board()
        {
            ResetBoard();
        }

        public void ResetBoard()
        {
            // Standard chess starting positions
            _board = new string[,]
            {
                { "R", "N", "B", "Q", "K", "B", "N", "R" }, // White pieces
                { "P", "P", "P", "P", "P", "P", "P", "P" },
                { "",  "",  "",  "",  "",  "",  "",  "" },
                { "",  "",  "",  "",  "",  "",  "",  "" },
                { "",  "",  "",  "",  "",  "",  "",  "" },
                { "",  "",  "",  "",  "",  "",  "",  "" },
                { "p", "p", "p", "p", "p", "p", "p", "p" }, // Black pieces
                { "r", "n", "b", "q", "k", "b", "n", "r" }
            };
            
            _whiteKingsideCastle = true;
            _whiteQueensideCastle = true;
            _blackKingsideCastle = true;
            _blackQueensideCastle = true;
        }

        public void SetCustomPosition(string fen)
        {
            // Start with a clean board
            _board = new string[,]
            {
                { "",  "",  "",  "",  "",  "",  "",  "" }, // White pieces
                { "",  "",  "",  "",  "",  "",  "",  "" },
                { "",  "",  "",  "",  "",  "",  "",  "" },
                { "",  "",  "",  "",  "",  "",  "",  "" },
                { "",  "",  "",  "",  "",  "",  "",  "" },
                { "",  "",  "",  "",  "",  "",  "",  "" },
                { "",  "",  "",  "",  "",  "",  "",  "" }, // Black pieces
                { "",  "",  "",  "",  "",  "",  "",  "" }
            };

            // Split the FEN string into its components
            string[] parts = fen.Split(' ');
            if (parts.Length < 4)
            {
                throw new ArgumentException("Invalid FEN string: must have at least 4 fields.");
            }

            string piecePlacement = parts[0];
            string activeColor = parts[1];
            string castlingAvailability = parts[2];
            string enPassantTarget = parts[3];
            // Optional: parts[4] is the halfmove clock, parts[5] is the fullmove number

            // Parse the piece placement
            string[] ranks = piecePlacement.Split('/');
            if (ranks.Length != 8)
            {
                throw new ArgumentException("Invalid FEN string: must have exactly 8 ranks in the piece placement.");
            }

            for (int y = 0; y < 8; y++)
            {
                int x = 0;
                foreach (char c in ranks[7 - y]) // Parse from rank 8 to rank 1
                {
                    if (char.IsDigit(c))
                    {
                        // Empty squares (e.g., '3' means 3 empty squares)
                        x += c - '0';
                    }
                    else
                    {
                        // Piece character
                        if (x >= 8)
                        {
                            throw new ArgumentException("Invalid FEN string: too many pieces in a rank.");
                        }
                        _board[y, x] = c.ToString();
                        x++;
                    }
                }

                if (x != 8)
                {
                    throw new ArgumentException("Invalid FEN string: rank must have exactly 8 squares.");
                }
            }

            // Parse active color
            bool isWhiteToMove = activeColor == "w";
            Debug.Log($"Active color: {(isWhiteToMove ? "White" : "Black")}");

            // Parse castling availability
            _whiteKingsideCastle = castlingAvailability.Contains('K');
            _whiteQueensideCastle = castlingAvailability.Contains('Q');
            _blackKingsideCastle = castlingAvailability.Contains('k');
            _blackQueensideCastle = castlingAvailability.Contains('q');

            // Parse en passant target square
            _enPassantTarget = enPassantTarget == "-" ? null : ParseSquare(enPassantTarget);
            Debug.Log($"En Passant target: {enPassantTarget}");
        }

        private (int, int) ParseSquare(string square)
        {
            int x = square[0] - 'a';
            int y = square[1] - '1';
            return (x, y);
        }
        
        public string GetPiece(int x, int y)
        {
            if (IsOutOfBounds(x, y))
            {
                return null;
            }

            return _board[y, x];
        }

        public bool IsMoveLegal(string move)
        {
            if (move == "O-O" || move == "O-O-O")
            {
                return IsCastlingLegal(move);
            }
            
            int fromX = move[0] - 'a';
            int fromY = move[1] - '1';
            int toX = move[2] - 'a';
            int toY = move[3] - '1';

            if (IsOutOfBounds(fromX, fromY) || IsOutOfBounds(toX, toY))
            {
                return false;
            }

            string piece = _board[fromY, fromX];
            if (string.IsNullOrEmpty(piece))
            {
                return false;
            }

            // Check if the move is valid for the piece
            var legalMoves = PieceMove.GetLegalMoves(piece, fromX, fromY, _board, _enPassantTarget);
            if (!legalMoves.Contains((toX, toY)))
            {
                return false;
            }

            // Simulate the move and check if it leaves the king in check
            bool isWhite = char.IsUpper(piece[0]);
            if (!WouldMoveResolveCheck(fromX, fromY, toX, toY, isWhite))
            {
                return false;
            }

            return true;
        }

        private bool IsOutOfBounds(int x, int y)
        {
            return x < 0 || x >= 8 || y < 0 || y >= 8;
        }
        
        public void ApplyMove(string move)
        {
            if (move == "O-O")
            {
                PerformKingsideCastle();
            }
            else if (move == "O-O-O")
            {
                PerformQueensideCastle();
            }
            
            else
            {
                int fromX = move[0] - 'a';
                int fromY = move[1] - '1';
                int toX = move[2] - 'a';
                int toY = move[3] - '1';

                Debug.Log($"moving from [{fromX},{fromY}] to [{toX},{toY}]");

                string piece = _board[fromY, fromX];
                _board[toY, toX] = piece;
                _board[fromY, fromX] = "";

                if (IsEnPassantMove(fromX, fromY, toX, toY))
                {
                    // Remove the captured pawn
                    int capturedPawnY = toY + (fromY > toY ? 1 : -1);
                    _board[capturedPawnY, toX] = "";
                }


                UpdateCastlingFlags(fromX, fromY, piece);
                if (piece == "K" || piece == "k")
                {
                    UpdateKingPosition(char.IsUpper(piece[0]), toX, toY);
                }

                if (piece.ToLower() == "p" && Math.Abs(toY - fromY) == 2)
                {
                    // Update en passant target when a pawn moves two squares forward
                    _enPassantTarget = (fromX, (fromY + toY) / 2);
                }
                else
                {
                    // Clear the en passant target otherwise
                    _enPassantTarget = null;
                }
            }
        }

        public string GetStringBoard()
        {
            StringBuilder sb = new StringBuilder();
            for (int y = 7; y >= 0; y--)
            {
                for (int x = 0; x < 8; x++)
                {
                    string piece = _board[y, x];
                    sb.Append(string.IsNullOrEmpty(piece) ? "." : piece);
                    sb.Append(" ");
                }
                sb.AppendLine();
            }

            return sb.ToString();
        }
        
        private void PerformKingsideCastle()
        {
            if (_whiteKingsideCastle)
            {
                // White short castle
                _board[0, 6] = "K";
                _board[0, 4] = "";
                _board[0, 5] = "R";
                _board[0, 7] = "";
                _whiteKingsideCastle = false;
                _whiteQueensideCastle = false;
            }
            else if (_blackKingsideCastle)
            {
                // Black short castle
                _board[7, 6] = "k";
                _board[7, 4] = "";
                _board[7, 5] = "r";
                _board[7, 7] = "";
                _blackKingsideCastle = false;
                _blackQueensideCastle = false;
            }
        }

        private void PerformQueensideCastle()
        {
            if (_whiteQueensideCastle)
            {
                // White long castle
                _board[0, 2] = "K";
                _board[0, 4] = "";
                _board[0, 3] = "R";
                _board[0, 0] = "";
                _whiteKingsideCastle = false;
                _whiteQueensideCastle = false;
            }
            else if (_blackQueensideCastle)
            {
                // Black long castle
                _board[7, 2] = "k";
                _board[7, 4] = "";
                _board[7, 3] = "r";
                _board[7, 0] = "";
                _blackKingsideCastle = false;
                _blackQueensideCastle = false;
            }
        }

        private bool IsCastlingLegal(string move)
        {
            if (move == "O-O") // kingside castle
            {
                if (_whiteKingsideCastle)
                {
                    return IsPathClear(4, 0, 6, 0) &&
                           !IsKingInCheck(true) && // Current position
                           !WouldMoveThroughCheck(4, 0, 5, 0, true) && // Through square
                           !WouldMoveThroughCheck(4, 0, 6, 0, true); // Destination square
                }

                if (_blackKingsideCastle)
                {
                    return IsPathClear(4, 7, 6, 7) &&
                           !IsKingInCheck(false) &&
                           !WouldMoveThroughCheck(4, 7, 5, 7, false) &&
                           !WouldMoveThroughCheck(4, 7, 6, 7, false);
                }
            }
            else if (move == "O-O-O") // queenside castle
            {
                if (_whiteQueensideCastle)
                {
                    return IsPathClear(4, 0, 2, 0) &&
                           !IsKingInCheck(true) &&
                           !WouldMoveThroughCheck(4, 0, 3, 0, true) &&
                           !WouldMoveThroughCheck(4, 0, 2, 0, true);
                }

                if (_blackQueensideCastle)
                {
                    return IsPathClear(4, 7, 2, 7) &&
                           !IsKingInCheck(false) &&
                           !WouldMoveThroughCheck(4, 7, 3, 7, false) &&
                           !WouldMoveThroughCheck(4, 7, 2, 7, false);
                }
            }

            return false;
        }

        private bool WouldMoveThroughCheck(int fromX, int fromY, int toX, int toY, bool isWhite)
        {
            // Simulate the move
            string temp = _board[toY, toX];
            _board[toY, toX] = _board[fromY, fromX];
            _board[fromY, fromX] = "";

            bool inCheck = IsKingInCheck(isWhite);

            // Undo the move
            _board[fromY, fromX] = _board[toY, toX];
            _board[toY, toX] = temp;

            return inCheck;
        }

        private bool IsPathClear(int fromX, int fromY, int toX, int toY)
        {
            int step = fromX < toX ? 1 : -1;
            for (int x = fromX + step; x != toX; x += step)
            {
                if (!string.IsNullOrEmpty(_board[fromY, x]))
                {
                    return false;
                }
            }

            return true;
        }

        public bool IsKingInCheck(bool isWhite)
        {
            // Locate the king
            (int kingX, int kingY) = FindKing(isWhite);

            // Check if any opponent piece can attack the king
            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    string piece = _board[y, x];
                    if (!string.IsNullOrEmpty(piece) && char.IsUpper(piece[0]) != isWhite)
                    {
                        var legalMoves = PieceMove.GetLegalMoves(piece, x, y, _board, _enPassantTarget);
                        if (legalMoves.Contains((kingX, kingY)))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }
        
        public bool IsCheckmate(bool isWhite)
        {
            if (!IsKingInCheck(isWhite)) return false;

            // Check if the player has any legal moves to escape
            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    string piece = _board[y, x];
                    if (!string.IsNullOrEmpty(piece) && char.IsUpper(piece[0]) == isWhite)
                    {
                        var legalMoves = PieceMove.GetLegalMoves(piece, x, y, _board, _enPassantTarget);
                        foreach (var (toX, toY) in legalMoves)
                        {
                            if (WouldMoveResolveCheck(x, y, toX, toY, isWhite))
                            {
                                return false; // Found a valid escape move
                            }
                        }
                    }
                }
            }

            return true; // No moves resolve the check
        }
        
        public bool IsStalemate(bool isWhite)
        {
            if (IsKingInCheck(isWhite)) return false;

            // Check if the player has any legal moves
            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    string piece = _board[y, x];
                    if (!string.IsNullOrEmpty(piece) && char.IsUpper(piece[0]) == isWhite)
                    {
                        var legalMoves = PieceMove.GetLegalMoves(piece, x, y, _board, _enPassantTarget);
                        foreach (var (toX, toY) in legalMoves)
                        {
                            if (WouldMoveResolveCheck(x, y, toX, toY, isWhite))
                            {
                                return false; // Found a valid move
                            }
                        }
                    }
                }
            }

            return true; // No legal moves and not in check
        }

        private bool WouldMoveResolveCheck(int fromX, int fromY, int toX, int toY, bool isWhite)
        {
            // Simulate the move
            string temp = _board[toY, toX];
            _board[toY, toX] = _board[fromY, fromX];
            _board[fromY, fromX] = "";

            // Check if the king is in check after the move
            bool inCheck = IsKingInCheck(isWhite);

            // Undo the move
            _board[fromY, fromX] = _board[toY, toX];
            _board[toY, toX] = temp;

            return !inCheck;
        }
        
        private (int, int) FindKing(bool isWhite)
        {
            return isWhite ? _whiteKingPosition : _blackKingPosition;
        }

        private void UpdateKingPosition(bool isWhite, int toX, int toY)
        {
            if (isWhite) _whiteKingPosition = (toX, toY);
            else _blackKingPosition = (toX, toY);
        }

        private void UpdateCastlingFlags(int fromX, int fromY, string piece)
        {
            if (piece == "K")
            {
                _whiteKingsideCastle = false;
                _whiteQueensideCastle = false;
            }
            if (piece == "R" && fromX == 0 && fromY == 0) _whiteQueensideCastle = false;
            if (piece == "R" && fromX == 7 && fromY == 0) _whiteKingsideCastle = false;
            if (piece == "k")
            {
                _blackKingsideCastle = false;
                _blackQueensideCastle = false;
            }
            if (piece == "r" && fromX == 0 && fromY == 0) _blackQueensideCastle = false;
            if (piece == "r" && fromX == 7 && fromY == 0) _blackKingsideCastle = false;
        }
        
        public bool IsEnPassantMove(int fromX, int fromY, int toX, int toY)
        {
            if (_enPassantTarget == null) return false;

            (int targetX, int targetY) = _enPassantTarget.Value;
            if (toX == targetX && toY == targetY)
            {
                string piece = _board[fromY, fromX];
                return piece.ToLower() == "p";
            }

            return false;
        }
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;

namespace RookWorks.Gameplay
{
    public class Board
    {
        public enum BoardState
        {
            Playing,
            WhiteWon,
            BlackWon,
            Tie,
            Reviewing
        }


        public string[,] Squares => _squares;
        private string[,] _squares = new string[8, 8];
        private bool _whiteCanCastleKingside = true;
        private bool _whiteCanCastleQueenside = true;
        private bool _blackCanCastleKingside = true;
        private bool _blackCanCastleQueenside = true;
        private (int, int) _whiteKingPosition = (4, 0);
        private (int, int) _blackKingPosition = (4, 7);
        private (int, int)? _enPassantTarget;
        public bool IsWhiteTurn => _isWhiteTurn;
        private bool _isWhiteTurn;

        public bool IsLiveBoard
        {
            get
            {
                return _boardState != BoardState.Playing && _moveIndex == _fens.Count;
            }
        }
        
        private BoardState _boardState;
        
        public List<string> WhitePieceRemoved => _whitePieceRemoved;
        private List<string> _whitePieceRemoved = new();
        public List<string> BlackPieceRemoved => _blackPieceRemoved;
        private List<string> _blackPieceRemoved = new();

        private List<string> _fens = new();

        private int _moveIndex;
        
        public Board()
        {
            ResetBoard();
        }

        public void ResetBoard()
        {
            // Standard chess starting positions
            _squares = new string[,]
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
            
            _whiteCanCastleKingside = true;
            _whiteCanCastleQueenside = true;
            _blackCanCastleKingside = true;
            _blackCanCastleQueenside = true;
            _isWhiteTurn = true;
            _boardState = BoardState.Playing;
            _whitePieceRemoved.Clear();
            _blackPieceRemoved.Clear();
            _moveIndex = 0;
            _fens.Clear();
            _fens.Add(GetFen());
        }

        public void SetLoadGame()
        {
            _boardState = BoardState.Reviewing;
            SetInitialPosition();
        }

        public void SetCustomPosition(string fen, bool isReset = false)
        {
            // Start with a clean board
            _squares = new string[,]
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
                        _squares[y, x] = c.ToString();
                        x++;
                    }
                }

                if (x != 8)
                {
                    throw new ArgumentException("Invalid FEN string: rank must have exactly 8 squares.");
                }
            }
            _whitePieceRemoved.Clear();
            _blackPieceRemoved.Clear();
            if (isReset)
            {
                _moveIndex = 0;
                _fens.Clear();
                _fens.Add(GetFen());
            }

            // Parse active color
            _isWhiteTurn = activeColor == "w";
            Debug.Log($"Active color: {(_isWhiteTurn ? "White" : "Black")}");

            // Parse castling availability
            _whiteCanCastleKingside = castlingAvailability.Contains('K');
            _whiteCanCastleQueenside = castlingAvailability.Contains('Q');
            _blackCanCastleKingside = castlingAvailability.Contains('k');
            _blackCanCastleQueenside = castlingAvailability.Contains('q');

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

        private void SetInitialPosition()
        {
            if (_fens.Count > 0)
            {
                _moveIndex = 0;
                SetCustomPosition(_fens[0]);
            }
        }

        public void PreviousPosition()
        {
            if (_moveIndex > 0)
            {
                _moveIndex--;
                SetCustomPosition(_fens[_moveIndex]);
            }
        }

        public void NextPosition()
        {
            if (_moveIndex < _fens.Count - 1)
            {
                _moveIndex++;
                SetCustomPosition(_fens[_moveIndex]);
            }
        }

        public bool TryMove(string move)
        {
            if (IsLegalMove(move))
            {
                ApplyMove(move);
                _fens.Add(GetFen());
                _moveIndex++;
                return true;
            }

            return false;
        }
        
        public string GetFen()
        {
            // 1. Piece Placement
            string piecePlacement = "";
            for (int y = 7; y >= 0; y--)
            {
                int emptyCount = 0;
                for (int x = 0; x < 8; x++)
                {
                    string piece = _squares[y, x];
                    if (string.IsNullOrEmpty(piece))
                    {
                        emptyCount++;
                    }
                    else
                    {
                        if (emptyCount > 0)
                        {
                            piecePlacement += emptyCount.ToString();
                            emptyCount = 0;
                        }
                        piecePlacement += piece;
                    }
                }
                if (emptyCount > 0)
                {
                    piecePlacement += emptyCount.ToString();
                }
                if (y > 0)
                {
                    piecePlacement += "/";
                }
            }

            // 2. Active Color
            string activeColor = _isWhiteTurn ? "w" : "b";

            // 3. Castling Availability
            string castlingAvailability = "";
            if (_whiteCanCastleKingside) castlingAvailability += "K";
            if (_whiteCanCastleQueenside) castlingAvailability += "Q";
            if (_blackCanCastleKingside) castlingAvailability += "k";
            if (_blackCanCastleQueenside) castlingAvailability += "q";
            if (string.IsNullOrEmpty(castlingAvailability)) castlingAvailability = "-";

            // 4. En Passant Target Square
            string enPassantTarget = _enPassantTarget != null
                ? $"{(char)('a' + _enPassantTarget.Value.Item1)}{_enPassantTarget.Value.Item2 + 1}"
                : "-";

            // 5. Halfmove Clock (Placeholder: Needs to track moves for 50-move rule)
            int halfmoveClock = 0; // Implement tracking for this based on gameplay

            // 6. Fullmove Number (Placeholder: Needs to track full moves)
            int fullmoveNumber = (_moveIndex / 2) + 1; // Fullmove starts at 1, incremented after Black's turn

            // Combine all components into a FEN string
            return $"{piecePlacement} {activeColor} {castlingAvailability} {enPassantTarget} {halfmoveClock} {fullmoveNumber}";
        }

        public bool IsPlayerPieceInSquare(string square)
        {
            int fromX = square[0] - 'a';
            int fromY = square[1] - '1';
            if (_squares[fromY, fromX].Length > 0)
                return _isWhiteTurn ? char.IsUpper(_squares[fromY, fromX][0]) : char.IsLower(_squares[fromY, fromX][0]);
            return false;
        }

        private bool IsPlayerPiece(string piece)
        {
            // Check if the piece belongs to the current player
            return _isWhiteTurn ? char.IsUpper(piece[0]) : char.IsLower(piece[0]);
        }

        public bool IsLegalMove(string move)
        {
            int fromX = move[0] - 'a';
            int fromY = move[1] - '1';
            int toX = move[2] - 'a';
            int toY = move[3] - '1';

            if (IsOutOfBounds(fromX, fromY) || IsOutOfBounds(toX, toY))
            {
                Debug.Log("out of bounds");
                return false;
            }

            

            string piece = _squares[fromY, fromX];
            if (string.IsNullOrEmpty(piece) || !IsPlayerPiece(piece))
            {
                Debug.Log("wrong piece");
                return false;
            }
            
            
            bool isWhite = char.IsUpper(piece[0]);
            
            bool canCastleKingside = !isWhite ? _blackCanCastleKingside : _whiteCanCastleKingside;
            bool canCastleQueenside = !isWhite ? _blackCanCastleQueenside : _whiteCanCastleQueenside;
            
            // Check if the move is valid for the piece
            var legalMoves = PieceMove.GetLegalMoves(piece, fromX, fromY, _squares, _enPassantTarget, isWhite,
                canCastleKingside, canCastleQueenside);
            if (!legalMoves.Contains((toX, toY)))
            {
                Debug.Log($"Illegal move {move}: legal moves for piece {piece} are {legalMoves.Count}");
                return false;
            }

            // Simulate the move and check if it leaves the king in check
            if (!WouldMoveResolveCheck(fromX, fromY, toX, toY))
            {
                Debug.Log("not resolve check ");
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
            int fromX = move[0] - 'a';
            int fromY = move[1] - '1';
            int toX = move[2] - 'a';
            int toY = move[3] - '1';

            Debug.Log($"moving from [{fromX},{fromY}] to [{toX},{toY}]");

            string piece = _squares[fromY, fromX];
            
            if (piece.ToLower() == Piece.King && Math.Abs(fromX - toX) == 2)
            {
                if (toX > fromX) // Kingside castling
                {
                    PerformKingsideCastle();
                }
                else // Queenside castling
                {
                    PerformQueensideCastle();
                }
                UpdateKingPosition(char.IsUpper(piece[0]), toX, toY);
            }
            else
            {
                // Check if the move is a promotion
                bool isPromotion = piece.ToLower() == Piece.Pawn &&
                                   (toY == 0 || toY == 7);

                if (isPromotion && move.Length == 5)
                {
                    // Handle pawn promotion
                    char promotionPiece = move[4]; // Get the promotion piece (e.g., 'q', 'r', 'b', 'n')
                    _squares[toY, toX] = PromotePawn(piece[0], promotionPiece);
                }
                else
                {
                    UpdatePiecesRemoved(toX, toY);
                    _squares[toY, toX] = piece;
                }
                
                if (IsEnPassantMove(fromX, fromY, toX, toY))
                {
                    // Remove the captured pawn
                    int capturedPawnY = toY + (fromY > toY ? 1 : -1);
                    UpdatePiecesRemoved(toX, capturedPawnY);
                    _squares[capturedPawnY, toX] = "";
                }

                _squares[fromY, fromX] = "";

                UpdateCastlingFlags(fromX, fromY, piece);
                if (piece.ToLower() == Piece.King)
                {
                    UpdateKingPosition(char.IsUpper(piece[0]), toX, toY);
                }

                if (piece.ToLower() == Piece.Pawn && Math.Abs(toY - fromY) == 2)
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

            _isWhiteTurn = !_isWhiteTurn;
        }

        private void UpdatePiecesRemoved(int x, int y)
        {
            var piece = _squares[y, x];
            if (piece != "")
            {
                if (_isWhiteTurn)
                {
                    _blackPieceRemoved.Add(piece);
                }
                else
                {
                    _whitePieceRemoved.Add(piece);
                }
            }
        }
        
        private string PromotePawn(char color, char promotionPiece)
        {
            // Convert the promotion piece to uppercase (white) or lowercase (black) based on the pawn's color
            char promotedPiece = char.IsUpper(color) ? char.ToUpper(promotionPiece) : char.ToLower(promotionPiece);

            // Return the promoted piece as a string
            return promotedPiece.ToString();
        }

        private void PerformKingsideCastle()
        {
            if (_whiteCanCastleKingside)
            {
                // White short castle
                _squares[0, 6] = "K";
                _squares[0, 4] = "";
                _squares[0, 5] = "R";
                _squares[0, 7] = "";
                _whiteCanCastleKingside = false;
                _whiteCanCastleQueenside = false;
            }
            else if (_blackCanCastleKingside)
            {
                // Black short castle
                _squares[7, 6] = "k";
                _squares[7, 4] = "";
                _squares[7, 5] = "r";
                _squares[7, 7] = "";
                _blackCanCastleKingside = false;
                _blackCanCastleQueenside = false;
            }
        }

        private void PerformQueensideCastle()
        {
            if (_whiteCanCastleQueenside)
            {
                // White long castle
                _squares[0, 2] = "K";
                _squares[0, 4] = "";
                _squares[0, 3] = "R";
                _squares[0, 0] = "";
                _whiteCanCastleKingside = false;
                _whiteCanCastleQueenside = false;
            }
            else if (_blackCanCastleQueenside)
            {
                // Black long castle
                _squares[7, 2] = "k";
                _squares[7, 4] = "";
                _squares[7, 3] = "r";
                _squares[7, 0] = "";
                _blackCanCastleKingside = false;
                _blackCanCastleQueenside = false;
            }
        }

        private bool IsKingInCheck()
        {
            // Locate the king
            (int kingX, int kingY) = FindKing();

            // Check if any opponent piece can attack the king
            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    string piece = _squares[y, x];
                    if (!string.IsNullOrEmpty(piece) && char.IsUpper(piece[0]) != _isWhiteTurn)
                    {
                        var legalMoves = PieceMove.GetLegalMoves(piece, x, y, _squares, _enPassantTarget, !_isWhiteTurn, false, false);
                        if (legalMoves.Contains((kingX, kingY)))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }
        
        public bool IsCheckmate()
        {
            if (!IsKingInCheck()) return false;

            // Check if the player has any legal moves to escape
            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    string piece = _squares[y, x];
                    if (!string.IsNullOrEmpty(piece) && char.IsUpper(piece[0]) == _isWhiteTurn)
                    {
                        var legalMoves = PieceMove.GetLegalMoves(piece, x, y, _squares, _enPassantTarget, !_isWhiteTurn, false, false);
                        foreach (var (toX, toY) in legalMoves)
                        {
                            if (WouldMoveResolveCheck(x, y, toX, toY))
                            {
                                return false; // Found a valid escape move
                            }
                        }
                    }
                }
            }

            return true; // No moves resolve the check
        }
        
        public bool IsStalemate()
        {
            if (IsKingInCheck()) return false;

            // Check if the player has any legal moves
            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    string piece = _squares[y, x];
                    if (!string.IsNullOrEmpty(piece) && char.IsUpper(piece[0]) == _isWhiteTurn)
                    {
                        var legalMoves = PieceMove.GetLegalMoves(piece, x, y, _squares, _enPassantTarget, !_isWhiteTurn, false, false);
                        foreach (var (toX, toY) in legalMoves)
                        {
                            if (WouldMoveResolveCheck(x, y, toX, toY))
                            {
                                return false; // Found a valid move
                            }
                        }
                    }
                }
            }

            return true; // No legal moves and not in check
        }

        private bool WouldMoveResolveCheck(int fromX, int fromY, int toX, int toY)
        {
            // Simulate the move
            string temp = _squares[toY, toX];
            _squares[toY, toX] = _squares[fromY, fromX];
            _squares[fromY, fromX] = "";

            // Check if the king is in check after the move
            bool inCheck = IsKingInCheck();

            // Undo the move
            _squares[fromY, fromX] = _squares[toY, toX];
            _squares[toY, toX] = temp;

            return !inCheck;
        }
        
        private (int, int) FindKing()
        {
            return _isWhiteTurn ? _whiteKingPosition : _blackKingPosition;
        }

        private void UpdateKingPosition(bool isWhite, int toX, int toY)
        {
            if (isWhite) _whiteKingPosition = (toX, toY);
            else _blackKingPosition = (toX, toY);
        }

        private void UpdateCastlingFlags(int fromX, int fromY, string piece)
        {
            switch (piece)
            {
                case "K":
                    _whiteCanCastleKingside = false;
                    _whiteCanCastleQueenside = false;
                    break;
                case "R" when fromX == 0 && fromY == 0:
                    _whiteCanCastleQueenside = false;
                    break;
                case "R" when fromX == 7 && fromY == 0:
                    _whiteCanCastleKingside = false;
                    break;
                case "k":
                    _blackCanCastleKingside = false;
                    _blackCanCastleQueenside = false;
                    break;
                case "r" when fromX == 0 && fromY == 0:
                    _blackCanCastleQueenside = false;
                    break;
                case "r" when fromX == 7 && fromY == 0:
                    _blackCanCastleKingside = false;
                    break;
            }
        }
        
        private bool IsEnPassantMove(int fromX, int fromY, int toX, int toY)
        {
            if (_enPassantTarget == null) return false;

            (int targetX, int targetY) = _enPassantTarget.Value;
            if (toX == targetX && toY == targetY)
            {
                string piece = _squares[fromY, fromX];
                return piece.ToLower() == Piece.Pawn;
            }

            return false;
        }
    }
}

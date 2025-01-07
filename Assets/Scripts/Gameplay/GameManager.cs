using UnityEngine;

namespace RookWorks
{
    public class GameManager 
    {
        private readonly Board _board;
        private bool _isWhiteTurn;
        
        public GameManager()
        {
            _board = new Board();
            
        }

        public void SetBoardPosition(string position)
        {
            _board.SetCustomPosition(position);
        }


        public void DoMove(string move)
        {
            if (_board.IsMoveLegal(move))
            {
                if (IsPlayerPiece(move))
                {
                    _board.ApplyMove(move);

                    // Check if the opponent is in checkmate
                    if (_board.IsCheckmate(!_isWhiteTurn))
                    {
                        Debug.Log($"Checkmate! {(_isWhiteTurn ? "White" : "Black")} wins!");
                        return; // End the game
                    }
                    
                    if (_board.IsStalemate(!_isWhiteTurn))
                    {
                        Debug.Log($"Stalemate! draw");
                        return; // End the game
                    }

                    // Check if the opponent is in check
                    if (_board.IsKingInCheck(!_isWhiteTurn))
                    {
                        Debug.Log($"{(_isWhiteTurn ? "Black" : "White")} is in check!");
                    }

                    // Switch turn
                    _isWhiteTurn = !_isWhiteTurn;
                    Debug.Log($"Move {move} applied successfully.");
                }
                else
                {
                    Debug.Log("You cannot move your opponent's pieces!");
                }
            }
            else
            {
                Debug.Log("Invalid move! Try again.");
            }
        }

        public string GetStringBoard()
        {
            return _board.GetStringBoard();
        }
        
        private bool IsPlayerPiece(string move)
        {
            int fromX = move[0] - 'a';
            int fromY = move[1] - '1';

            string piece = _board.GetPiece(fromX, fromY);

            // Check if the piece belongs to the current player
            return _isWhiteTurn ? char.IsUpper(piece[0]) : char.IsLower(piece[0]);
        }
    }
    
}

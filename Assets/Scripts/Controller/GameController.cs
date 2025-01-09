using System.Collections;
using System.Collections.Generic;
using RookWorks.Gameplay;
using RookWorks.Visualization;
using UnityEngine;

namespace RookWorks.Controller
{
    public class GameController : MonoBehaviour
    {
        [SerializeField] private BoardView _boardView;
        private readonly Board _board = new Board();
        private bool _isGameOver;

        public bool IsGameOver => _isGameOver;

        private void Start()
        {
            _boardView.CreateBoard(_board.Squares);
        }

        public void TryMove(string from, string to)
        {
            if (!_isGameOver)
            {
                string move = $"{from}{to}";
                if (_board.TryMove(move))
                {
                    _boardView.RefreshBoard(_board.Squares);
                    
                    if (_board.IsCheckmate())
                    {
                        _isGameOver = true;
                    }
                    
                    if (_board.IsStalemate())
                    {
                        _isGameOver = true;
                    }
                }
            }
        }

        public bool IsPlayerPieceInSquare(string square)
        {
            return _board.IsPlayerPieceInSquare(square);
        }
    }
}
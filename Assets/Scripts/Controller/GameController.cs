using System;
using System.Collections.Generic;
using RookWorks.Gameplay;
using RookWorks.Visualization;
using UnityEngine;

namespace RookWorks.Controller
{
    public class GameController : MonoBehaviour
    {
        [SerializeField] private PanelView _panelView;
        [SerializeField] private BoardView _boardView;
        private readonly Board _board = new Board();
        private List<ChessGameData> _loadedGames;

        public bool IsGameOver => _board.IsLiveBoard;

        private void Start()
        {
            _panelView.Initialize(StartGame, LoadGames);
        }

        private void StartGame(Guid guid)
        {
            _board.ResetBoard();
            var game = FindGame(guid);
            if (game != null)
            {
                string fen = null;
                if (game.Metadata.ContainsKey("FEN"))
                {
                    fen = game.Metadata["FEN"];
                    _board.SetCustomPosition(fen, true);
                }

                List<string> uciMoves = new List<string>();
                foreach (var move in game.Moves)
                {
                    string uciMove = MoveNotationConverter.ConvertToUCI(move, _board);
                    if (!_board.TryMove(uciMove))
                    {
                        _board.ResetBoard();
                        throw new Exception("Invalid move");
                    }
                }
                _board.SetLoadGame();
            }

            _boardView.RefreshBoard(_board.Squares);
        }

        private ChessGameData FindGame(Guid guid)
        {
            foreach (var game in _loadedGames)
            {
                if (game.Metadata["GUID"] == guid.ToString())
                {
                    return game;
                }
            }

            return null;
        }

        private void LoadGames()
        {
            string fileContent = FileLoader.LoadFile();
            if (!string.IsNullOrEmpty(fileContent))
            {
                _loadedGames = PGNParser.ParsePGNFile(fileContent);
                List<Dictionary<string, string>> metadatas = new();
                foreach (var game in _loadedGames)
                {
                    game.Metadata.Add("GUID", Guid.NewGuid().ToString());
                    metadatas.Add(game.Metadata);
                }

                _panelView.ShowLoadedGames(metadatas);
            }
        }

        public void PreviousPosition()
        {
            _board.PreviousPosition();
            _boardView.RefreshBoard(_board.Squares);
        }

        public void NextPosition()
        {
            _board.NextPosition();
            _boardView.RefreshBoard(_board.Squares);
        }

        public void TryMove(string from, string to)
        {
            string move = $"{from}{to}";
            if (_board.TryMove(move))
            {
                _boardView.RefreshBoard(_board.Squares);
            }
        }

        public bool IsPlayerPieceInSquare(string square)
        {
            return _board.IsPlayerPieceInSquare(square);
        }
    }
}
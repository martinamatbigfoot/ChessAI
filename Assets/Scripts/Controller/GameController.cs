using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using RookWorks.Gameplay;
using RookWorks.Stockfish;
using RookWorks.Visualization;
using UnityEngine;
using UnityEngine.UI;

namespace RookWorks.Controller
{
    public class GameController : MonoBehaviour
    {
        [SerializeField] private PanelView _panelView;
        [SerializeField] private Image _evalBarIndicator;
        [SerializeField] private BoardView _boardView;
        private readonly Board _board = new Board();
        private List<ChessGameData> _loadedGames = new();
        private StockfishInterface _stockfishInterface;
        private List<MoveInfo> _bestMoves = new();
        private Coroutine _evalBarCoroutine;
        public bool IsGameOver => _board.IsLiveBoard;

        private void Start()
        {
            _stockfishInterface = new StockfishInterface();
            _stockfishInterface.OnStockfishOutput += ParseStockfishOutput;
            _panelView.Initialize(StartGame, LoadGames);
        }

        private void OnDestroy()
        {
            _stockfishInterface.Kill();
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
                foreach (var move in game.Moves)
                {
                    

                    string uciMove = MoveNotationConverter.ConvertToUci(move, _board);
                    Debug.Log($"move {move} converted to uci as {uciMove}");
                    if (!_board.TryMove(uciMove))
                    {
                        _board.ResetBoard();
                        throw new Exception("Invalid move");
                    }
                }
                _board.SetLoadGame();
                _stockfishInterface.SetPosition(fen, _board.Moves);
                _stockfishInterface.GoDepth(5);
            }
            else
            {
                _stockfishInterface.SetInitialPosition();
                _stockfishInterface.GoDepth(5);
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
            _loadedGames.Clear();
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
            _stockfishInterface.SetPosition(_board.Moves);
            _bestMoves.Clear();
            _stockfishInterface.GoDepth(5);
        }

        public bool IsPlayerPieceInSquare(string square)
        {
            return _board.IsPlayerPieceInSquare(square);
        }

        private void ParseStockfishOutput(string output)
        {
            Regex multipvRegex = new Regex(@"info depth (\d+) seldepth \d+ multipv (\d+) score cp (-?\d+) nodes \d+ nps \d+ hashfull \d+ tbhits \d+ time \d+ pv ([a-z0-9\s]+)");
            MatchCollection multipvMatches = multipvRegex.Matches(output);

            foreach (Match match in multipvMatches)
            {
                if (match.Success)
                {
                    // Extract depth, multipv, score, and the next moves (pv)
                    int depth = int.Parse(match.Groups[1].Value) ;
                    int multipv = int.Parse(match.Groups[2].Value);
                    int score = int.Parse(match.Groups[3].Value);
                    string moveValue = match.Groups[4].Value.Split(' ')[0];
                    MoveInfo move = new(depth, multipv, score, moveValue);

                    if (_bestMoves.Count < multipv)
                    {
                        _bestMoves.Add(move);
                    }
                    else
                    {
                        _bestMoves[multipv - 1] = move;
                    }

                    _boardView.RefreshBestMoves(_bestMoves);
                    if (!_board.IsWhiteTurn)
                        score = -score;
                    if (multipv == 1)
                    {
                        if (_evalBarCoroutine != null)
                            StopCoroutine(_evalBarCoroutine);
                        float evalValue = (1000f + (float)score) / 2000f;
                        _evalBarCoroutine = StartCoroutine(AnimateEvalBar(evalValue, 0.5f));
                    }

                    return;
                }
            }
            
            Regex bestMoveRegex = new Regex(@"bestmove\s([a-z0-9]+)\sponder\s([a-z0-9]+)");
            Match bestMoveMatch = bestMoveRegex.Match(output);

            if (bestMoveMatch.Success)
            {
                // Extract the best move and ponder move from the regex groups
                string bestMove = bestMoveMatch.Groups[1].Value;  // bestmove
                string ponderMove = bestMoveMatch.Groups[2].Value;  // ponder move
            }
        }

        private IEnumerator AnimateEvalBar(float evalTarget, float time)
        {
            float currentTime = 0;
            while (currentTime < time)
            {
                yield return null;
                currentTime += Time.deltaTime;
                _evalBarIndicator.fillAmount = Mathf.Lerp(_evalBarIndicator.fillAmount, evalTarget, currentTime);
            }
        }
    }

}
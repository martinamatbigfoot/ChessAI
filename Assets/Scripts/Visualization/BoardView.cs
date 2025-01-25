using System;
using System.Collections.Generic;
using UnityEngine;

namespace RookWorks.Visualization
{
    public class BoardView : MonoBehaviour
    {
        [Serializable]
        struct PieceSprite
        {
            public string Id;
            public Sprite Image;
        }

        [SerializeField] private GameObject _squarePrefab; // Prefab for squares
        [SerializeField] private GameObject _moveIndicatorPrefab;
        [SerializeField] private Transform _boardParent;
        [SerializeField] private PieceSprite[] _pieceSprites;
        

        private readonly SquareView[,] _squareObjects = new SquareView[8, 8];
        private List<MoveIndicatorView> _moveIndicatorViews = new();
        private bool _inited;

        private void InitBoard(string[,] squares)
        {
            // Create squares and pieces only once
            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    // Create square if it doesn't exist
                    if (_squareObjects[x, y] == null)
                    {
                        bool isLightSquare = (x + y) % 2 == 0;

                        string piece = squares[x, y];
                        Sprite sprite = GetPieceSprite(piece);
                        _squareObjects[x, y] = Instantiate(_squarePrefab, _boardParent).GetComponent<SquareView>();
                        _squareObjects[x, y].Initialize(y,x, sprite, isLightSquare);
                    }
                }
            }

            _inited = true;
        }

        public void RefreshBoard(string[,] squares)
        {
            if (!_inited)
                InitBoard(squares);
            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    string piece = squares[x, y];
                    Sprite sprite = GetPieceSprite(piece);
                    _squareObjects[x, y].SetPiece(sprite);
                }
            }
        }

        public void RefreshBestMoves(List<MoveInfo> moves)
        {
            for (var i = 0; i < moves.Count; i++)
            {
                MoveInfo move = moves[i];
                if (_moveIndicatorViews.Count <= i)
                {
                    GameObject moveIndicatorGameObject = Instantiate(_moveIndicatorPrefab, _boardParent);
                    var view = moveIndicatorGameObject.GetComponent<MoveIndicatorView>();
                    _moveIndicatorViews.Add(view);
                }
                _moveIndicatorViews[i].IndicateMove(move, moves.Count);
            }
        }

        private Sprite GetPieceSprite(string piece)
        {
            foreach (PieceSprite pieceSprite in _pieceSprites)
            {
                if (pieceSprite.Id == piece)
                {
                    return pieceSprite.Image;
                }
            }

            return null;
        }
        
    }
    
}

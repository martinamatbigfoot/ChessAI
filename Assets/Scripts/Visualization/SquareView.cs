using TMPro;
using UnityEngine;

namespace RookWorks.Visualization
{
    public class SquareView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _background;
        [SerializeField] private SpriteRenderer _piece;
        [SerializeField] private TextMeshPro _rank;
        [SerializeField] private TextMeshPro _file;
        [SerializeField] private Theme _theme;
        private string _square;
        private bool _isLightSquare; 

        public string Square => _square;

        public void Initialize(int x, int y, Sprite pieceImage, bool isLightSquare)
        {
            transform.localPosition = new Vector3(x, y, 0);
            char fileLetter = (char)(x + 97);
            _square = $"{fileLetter}{y + 1}";
            name = $"Square_{_square} ";
            SetPiece(pieceImage);
            _isLightSquare = isLightSquare;
            SetTexts(x, y);
            SetColor(_theme.GetColor(_isLightSquare));
        }

        public void SetPiece(Sprite pieceImage)
        {
            _piece.sprite = pieceImage;
        }

        private void SetTexts(int x, int y)
        {
            Color color = _theme.GetColor(!_isLightSquare);

            if (x == 7)
            {
                _rank.text = (y + 1).ToString();
                _rank.color = color;
            }
            else
            {
                _rank.text = "";
            }

            if (y == 0)
            {
                _file.text = ((char)(x + 97)).ToString();
                _file.color = color;
            }
            else
            {
                _file.text = "";
            }
        }

        private void SetColor(Color color)
        {
            _background.color = color;
        }

        public void Select()
        {
            SetColor(_theme.GetSelectedColor(_isLightSquare));
        }
        
        public void Unselect()
        {
            SetColor(_theme.GetColor(_isLightSquare));
            ResetPiece();
        }

        public void DragPiece(Vector3 position)
        {
            _piece.transform.position = position;
        }

        public void ResetPiece()
        {
            _piece.transform.localPosition = Vector3.zero;
        }
    }
}

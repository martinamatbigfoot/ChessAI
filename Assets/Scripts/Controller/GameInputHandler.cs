using RookWorks.Visualization;
using UnityEngine;

namespace RookWorks.Controller
{
    public class GameInputHandler : MonoBehaviour
    {
        [SerializeField] private GameController _gameController;
        [SerializeField] private Camera _mainCamera; 
        private SquareView _draggingSquare;
        private SquareView _selectedSquare;



        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                _gameController.PreviousPosition();
            }

            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                _gameController.NextPosition();
            }

            if (!_gameController.IsGameOver)
            {
                // Handle mouse input
                if (Input.GetMouseButtonDown(0))
                {
                    StartDrag(Input.mousePosition);
                }
                else if (Input.GetMouseButton(0))
                {
                    Drag(Input.mousePosition);
                }
                else if (Input.GetMouseButtonUp(0))
                {
                    EndDrag(Input.mousePosition);
                }

                // Handle touch input
                if (Input.touchCount > 0)
                {
                    Touch touch = Input.GetTouch(0);

                    switch (touch.phase)
                    {
                        case TouchPhase.Began:
                            StartDrag(touch.position);
                            break;

                        case TouchPhase.Moved:
                            Drag(touch.position);
                            break;

                        case TouchPhase.Ended:
                        case TouchPhase.Canceled:
                            EndDrag(touch.position);
                            break;
                    }
                }
            }
        }

        private void StartDrag(Vector3 screenPosition)
        {
            Vector3 worldPosition = _mainCamera.ScreenToWorldPoint(screenPosition);
            RaycastHit2D hit = Physics2D.Raycast(worldPosition, Vector2.zero);

            if (hit.collider != null)
            {
                var squareView = hit.collider.GetComponent<SquareView>();
                if (squareView != null)
                {
                    if (_selectedSquare == null)
                    {
                        if (_gameController.IsPlayerPieceInSquare(squareView.Square))
                        {
                            _draggingSquare = squareView;
                            _draggingSquare.Select();
                            Debug.Log($"Started dragging from square: {_draggingSquare.Square}");
                        }
                    }
                    else if (_selectedSquare == squareView)
                    {
                        Debug.Log($"Cancel move from square: {_selectedSquare.Square}");
                        squareView.Unselect();
                        _selectedSquare = null;
                        _draggingSquare = null;
                    }
                    else
                    {
                        _selectedSquare.Unselect();
                        Debug.Log($"trying to move from square: {_selectedSquare.Square} to {squareView.Square}");
                        TryMovePiece(_selectedSquare.Square, squareView.Square);
                        _selectedSquare = null;
                        _draggingSquare = null;
                    }
                }
            }
        }

        private void Drag(Vector3 screenPosition)
        {
            if (_draggingSquare != null && _selectedSquare == null)
            {
                // Update the dragged piece's position to follow the cursor or touch
                Vector3 worldPosition = _mainCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, 0));
                worldPosition.z = 0;
                _draggingSquare.DragPiece(worldPosition);
            }
        }

        private void EndDrag(Vector3 screenPosition)
        {
            if (_draggingSquare != null && _selectedSquare == null)
            {
                Vector3 worldPosition = _mainCamera.ScreenToWorldPoint(screenPosition);
                RaycastHit2D hit = Physics2D.Raycast(worldPosition, Vector2.zero);
                if (hit.collider != null)
                {
                    var targetSquare = hit.collider.GetComponent<SquareView>();
                    if (targetSquare != null)
                    {
                        if (targetSquare != _draggingSquare)
                        {
                            Debug.Log($"Dragged piece to square: {targetSquare.Square}");
                            _draggingSquare.Unselect();
                            TryMovePiece(_draggingSquare.Square, targetSquare.Square);
                            return;
                        }
                        else if (targetSquare == _draggingSquare)
                        {
                            if (_gameController.IsPlayerPieceInSquare(_draggingSquare.Square))
                            {
                                _selectedSquare = _draggingSquare;
                                _selectedSquare.ResetPiece();
                                _selectedSquare.Select();
                            }
                        }
                    }
                }
                _draggingSquare = null;
            }
        }

        private void TryMovePiece(string from, string to)
        {
            Debug.Log($"Move piece from {from} to {to}");
            _gameController.TryMove(from, to);
        }
    }
}

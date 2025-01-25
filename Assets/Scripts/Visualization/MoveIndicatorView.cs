using System;
using UnityEngine;

namespace RookWorks.Visualization
{
    public class MoveIndicatorView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _renderer;
        private Color _bestColor = new Color(0.1f, 0.1f, 0.1f, 0.5f);
        private Color _worstColor = new Color(0.6f, 0.6f, 0.6f, 0.5f);
        private const float _bestSize = 1;
        private const float _worstSize = 0.6f;
        public void IndicateMove(MoveInfo move, int totalMoves)
        {
            int fromX = move.Move[0] - 'a';
            int fromY = move.Move[1] - '1';
            int toX = move.Move[2] - 'a';
            int toY = move.Move[3] - '1';
            
            float x = (float)(fromX + (float)(toX - fromX) / 2f);
            float y = (float)(fromY + (float)(toY - fromY) / 2f);
            transform.localPosition = new Vector3(x, y , 0);

            float t = 0;
            if (totalMoves > 1)
            {
                t = (float)(move.MultiPv -1) / (float)(totalMoves - 1);
            }
            _renderer.color = Color.Lerp(_bestColor, _worstColor, t);
            
            float size = Vector2.Distance(new(fromX, fromY), new Vector2(toX, toY));
            float ySize = Mathf.Lerp(_bestSize, _worstSize, t);
            _renderer.size = new Vector2(size + 0.2f, ySize);
            transform.eulerAngles = Vector3.zero;
            float angle = GetAngle(fromX, fromY, toX, toY);
            transform.Rotate(Vector3.forward, angle);
        }
        
        private float  GetAngle(int fromX, int fromY, int toX, int toY)
        {
            // Calculate the difference in X and Y coordinates
            int deltaX = toX - fromX;
            int deltaY = toY - fromY;

            // Special case when the squares are in the same column (deltaX == 0)
            if (deltaX == 0)
            {
                // If toY > fromY, the angle is 90 degrees (upwards), otherwise, it's 270 degrees (downwards)
                return (deltaY > 0) ? 90 : 270;
            }

            // Calculate the angle in radians between the horizontal line and the line between the squares
            double angleRadians = Math.Atan2(deltaY, deltaX);

            // Convert radians to degrees
            double angleDegrees = angleRadians * (180 / Math.PI);

            // Adjust angle to be between 0 and 180 (positive angle between horizontal and the line)
            if (angleDegrees < 0)
            {
                angleDegrees += 360;
            }

            return (float)angleDegrees;
        }
    }
}

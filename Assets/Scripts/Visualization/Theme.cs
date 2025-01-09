using UnityEngine;

namespace RookWorks.Visualization
{
    [CreateAssetMenu(menuName = "RookWorks/Theme")]
    public class Theme : ScriptableObject
    {
        [field: SerializeField] public Color LightColor { get; private set; }
        [field: SerializeField] public Color DarkColor { get; private set; }
        [field: SerializeField] public Color LightSelectedColor { get; private set; }
        [field: SerializeField] public Color DarkSelectedColor { get; private set; }
        [field: SerializeField] public Color LightLastMoveColor { get; private set; }
        [field: SerializeField] public Color DarkLastMoveColor { get; private set; }

        public Color GetColor(bool isLightColor)
        {
            return isLightColor ? LightColor : DarkColor;
        }
        
        public Color GetSelectedColor(bool isLightColor)
        {
            return isLightColor ? LightSelectedColor : DarkSelectedColor;
        }
        
        public Color GetLastMoveColor(bool isLightColor)
        {
            return isLightColor ? LightLastMoveColor : DarkLastMoveColor;
        }
    }
}



using UnityEngine;

[CreateAssetMenu(fileName = "ColorPalette", menuName = "WaterSort/Color Palette")]
public class ColorPaletteSO : ScriptableObject
{
    [SerializeField] private Color[] colorsPalette = new Color[8];

    public int Count => colorsPalette.Length;

    public Color GetColor(int index)
    {
        if (index < 0 || index >= colorsPalette.Length)
        {
            Debug.LogError($"Color index {index} out of range.");
            return Color.white;
        }

        return colorsPalette[index];
    }
}
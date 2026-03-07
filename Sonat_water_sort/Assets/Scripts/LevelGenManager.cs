using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.Rendering.DebugUI.Table;

public class LevelGenManager : MonoBehaviour
{
    [Header("Game Manager")]
    [SerializeField] private GameManager gameManager;

    [Header("Grid")]
    [SerializeField] private Grid grid;

    [Header("Level Gen Setup")]
    [SerializeField] private GameObject bottlePrefab;
    [SerializeField] private int columns = 4;
    [SerializeField] private int rows = 2;
    [SerializeField] private int colorAmount = 2;
    [SerializeField] private ColorPaletteSO palette;

    private List<Bottle> allBottles = new List<Bottle>();

    private void Start()
    {
        SpawnBottles();
    }

    public void SpawnBottles()
    {
        allBottles.Clear();

        int totalBottles = rows * columns;
        int playableBottles = totalBottles - 1;
        int slotsPerBottle = 4;

        List<int> colorPool = new List<int>();

        // create color pool (each color fills one bottle)
        for (int i = 0; i < colorAmount; i++)
        {
            for (int j = 0; j < slotsPerBottle; j++)
            {
                colorPool.Add(i);
            }
        }

        // if more bottles than colors, fill remaining with additional colors
        int remainingBottles = playableBottles - colorAmount;
        int nextColorIndex = colorAmount;

        for (int i = 0; i < remainingBottles; i++)
        {
            int colorIndex = nextColorIndex % palette.Count;

            for (int j = 0; j < slotsPerBottle; j++)
            {
                colorPool.Add(colorIndex);
            }

            nextColorIndex++;
        }

        // shuffle colors
        for (int i = 0; i < colorPool.Count; i++)
        {
            int rand = Random.Range(i, colorPool.Count);
            (colorPool[i], colorPool[rand]) = (colorPool[rand], colorPool[i]);
        }

        int poolIndex = 0;

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                Vector3Int cellPosition = new Vector3Int(x, y, 0);
                Vector3 worldPosition = grid.GetCellCenterWorld(cellPosition);

                GameObject bottleObj = Instantiate(
                    bottlePrefab,
                    worldPosition,
                    Quaternion.identity,
                    grid.transform
                );

                Bottle bottle = bottleObj.GetComponent<Bottle>();

                if (bottle != null)
                {
                    allBottles.Add(bottle);

                    int[] colors = new int[4];

                    if (allBottles.Count == totalBottles) // last bottle = empty
                    {
                        colors = new int[] { -1, -1, -1, -1 };
                    }
                    else
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            colors[i] = colorPool[poolIndex++];
                        }
                    }

                    bottle.InitializeFromPalette(colors);
                }
            }
        }
        gameManager.SetBottles(allBottles);
    }
}

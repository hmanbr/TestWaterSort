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
        SpawnLevel();
        //DebugPrintBottleColors();
    }

    public void DebugPrintBottleColors()
    {
        if (allBottles == null || allBottles.Count == 0)
        {
            Debug.Log("No bottles found.");
            return;
        }

        for (int i = 0; i < allBottles.Count; i++)
        {
            Bottle bottle = allBottles[i];

            string colors = bottle.GetCurrentColorsInBottle();

            Debug.Log($"Bottle {i}: {colors}");
        }
    }


    public void SpawnLevel()
    {
        allBottles.Clear();

        int bottleCount = Mathf.Max(rows * columns, 4);

        SpawnBottles();

        GenerateColors(bottleCount);
        gameManager.SetBottles(allBottles);
    }

    void SpawnBottles()
    {
        int bottleIndex = 1;
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
                bottleObj.gameObject.name = "Bottle" + bottleIndex++;

                Bottle bottle = bottleObj.GetComponent<Bottle>();

                if (bottle != null)
                    allBottles.Add(bottle);
            }
        }
    }

    void GenerateColors(int bottleCount)
    {
        //bottleCount -= 2; //DEBUG ONLY 

        const int bottleCapacity = 4;

        int paletteCount = palette.Count;
        int colorTypes = Mathf.Clamp(colorAmount, 2, paletteCount);

        int emptyBottleCount = 2;
        int filledBottleCount = bottleCount - emptyBottleCount;

        int bottlesPerColor = filledBottleCount / colorTypes;

        bool valid = false;

        while (!valid)
        {
            List<int> colorPool = new List<int>();

            for (int color = 0; color < colorTypes; color++)
            {
                for (int b = 0; b < bottlesPerColor; b++)
                {
                    for (int i = 0; i < bottleCapacity; i++)
                    {
                        colorPool.Add(color);
                    }
                }
            }

            Shuffle(colorPool);

            int index = 0;
            valid = true;

            for (int i = 0; i < filledBottleCount; i++)
            {
                int first = colorPool[index];

                bool allSame = true;

                for (int j = 1; j < bottleCapacity; j++)
                {
                    if (colorPool[index + j] != first)
                    {
                        allSame = false;
                        break;
                    }
                }

                if (allSame)
                {
                    valid = false;
                    break;
                }

                index += bottleCapacity;
            }

            if (!valid)
                continue;

            index = 0;

            for (int i = 0; i < allBottles.Count; i++)
            {
                int[] colors = new int[bottleCapacity];

                if (i < filledBottleCount)
                {
                    for (int j = 0; j < bottleCapacity; j++)
                        colors[j] = colorPool[index++];
                }
                else
                {
                    for (int j = 0; j < bottleCapacity; j++)
                        colors[j] = -1;
                }

                allBottles[i].InitializeFromPalette(colors);
            }
        }
    }

    void Shuffle(List<int> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int rand = Random.Range(0, i + 1);
            (list[i], list[rand]) = (list[rand], list[i]);
        }
    }
}
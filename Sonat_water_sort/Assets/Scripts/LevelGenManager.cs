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

        int bottleCount = rows * columns;

        // spawn bottles
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
                    allBottles.Add(bottle);
            }
        }

        GenerateBottleColors(bottleCount);
        gameManager.SetBottles(allBottles);
    }

    void GenerateBottleColors(int bottleCount)
    {
        const int bottleCapacity = 4;

        // Usually water sort has 1–2 empty bottles
        //int emptyBottleCount = 2;
        int emptyBottleCount = 4; //FOR DEBUG

        int filledBottleCount = bottleCount - emptyBottleCount;

        int totalColorUnits = filledBottleCount * bottleCapacity;

        List<int> colorPool = new List<int>(totalColorUnits);

        // pick colors from palette
        for (int i = 0; i < filledBottleCount; i++)
        {
            int colorIndex = i % palette.Count;

            for (int j = 0; j < bottleCapacity; j++)
            {
                colorPool.Add(colorIndex);
            }
        }

        Shuffle(colorPool);

        int poolIndex = 0;

        for (int i = 0; i < allBottles.Count; i++)
        {
            int[] colors = new int[bottleCapacity];

            if (i < filledBottleCount)
            {
                for (int j = 0; j < bottleCapacity; j++)
                {
                    colors[j] = colorPool[poolIndex++];
                }
            }
            else
            {
                // empty bottle
                for (int j = 0; j < bottleCapacity; j++)
                {
                    colors[j] = -1;
                }
            }

            allBottles[i].InitializeFromPalette(colors);
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

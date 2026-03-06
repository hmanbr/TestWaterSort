using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    [Header("Grid")]
    [SerializeField] private Grid grid;

    [Header("Bottle Setup")]
    [SerializeField] private GameObject bottlePrefab;
    [SerializeField] private int columns = 5;
    [SerializeField] private int rows = 2;

    private List<Bottle> allBottles = new List<Bottle>();

    void Start()
    {
        SpawnBottles();
    }

    public void SpawnBottles()
    {
        allBottles.Clear();

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
                    transform
                );

                Bottle bottle = bottleObj.GetComponent<Bottle>();

                if (bottle != null)
                    allBottles.Add(bottle);
            }
        }
    }

    public List<Bottle> GetBottles()
    {
        return allBottles;
    }
}
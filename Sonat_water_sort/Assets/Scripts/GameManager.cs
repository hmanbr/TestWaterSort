using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    private Bottle sourceBottle;
    private Bottle targetBottle;

    private List<Bottle> allBottles = new List<Bottle>();

    private bool isPouring = false;

    [Header("Grid")]
    [SerializeField] private Grid grid;

    void Start()
    {
        //Debuging purpose
        GetAllBottles();
        foreach (var bottle in allBottles)
        {
            bottle.OnPourComplete += Bottle_OnPourComplete;
        }
        //Debuging purpose
    }
    private void GetAllBottles()
    {
        allBottles.Clear();

        foreach (Transform child in grid.transform)
        {
            Bottle bottle = child.GetComponent<Bottle>();
            if (bottle != null)
            {
                allBottles.Add(bottle);
            }
        }
    }

    void AttemptPour()
    {
        if (sourceBottle == null || targetBottle == null)
            return;

        isPouring = true;

        sourceBottle.PourTo(targetBottle);

        ClearSelection();
    }

    public void HandleBottleSelected(Bottle bottle)
    {
        if (isPouring) return;

        // selecting source
        if (sourceBottle == null)
        {
            if (!bottle.CanBeSourceBottle())
                return;

            sourceBottle = bottle;
            sourceBottle.AnimateSelect();
            return;
        }

        targetBottle = bottle;

        if (sourceBottle == targetBottle)
        {
            sourceBottle.AnimateDeselect();
            ClearSelection();
            return;
        }

        AttemptPour();
    }

    private void ClearSelection()
    {
        sourceBottle = null;
        targetBottle = null;
    }

    public bool HasSourceBottle()
    {
        return sourceBottle != null;
    }

    public List<Bottle> GetBottles()
    {
        return allBottles;
    }

    public void SetBottles(List<Bottle> bottles)
    {
        allBottles.Clear();
        allBottles.AddRange(bottles);

        foreach (var bottle in allBottles)
        {
            bottle.OnPourComplete += Bottle_OnPourComplete;
        }
    }

    private void Bottle_OnPourComplete(Bottle source, Bottle target, bool success)
    {
        isPouring = false;
    }

    void OnDisable()
    {
        foreach (var bottle in allBottles)
        {
            if (bottle != null)
                bottle.OnPourComplete -= Bottle_OnPourComplete;
        }
    }
}
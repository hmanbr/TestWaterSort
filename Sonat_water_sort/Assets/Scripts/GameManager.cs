using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    private Bottle sourceBottle;
    private Bottle targetBottle;

    private List<Bottle> allBottles = new List<Bottle>();

    private bool isPouring = false;

    void Start()
    {

    }

    void AttemptPour()
    {
        if (sourceBottle == null || targetBottle == null)
            return;

        isPouring = true;

        sourceBottle.CancelSelect(); // optional if you have this
        sourceBottle.PourTo(targetBottle);

        ClearSelection();
    }

    public void HandleBottleSelected(Bottle bottle)
    {
        if (isPouring) return;

        if (sourceBottle == null)
        {
            sourceBottle = bottle;
            sourceBottle.AnimateSelect();
            return;
        }

        targetBottle = bottle;

        if (sourceBottle == targetBottle)
        {
            ClearSelection();
            return;
        }

        AttemptPour();
    }

    private void ClearSelection()
    {
        //sourceBottle.AnimateDeselect();

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

    private void Bottle_OnPourComplete(Bottle arg1, Bottle arg2)
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
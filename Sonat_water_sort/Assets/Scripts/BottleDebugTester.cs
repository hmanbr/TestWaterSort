using UnityEngine;
using UnityEngine.InputSystem;
using static Unity.VisualScripting.Member;
using static UnityEngine.GraphicsBuffer;

public class BottleDebugTester : MonoBehaviour
{
    [SerializeField] Bottle[] bottles;

    int selectedIndex = -1;
    int targetIndex = -1;
    void Start()
    {
        foreach (var bottle in bottles)
        {
            bottle.OnPourComplete += Bottle_OnPourComplete;
            bottle.OnBottleComplete += Bottle_OnBottleComplete;
        }
    }

    private void Bottle_OnBottleComplete(Bottle source)
    {
        Debug.Log($"Bottle completed, colors: {source.GetCurrentColorsInBottle()}");
    }

    private void Bottle_OnPourComplete(Bottle source, Bottle target, bool success)
    {
        Debug.Log($"Pour {System.Array.IndexOf(bottles, source) + 1} -> {System.Array.IndexOf(bottles, target) + 1}");

        Debug.Log($"Bottle num {System.Array.IndexOf(bottles, source) + 1} colors: {source.GetCurrentColorsInBottle()}");
        Debug.Log($"Bottle num {System.Array.IndexOf(bottles, target) + 1} colors: {target.GetCurrentColorsInBottle()}");
    }

    void Update()
    {
        if (Keyboard.current.digit1Key.wasPressedThisFrame) SelectBottle(0);
        if (Keyboard.current.digit2Key.wasPressedThisFrame) SelectBottle(1);
        if (Keyboard.current.digit3Key.wasPressedThisFrame) SelectBottle(2);
        if (Keyboard.current.digit4Key.wasPressedThisFrame) SelectBottle(3);

        if (Keyboard.current.pKey.wasPressedThisFrame)
            TryPour();
    }

    void SelectBottle(int index)
    {
        if (index >= bottles.Length) return;

        if (selectedIndex == -1)
        {
            selectedIndex = index;
            Debug.Log($"Selected source bottle: {index + 1}");
        }
        else
        {
            targetIndex = index;
            Debug.Log($"Selected target bottle: {index + 1}");
        }
    }

    void TryPour()
    {
        if (selectedIndex == -1 || targetIndex == -1) return;

        bottles[selectedIndex].PourTo(bottles[targetIndex]);

        selectedIndex = -1;
        targetIndex = -1;
    }
}
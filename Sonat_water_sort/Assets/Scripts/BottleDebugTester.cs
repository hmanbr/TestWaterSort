using UnityEngine;
using UnityEngine.InputSystem;

public class BottleDebugTester : MonoBehaviour
{
    [SerializeField] Bottle[] bottles;

    int selectedIndex = -1;
    int targetIndex = -1;

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

        Debug.Log($"Pour {selectedIndex + 1} -> {targetIndex + 1}");
        Debug.Log($"Bottle num {selectedIndex + 1} colors: {bottles[selectedIndex].GetCurrentColorsInBottle()} ");
        Debug.Log($"Bottle num {targetIndex + 1} colors: {bottles[targetIndex].GetCurrentColorsInBottle()} ");

        selectedIndex = -1;
        targetIndex = -1;
    }
}
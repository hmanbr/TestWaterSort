using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{

    [SerializeField] private Camera mainCamera;
    [SerializeField] private GameManager gameManager;

    private PlayerInput playerInput;

    private InputAction touchPressAction;
    private InputAction touchPositionAction;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        touchPressAction = playerInput.actions["TouchPress"];
        touchPositionAction = playerInput.actions["TouchPosition"];
    }

    private void OnEnable()
    {
        touchPressAction.performed += TouchPressAction_performed;
    }
    private void OnDisable()
    {
        touchPressAction.performed -= TouchPressAction_performed;
    }

    private void TouchPressAction_performed(InputAction.CallbackContext obj)
    {
        Vector2 mousePos2D = Camera.main.ScreenToWorldPoint(touchPositionAction.ReadValue<Vector2>());

        RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);

        if (hit.collider != null)
        {
            Bottle bottle = hit.collider.GetComponent<Bottle>();

            if (bottle != null)
            {
                gameManager.HandleBottleSelected(bottle);
            }
        }
    }  
}
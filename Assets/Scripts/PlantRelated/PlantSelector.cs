using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
public class PlantSelector : MonoBehaviour
{
    private bool canSwap = true;
    [SerializeField] private float canSwapDelay = 0.5f;
    public void OnChangePlantType(InputAction.CallbackContext context)
    {
        if(!canSwap) 
        { 
            return;
        }
        if (!context.performed) 
        { 
            return; 
        }


        Vector2 scroll = context.ReadValue<Vector2>();

        if (scroll.y > 0.2f)
        {
            PlantPoolManager.PlantPoolManagerInstance.ChangeSelectedType(1);
        }
        else if (scroll.y < -0.2f)
        {
            PlantPoolManager.PlantPoolManagerInstance.ChangeSelectedType(-1);
        }
        CanSwapCountdown();
    }

    private IEnumerator CanSwapCountdown()
    {
        canSwap = false;
        yield return new WaitForSeconds(canSwapDelay);
        canSwap = true;
    }
}

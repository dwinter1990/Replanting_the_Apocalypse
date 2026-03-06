using UnityEngine;
using UnityEngine.InputSystem;
public class PlantSelector : MonoBehaviour
{
    public void OnChangePlantType(InputAction.CallbackContext context)
    {
        if(!context.performed) 
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
    }
}

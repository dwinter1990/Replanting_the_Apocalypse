using UnityEngine;
using UnityEngine.UI;
public class TooFarGoBack : MonoBehaviour
{
    [SerializeField] private RawImage goBackIndicator;

    private void Start()
    {
        goBackIndicator.enabled = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag("Player"))
        {
            goBackIndicator.enabled = true;
        }
    }
    private void OnCollisionExit(Collision collision)
    {
        if(collision.gameObject.CompareTag("Player"))
        {
            goBackIndicator.enabled = false;
        }
    }
}

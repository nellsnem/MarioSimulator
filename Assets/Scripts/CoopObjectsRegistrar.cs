using UnityEngine;

public class CoopObjectsRegistrar : MonoBehaviour
{
    private void OnEnable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.coopObjects = gameObject;
        }
    }

    private void Awake()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.coopObjects = gameObject;
        }
    }
}
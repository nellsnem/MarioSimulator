using UnityEngine;

public class CoopObjectsRegistrar : MonoBehaviour
{
    // ==========================================
    // 1. MONOBEHAVIOUR METHODS
    // ==========================================
    private void Awake()
    {
        RegisterSelf();
    }

    private void OnEnable()
    {
        RegisterSelf();
    }

    // ==========================================
    // 2. PRIVATE METHODS
    // ==========================================
    private void RegisterSelf()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.coopObjects = gameObject;
        }
    }
}
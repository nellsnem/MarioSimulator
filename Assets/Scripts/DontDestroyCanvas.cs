using UnityEngine;
 
public class DontDestroyCanvas : MonoBehaviour
{
    // ==========================================
    // 1. PRIVATE FIELDS
    // ==========================================
    private static DontDestroyCanvas _instance;

    // ==========================================
    // 2. MONOBEHAVIOUR METHODS
    // ==========================================
    private void Awake()
    {
        if (_instance != null)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
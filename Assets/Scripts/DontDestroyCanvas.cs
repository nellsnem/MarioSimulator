using UnityEngine;

public class DontDestroyCanvas : MonoBehaviour
{
    private static DontDestroyCanvas instance;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
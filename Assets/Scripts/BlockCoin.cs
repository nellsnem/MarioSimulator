using System.Collections;
using UnityEngine;

 
public class BlockCoin : MonoBehaviour
{
    // ==========================================
    // 1. CONSTANTS
    // ==========================================
    private const float COIN_RISE_HEIGHT    = 2f;
    private const float COIN_ANIM_DURATION  = 0.25f;

    // ==========================================
    // 2. MONOBEHAVIOUR METHODS
    // ==========================================
    private void Start()
    {
        GameManager.Instance.AddCoin();
        GameManager.Instance.AddScore(100);

        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.PlayCoin();
        }

        StartCoroutine(CoinAnimation());
    }

    // ==========================================
    // 3. COROUTINES
    // ==========================================
    private IEnumerator CoinAnimation()
    {
        Vector3 startPosition = transform.localPosition;
        Vector3 topPosition   = startPosition + Vector3.up * COIN_RISE_HEIGHT;

        yield return MoveFromTo(startPosition, topPosition);
        yield return MoveFromTo(topPosition, startPosition);

        Destroy(gameObject);
    }

    private IEnumerator MoveFromTo(Vector3 from, Vector3 to)
    {
        float timeElapsed = 0f;

        while (timeElapsed < COIN_ANIM_DURATION)
        {
            float progress = timeElapsed / COIN_ANIM_DURATION;
            transform.localPosition = Vector3.Lerp(from, to, progress);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
    }
}
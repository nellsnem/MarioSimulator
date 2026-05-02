using System.Collections;
using UnityEngine;

public class BlockCoin : MonoBehaviour
{ 
    private void Start()
    {
        GameManager.Instance.AddCoin();
        GameManager.Instance.AddScore(100);
        if (MusicManager.Instance != null) MusicManager.Instance.PlayCoin();
        StartCoroutine(CoinAnimation());
    }
 
    private IEnumerator CoinAnimation()
    {
        Vector3 startPosition = transform.localPosition;
        Vector3 topPosition = startPosition + Vector3.up * 2f;
 
        yield return MoveFromTo(startPosition, topPosition);
 
        yield return MoveFromTo(topPosition, startPosition);
 
        Destroy(gameObject);
    }
 
    private IEnumerator MoveFromTo(Vector3 from, Vector3 to)
    {
        float timeElapsed = 0f;
        float animationDuration = 0.25f;

        while (timeElapsed < animationDuration)
        {
             
            float progress = timeElapsed / animationDuration;
            transform.localPosition = Vector3.Lerp(from, to, progress);

            timeElapsed += Time.deltaTime;
            yield return null;  
        }
    }
}
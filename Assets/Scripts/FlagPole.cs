using System.Collections;
using UnityEngine;

public class FlagPole : MonoBehaviour
{
    [Header("Об'єкти")]
    public Transform flag;
    public Transform poleBottom;
    public Transform castle;

    [Header("Налаштування")]
    public float speed = 6f;

    private bool triggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered) return;

     if (other.CompareTag("Player") && other.TryGetComponent(out PlayerMovement player))
    {
        triggered = true;

        if (GameManager.Instance != null)
        { 
            GameManager.Instance.StartWinSequence(); 
        }

        StartCoroutine(MoveTo(flag, poleBottom.position)); 
        StartCoroutine(LevelCompleteSequence(player)); 
    }
    }

    private IEnumerator LevelCompleteSequence(PlayerMovement player)
    { 
        player.enabled = false;

        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.linearVelocity = Vector2.zero;

        foreach (Collider2D col in player.GetComponents<Collider2D>())
            col.enabled = false;
 
        PlayerVisuals visuals = player.GetComponent<PlayerVisuals>();
        if (visuals != null)
        {
            visuals.enabled = false;
            visuals.spriteRenderer.sprite = player.isBig ? visuals.bigJump : visuals.smallJump;
        }
 
        SetFacing(player.transform, facingRight: false);
        yield return MoveTo(player.transform, poleBottom.position);
 
        SetFacing(player.transform, facingRight: true);
        yield return AutoRunToCastle(player, visuals, castle.position);
 
        player.gameObject.SetActive(false);
 
        yield return new WaitForSeconds(0.5f);
        while (GameManager.Instance != null && GameManager.Instance.time > 0)
        {
            GameManager.Instance.time--;
            GameManager.Instance.AddScore(50);
            yield return new WaitForSeconds(0.01f);
        }
 
        yield return new WaitForSeconds(0.5f);

       if (GameManager.Instance != null)
    {
        GameManager.Instance.ShowVictoryUI();  
    }
    }

    private IEnumerator AutoRunToCastle(PlayerMovement player, PlayerVisuals visuals, Vector3 target)
    {
        if (visuals == null) yield break;

        Sprite[] runFrames = player.isBig ? visuals.bigRun : visuals.smallRun;
        if (runFrames == null || runFrames.Length == 0) yield break;

        int frame = 0;
        float timer = 0f;

        while (Vector3.Distance(player.transform.position, target) > 0.1f)
        {
            player.transform.position = Vector3.MoveTowards(
                player.transform.position, target, speed * Time.deltaTime);

            timer += Time.deltaTime;
            if (timer >= 0.1f)
            {
                timer = 0f;
                frame = (frame + 1) % Mathf.Min(3, runFrames.Length);
                visuals.spriteRenderer.sprite = runFrames[frame];
                visuals.spriteRenderer.color = Color.white;
            }
            yield return null;
        }

        player.transform.position = target;
    }
 
    private void SetFacing(Transform t, bool facingRight)
    {
        Vector3 s = t.localScale;
        s.x = facingRight ? Mathf.Abs(s.x) : Mathf.Abs(s.x);
        t.localScale = s;
    }

    private IEnumerator MoveTo(Transform subject, Vector3 targetPos)
    {
        while (Vector3.Distance(subject.position, targetPos) > 0.1f)
        {
            subject.position = Vector3.MoveTowards(subject.position, targetPos, speed * Time.deltaTime);
            yield return null;
        }
        subject.position = targetPos;
    }
    
}
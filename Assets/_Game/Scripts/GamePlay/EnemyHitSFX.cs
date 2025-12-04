using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class EnemyHitSFX : MonoBehaviour
{
    public LayerMask ballLayers;
    public LayerMask blockLayers;

    [Header("Âm thanh")]
    public AudioClip sfxHitBall;   
    public AudioClip sfxHitBlock;

    void OnCollisionEnter2D(Collision2D collision)
    {
        HandleHit(collision.gameObject.layer);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        HandleHit(other.gameObject.layer);
    }

    void HandleHit(int otherLayer)
    {
        if (((1 << otherLayer) & ballLayers) != 0)
        {
            PlaySfx(sfxHitBall);
            return;
        }

        if (((1 << otherLayer) & blockLayers) != 0)
        {
            PlaySfx(sfxHitBlock);
            return;
        }
    }

    void PlaySfx(AudioClip clip)
    {
        if (clip != null)
        {
            AudioManager.Instance?.PlaySFX(clip);
        }
        else
        {
            AudioManager.Instance?.PlayMergeBall();
        }
    }
}

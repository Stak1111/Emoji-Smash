using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SpriteRenderer))]
public class SpriteSwapOnCollision : MonoBehaviour
{
    [Header("Sprite Swapping")]
    public Sprite newSprite;
    public float duration = 0.5f;

    [Header("Sound Effects")]
    [Range(0, 7)]
    public int soundIndexMax = 7;
    public AudioClip[] collisionSounds = new AudioClip[8];
    public float soundVolume = 1f;

    private SpriteRenderer spriteRenderer;
    private Sprite originalSprite;
    private bool isSwapping = false;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalSprite = spriteRenderer.sprite;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isSwapping && collision.gameObject)
        {
            PlayRandomSound();
            StartCoroutine(SwapSpriteForDuration());
        }
    }

    void PlayRandomSound()
    {
        int maxIndex = Mathf.Min(soundIndexMax + 1, collisionSounds.Length);
        if (maxIndex <= 0) return;

        int index = Random.Range(0, maxIndex);
        AudioClip clip = collisionSounds[index];

        if (clip != null)
        {
            GameObject tempAudio = new GameObject("TempAudio");
            AudioSource tempSource = tempAudio.AddComponent<AudioSource>();
            tempSource.clip = clip;
            tempSource.volume = soundVolume;
            tempSource.spatialBlend = 0f; // 2D
            tempSource.Play();

            Destroy(tempAudio, clip.length + 0.1f); // Cleanup after play
        }
        else
        {
            Debug.LogWarning($"No AudioClip assigned at index {index}.");
        }
    }

    IEnumerator SwapSpriteForDuration()
    {
        isSwapping = true;
        spriteRenderer.sprite = newSprite;

        yield return new WaitForSeconds(duration);

        spriteRenderer.sprite = originalSprite;
        isSwapping = false;
    }
}

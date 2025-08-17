using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]
public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    [Header("Music Settings")]
    public List<AudioClip> songs = new List<AudioClip>(); // Up to 10 songs
    [Range(0f, 1f)]
    public float musicVolume = 0.8f;

    private AudioSource audioSource;
    private List<int> remainingIndices = new List<int>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        audioSource = GetComponent<AudioSource>();
        audioSource.loop = false;
        audioSource.playOnAwake = false;
        audioSource.volume = musicVolume;

        ShuffleSongs();
        PlayNextSong();
    }

    void Update()
    {
        audioSource.volume = musicVolume;

        if (!audioSource.isPlaying && songs.Count > 0)
        {
            PlayNextSong();
        }
    }

    void ShuffleSongs()
    {
        remainingIndices.Clear();

        for (int i = 0; i < songs.Count; i++)
        {
            remainingIndices.Add(i);
        }

        // Optionally shuffle the order
        for (int i = 0; i < remainingIndices.Count; i++)
        {
            int swapIndex = Random.Range(i, remainingIndices.Count);
            (remainingIndices[i], remainingIndices[swapIndex]) = (remainingIndices[swapIndex], remainingIndices[i]);
        }
    }

    void PlayNextSong()
    {
        if (remainingIndices.Count == 0)
            ShuffleSongs(); // All songs played, restart randomly

        int nextIndex = remainingIndices[0];
        remainingIndices.RemoveAt(0);

        if (songs[nextIndex] != null)
        {
            audioSource.clip = songs[nextIndex];
            audioSource.Play();
        }
        else
        {
            Debug.LogWarning("MusicManager: Attempted to play a null song.");
        }
    }
}

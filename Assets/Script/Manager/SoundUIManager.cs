using UnityEngine;

public class SoundUIManager : MonoBehaviour
{
    public static SoundUIManager Instance { get; private set; }

    [SerializeField] private AudioSource audioSource;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        audioSource ??= gameObject.AddComponent<AudioSource>();
    }

    public void PlayUISound(AudioClip clip)
    {
        if (clip == null) return;
        audioSource.PlayOneShot(clip);
    }

    public void PlayLoopSound(AudioClip clip)
    {
        if (clip == null) return;
        audioSource.clip = clip;
        audioSource.loop = true;
        audioSource.Play();
    }

    public void StopLoopSound()
    {
        audioSource.loop = false;
        audioSource.Stop();
    }
}

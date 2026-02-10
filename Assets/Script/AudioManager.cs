using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    [Header("audio source")]
    [SerializeField] AudioSource musicSource;
    [SerializeField] AudioSource SFXSource;
    [Header("audio clip")]
    public AudioClip background;
    public AudioClip fall;
    public AudioClip checkpoint;
    public AudioClip Walk;
    public AudioClip runBatton;
    public AudioClip button;

    private void Start()
    {
        musicSource.clip = background;
        musicSource.Play();
    }

    public void PlaySFX(AudioClip Clip)
    {
        SFXSource.PlayOneShot(Clip);
    }

}

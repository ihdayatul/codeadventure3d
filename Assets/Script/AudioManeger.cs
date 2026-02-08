using UnityEngine;
using UnityEngine.Audio;

public class AudioManeger : MonoBehaviour
{
    [Header("audio source")]
    [SerializeField] AudioSource musicSource;
    [SerializeField] AudioSource SFXSource;
    [Header("audio clip")]
    public AudioClip background;
    public AudioClip fall;
    public AudioClip checkpoint;
    public AudioClip wolk;
    public AudioClip runBatton;

}

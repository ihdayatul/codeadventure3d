using UnityEngine;

public class FallTrigger : MonoBehaviour
{
    AudioManager audioManager;

    private void Awake()
    {
        audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
    }
    private void OnTriggerEnter(Collider other)
    {
        audioManager.PlaySFX(audioManager.fall);
        if (other.CompareTag("Player"))
        {
            GameManager.instance.PlayerFall();
        }
    }
}

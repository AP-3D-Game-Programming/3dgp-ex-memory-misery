using UnityEngine;
using System.Collections;

public class RandomSpookySounds : MonoBehaviour
{
    [Header("Geluiden")]
    public AudioClip[] spookyClips; 
    public AudioSource source;

    [Header("Timing")]
    public float minWait = 5f;  
    public float maxWait = 15f; 
    void Start()
    {
        if (source == null) source = GetComponent<AudioSource>();
        StartCoroutine(PlaySoundLoop());
    }

    IEnumerator PlaySoundLoop()
    {
        while (true) 
        {
            float waitTime = Random.Range(minWait, maxWait);
            yield return new WaitForSeconds(waitTime);

            if (spookyClips.Length > 0)
            {
                AudioClip clip = spookyClips[Random.Range(0, spookyClips.Length)];

                source.pitch = Random.Range(0.8f, 1.1f);

                source.PlayOneShot(clip);
            }
        }
    }
}
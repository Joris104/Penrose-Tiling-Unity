using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicController : MonoBehaviour
{
    AudioSource audioSource;
    [SerializeField]
    public List<AudioClip> clips = new List<AudioClip>();
    int clip = 0;
    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = clips[clip];
        audioSource.Play();
    }

    // Update is called once per frame
    void Update()
    {
        if (!audioSource.isPlaying)
        {
            clip = (clip+1)%clips.Count;
            audioSource.clip = clips[clip];
            audioSource.Play();
        }
    }
}

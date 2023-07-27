using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioPlayer : MonoBehaviour
{
    bool isPlay = false;
    public AudioSource audioSource;
    float clock = 0;
    private void OnEnable()
    {
        isPlay = false;
        clock = 0;
    }
    private void Update()
    {
        if (isPlay)
        {
            clock += Time.deltaTime;
            if (clock >= audioSource.clip.length) 
            {
                AudioController.instance.ReturnPool(gameObject);
            }
        }
    }
    public void Playing(AudioClip clip)
    {
        if (!isPlay)
        {
            audioSource.clip = clip;
            audioSource.Play();
            isPlay = true;
        }
    }
}

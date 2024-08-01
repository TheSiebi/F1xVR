using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioPlay : MonoBehaviour
{
    public AudioClip menu;
    public AudioSource audioSource;

    void Start()
    {
        float newScale = Mathf.Lerp(0f, 0.25f, 0.5f);
        audioSource.volume = newScale;
        audioSource.PlayOneShot(menu);
    }
}

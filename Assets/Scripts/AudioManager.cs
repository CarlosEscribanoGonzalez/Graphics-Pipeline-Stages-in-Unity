using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    private AudioSource musicSource;
    [SerializeField] private List<AudioClip> musicClips;
    private int currentId = 0;


    private void Awake()
    {
        instance = this;
        musicSource = GetComponent<AudioSource>();
    }

    public void PlaySFX(AudioClip audioClip, float volume = 1f)
    {
        StartCoroutine(PlaySFXCoroutine(audioClip, volume));
    }

    public void StopMusic()
    {
        musicSource.Stop();
        currentId = (currentId + 1) % 2;
    }

    public void PlayMusic()
    {
        musicSource.clip = musicClips[currentId];
        musicSource.Play();
    }


    IEnumerator PlaySFXCoroutine(AudioClip audioClip, float volume = 1f)
    {
        AudioSource audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = audioClip;
        audioSource.volume = volume;
        audioSource.Play();

        yield return new WaitForSeconds(audioSource.clip.length * 2);

        Destroy(audioSource);
    }   
}
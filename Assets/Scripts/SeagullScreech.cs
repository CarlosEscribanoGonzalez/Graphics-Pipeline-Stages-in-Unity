using UnityEngine;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Audio;

public class SeagullScreech : MonoBehaviour
{

    private AudioSource soundSource;
    [SerializeField] private List<AudioClip> audioClips;
    private int currentId = 0;
    private float timer;

    bool isUnderwater = false;

    private void Start()
    {
        soundSource = GetComponent<AudioSource>();
        NextAudio();
        timer = Random.Range(20.0f, 30.0f);
    }

    private void FixedUpdate()
    {
        timer = timer - Time.deltaTime;
        if (timer <= 0) {
            timer = Random.Range(20.0f, 30.0f);
            StartCoroutine(PlaySFXCoroutine());
        }
    }

    private void NextAudio() {
        currentId = Random.Range(0, 2);
        soundSource.clip = audioClips[currentId];
        if (currentId == 0)
        {
            soundSource.volume = 0.6f;
        }
        else {
            soundSource.volume = 1.0f;
        }
    }

    IEnumerator PlaySFXCoroutine()
    {
        if (!isUnderwater) {
            soundSource.Play();
            yield return new WaitForSeconds(soundSource.clip.length);
            soundSource.Stop();
        }     
        NextAudio();
    }

    public void changeStatus() {
        isUnderwater = !isUnderwater;
    }

}

using UnityEngine;
using System.Collections;

/// <summary>
/// Singleton AudioManager used by tk2dUISoundButton ex: tk2dUIAudioManager.Instance.Play(audioClip);
/// </summary>
[AddComponentMenu("2D Toolkit/UI/Core/tk2dUIAudioManager")]
public class tk2dUIAudioManager : MonoBehaviour
{
    private static tk2dUIAudioManager instance;

    private AudioSource audioSrc;

    public static tk2dUIAudioManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType(typeof(tk2dUIAudioManager)) as tk2dUIAudioManager;
                if (instance == null)
                {
                    instance = new GameObject("tk2dUIAudioManager").AddComponent<tk2dUIAudioManager>();
                }
            }
            return instance;
        }
    }

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            if (instance != this)
            {
                Destroy(this); // remove self, but don't destroy the gameobject its attached to. i.e. don't kill the host object.
                return;
            }
        }
        Setup();
    }

    private void Setup()
    {
        if (audioSrc == null)
        {
            audioSrc = gameObject.GetComponent<AudioSource>();
        }
        if (audioSrc == null)
        {
            audioSrc = gameObject.AddComponent<AudioSource>();
            audioSrc.playOnAwake = false;
        }
    }

    /// <summary>
    /// Plays (One Shot) audio clip
    /// </summary>
    public void Play(AudioClip clip)
    {
        audioSrc.PlayOneShot(clip);
    }
}

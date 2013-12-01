using UnityEngine;
using System.Collections;

/// <summary>
/// Plays audioclips based on uiItem events
/// </summary>
[AddComponentMenu("2D Toolkit/UI/tk2dUISoundItem")]
public class tk2dUISoundItem : tk2dUIBaseItemControl
{
    /// <summary>
    /// Audio clip to play when the button transitions from up to down state. Requires an AudioSource component to be attached to work.
    /// </summary>
    public AudioClip downButtonSound;
    /// <summary>
    /// Audio clip to play when the button transitions from down to up state. Requires an AudioSource component to be attached to work.
    /// </summary>
    public AudioClip upButtonSound;
    /// <summary>
    /// Audio clip to play when the button is clicked. Requires an AudioSource component to be attached to work.
    /// </summary>
    public AudioClip clickButtonSound;
    /// <summary>
    /// Audio clip to play when the button on release. Requires an AudioSource component to be attached to work.
    /// </summary>
    public AudioClip releaseButtonSound;

    void OnEnable()
    {
        if (uiItem)
        {
            if (downButtonSound != null) { uiItem.OnDown += PlayDownSound; }
            if (upButtonSound != null) { uiItem.OnUp += PlayUpSound; }
            if (clickButtonSound != null) { uiItem.OnClick += PlayClickSound; }
            if (releaseButtonSound != null) { uiItem.OnRelease += PlayReleaseSound; }
        }
    }

    void OnDisable()
    {
        if (uiItem)
        {
            if (downButtonSound != null) { uiItem.OnDown -= PlayDownSound; }
            if (upButtonSound != null) { uiItem.OnUp -= PlayUpSound; }
            if (clickButtonSound != null) { uiItem.OnClick -= PlayClickSound; }
            if (releaseButtonSound != null) { uiItem.OnRelease -= PlayReleaseSound; }
        }
    }

    private void PlayDownSound()
    {
        PlaySound(downButtonSound);
    }

    private void PlayUpSound()
    {
        PlaySound(upButtonSound);
    }

    private void PlayClickSound()
    {
        PlaySound(clickButtonSound);
    }

    private void PlayReleaseSound()
    {
        PlaySound(releaseButtonSound);
    }

    //plays audioclip using audio manager
    private void PlaySound(AudioClip source)
    {
        tk2dUIAudioManager.Instance.Play(source);
    }

}

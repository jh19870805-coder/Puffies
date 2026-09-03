using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AudioCatalog", menuName = "Puffies/Audio Catalog")]
public sealed class AudioCatalog : ScriptableObject
{
    [SerializeField]
    private AudioClip[] clips = Array.Empty<AudioClip>();

    public IReadOnlyList<AudioClip> Clips => clips;

#if UNITY_EDITOR
    public void SetClips(AudioClip[] audioClips)
    {
        clips = audioClips ?? Array.Empty<AudioClip>();
    }
#endif
}

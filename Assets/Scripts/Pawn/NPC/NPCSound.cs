using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class NPCSound : MonoBehaviour
{
    [Header("Sound Groups")]
    [SerializeField] private List<SoundGroup> soundGroups; //Used because Unity doesn't want to serialize a dictionary.

    private Dictionary<SoundType, List<AudioClip>> _audioClipsDictionary = new();
    private AudioSource _audioSource;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();

        _audioClipsDictionary = new Dictionary<SoundType, List<AudioClip>>();
        foreach (SoundGroup group in soundGroups)
        {
            _audioClipsDictionary[group.soundType] = group.clips;
        }
    }

    public void PlayRandomWalk(float volume = 1f)
    {
        List<AudioClip> clips = _audioClipsDictionary[SoundType.Walking];
        AudioClip randomClip = clips[Random.Range(0, clips.Count)];

        if(!_audioSource.isPlaying)
            PlayAudio(randomClip, volume);
    }

    public void PlayRandomMeow(float volume = 2f)
    {
        List<AudioClip> clips = _audioClipsDictionary[SoundType.Meowing];
        AudioClip randomClip = clips[Random.Range(0, clips.Count)];

        if(!_audioSource.isPlaying)
            PlayAudio(randomClip, volume);
    }

    public void PlayAudio(AudioClip sound, float volume)
    {
        _audioSource.PlayOneShot(sound, volume);
    }
}

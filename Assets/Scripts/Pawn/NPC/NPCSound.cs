using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class NPCSound : MonoBehaviour
{
    [Header("Sound Groups")]
    [SerializeField] private List<SoundGroup> _soundGroups; //Used because Unity doesn't want to serialize a dictionary.

    [SerializeField] private float _pitchVariance = 0.05f;

    private Dictionary<SoundType, List<AudioClip>> _audioClipsDictionary = new();
    private AudioSource _audioSource;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();

        _audioClipsDictionary = new Dictionary<SoundType, List<AudioClip>>();
        foreach (SoundGroup group in _soundGroups)
        {
            _audioClipsDictionary[group.soundType] = group.clips;
        }
    }

    public void PlayRandomWalk(float volume = 1f)
    {
        SetRandomPitch();
        List<AudioClip> clips = _audioClipsDictionary[SoundType.Walking];
        AudioClip randomClip = clips[Random.Range(0, clips.Count)];

        if(!_audioSource.isPlaying)
            PlayAudio(randomClip, volume);
    }

    public void PlayRandomMeow(float volume = 2f)
    {
        SetRandomPitch();
        List<AudioClip> clips = _audioClipsDictionary[SoundType.Meowing];
        AudioClip randomClip = clips[Random.Range(0, clips.Count)];

        if(!_audioSource.isPlaying)
            PlayAudio(randomClip, volume);
    }

    public void PlayAudio(AudioClip sound, float volume)
    {
        _audioSource.PlayOneShot(sound, volume);
    }

    private void SetRandomPitch()
    {
        float randomPitch = Random.Range(1f - _pitchVariance, 1f + _pitchVariance);

        _audioSource.pitch = randomPitch;
    }
}

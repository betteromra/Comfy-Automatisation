using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using static DayNightCycleManager;

public enum SoundType
{
    Music,
    Ambience,
    SunriseAmbience,
    DayAmbience,
    SunsetAmbience,
    NightAmbience,
    Walking,
    Meowing,
    Popping
}

[System.Serializable]
public class SoundGroup
{
    public SoundType soundType;
    public List<AudioClip> clips;
}

public class SoundManager : MonoBehaviour
{
    [Header("Sound Groups")]
    [SerializeField] private List<SoundGroup> soundGroups; //Used because Unity doesn't want to serialize a dictionary.

    [Header("Audio Sources")]
    [SerializeField] private AudioSource _musicAudioSource;
    [SerializeField] private AudioSource _ambienceSound;
    [SerializeField] private AudioSource _sfxSound;

    [Header("Settings")]
    [Range(0, 1)][SerializeField] private float _chanceOfPlayingAmbience = 0.5f;
    [SerializeField] private Vector2 _ambiencePlayDelay = new(20f, 40f);
    [Range(0, 1)][SerializeField] private float _chanceOfPlayingMusic = 0.7f;
    [SerializeField] private Vector2 _musicPlayDelay = new(20f, 40f);

    private Dictionary<SoundType, List<AudioClip>> _audioClipsDictionary = new();
    private TimeOfDay _timeOfDay;

    private Timer _ambienceTimer;
    private Timer _musicTimer;

    private void OnEnable()
    {
        GameManager.instance.buildingManager.onBuildingCreated += PlayRandomPopping;
        GameManager.instance.buildingManager.onBuildingDeleted += PlayRandomPopping;
    }

    private void Osable()
    {
        GameManager.instance.buildingManager.onBuildingCreated -= PlayRandomPopping;
        GameManager.instance.buildingManager.onBuildingDeleted -= PlayRandomPopping;  
    }

    private void Awake()
    {
        _audioClipsDictionary = new Dictionary<SoundType, List<AudioClip>>();
        foreach (SoundGroup group in soundGroups)
        {
            _audioClipsDictionary[group.soundType] = group.clips;
        }
    }

    private void Start()
    {
        _ambienceTimer = GetRandomTime(_ambiencePlayDelay);
        _musicTimer = GetRandomTime(_musicPlayDelay);

        _timeOfDay = GameManager.instance.dayNightCycleManager.CurrentTimeOfDay;
        PlayRandomAmbience();
        PlayRandomMusic();
    }

    private void Update()
    {
        if (_ambienceTimer.IsOverLoop())
        {
            if (Random.value < _chanceOfPlayingAmbience)
                PlayRandomAmbience();

            _ambienceTimer.duration = Random.Range(_ambiencePlayDelay.x, _ambiencePlayDelay.y); // reset random delay
        }

        if (_musicTimer.IsOverLoop())
        {
            if (Random.value < _chanceOfPlayingMusic)
                PlayRandomMusic();

            _musicTimer.duration = Random.Range(_musicPlayDelay.x, _musicPlayDelay.y);
        }
    }

    public void PlayRandomPopping()
    {
        List<AudioClip> clips = _audioClipsDictionary[SoundType.Popping];

        AudioClip randomClip = clips[Random.Range(0, clips.Count)];
        PlaySFX(randomClip, 1f);
    }

    public void PlayRandomAmbience(float volume = 1f)
    {
        SoundType soundType = GetAmbienceTypeForTimeOfDay(_timeOfDay);

        List<AudioClip> clips = _audioClipsDictionary[soundType];
        clips.AddRange(_audioClipsDictionary[SoundType.Ambience]); //Adds standard ambience as well

        AudioClip randomClip = clips[Random.Range(0, clips.Count)];
        PlayAmbience(randomClip, volume);
    }

    public void PlayRandomMusic(float volume = 1f)
    {
        if (_musicAudioSource.isPlaying) return;

        List<AudioClip> clips = _audioClipsDictionary[SoundType.Music];
        AudioClip randomClip = clips[Random.Range(0, clips.Count)];
        PlayMusic(randomClip, volume);
    }

    public void PlayAmbience(AudioClip sound, float volume)
    {
        _ambienceSound.PlayOneShot(sound, volume);
    }

    public void PlayMusic(AudioClip sound, float volume = 1)
    {
        _musicAudioSource.PlayOneShot(sound, volume);
    }

    public void PlaySFX(AudioClip sound, float volume = 1)
    {
        _sfxSound.PlayOneShot(sound, volume);
    }

    private SoundType GetAmbienceTypeForTimeOfDay(TimeOfDay time)
    {
        return time switch
        {
            TimeOfDay.Sunrise => SoundType.SunriseAmbience,
            TimeOfDay.Day => SoundType.DayAmbience,
            TimeOfDay.Sunset => SoundType.SunsetAmbience,
            TimeOfDay.Night => SoundType.NightAmbience,
            _ => SoundType.Ambience
        };
    }

    private Timer GetRandomTime(Vector2 waitTime) => new(waitTime.x, waitTime.y);
}
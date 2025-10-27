using UnityEngine;

public class GameManager : MonoBehaviour
{
  static GameManager _instance;
  public static GameManager instance { get => _instance; }
  [SerializeField] LevelManager _levelManager;
  public LevelManager levelManager { get => _levelManager; }
  [SerializeField] CameraManager _cameraManager;
  public CameraManager cameraManager { get => _cameraManager; }
  [SerializeField] SoundManager _soundManager;
  public SoundManager soundManager { get => _soundManager; }
  [SerializeField] Player _player;
  public Player player { get => _player; }
  void Awake()
  {
    if (_instance != null && _instance != this)
    {
      _instance.OnSceneLoaded(_levelManager, _cameraManager, _soundManager, _player);
      Destroy(gameObject);
      return;
    }
    else
    {
      DontDestroyOnLoad(this);
      _instance = this;
      OnSceneLoaded(_levelManager, _cameraManager, _soundManager, _player);
    }
  }

  public void OnSceneLoaded(LevelManager levelManager, CameraManager cameraManager, SoundManager soundManager, Player player)
  {
    _levelManager = levelManager;
    _cameraManager = cameraManager;
    _soundManager = soundManager;
    _player = player;
  }
}

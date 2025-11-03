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
  [SerializeField] BuildingManager _buildingManager;
  public BuildingManager buildingManager { get => _buildingManager; }
  [SerializeField] NonPlayableCharacterManager _nonPlayableCharacter;
  public NonPlayableCharacterManager nonPlayableCharacter { get => _nonPlayableCharacter; }
  [SerializeField] UserInterfaceManager _userInterfaceManager;
  public UserInterfaceManager userInterfaceManager { get => _userInterfaceManager; }
  [SerializeField] SelectionManager _selectionManager;
  public SelectionManager selectionManager { get => _selectionManager; }
  [SerializeField] QuestManager _questManager;
  public QuestManager questManager { get => _questManager; }
  [SerializeField] Player _player;
  public Player player { get => _player; }
  void Awake()
  {
    if (_instance != null && _instance != this)
    {
      _instance.OnSceneLoaded(_levelManager, _cameraManager, _soundManager, _buildingManager, _userInterfaceManager,  _selectionManager, _questManager, _player);
      Destroy(gameObject);
      return;
    }
    else
    {
      DontDestroyOnLoad(this);
      _instance = this;
      OnSceneLoaded(_levelManager, _cameraManager, _soundManager, _buildingManager, _userInterfaceManager, _selectionManager, _questManager, _player);
    }
  }

  public void OnSceneLoaded(LevelManager levelManager, CameraManager cameraManager, SoundManager soundManager, BuildingManager buildingManager, UserInterfaceManager userInterfaceManager, SelectionManager selectionManager, QuestManager questManager, Player player)
  {
    _levelManager = levelManager;
    _cameraManager = cameraManager;
    _soundManager = soundManager;
    _player = player;
    _buildingManager = buildingManager;
    _userInterfaceManager = userInterfaceManager;
    _selectionManager = selectionManager;
    _questManager = questManager;
  }
}

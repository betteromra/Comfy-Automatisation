using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UserInterfaceManager : MonoBehaviour
{
    [SerializeField] ToolBarUI _toolbarUI;
    [SerializeField] DisplayRessourceUI _displayRessourceUI;
    public DisplayRessourceUI displayRessourceUI { get => _displayRessourceUI; }
    [SerializeField] float ingredientOnBuildingSize = 1;
    [SerializeField] Canvas _mainCanvas;
    public Canvas mainCanvas { get => _mainCanvas; }
    [SerializeField] Color[] _quality;
    public Color[] quality { get => _quality; }
    [SerializeField] TextMeshProUGUI _buildingAmount;
    [SerializeField] TextMeshProUGUI _npcAmount;
    [SerializeField] TextMeshProUGUI _gold;
    BuildingUI _currentBuildingUIOpen;
    public BuildingUI currentBuildingUIOpen
    {
        get => _currentBuildingUIOpen; set
        {
            _currentBuildingUIOpen = value;
            _isBuildingUIOpen = _currentBuildingUIOpen != null;
        }
    }
    bool _isBuildingUIOpen = false;
    public bool isBuildingUIOpen { get => _isBuildingUIOpen; }
    CameraManager _cameraManager;
    BuildingManager _buildingManager;
    NonPlayableCharacterManager _npcManager;
    ShopManager _shopManager;
    Vector2 _screenOffSetNeed = Vector2.zero;
    public Vector2 screenOffSetNeeded { get => _screenOffSetNeed; }

    private void Awake()
    {
        _cameraManager = GameManager.instance.cameraManager;
        _buildingManager = GameManager.instance.buildingManager;
        _npcManager = GameManager.instance.nonPlayableCharacter;
        _shopManager = GameManager.instance.shopManager;
        _screenOffSetNeed.x = _mainCanvas.GetComponent<CanvasScaler>().referenceResolution.x * .5f;
        _screenOffSetNeed.y = -_mainCanvas.GetComponent<CanvasScaler>().referenceResolution.y * .5f;
        UpdateSize();
    }
    private void OnEnable()
    {
        _cameraManager.onZoom += UpdateSize;
        _buildingManager.onBuildingCreated += BuildingCreated;
        _buildingManager.onBuildingDeleted += BuildingDeleted;
        _shopManager.onGoldChanged += RefreshGoldAmount;
        _npcManager.onNpcCreated += RefreshNpcAmount;
        _npcManager.onNpcDeleted += RefreshNpcAmount;
    }
    private void OnDisable()
    {
        _cameraManager.onZoom -= UpdateSize;
        _buildingManager.onBuildingCreated -= UpdateSize;
        _buildingManager.onBuildingDeleted -= BuildingDeleted;
        _shopManager.onGoldChanged -= RefreshGoldAmount;
        _npcManager.onNpcCreated -= RefreshNpcAmount;
        _npcManager.onNpcDeleted -= RefreshNpcAmount;
    }

    void BuildingCreated()
    {
        UpdateSize();
        RefreshBuildAmount();
    }
    void BuildingDeleted()
    {
        RefreshBuildAmount();
    }

    void UpdateSize()
    {
        float zoomLevel = GameManager.instance.cameraManager.zoomLevel;
        foreach (Building building in _buildingManager.buildings)
        {
            if (building.ressourceAndAmountToDisplayUI != null)
            {
                building.ressourceAndAmountToDisplayUI.transform.localScale = Vector3.one * (.5f + ingredientOnBuildingSize * zoomLevel);
            }
        }
    }

    public void RefreshBuildAmount()
    {
        _buildingAmount.text = _buildingManager.buildingsCreated.Count + "";
    }
    public void RefreshNpcAmount()
    {
        _npcAmount.text = _npcManager.npcs.Count + "";
    }
    public void RefreshGoldAmount()
    {
        _gold.text = _shopManager.goldStored + "";
    }
}

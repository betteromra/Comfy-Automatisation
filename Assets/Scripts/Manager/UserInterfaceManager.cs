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
    Vector2 _screenOffSetNeed = Vector2.zero;
    public Vector2 screenOffSetNeeded { get => _screenOffSetNeed; }

    private void Awake()
    {
        _cameraManager = GameManager.instance.cameraManager;
        _buildingManager = GameManager.instance.buildingManager;
        _screenOffSetNeed.x = _mainCanvas.GetComponent<CanvasScaler>().referenceResolution.x * .5f;
        _screenOffSetNeed.y = -_mainCanvas.GetComponent<CanvasScaler>().referenceResolution.y * .5f;
        UpdateSize();
    }
    private void OnEnable()
    {
        _cameraManager.onZoom += UpdateSize;
        _buildingManager.onBuildingCreated += UpdateSize;
    }
    private void OnDisable()
    {
        _cameraManager.onZoom -= UpdateSize;
        _buildingManager.onBuildingCreated -= UpdateSize;
    }

    void UpdateSize()
    {
        float zoomLevel = GameManager.instance.cameraManager.zoomLevel;
        foreach (Building building in GameManager.instance.buildingManager.buildings)
        {
            if (building.ressourceAndAmountToDisplayUI != null)
            {
                building.ressourceAndAmountToDisplayUI.transform.localScale = Vector3.one * (.5f + ingredientOnBuildingSize * zoomLevel);
            }
        }
    }
}

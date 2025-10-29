using UnityEngine;

public class UserInterfaceManager : MonoBehaviour
{
    [SerializeField] ToolBarUI _toolbar;
    [SerializeField] float ingredientOnBuildingSize = 1;
    CameraManager _cameraManager;

    private void Awake()
    {
        _cameraManager = GameManager.instance.cameraManager;
        UpdateSize();
    }
    private void OnEnable()
    {
        _cameraManager.onZoom += UpdateSize;
    }

    void UpdateSize()
    {
        float zoomLevel = GameManager.instance.cameraManager.zoomLevel;
        foreach (Building building in GameManager.instance.buildingManager.buildings)
        {
            if (building.ingredientToDisplayUI != null)
            {
                building.ingredientToDisplayUI.transform.localScale = Vector3.one * (.5f + ingredientOnBuildingSize * zoomLevel);
            }
        }
    }
}

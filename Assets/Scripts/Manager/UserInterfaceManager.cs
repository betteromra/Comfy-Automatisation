using UnityEngine;

public class UserInterfaceManager : MonoBehaviour
{
    [SerializeField] float ingredientOnBuildingSize = 1;
    Player _player;

    private void Awake()
    {
        _player = GameManager.instance.player;
    }
    private void OnEnable()
    {
        _player.onZoomCamera += UpdateSize;
    }

    void UpdateSize()
    {
        float zoomLevel = GameManager.instance.cameraManager.zoomLevel;
        foreach (Building building in GameManager.instance.buildingManager.buildings)
        {
            if (building.ingredientToDisplayUI != null)
            {
                building.ingredientToDisplayUI.transform.localScale *= ingredientOnBuildingSize * zoomLevel;
            }
        }
    }
}

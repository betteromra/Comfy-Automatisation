using System.Linq;
using UnityEngine;

public class DisplayBuildingToolBarUI : MonoBehaviour
{
    [SerializeField] ToolBarUI _toolBarUI;
    [SerializeField] BuildingSO[] _buildingsSO;
    [SerializeField] BuildingToolBarUI[] _buildingsToolBarUI;
    BuildingSO[] _canCraftBuildingSO;
    BuildingManager _buildingManager;
    void Awake()
    {
        _buildingManager = GameManager.instance.buildingManager;
        _canCraftBuildingSO = _buildingsSO; // to change for actual craftable building
        Refresh();
    }

    void OnEnable()
    {
        _buildingManager.barn.inventory.onContentChange += Refresh;
    }

    void OnDisable()
    {
        _buildingManager.barn.inventory.onContentChange -= Refresh;
    }

    void Refresh()
    {
        for (int i = 0; i < _buildingsSO.Length; i++)
        {
            BuildingToolBarUI buildingToolBarUI = _buildingsToolBarUI[i];
            BuildingSO buildingSO = _buildingsSO[i];

            buildingToolBarUI.DisplayBuilding(buildingSO, _buildingManager.CanBuild(buildingSO), _toolBarUI);
        }
    }
}

using System.Linq;
using UnityEngine;

public class DisplayBuildingToolBarUI : MonoBehaviour
{
    [SerializeField] ToolBarUI _toolBarUI;
    [SerializeField] BuildingSO[] _buildingsSO;
    [SerializeField] BuildingToolBarUI[] _buildingsToolBarUI;
    BuildingSO[] _canCraftBuildingSO;
    void Awake()
    {
        _canCraftBuildingSO = _buildingsSO; // to change for actual craftable building
        Refresh();
    }

    void Refresh()
    {
        for (int i = 0; i < _buildingsSO.Length; i++)
        {
            BuildingToolBarUI buildingToolBarUI = _buildingsToolBarUI[i];
            BuildingSO buildingSO = _buildingsSO[i];

            buildingToolBarUI.DisplayBuilding(buildingSO, _canCraftBuildingSO.Contains(buildingSO), _toolBarUI);
        }
    }
}

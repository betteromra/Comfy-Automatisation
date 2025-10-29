using UnityEngine;
using UnityEngine.UI;

public class BuildingToolBarUI : MonoBehaviour
{
    [SerializeField] Image _image;
    [SerializeField] Button _button;
    public void DisplayBuilding(BuildingSO building, bool canCraft, ToolBarUI toolBarUI)
    {
        _image.sprite = building.icon;

        _button.onClick.AddListener(() => toolBarUI.SelectBuilding(building));
        _button.interactable = canCraft;
    }
}

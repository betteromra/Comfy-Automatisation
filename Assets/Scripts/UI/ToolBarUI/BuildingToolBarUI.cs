using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class BuildingToolBarUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] Image _image;
    [SerializeField] Button _button;
    BuildingSO _buildingSO;
    public void DisplayBuilding(BuildingSO buildingSO, bool canCraft, ToolBarUI toolBarUI)
    {
        _buildingSO = buildingSO;
        _image.sprite = _buildingSO.ressourceSO.icon;

        _button.onClick.RemoveAllListeners();
        _button.onClick.AddListener(() => toolBarUI.SelectBuilding(_buildingSO));
        _button.interactable = canCraft;
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        GameManager.instance.userInterfaceManager.displayRessourceUI.Display(true, new RessourceAndAmount(_buildingSO.ressourceSO), new Vector2(1, 1));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        GameManager.instance.userInterfaceManager.displayRessourceUI.Display(false);
    }

}

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BuildingUI : MonoBehaviour
{
  [SerializeField] protected Building _building;
  [SerializeField] protected TextMeshProUGUI _name;
  [SerializeField] protected GameObject _container;
  protected Selectable _buildingSelectable;
  protected bool _open = false;
  protected virtual void Awake()
  {
    _buildingSelectable = _building.GetComponent<Selectable>();
    _name.text = _building.buildingSO.actualName;
  }
  protected virtual void OnEnable()
  {
    _buildingSelectable.onSelfSelected += OnBuildingSelected;
  }
  protected virtual void OnDisable()
  {
    _buildingSelectable.onSelfSelected -= OnBuildingSelected;
  }
  void OnBuildingSelected(bool selected)
  {
    _open = selected;
    UserInterfaceManager userInterfaceManager = GameManager.instance.userInterfaceManager;
    if (_open) userInterfaceManager.currentBuildingUIOpen = this;
    else
    {
      GameManager.instance.userInterfaceManager.displayRessourceUI.Display(false);
      userInterfaceManager.currentBuildingUIOpen = null;
    }
    Refresh();
  }
  protected virtual void Refresh()
  {
    _container.SetActive(_open);
  }
}

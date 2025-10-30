using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BuildingUI : MonoBehaviour
{
  [SerializeField] protected Building _building;
  [SerializeField] protected TextMeshProUGUI _name;
  protected Selectable _buildingSelectable;
  protected bool _open = false;
  protected virtual void Awake()
  {
    _buildingSelectable = _building.GetComponent<Selectable>();
    _name.text = _building.buildingSO.actualName;
  }
  void OnEnable()
  {
    _buildingSelectable.onSelectionChanged += BuildingSelectionChanged;
  }
  void OnDisable()
  {
    _buildingSelectable.onSelectionChanged -= BuildingSelectionChanged;
  }
  void BuildingSelectionChanged(bool selected)
  {
    _open = selected;
    Refresh();
  }
  protected virtual void Refresh()
  {
    gameObject.SetActive(_open);
  }
}

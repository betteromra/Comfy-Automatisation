using UnityEngine;
using UnityEngine.UI;

public class StorageUI : BuildingUI
{
  [SerializeField] DisplayInventoryUI _displayInventoryUI;
  protected override void Awake()
  {
    base.Awake();

    StorageBuilding storageBuilding = _building as StorageBuilding;
    _displayInventoryUI.inventory = storageBuilding.inventory;
  }

  protected override void Refresh()
  {
    base.Refresh();
    
    _displayInventoryUI.Refresh();
  }
}

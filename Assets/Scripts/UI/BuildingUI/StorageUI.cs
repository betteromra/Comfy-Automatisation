using UnityEngine;
using UnityEngine.UI;

public class StorageUI : BuildingUI
{
  [SerializeField] DisplayInventoryUI _displayInventoryUI;
  protected override void Awake()
  {
    StorageBuilding storageBuilding = _building as StorageBuilding;
    _displayInventoryUI.inventory = storageBuilding.inventory;
  }
}

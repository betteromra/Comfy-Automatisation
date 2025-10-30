using UnityEngine;
using UnityEngine.UI;

public class CrafterUI : BuildingUI
{
  [SerializeField] DisplayInventoryUI _displayInventoryInputUI;
  [SerializeField] DisplayInventoryUI _displayInventoryOutputUI;
  [SerializeField] Slider _ressourceCrafting;
  CraftBuilding _craftingBuilding;

  protected override void Awake()
  {
    base.Awake();

    _craftingBuilding = _building as CraftBuilding;
    _displayInventoryInputUI.inventory = _craftingBuilding.inputInventory;
    _displayInventoryOutputUI.inventory = _craftingBuilding.outputInventory;
  }

  protected override void OnEnable()
  {
    base.OnEnable();
    _craftingBuilding.onCrafting += RefreshCraftingBar;
  }

  protected override void OnDisable()
  {
    base.OnDisable();
    _craftingBuilding.onCrafting -= RefreshCraftingBar;
  }

  void RefreshCraftingBar()
  {
    if (_open) _ressourceCrafting.value = _craftingBuilding.craftingTimer.PercentTime();
  }

  protected override void Refresh()
  {
    base.Refresh();

    _displayInventoryInputUI.Refresh();
    _displayInventoryOutputUI.Refresh();
  }
}

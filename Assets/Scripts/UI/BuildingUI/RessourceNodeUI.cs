using UnityEngine;
using UnityEngine.UI;

public class RessourceNodeUI : BuildingUI
{
  [SerializeField] DisplayInventoryUI _displayInventoryUI;
  [SerializeField] Slider _ressourceExtraction;
  RessourceNode _ressourceNode;
  protected override void Awake()
  {
    base.Awake();

    _ressourceNode = _building as RessourceNode;
    _displayInventoryUI.inventory = _ressourceNode.inventory;
  }
  protected override void OnEnable()
  {
    base.OnEnable();
    _ressourceNode.onExtraction += RefreshExtracingBar;
  }

  protected override void OnDisable()
  {
    base.OnDisable();
    _ressourceNode.onExtraction -= RefreshExtracingBar;
  }

  void RefreshExtracingBar()
  {
    if (_open) _ressourceExtraction.value = _ressourceNode.extractionTimer.PercentTime();
  }

  protected override void Refresh()
  {
    base.Refresh();

    _displayInventoryUI.Refresh();
    RefreshExtracingBar();
  }
}

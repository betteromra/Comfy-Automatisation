using UnityEngine;
using UnityEngine.UI;

public class RessourceNodeUI : BuildingUI
{
  [SerializeField] DisplayInventoryUI _displayInventoryUI;
  [SerializeField] Slider _ressourceExtraction;
  RessourceNode _ressourceNode;
  protected override void Awake()
  {
    _ressourceNode = _building as RessourceNode;
    _displayInventoryUI.inventory = _ressourceNode.inventory;
  }
  void OEnable()
  {
    _ressourceNode.onExtraction += RefreshSlider;
  }

  void OnDisable()
  {
    _ressourceNode.onExtraction -= RefreshSlider;
  }

  void RefreshSlider()
  {
    if (_open) _ressourceExtraction.value = _ressourceNode.extractionTimer.PercentTime();
  }
}

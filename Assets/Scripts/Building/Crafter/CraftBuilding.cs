using UnityEngine;

public class CraftBuilding : Building
{
    [SerializeField] Inventory _inputInventory;
    [SerializeField] Inventory _outputInventory;
    protected override void Awake()
    {
        base.Awake();
        InventoryChange();
    }
    void OnEnable()
    {
        _outputInventory.onInventoryChange += InventoryChange;
    }

    void OnDisable()
    {
        _outputInventory.onInventoryChange -= InventoryChange;
    }
    void InventoryChange()
    {
        RessourceSO ressourceSO = _outputInventory.MostRessourceInside();
        UpdateIngredientToDisplay(ressourceSO,  _outputInventory.Contains(ressourceSO));
    }
}

using UnityEngine;

public class StorageBuilding : Building
{
    [SerializeField] protected Inventory _inventory;
    protected override void Awake()
    {
        base.Awake();
        InventoryChange();
    }
    void OnEnable()
    {
        _inventory.onInventoryChange += InventoryChange;
    }

    void OnDisable()
    {
        _inventory.onInventoryChange -= InventoryChange;
    }
    void InventoryChange()
    {
        RessourceSO ressourceSO = _inventory.MostRessourceInside();
        UpdateIngredientToDisplay(ressourceSO,  _inventory.Contains(ressourceSO));
    }
}

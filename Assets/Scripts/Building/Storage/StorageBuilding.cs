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
        _ingredientToDisplay.ressource = _inventory.MostRessourceInside();
        _ingredientToDisplay.amount = _inventory.Contains(_ingredientToDisplay.ressource);
        UpdateIngredientToDisplay();
    }
}

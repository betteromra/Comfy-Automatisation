using Unity.VisualScripting;
using UnityEngine;

public class RessourceNode : Building
{
    [SerializeField] Inventory _inventory;
    RessourceNodeSO _ressourceNodeSO;
    protected override void Awake()
    {
        base.Awake();
        _ressourceNodeSO = _buildingSO as RessourceNodeSO;
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

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
        RessourceSO ressourceSO = _inventory.MostRessourceInside();
        UpdateIngredientToDisplay(ressourceSO,  _inventory.Contains(ressourceSO));
    }
}

using UnityEngine;
using System;

public class CraftBuilding : Building
{
    [SerializeField] Inventory _inputInventory;
    public Inventory inputInventory { get => _inputInventory; }
    [SerializeField] Inventory _outputInventory;
    public Inventory outputInventory { get => _outputInventory; }
    [SerializeField] RecipeSO _selectedRecipeSO;
    public RecipeSO selectedRecipeSO { get => _selectedRecipeSO; set => _selectedRecipeSO = value; }
    Timer _craftingTimer;
    public Timer craftingTimer { get => _craftingTimer; }
    public event Action onCrafting;
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
        UpdateIngredientToDisplay(ressourceSO, _outputInventory.Contains(ressourceSO));
    }
}

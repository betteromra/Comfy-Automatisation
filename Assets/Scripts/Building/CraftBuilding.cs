using UnityEngine;
using System;
using System.Collections;

public class CraftBuilding : Building
{
    [SerializeField] Inventory _inputInventory;
    public Inventory inputInventory { get => _inputInventory; }
    [SerializeField] Inventory _outputInventory;
    public Inventory outputInventory { get => _outputInventory; }
    [SerializeField] RecipeSO _selectedRecipeSO;
    public RecipeSO selectedRecipeSO
    {
        get => _selectedRecipeSO;
        set
        {
            _selectedRecipeSO = value;
            _craftingTimer = new Timer(_selectedRecipeSO.craftingTime);
            _inputInventory.ClearInventory();
            UpdateIngredientToDisplay(new RessourceAndAmount(_selectedRecipeSO.ingredientsOutput[0].ressourceSO, -1));
        }
    }
    Timer _craftingTimer;
    public Timer craftingTimer { get => _craftingTimer; }
    Coroutine _crafting;
    public event Action onCrafting;

    protected override void Awake()
    {
        base.Awake();
        OutputInventoryChange();
    }
    void OnEnable()
    {
        _inputInventory.onInventoryChange += InputInventoryChange;
        _outputInventory.onInventoryChange += OutputInventoryChange;
    }

    void OnDisable()
    {
        _inputInventory.onInventoryChange -= InputInventoryChange;
        _outputInventory.onInventoryChange -= OutputInventoryChange;
    }
    void OutputInventoryChange()
    {

        RessourceSO ressourceSO = _outputInventory.MostRessourceInside();
        if (ressourceSO != null)
        {
            UpdateIngredientToDisplay(new RessourceAndAmount(ressourceSO, _outputInventory.Contains(ressourceSO)));
        }
        else if (_selectedRecipeSO != null)
        {
            UpdateIngredientToDisplay(new RessourceAndAmount(_selectedRecipeSO.ingredientsOutput[0].ressourceSO, -1));
        }
    }

    void InputInventoryChange()
    {
        if (_crafting == null && CanCraft())
        {
            _crafting = StartCoroutine(Craft());
        }
    }

    bool CanCraft()
    {
        if (_selectedRecipeSO == null) return false;
        if (!_outputInventory.CanAdd(_selectedRecipeSO.ingredientsOutput[0])) return false;

        foreach (RessourceAndAmount ressourceAndAmount in _selectedRecipeSO.ingredientsInput)
        {
            if (!_inputInventory.ContainsAmount(ressourceAndAmount)) return false;
        }

        return true;
    }

    IEnumerator Craft()
    {
        _craftingTimer.Restart();
        yield return null;
        while (true)
        {
            if (_craftingTimer.IsOver())
            {
                if (_selectedRecipeSO.Make(_inputInventory, _outputInventory))
                {
                    if (CanCraft()) _craftingTimer.Restart();
                    else break;
                }
                else break;
            }
            onCrafting?.Invoke();
            yield return null;
        }
        _crafting = null;
    }
}

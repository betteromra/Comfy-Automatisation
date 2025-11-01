using UnityEngine;
using System;
using System.Collections;

public class CraftBuilding : Building
{
    [SerializeField] protected CraftBuildingSO _craftBuildingSO;
    public CraftBuildingSO craftBuildingSO { get => _craftBuildingSO; }
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
            if (_selectedRecipeSO == value) return;

            _selectedRecipeSO = value;
            OnRecipeChange();
        }
    }
    Timer _craftingTimer;
    public Timer craftingTimer { get => _craftingTimer; }
    Coroutine _crafting;
    public event Action onCrafting;

    protected override void Awake()
    {
        base.Awake();
        OutputContentChange();
        _buildingSO = _craftBuildingSO;
        _inputInventory.maxSameRessourceSpace = _craftBuildingSO.inputSpace;
    }
    protected override  void OnEnable()
    {
        base.OnEnable();
        _inputInventory.onContentChange += InputContentChange;
        _outputInventory.onContentChange += OutputContentChange;
    }

    protected override  void OnDisable()
    {
        base.OnDisable();
        _inputInventory.onContentChange -= InputContentChange;
        _outputInventory.onContentChange -= OutputContentChange;
    }
    #region Crafting Logic
    void OutputContentChange()
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
    void OnRecipeChange()
    {
        // Change the variable based on the new recipe
        _craftingTimer = new Timer(_selectedRecipeSO.craftingTime * _craftBuildingSO.craftingSpeed);
        _inputInventory.ClearInventory(_selectedRecipeSO.ingredientsInput);

        _inputInventory.ClearWhiteList();
        _outputInventory.ClearWhiteList();

        _inputInventory.WhiteList(_selectedRecipeSO.ingredientsInput);
        _outputInventory.WhiteList(_selectedRecipeSO.ingredientsOutput);

        UpdateIngredientToDisplay(new RessourceAndAmount(_selectedRecipeSO.ingredientsOutput[0].ressourceSO, -1));
    }

    void InputContentChange()
    {
        StartCrafting();
    }

    void StartCrafting()
    {
        if (_crafting == null && CanCraft())
        {
            _crafting = StartCoroutine(Craft());
        }
    }

    bool CanCraft()
    {
        if (_selectedRecipeSO == null) return false;

        // make sure we can add to the output
        if (!_outputInventory.CanAdd(_selectedRecipeSO.ingredientsOutput)) return false;

        // make sur we have the right amount
        return _inputInventory.ContainsAmount(_selectedRecipeSO.ingredientsInput);
    }

    IEnumerator Craft()
    {
        _craftingTimer.Restart();
        yield return null;
        while (true)
        {
            // wait until we can craft
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
    #endregion
}

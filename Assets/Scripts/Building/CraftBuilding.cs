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
            _craftingTimer = new Timer(_selectedRecipeSO.craftingTime * _craftBuildingSO.craftingSpeed);
            _inputInventory.ClearInventory(_selectedRecipeSO.ingredientsInput);

            _inputInventory.ClearWhiteList();
            _outputInventory.ClearWhiteList();

            _inputInventory.WhiteList(_selectedRecipeSO.ingredientsInput);
            _outputInventory.WhiteList(_selectedRecipeSO.ingredientsOutput);

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
        OutputContentChange();
        _buildingSO = _craftBuildingSO;
        _inputInventory.maxSameRessourceSpace = _craftBuildingSO.inputSpace;
    }
    void OnEnable()
    {
        _inputInventory.onContentChange += InputContentChange;
        _outputInventory.onContentChange += OutputContentChange;
    }

    void OnDisable()
    {
        _inputInventory.onContentChange -= InputContentChange;
        _outputInventory.onContentChange -= OutputContentChange;
    }
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
        if (!_outputInventory.CanAdd(_selectedRecipeSO.ingredientsOutput)) return false;

        return _inputInventory.ContainsAmount(_selectedRecipeSO.ingredientsInput);
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

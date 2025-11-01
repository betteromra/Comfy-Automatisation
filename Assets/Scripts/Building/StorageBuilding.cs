using UnityEngine;

public class StorageBuilding : Building
{
    [SerializeField] protected Inventory _inventory;
    public Inventory inventory { get => _inventory; }
    [SerializeField] protected StorageBuildingSO _storageBuildingSO;
    public StorageBuildingSO storageBuildingSO { get => _storageBuildingSO; }
    protected override void Awake()
    {
        base.Awake();
        _buildingSO = _storageBuildingSO;
        ContentChange();
    }
    void OnEnable()
    {
        _inventory.onContentChange += ContentChange;
    }

    void OnDisable()
    {
        _inventory.onContentChange -= ContentChange;
    }
    void ContentChange()
    {
        RessourceSO ressourceSO = _inventory.MostRessourceInside();
        UpdateIngredientToDisplay(new RessourceAndAmount(ressourceSO, _inventory.Contains(ressourceSO)));
    }
}

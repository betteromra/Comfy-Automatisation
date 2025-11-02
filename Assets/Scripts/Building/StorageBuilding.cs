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
    protected override void OnEnable()
    {
        base.OnEnable();
        _inventory.onContentChange += ContentChange;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        _inventory.onContentChange -= ContentChange;
    }
    void ContentChange()
    {
        RessourceSO ressourceSO = _inventory.MostRessourceInside();
        UpdateIngredientToDisplay(new RessourceAndAmount(ressourceSO, _inventory.Contains(ressourceSO)));
    }
}

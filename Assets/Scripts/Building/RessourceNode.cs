using Unity.VisualScripting;
using UnityEngine;
using System;

public class RessourceNode : Building
{
    [SerializeField] Inventory _inventory;
    public Inventory inventory { get => _inventory; }
    RessourceNodeSO _ressourceNodeSO;
    Timer _extractionTimer;
    public Timer extractionTimer { get => _extractionTimer; }
    bool _isFull;
    public event Action onExtraction;
    protected override void Awake()
    {
        base.Awake();
        _ressourceNodeSO = _buildingSO as RessourceNodeSO;
        _extractionTimer = new Timer(1 / _ressourceNodeSO.ressourcePerMinute * 60);
        InventoryChange();
    }
    void FixedUpdate()
    {
        if (_isFull) return;

        onExtraction?.Invoke();
        
        if (_extractionTimer.IsOver())
        {
            if (_inventory.Add(_ressourceNodeSO.ressource, 1)) _extractionTimer.Restart();
            else _isFull = true;
        }
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
        _isFull = false;
        RessourceSO ressourceSO = _inventory.MostRessourceInside();
        UpdateIngredientToDisplay(ressourceSO, _inventory.Contains(ressourceSO));
    }
}

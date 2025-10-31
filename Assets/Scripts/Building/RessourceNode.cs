using Unity.VisualScripting;
using UnityEngine;
using System;
using System.Collections;

public class RessourceNode : Building
{
    [SerializeField] Inventory _inventory;
    public Inventory inventory { get => _inventory; }
    RessourceNodeSO _ressourceNodeSO;
    Timer _extractionTimer;
    public Timer extractionTimer { get => _extractionTimer; }
    public event Action onExtraction;
    Coroutine _extracting;
    protected override void Awake()
    {
        base.Awake();
        _ressourceNodeSO = _buildingSO as RessourceNodeSO;
        _extractionTimer = new Timer(1 / _ressourceNodeSO.ressourcePerMinute * 60);
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
        if (_extracting == null && _inventory.CanAdd(_ressourceNodeSO.ressourceAndAmount))
        {
            _extracting = StartCoroutine(Extract());
        }
        RessourceSO ressourceSO = _inventory.MostRessourceInside();
        if (ressourceSO != null)
        {
            UpdateIngredientToDisplay(new RessourceAndAmount(ressourceSO, _inventory.Contains(ressourceSO)));
        }
        else
        {
            UpdateIngredientToDisplay(new RessourceAndAmount(_ressourceNodeSO.ressourceAndAmount.ressourceSO, -_ressourceNodeSO.ressourceAndAmount.amount));
        }
    }

    IEnumerator Extract()
    {
        _extractionTimer.Restart();
        yield return null;
        while (true)
        {
            if (_extractionTimer.IsOver())
            {
                if (_inventory.Add(_ressourceNodeSO.ressourceAndAmount))
                {
                    if (_inventory.CanAdd(_ressourceNodeSO.ressourceAndAmount))
                        _extractionTimer.Restart();
                }
                else break;
            }
            onExtraction?.Invoke();
            yield return null;
        }
        _extracting = null;
    }
}

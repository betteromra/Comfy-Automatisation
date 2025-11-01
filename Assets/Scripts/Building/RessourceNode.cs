using Unity.VisualScripting;
using UnityEngine;
using System;
using System.Collections;

public class RessourceNode : Building
{
    [SerializeField] Inventory _inventory;
    public Inventory inventory { get => _inventory; }
    [SerializeField] protected RessourceNodeSO _ressourceNodeSO;
    public RessourceNodeSO ressourceNodeSO { get => _ressourceNodeSO; }
    Timer _extractionTimer;
    public Timer extractionTimer { get => _extractionTimer; }
    public event Action onExtraction;
    Coroutine _extracting;
    protected override void Awake()
    {
        base.Awake();
        _buildingSO = _ressourceNodeSO;
        _extractionTimer = new Timer(1 / _ressourceNodeSO.extractionPerMinute * 60);

        _inventory.WhiteList(_ressourceNodeSO.ressourceAndAmount.ressourceSO);

        StartExtracting();
        InventoryChange();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        _inventory.onInventoryChange += InventoryChange;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        _inventory.onInventoryChange -= InventoryChange;
    }

    #region Extraction Logic
    void InventoryChange()
    {
        StartExtracting();
        RessourceSO ressourceSO = _inventory.MostRessourceInside();
        if (ressourceSO != null)
        {
            UpdateIngredientToDisplay(new RessourceAndAmount(ressourceSO, _inventory.Contains(ressourceSO)));
        }
        else
        {
            UpdateIngredientToDisplay(new RessourceAndAmount(_ressourceNodeSO.ressourceAndAmount.ressourceSO, -1));
        }
    }
    void StartExtracting()
    {
        if (_extracting == null && _inventory.CanAdd(_ressourceNodeSO.ressourceAndAmount))
        {
            _extracting = StartCoroutine(Extract());
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
    #endregion
}

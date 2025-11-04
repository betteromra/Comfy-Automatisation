using System;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    [SerializeField] float _percentageValueSell;
    int _goldStored = 0;
    public int goldStored { get => _goldStored; }
    public event Action onGoldChanged;
    public void Sell(RessourceAndAmount ressourceAndAmount)
    {
        _goldStored += Mathf.FloorToInt(ressourceAndAmount.value * _percentageValueSell);
        onGoldChanged?.Invoke();
    }
    public int CanBuyHowMany(RessourceSO ressourceSO)
    {
        if (ressourceSO == null) return 0;
        return Mathf.FloorToInt(_goldStored / ressourceSO.value);
    }
    public bool Buy(RessourceAndAmount ressourceAndAmount)
    {
        if (_goldStored - ressourceAndAmount.value >= 0)
        {
            _goldStored -= ressourceAndAmount.value;
            onGoldChanged?.Invoke();
            return true;
        }
        return false;
    }
}

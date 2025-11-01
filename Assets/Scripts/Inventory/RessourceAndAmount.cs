using System;
using UnityEngine;
using System.Collections.Generic;

[Serializable]
public class RessourceAndAmount
{
    [SerializeField] RessourceSO _ressourceSO;
    public RessourceSO ressourceSO { get => _ressourceSO; set => _ressourceSO = value; }
    [SerializeField] int _amount = 1;
    public int amount { get => _amount; set => _amount = value; }
    public int value
    {
        get => _ressourceSO.value * _amount;
    }
    public int rawValue
    {
        get => _ressourceSO.rawValue * _amount;
    }
    public int weight
    {
        get => _ressourceSO.weight * _amount;
    }
    public int spaceTotal
    {
        get => _ressourceSO.spacePerUnit * _amount;
    }

    public RessourceAndAmount(RessourceSO ressourceSO, int amount = 1)
    {
        _ressourceSO = ressourceSO;
        _amount = amount;
    }
    public RessourceAndAmount(KeyValuePair<RessourceSO, int> ressourceAndAmount) : this(ressourceAndAmount.Key, ressourceAndAmount.Value) { }
    public RessourceAndAmount(RessourceAndAmount ressourceAndAmount) : this(ressourceAndAmount.ressourceSO, ressourceAndAmount.amount) { }
    public void Reset()
    {
        _ressourceSO = null;
        _amount = 1;
    }
}

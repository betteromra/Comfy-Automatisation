using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

public class Inventory : MonoBehaviour
{
    [Header("Limitation")]
    [SerializeField] int _maxWeight = int.MaxValue; // Maximum weight for the inventory
    int _weight = 0;
    public int weight { get => _weight; }
    int _value = 0;
    public int value { get => _value; }
    // Maximum amount of different ressource
    [SerializeField] int _maxDifferentRessourceAmount = int.MaxValue;
    int _differentRessourceAmount = 0;
    // Maximum amount of the same ressource
    [SerializeField] int _maxSameRessourceSpace = int.MaxValue;
    Dictionary<RessourceSO, int> _ressourceStored = new Dictionary<RessourceSO, int>();

    // Event for when the inventoryChange
    public event Action onInventoryChange;

    #region Managing
    public void ClearInventory()
    {
        _ressourceStored = new Dictionary<RessourceSO, int>();
        _weight = 0;
        _value = 0;
        _differentRessourceAmount = 0;
        onInventoryChange?.Invoke();
    }
    public int Contains(RessourceSO objectToAdd)
    {
        if (objectToAdd == null) return 0;
        if (_ressourceStored.ContainsKey(objectToAdd))
        {
            return _ressourceStored[objectToAdd];
        }

        return 0;
    }

    public List<KeyValuePair<RessourceSO, int>> GetAllContent()
    {
        return _ressourceStored.ToList();
    }

    public bool ContainsAmount(RessourceAndAmount ressourceAndAmount)
    {
        if (ressourceAndAmount.ressourceSO == null || ressourceAndAmount.amount == 0) return false;
        if (_ressourceStored.ContainsKey(ressourceAndAmount.ressourceSO))
        {
            return ressourceAndAmount.amount <= _ressourceStored[ressourceAndAmount.ressourceSO];
        }

        return false;
    }

    public bool Add(RessourceAndAmount ressourceAndAmount)
    {
        if (!CanAdd(ressourceAndAmount)) return false;

        if (_ressourceStored.ContainsKey(ressourceAndAmount.ressourceSO))
        {
            _ressourceStored[ressourceAndAmount.ressourceSO] += ressourceAndAmount.amount;
        }
        else _ressourceStored.Add(ressourceAndAmount.ressourceSO, ressourceAndAmount.amount);

        onInventoryChange?.Invoke();

        // Adjust weight and ressource space
        _weight += ressourceAndAmount.weight;
        _value += ressourceAndAmount.value;
        _differentRessourceAmount++;
        return true;
    }

    public bool CanAdd(RessourceAndAmount ressourceAndAmount)
    {
        if (ressourceAndAmount.ressourceSO == null || ressourceAndAmount.amount <= 0) return false;

        if (WeightLeft() < ressourceAndAmount.weight)
        {
            // too heavy or not enough space
            Debug.LogWarning("Failed to add item : " + ressourceAndAmount.ressourceSO.actualName);
            return false;
        }

        if (_ressourceStored.ContainsKey(ressourceAndAmount.ressourceSO))
        {
            return _maxSameRessourceSpace >= (_ressourceStored[ressourceAndAmount.ressourceSO] + ressourceAndAmount.amount) * ressourceAndAmount.spaceTotal;
        }
        else
        {
            // verify if we can add a new sort of item
            if (DifferentRessourceSpaceLeft() <= 0)
            {
                Debug.LogWarning("Failed to add item : " + ressourceAndAmount.ressourceSO.actualName);
                return false;
            }
            return _maxSameRessourceSpace >= ressourceAndAmount.spaceTotal;
        }
    }

    public void Remove(RessourceAndAmount ressourceAndAmount)
    {
        if (ressourceAndAmount.ressourceSO == null || ressourceAndAmount.amount <= 0) return;
        if (_ressourceStored.ContainsKey(ressourceAndAmount.ressourceSO))
        {
            _ressourceStored[ressourceAndAmount.ressourceSO] -= ressourceAndAmount.amount;
            onInventoryChange?.Invoke();

            // keep track of the actual number of ressource removed
            int amountRemoved = ressourceAndAmount.amount;
            if (_ressourceStored[ressourceAndAmount.ressourceSO] < 0) amountRemoved += _ressourceStored[ressourceAndAmount.ressourceSO];

            // Make sure not negative object are stored
            _ressourceStored[ressourceAndAmount.ressourceSO] = Mathf.Clamp(_ressourceStored[ressourceAndAmount.ressourceSO], 0, int.MaxValue);

            // Adjust weight and ressource space
            _weight -= ressourceAndAmount.weight;
            _value -= ressourceAndAmount.value;
            _differentRessourceAmount--;
        }
        else
        {
            Debug.LogWarning("Could not remove item : " + ressourceAndAmount.ressourceSO.actualName + " since not present");
        }
    }

    public RessourceSO MostRessourceInside()
    {
        RessourceSO mostRessource = null;

        if (_ressourceStored.Count() == 0) return null;

        foreach (KeyValuePair<RessourceSO, int> ressourceAndAmount in _ressourceStored)
        {
            if (mostRessource == null)
            {
                mostRessource = ressourceAndAmount.Key;
                continue;
            }
            if (ressourceAndAmount.Value > _ressourceStored[mostRessource]) mostRessource = ressourceAndAmount.Key;
        }
        return mostRessource;
    }

    int WeightLeft()
    {
        return _maxWeight - _weight;
    }

    int DifferentRessourceSpaceLeft()
    {
        return _maxDifferentRessourceAmount - _differentRessourceAmount;
    }

    #endregion
}

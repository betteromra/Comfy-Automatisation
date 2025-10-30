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

    public bool ContainsAmount(RessourceSO objectToAdd, int amount)
    {
        if (objectToAdd == null || amount == 0) return false;
        if (_ressourceStored.ContainsKey(objectToAdd))
        {
            return amount <= _ressourceStored[objectToAdd];
        }

        return false;
    }

    public bool Add(RessourceSO objectToAdd, int amount)
    {
        if (objectToAdd == null || amount == 0) return false;
        int weightToAdd = objectToAdd.weight * amount;

        // Verify if the inventory can carry those item
        if (WeightLeft() <= weightToAdd || DifferentRessourceSpaceLeft() <= 0)
        {
            // too heavy or not enough space
            Debug.LogWarning("Failed to add item : " + objectToAdd.actualName);
            return false;
        }

        if (_ressourceStored.ContainsKey(objectToAdd))
        {
            if (_maxSameRessourceSpace >= (_ressourceStored[objectToAdd] + amount) * objectToAdd.spacePerUnit)
            {
                _ressourceStored[objectToAdd] += amount;
                onInventoryChange?.Invoke();
            }
            else
            {
                Debug.LogWarning("Failed to add item : " + objectToAdd.actualName);
                return false;
            }
        }
        else
        {
            if (_maxSameRessourceSpace >= amount * objectToAdd.spacePerUnit)
            {
                _ressourceStored.Add(objectToAdd, amount);
                onInventoryChange?.Invoke();
            }
            else
            {
                Debug.LogWarning("Failed to add item : " + objectToAdd.actualName);
                return false;
            }
        }

        // Adjust weight and ressource space
        _weight += weightToAdd;
        _value += objectToAdd.value * amount;
        _differentRessourceAmount++;
        return true;
    }

    public void Remove(RessourceSO objectToAdd, int amount)
    {
        if (objectToAdd == null || amount == 0) return;
        if (_ressourceStored.ContainsKey(objectToAdd))
        {
            _ressourceStored[objectToAdd] -= amount;
            onInventoryChange?.Invoke();

            // keep track of the actual number of ressource removed
            int amountRemoved = amount;
            if (_ressourceStored[objectToAdd] < 0) amountRemoved += _ressourceStored[objectToAdd];

            // Make sure not negative object are stored
            _ressourceStored[objectToAdd] = Mathf.Clamp(_ressourceStored[objectToAdd], 0, int.MaxValue);

            // Adjust weight and ressource space
            _weight -= objectToAdd.weight * amountRemoved;
            _value -= objectToAdd.value * amount;
            _differentRessourceAmount--;
        }
        else
        {
            Debug.LogWarning("Could not remove item : " + objectToAdd.actualName + " since not present");
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

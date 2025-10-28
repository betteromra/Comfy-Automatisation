using UnityEngine;
using System.Collections.Generic;

public class Inventory : MonoBehaviour
{
    [Header("Limitation")]
    [SerializeField] int _maxWeight = int.MaxValue; // Maximum weight for the inventory
    int _weight = 0;
    // Maximum amount of different ressource
    [SerializeField] int _maxDifferentRessourceAmount = int.MaxValue;
    int _differentRessourceAmount = 0;
    // Maximum amount of the same ressource
    [SerializeField] int _maxSameRessourceAmount = int.MaxValue;
    Dictionary<RessourceSO, int> _objectStored;

    #region Managing
    public bool Contains(RessourceSO objectToAdd, int amount)
    {
        if (_objectStored.ContainsKey(objectToAdd))
        {
            return amount <= _objectStored[objectToAdd];
        }

        return false;
    }

    public void Add(RessourceSO objectToAdd, int amount)
    {
        int weightToAdd = objectToAdd.weight * amount;

        // Verify if the inventory can carry those item
        if (WeightLeft() < weightToAdd || DifferentRessourceSpaceLeft() <= 0)
        {
            // too heavy or not enough space
            Debug.LogError("Failed to add item : " + objectToAdd.actualName);
            return;
        }

        if (_objectStored.ContainsKey(objectToAdd))
        {
            if (_maxSameRessourceAmount <= _objectStored[objectToAdd] + amount)
            {
                _objectStored[objectToAdd] += amount;
            }
            else
            {
                Debug.LogError("Failed to add item : " + objectToAdd.actualName);
                return;
            }
        }
        else
        {
            if (_maxSameRessourceAmount <= amount)
            {
                _objectStored.Add(objectToAdd, amount);
            }
            else
            {
                Debug.LogError("Failed to add item : " + objectToAdd.actualName);
                return;
            }
        }

        // Adjust weight and ressource space
        _weight += weightToAdd;
        _differentRessourceAmount++;

    }

    public void Remove(RessourceSO objectToAdd, int amount)
    {
        if (_objectStored.ContainsKey(objectToAdd))
        {
            _objectStored[objectToAdd] -= amount;

            // keep track of the actual number of ressource removed
            int amountRemoved = amount;
            if (_objectStored[objectToAdd] < 0) amountRemoved += _objectStored[objectToAdd];

            // Make sure not negative object are stored
            _objectStored[objectToAdd] = Mathf.Clamp(_objectStored[objectToAdd], 0, int.MaxValue);

            // Adjust weight and ressource space
            _weight -= objectToAdd.weight * amountRemoved;
            _differentRessourceAmount--;
        }
        else
        {
            Debug.LogError("Could not remove item : " + objectToAdd.actualName + " since not present");
        }
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

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
    public int maxSameRessourceSpace { get => _maxSameRessourceSpace; set => _maxSameRessourceSpace = value; }
    Dictionary<RessourceSO, int> _ressourcesStored = new Dictionary<RessourceSO, int>();
    public Dictionary<RessourceSO, int> ressourcesStored { get => new Dictionary<RessourceSO, int>(_ressourcesStored); }
    List<RessourceSO> _ressourcesWhiteListed = new List<RessourceSO>();
    public List<RessourceSO> ressourcesWhiteListed { get => new List<RessourceSO>(_ressourcesWhiteListed); }

    // Event for when the inventoryChange
    public event Action onContentChange;
    public event Action onInventoryChange;
    void OnEnable()
    {
        onContentChange += InventoryChanged;
    }
    void OnDisable()
    {
        onContentChange -= InventoryChanged;
    }

    #region Managing
    public void ClearInventory(RessourceAndAmount[] exceptions = null)
    {
        // if it is empty return
        if (_differentRessourceAmount == 0) return;

        if (exceptions == null) exceptions = new RessourceAndAmount[0];

        Dictionary<RessourceSO, int> execeptionRessourcesStored = new Dictionary<RessourceSO, int>();

        // Keep track of all the exception so we can add them back after the clear
        foreach (RessourceAndAmount exception in exceptions)
        {
            if (_ressourcesStored.ContainsKey(exception.ressourceSO))
            {
                execeptionRessourcesStored.Add(exception.ressourceSO, _ressourcesStored[exception.ressourceSO]);
            }
        }

        // Clear inventory
        _ressourcesStored = new Dictionary<RessourceSO, int>();
        _weight = 0;
        _value = 0;
        _differentRessourceAmount = 0;
        if (exceptions.Length > 0) onContentChange?.Invoke();

        Add(execeptionRessourcesStored.Select(kvp => new RessourceAndAmount(kvp)).ToArray());
    }

    public int ContainsHowMany(RessourceSO ressourceSO)
    {
        if (ressourceSO == null) return 0;
        if (_ressourcesStored.ContainsKey(ressourceSO))
        {
            return _ressourcesStored[ressourceSO];
        }

        return 0;
    }

    public bool ContainsAmount(RessourceAndAmount ressourceAndAmount)
    {
        if (ressourceAndAmount.ressourceSO == null || ressourceAndAmount.amount == 0) return false;
        if (_ressourcesStored.ContainsKey(ressourceAndAmount.ressourceSO))
        {
            return ressourceAndAmount.amount <= _ressourcesStored[ressourceAndAmount.ressourceSO];
        }

        return false;
    }
    public bool ContainsAmount(RessourceAndAmount[] ressourcesAndAmount)
    {
        foreach (RessourceAndAmount ressourceAndAmount in ressourcesAndAmount)
        {
            if (!ContainsAmount(ressourceAndAmount)) return false;
        }

        return true;
    }

    public bool Add(RessourceAndAmount ressourceAndAmount, bool verify = true, bool sendEvent = true)
    {
        if (verify && !CanAdd(ressourceAndAmount)) return false;

        RessourceAndAmount add = new(ressourceAndAmount);

        _ressourcesStored[add.ressourceSO] = _ressourcesStored.GetValueOrDefault(add.ressourceSO) + add.amount;

        // Adjust weight and ressource space
        _weight += add.weight;
        _value += add.value;
        _differentRessourceAmount++;

        if (sendEvent) onContentChange?.Invoke();

        return true;
    }
    public bool Add(RessourceAndAmount[] ressourcesAndAmount)
    {
        if (!CanAdd(ressourcesAndAmount)) return false;
        foreach (RessourceAndAmount ressourceAndAmount in ressourcesAndAmount)
        {
            Add(ressourceAndAmount, false, false);
        }
        onContentChange?.Invoke();
        return true;
    }

    public bool CanAdd(RessourceAndAmount ressourceAndAmount)
    {
        if (ressourceAndAmount.ressourceSO == null || ressourceAndAmount.amount <= 0) return false;
        RessourceAndAmount canAdd = new(ressourceAndAmount);

        if (!IsWhiteListed(ressourceAndAmount.ressourceSO)) return false;

        if (WeightLeft() < canAdd.weight)
        {
            // too heavy or not enough space
            Debug.LogWarning("Failed to add item : " + canAdd.ressourceSO.actualName);
            return false;
        }

        if (_ressourcesStored.ContainsKey(canAdd.ressourceSO))
        {
            return _maxSameRessourceSpace >= (_ressourcesStored[canAdd.ressourceSO] + canAdd.amount) * canAdd.spaceTotal;
        }
        else
        {
            // verify if we can add a new sort of item
            if (DifferentRessourceSpaceLeft() <= 0)
            {
                Debug.LogWarning("Failed to add item : " + canAdd.ressourceSO.actualName);
                return false;
            }
            return _maxSameRessourceSpace >= canAdd.spaceTotal;
        }
    }

    public bool CanAdd(RessourceAndAmount[] ressourcesAndAmount)
    {
        foreach (RessourceAndAmount ressourceAndAmount in ressourcesAndAmount)
        {
            if (!CanAdd(ressourceAndAmount)) return false;
        }
        return true;
    }

    public int CanAddHowMany(RessourceSO ressourceSO)
    {
        if (!IsWhiteListed(ressourceSO)) return 0;
        if (DifferentRessourceSpaceLeft() <= 0) return 0;
        int amountAddableBySpace = SameRessourceSpaceLeft(ressourceSO);
        int amountAddableByWeight = WeightLeft() / ressourceSO.weight;

        return Mathf.Min(amountAddableBySpace, amountAddableByWeight);
    }

    public int Remove(RessourceAndAmount ressourceAndAmount, bool sendEvent = true)
    {
        if (ressourceAndAmount.ressourceSO == null || ressourceAndAmount.amount <= 0) return 0;
        RessourceAndAmount removed = new(ressourceAndAmount);

        if (_ressourcesStored.ContainsKey(removed.ressourceSO))
        {
            _ressourcesStored[removed.ressourceSO] -= removed.amount;

            // keep track of the actual number of ressource removed
            if (_ressourcesStored[removed.ressourceSO] <= 0)
            {
                removed.amount += _ressourcesStored[removed.ressourceSO];
                _ressourcesStored.Remove(removed.ressourceSO);
            }

            // Adjust weight and ressource space
            _weight -= removed.weight;
            _value -= removed.value;
            _differentRessourceAmount--;

            if (sendEvent) onContentChange?.Invoke();

            return removed.amount;
        }
        else
        {
            Debug.LogWarning("Could not remove item : " + removed.ressourceSO.actualName + " since not present");
        }
        return 0;
    }

    public void Remove(RessourceAndAmount[] ressourcesAndAmount)
    {
        foreach (RessourceAndAmount ressourceAndAmount in ressourcesAndAmount)
        {
            Remove(ressourceAndAmount, false);
        }
        onContentChange?.Invoke();
    }

    public bool IsWhiteListed(RessourceSO ressourceSO)
    {
        return _ressourcesWhiteListed.Count == 0 || _ressourcesWhiteListed.Contains(ressourceSO);
    }

    public void WhiteList(RessourceSO ressourceSO, bool whiteList = true)
    {
        if (_ressourcesWhiteListed.Contains(ressourceSO))
        {
            if (!whiteList)
            {
                _ressourcesWhiteListed.Remove(ressourceSO);
            }
        }
        else
        {
            if (whiteList)
            {
                _ressourcesWhiteListed.Add(ressourceSO);
            }
        }
        InventoryChanged();
    }
    public void WhiteList(RessourceAndAmount ressourceAndAmount, bool whiteList = true)
    {
        WhiteList(ressourceAndAmount.ressourceSO, whiteList);
    }

    public void WhiteList(RessourceSO[] ressourcesSO, bool whiteList = true)
    {
        foreach (RessourceSO ressourceSO in ressourcesSO)
        {
            WhiteList(ressourceSO, whiteList);
        }
    }

    public void WhiteList(RessourceAndAmount[] ressourcesAndAmount, bool whiteList = true)
    {
        foreach (RessourceAndAmount ressourceAndAmount in ressourcesAndAmount)
        {
            WhiteList(ressourceAndAmount, whiteList);
        }
    }

    public void ClearWhiteList(RessourceSO[] exceptions = null)
    {
        _ressourcesWhiteListed.Clear();
        if (exceptions != null) WhiteList(exceptions);
    }

    public RessourceSO MostRessourceInside()
    {
        RessourceSO mostRessource = null;

        if (_ressourcesStored.Count() == 0) return null;

        foreach (KeyValuePair<RessourceSO, int> ressourceAndAmount in _ressourcesStored)
        {
            if (mostRessource == null)
            {
                mostRessource = ressourceAndAmount.Key;
                continue;
            }
            if (ressourceAndAmount.Value > _ressourcesStored[mostRessource]) mostRessource = ressourceAndAmount.Key;
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

    int SameRessourceSpaceLeft(RessourceSO ressourceSO)
    {
        if (!_ressourcesStored.ContainsKey(ressourceSO)) return _maxSameRessourceSpace;
        return _maxSameRessourceSpace - _ressourcesStored[ressourceSO];
    }

    void InventoryChanged()
    {
        onInventoryChange?.Invoke();
    }

    #endregion
}

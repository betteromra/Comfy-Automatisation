using UnityEngine;
using System.Collections.Generic;

public class Inventory : MonoBehaviour
{
    [SerializeField] int _slot;
    Dictionary<RessourceSO, int> _objectStored;

    #region Unity

    private void Awake() {
        
    }

    #endregion

    #region Managing
    public bool Contains(RessourceSO objectToAdd, int amount)
    {
        if (_objectStored.ContainsKey(objectToAdd))
        {
            if (amount <= _objectStored[objectToAdd])
            {
                return true;
            }
        }

        return false;
    }
    public void Add(RessourceSO objectToAdd, int amount)
    {
        if (_objectStored.ContainsKey(objectToAdd))
        {

        }
        else
        {
            _objectStored.Add(objectToAdd, amount);
        }

    }
    public void Remove(RessourceSO objectToAdd, int amount)
    {
        if (_objectStored.ContainsKey(objectToAdd))
        {
            _objectStored[objectToAdd] -= amount;
            Mathf.Clamp(_objectStored[objectToAdd], 0, int.MaxValue);
        }
    }
    #endregion
}

using UnityEngine;

[System.Serializable]
public class Ingredient
{
    [SerializeField] RessourceSO _ressource;
    public RessourceSO ressource { get => _ressource; }
    [SerializeField] int _amount;
    public int amount { get => _amount; }
}

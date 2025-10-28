using UnityEngine;

[System.Serializable]
public class Ingredient
{
    [SerializeField] RessourceSO _ressource;
    public RessourceSO ressource { get => _ressource; set => _ressource = value; }
    [SerializeField] int _amount = 1;
    public int amount { get => _amount; set => _amount = value; }

}

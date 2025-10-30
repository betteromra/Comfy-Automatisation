using UnityEngine;

[System.Serializable]
public class Ingredient
{
    [SerializeField] RessourceSO _ressourceSO;
    public RessourceSO ressourceSO { get => _ressourceSO; set => _ressourceSO = value; }
    [SerializeField] int _amount = 1;
    public int amount { get => _amount; set => _amount = value; }

}

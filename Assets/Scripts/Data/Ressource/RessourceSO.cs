using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[CreateAssetMenu(fileName = "Ressource", menuName = "Scriptable Objects/Ressource")]
public class RessourceSO : ScriptableObject
{
  // Name to be fetched when needing the name used to display
  // in game, else use name
  [SerializeField] string _actualName;
  public string actualName
  {
    get
    {
      if (_actualName == "") return name;
      else return _actualName;
    }
  }
  [SerializeField, TextArea] string _description;
  public string description { get => _description; }
  [SerializeField] Sprite _icon;
  public Sprite icon { get => _icon; }
  [SerializeField] Sprite _sprite;
  public Sprite sprite { get => _sprite; }
  [SerializeField] RecipeSO _recipe; // what it take to make itself
  public RecipeSO recipe { get => _recipe; }
  [SerializeField] int _spacePerUnit = 1;
  public int spacePerUnit { get => _spacePerUnit; }
  [SerializeField] int _weight;
  public int weight { get => _weight; }
  // the value of the item based on his raw ressource
  [SerializeField] int _rawValue;
  public int rawValue
  {
    get
    {
      // if there is a recipe and it wasn't calculated yet, 
      // calculate the raw value from the children add them together
      if (_recipe != null && _rawValue == 0)
      {
        CalculateValue();
      }

      return _rawValue;
    }
  }
  [SerializeField] int _value;
  public int value
  {
    get
    {
      // if there is a recipe and it wasn't calculated yet, 
      // calculate the value from the children add them together
      if (_recipe != null && _rawValue == 0)
      {
        CalculateValue();
      }
      else
      {
        _value = _rawValue;
      }

      return _value;
    }
  }
  [SerializeField] Quality _quality;
  public Quality quality { get => _quality; }
  // the raw ressource it to take to make this ressource
  RessourceAndAmount[] _rawRessourceToMakeSelf;
  public RessourceAndAmount[] rawRessourceToMakeSelf
  {
    get
    {
      // if there is a recipe and it wasn't calculated yet, 
      // calculate the number of raw ingredient needed to make this
      // ressource
      if (_recipe != null && _rawRessourceToMakeSelf.Length == 0)
      {
        CalculateRawIngredientToMake();
      }
      return _rawRessourceToMakeSelf;
    }
  }

  #region RefreshParameterValue
  void CalculateValue()
  {
    // go in each children calculate their value and retrieve the correct rawValue
    foreach (RessourceAndAmount ingredient in _recipe.ingredientsInput)
    {
      _rawValue += ingredient.ressourceSO.rawValue;
      _value += ingredient.ressourceSO.value + 2;
    }
  }
  void CalculateRawIngredientToMake()
  {
    Dictionary<RessourceSO, int> rawRessourceToMakeSelf = new Dictionary<RessourceSO, int>();

    foreach (RessourceAndAmount ingredient in _recipe.ingredientsInput)
    {
      // if the ingredient have no recipe we hit the bottom of the chain, 
      // we can add this ingredient and the ammount
      if (ingredient.ressourceSO.recipe == null)
      {
        AddToRawRessourceWithNoDouble(ingredient, rawRessourceToMakeSelf);
      }
      // else we calculate the children before and then add his dictonary
      // to ours
      else
      {
        foreach (RessourceAndAmount rawRessourceAndAmount in ingredient.ressourceSO.rawRessourceToMakeSelf)
        {
          AddToRawRessourceWithNoDouble(rawRessourceAndAmount, rawRessourceToMakeSelf);
        }
      }
    }
    RessourceAndAmount[] rawRessourceToMakeSelfArray = rawRessourceToMakeSelf.Select(kvp => new RessourceAndAmount(kvp)).ToArray();

    _rawRessourceToMakeSelf = rawRessourceToMakeSelfArray;
  }

  void AddToRawRessourceWithNoDouble(RessourceAndAmount rawRessourceAndAmount, Dictionary<RessourceSO, int> rawRessourceToMakeSelf)
  {
    if (rawRessourceToMakeSelf.ContainsKey(rawRessourceAndAmount.ressourceSO))
    {
      rawRessourceToMakeSelf[rawRessourceAndAmount.ressourceSO] += rawRessourceAndAmount.amount;
    }
    else rawRessourceToMakeSelf.Add(rawRessourceAndAmount.ressourceSO, rawRessourceAndAmount.amount);
  }
  #endregion
}

public enum Quality
{
  Common,
  Rare,
  Epic,
  Lengedary
}

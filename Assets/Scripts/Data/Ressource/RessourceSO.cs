using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Ressource", menuName = "Scriptable Objects/Ressource")]
public class RessourceSO : ScriptableObject, Makeable
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
  [SerializeField] Sprite _sprite;
  public Sprite sprite { get => _sprite; }
  [SerializeField] RecipeSO _recipe; // what it take to make itself
  public RecipeSO recipe { get => _recipe; }
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
  [SerializeField] Quality _quality;
  public Quality quality { get => _quality; }
  // the raw ressource it to take to make this ressource
  Dictionary<RessourceSO, int> _rawRessourceToMakeSelf;
  public Dictionary<RessourceSO, int> rawRessourceToMakeSelf
  {
    get
    {
      // if there is a recipe and it wasn't calculated yet, 
      // calculate the number of raw ingredient needed to make this
      // ressource
      if (_recipe != null && _rawRessourceToMakeSelf.Count == 0)
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
    foreach (Ingredient ingredient in _recipe.ingredientsInput)
    {
      _rawValue += ingredient.ressource.rawValue;
    }
  }
  Dictionary<RessourceSO, int> CalculateRawIngredientToMake()
  {
    Dictionary<RessourceSO, int> rawRessourceToMakeSelf = new Dictionary<RessourceSO, int>();

    foreach (Ingredient ingredient in _recipe.ingredientsInput)
    {
      // if the ingredient have no recipe we hit the bottom of the chain, 
      // we can add this ingredient and the ammount
      if (ingredient.ressource.recipe == null)
      {
        AddToRawRessourceWithNoDouble(ingredient.ressource, ingredient.amount);
      }
      // else we calculate the children before and then add his dictonary
      // to ours
      else
      {
        foreach (KeyValuePair<RessourceSO, int> rawRessourceAndAmount in ingredient.ressource.rawRessourceToMakeSelf)
        {
          AddToRawRessourceWithNoDouble(rawRessourceAndAmount.Key, rawRessourceAndAmount.Value);
        }
      }
    }

    _rawRessourceToMakeSelf = rawRessourceToMakeSelf;

    return _rawRessourceToMakeSelf;
  }

  void AddToRawRessourceWithNoDouble(RessourceSO ressource, int amount)
  {
    if (_rawRessourceToMakeSelf.ContainsKey(ressource))
    {
      _rawRessourceToMakeSelf[ressource] += amount;
    }
    else _rawRessourceToMakeSelf.Add(ressource, amount);
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

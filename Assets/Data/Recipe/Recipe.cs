using UnityEngine;

[CreateAssetMenu(fileName = "Recipe", menuName = "Scriptable Objects/Recipe")]
public class RecipeSO : ScriptableObject, Makeable
{
  [SerializeField] IngredientSO[] _ingredients;
  public IngredientSO[] ingredients { get => _ingredients; }
}

using UnityEngine;

[CreateAssetMenu(fileName = "Building", menuName = "Scriptable Objects/Building")]
public class BuildingSO : ScriptableObject, Makeable
{
  [SerializeField] RecipeSO _recipe;
  public RecipeSO recipe { get => _recipe; }
  [SerializeField] RecipeSO _craftableRecipes; // the recipe the building can craft
  public RecipeSO craftableRecipes { get => _craftableRecipes; }
}


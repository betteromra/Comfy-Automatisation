using UnityEngine;

[CreateAssetMenu(fileName = "CraftBuilding", menuName = "Scriptable Objects/CraftBuilding")]
public class CraftBuildingSO : BuildingSO
{
  [SerializeField] RecipeSO[] _craftableRecipes; // the recipe the building can craft
  public RecipeSO[] craftableRecipes { get => _craftableRecipes; }
}

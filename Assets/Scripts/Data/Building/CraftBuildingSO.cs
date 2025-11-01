using UnityEngine;

[CreateAssetMenu(fileName = "CraftBuilding", menuName = "Scriptable Objects/CraftBuilding")]
public class CraftBuildingSO : BuildingSO
{
  [SerializeField] RecipeSO[] _craftableRecipes; // the recipe the building can craft
  public RecipeSO[] craftableRecipes { get => _craftableRecipes; }
  [SerializeField] int _inputSpace;
  public int inputSpace { get => _inputSpace; }
  [SerializeField] float _craftingSpeed;
  public float craftingSpeed { get => craftingSpeed; }
}

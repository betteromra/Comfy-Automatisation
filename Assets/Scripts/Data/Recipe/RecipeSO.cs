using UnityEngine;

[CreateAssetMenu(fileName = "Recipe", menuName = "Scriptable Objects/Recipe")]
public class RecipeSO : ScriptableObject
{
  [SerializeField] RessourceAndAmount[] _ingredientsInput;
  public RessourceAndAmount[] ingredientsInput { get => _ingredientsInput; }
  [SerializeField] RessourceAndAmount[] _ingredientsOutput;
  public RessourceAndAmount[] ingredientsOutput { get => _ingredientsOutput; }
  [SerializeField] float _craftingTime = 1;
  public float craftingTime { get => _craftingTime; }
  public bool Make(Inventory inputInventory, Inventory outputInventory = null)
  {
    if (!outputInventory) outputInventory = inputInventory;

    // Verify that the inventory have all the ingredient
    if (!inputInventory.ContainsAmount(_ingredientsInput)) return false;

    // Give the ressource to the output inventory first and see if it works
    if (!outputInventory.Add(_ingredientsOutput)) return false;
    inputInventory.Remove(_ingredientsInput);

    return true;
  }
  // public bool Make(Inventory inputInventory, Vector2 positionToSpawn = null)
  // {
  //   if (!outputInventory) outputInventory = inputInventory;

  //   // Verify that the inventory have all the ingredient
  //   if (!inputInventory.ContainsAmount(_ingredientsInput)) return false;

  //   // Give the ressource to the output inventory first and see if it works
  //   if (!outputInventory.Add(_ingredientsOutput)) return false;
  //   inputInventory.Remove(_ingredientsInput);

  //   return true;
  // }
}

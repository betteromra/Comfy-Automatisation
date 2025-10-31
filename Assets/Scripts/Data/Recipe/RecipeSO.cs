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
    foreach (RessourceAndAmount ingredient in ingredientsInput)
    {
      if (!inputInventory.ContainsAmount(ingredient))
      {
        return false;
      }
    }

    // Use ingredient
    foreach (RessourceAndAmount ingredient in ingredientsInput)
    {
      inputInventory.Remove(ingredient);
    }

    // Give the ressource to the output inventory
    foreach (RessourceAndAmount ingredient in ingredientsOutput)
    {
      outputInventory.Add(ingredient);
    }

    return true;
  }
}

using UnityEngine;

public interface Makeable
{
  public RecipeSO recipe { get; }
  // Crafte the object it is related too using it's recipe
  // If outputInventory is null then it is going to be the same
  // as the input
  bool Make(Inventory inputInventory, Inventory outputInventory = null)
  {
    if (!outputInventory) outputInventory = inputInventory;
    // Verify that the inventory have all the ingredient
    foreach (Ingredient ingredient in recipe.ingredientsInput)
    {
      if (!inputInventory.ContainsAmount(ingredient.ressourceSO, ingredient.amount))
      {
        return false;
      }
    }

    // Use ingredient
    foreach (Ingredient ingredient in recipe.ingredientsInput)
    {
      inputInventory.Remove(ingredient.ressourceSO, ingredient.amount);
    }

    // Give the ressource to the output inventory
    foreach (Ingredient ingredient in recipe.ingredientsOutput)
    {
      outputInventory.Add(ingredient.ressourceSO, ingredient.amount);
    }

    return true;
  }
}

using UnityEngine;

public interface Makeable
{
  public RecipeSO recipe { get; }
  bool Make(Inventory inventory)
  {
    // Verify that the inventory have all the ingredient
    foreach (Ingredient ingredient in recipe.ingredientsInput)
    {
      if (!inventory.Contains(ingredient.ressource, ingredient.amount))
      {
        return false;
      }
    }

    foreach (Ingredient ingredient in recipe.ingredientsInput)
    {
      inventory.Remove(ingredient.ressource, ingredient.amount);
    }

    foreach (Ingredient ingredient in recipe.ingredientsOutput)
    {
      inventory.Add(ingredient.ressource, ingredient.amount);
    }

    return true;
  }
}

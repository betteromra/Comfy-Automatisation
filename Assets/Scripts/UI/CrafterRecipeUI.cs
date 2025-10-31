
using UnityEngine;
using UnityEngine.UI;

public class CrafterRecipeUI : MonoBehaviour
{
    [SerializeField] Button[] _recipesButton;
    [SerializeField] RessourceAndAmountUI[] _recipes;
    [SerializeField] CrafterUI _crafterUI;

    public void Refresh(RecipeSO[] recipesSO)
    {
        int recipesSOLenght = recipesSO.Length;

        if (recipesSOLenght == 0) return;

        for (int i = 0; i < _recipesButton.Length; i++)
        {
            Button recipeButton = _recipesButton[i];

            // if we still have some ressource to show
            if (i < recipesSOLenght)
            {
                RecipeSO recipeSO = recipesSO[i];
                RessourceAndAmountUI recipe = _recipes[i];

                // show all the ingredient for the ressource
                recipeButton.gameObject.SetActive(true);
                recipeButton.onClick.RemoveAllListeners();
                recipeButton.onClick.AddListener(() => _crafterUI.SelectRecipe(recipeSO));
                recipe.DisplayRessourceAndAmount(recipeSO.ingredientsOutput[0]);
            }
            else
            {
                // when we found one that was unactive, we know that the other are
                // also inactive
                if (!recipeButton.gameObject.activeSelf) break;
                recipeButton.gameObject.SetActive(false);
            }
        }
    }
}

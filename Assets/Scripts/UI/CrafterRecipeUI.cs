
using UnityEngine;
using UnityEngine.UI;

public class CrafterRecipeUI : MonoBehaviour
{
    [SerializeField] Button[] _recipesButton;
    [SerializeField] Image[] _recipesImages;
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
                Image recipeImage = _recipesImages[i];

                // show all the ingredient for the ressource
                recipeButton.gameObject.SetActive(true);
                recipeButton.onClick.AddListener(() => _crafterUI.SelectRecipe(recipeSO));
                recipeImage.sprite = recipeSO.ingredientsOutput[0].ressourceSO.icon;
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

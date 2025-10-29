using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DisplayRessourceUI : MonoBehaviour
{
    [SerializeField] RessourceSO _ressource;
    [SerializeField] TextMeshProUGUI _name;
    [SerializeField] TextMeshProUGUI _weight;
    [SerializeField] TextMeshProUGUI _value;
    [SerializeField] GameObject _recipeContainer;
    [SerializeField] IngredientUI[] _ingredientsUI;
    [SerializeField] TextMeshProUGUI _description;
    [SerializeField] Color[] _quality;

    void Refresh()
    {
        _name.text = _ressource.actualName;
        _name.color = _quality[(int)_ressource.quality];
        _weight.text = _ressource.weight + "";
        _value.text = _ressource.rawValue + ""; // to change to actual value in the future

        _description.text = _ressource.description;

        ShowRecipe();
    }

    void ShowRecipe()
    {
        _recipeContainer.SetActive(_ressource.recipe != null);
        if (!_recipeContainer.activeSelf) return;

        for (int i = 0; i < _ingredientsUI.Length; i++)
        {
            IngredientUI ingredientUI = _ingredientsUI[i];

            // if we still have some ressource to show
            if (i < _ressource.recipe.ingredientsInput.Length)
            {
                Ingredient ingredient = _ressource.recipe.ingredientsInput[i];

                // show all the ingredient for the ressource
                ingredientUI.gameObject.SetActive(true);
                ingredientUI.DisplayIngredient(ingredient);
            }
            else
            {
                // when we found one that was unactive, we know that the other are
                // also inactive
                if (!ingredientUI.gameObject.activeSelf) break;
                ingredientUI.gameObject.SetActive(false);
            }
        }
    }

    void ShowRawRecipe()
    {
    }


    void Update()
    {
        Refresh();
    }
}

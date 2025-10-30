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
    [SerializeField] RessourceAndAmountUI[] _ressourcesAndAmountUI;
    [SerializeField] TextMeshProUGUI _description;
    [SerializeField] Color[] _quality;

    public void Refresh()
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

        int recipeLength = _ressource.recipe.ingredientsInput.Length;

        for (int i = 0; i < _ressourcesAndAmountUI.Length; i++)
        {
            RessourceAndAmountUI ressourceAndAmountUI = _ressourcesAndAmountUI[i];

            // if we still have some ressource to show
            if (i < recipeLength)
            {
                Ingredient ingredient = _ressource.recipe.ingredientsInput[i];

                // show all the ingredient for the ressource
                ressourceAndAmountUI.gameObject.SetActive(true);
                ressourceAndAmountUI.DisplayRessourceAndAmount(ingredient.ressourceSO, ingredient.amount);
            }
            else
            {
                // when we found one that was unactive, we know that the other are
                // also inactive
                if (!ressourceAndAmountUI.gameObject.activeSelf) break;
                ressourceAndAmountUI.gameObject.SetActive(false);
            }
        }
    }

    void ShowRawRecipe()
    {
    }
}

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DisplayRessourceUI : MonoBehaviour
{
    [SerializeField] RessourceSO _ressource;
    [SerializeField] TextMeshProUGUI _name;
    [SerializeField] TextMeshProUGUI _weight;
    [SerializeField] TextMeshProUGUI _value;
    [SerializeField] Image[] _ingredientsImage;
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
        for (int i = 0; i < _ingredientsImage.Length; i++)
        {
            Image ingredientImage = _ingredientsImage[i];

            // if we still have some ressource to show
            if (_ressource.recipe != null && i < _ressource.recipe.ingredientsInput.Length)
            {
                Ingredient ingredient = _ressource.recipe.ingredientsInput[i];

                // show all the ingredient for the ressource
                ingredientImage.gameObject.SetActive(true);
                ingredientImage.sprite = ingredient.ressource.icon;
            }
            else
            {
                // when we found one that was unactive, we know that the other are
                // also inactive
                if (!ingredientImage.gameObject.activeSelf) break;
                ingredientImage.gameObject.SetActive(false);
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

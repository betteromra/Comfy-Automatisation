using UnityEngine.UI;
using TMPro;
using UnityEngine;

public class IngredientUI : MonoBehaviour
{
    [SerializeField] Image _image;
    [SerializeField] GameObject _containerAmount;
    [SerializeField] TextMeshProUGUI _amount;

    public void DisplayIngredient(Ingredient ingredient)
    {
        bool hasRessource = ingredient.ressource != null;

        if (hasRessource)
        {
            // display the ressource with the correct ammount
            _image.sprite = ingredient.ressource.icon;

            _containerAmount.SetActive(ingredient.amount != 1);
            _amount.text = ingredient.amount + "";
        }
        else
        {
            _containerAmount.SetActive(false);
        }
        _image.gameObject.SetActive(hasRessource);
    }
}

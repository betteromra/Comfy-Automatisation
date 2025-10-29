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
        _image.sprite = ingredient.ressource.icon;

        _containerAmount.SetActive(ingredient.amount != 1);
        _amount.text = ingredient.amount + "";
    }
}

using UnityEngine.UI;
using TMPro;
using UnityEngine;

public class IngredientUI : MonoBehaviour
{
    [SerializeField] Image _image;
    public Image image { get => _image; }
    [SerializeField] TextMeshProUGUI _amount;

    public void DisplayIngredient(Ingredient ingredient)
    {
        _image.sprite = ingredient.ressource.icon;

        if (ingredient.amount == 1) _amount.text = "";
        else _amount.text = ingredient.amount + "";
    }
}

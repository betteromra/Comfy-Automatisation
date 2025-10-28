using UnityEngine.UI;
using TMPro;
using UnityEngine;

public class IngredientUI : MonoBehaviour
{
    [SerializeField] Image _image;
    public Image image { get => _image; }
    [SerializeField] TextMeshProUGUI _amount;
    public TextMeshProUGUI amount { get => _amount; }
}

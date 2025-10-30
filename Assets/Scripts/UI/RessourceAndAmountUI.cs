using UnityEngine.UI;
using TMPro;
using UnityEngine;

public class RessourceAndAmountUI : MonoBehaviour
{
    [SerializeField] Image _image;
    [SerializeField] GameObject _containerAmount;
    [SerializeField] TextMeshProUGUI _amount;
    int _ghostAmount;
    public int ghostAmount { get => _ghostAmount; set => _ghostAmount = value; }

    public void DisplayRessourceAndAmount(RessourceSO ressourceSO, int amount)
    {
        bool hasRessource = ressourceSO != null;

        if (hasRessource)
        {
            // display the ressource with the correct ammount
            _image.sprite = ressourceSO.icon;

            _containerAmount.SetActive(amount != 1);
            _amount.text = amount + "";
        }
        else
        {
            _containerAmount.SetActive(false);
        }
        gameObject.SetActive(hasRessource);
    }
}

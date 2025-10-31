using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine;

public class RessourceAndAmountUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] Image _image;
    [SerializeField] GameObject _containerAmount;
    [SerializeField] TextMeshProUGUI _amount;
    [SerializeField] bool _doesDisplayRessourceUI = true;
    RessourceAndAmount _ressourceAndAmount;
    bool _isGhost;

    // if negative ammount then ghost the ingredient
    public void DisplayRessourceAndAmount(RessourceAndAmount ressourceAndAmount)
    {
        bool hasRessource = ressourceAndAmount.ressourceSO != null;

        if (hasRessource)
        {
            _isGhost = ressourceAndAmount.amount < 0;
            if (_isGhost)
            {
                _image.color = new Color(.5f, .5f, .5f, .5f);
                ressourceAndAmount.amount = Mathf.Abs(ressourceAndAmount.amount);
            }
            else
            {
                _image.color = new Color(1, 1, 1, 1);
            }
            _ressourceAndAmount = ressourceAndAmount;
            // display the ressource with the correct ammount
            _image.sprite = _ressourceAndAmount.ressourceSO.icon;

            _containerAmount.SetActive(!(_ressourceAndAmount.amount == 1 || _ressourceAndAmount.amount == 0));
            _amount.text = _ressourceAndAmount.amount + "";
        }
        else
        {
            _containerAmount.SetActive(false);
        }
        gameObject.SetActive(hasRessource);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_doesDisplayRessourceUI) GameManager.instance.userInterfaceManager.displayRessourceUI.Display(true, _ressourceAndAmount);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_doesDisplayRessourceUI) GameManager.instance.userInterfaceManager.displayRessourceUI.Display(false, _ressourceAndAmount);
    }
}

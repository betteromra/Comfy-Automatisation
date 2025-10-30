using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class DisplayInventoryUI : MonoBehaviour
{
    [SerializeField] Inventory _inventory;
    public Inventory inventory { get => _inventory; set => _inventory = value; }
    [SerializeField] RessourceAndAmountUI[] _ressourcesAndAmountUI;
    [SerializeField] TextMeshProUGUI _weight;
    [SerializeField] TextMeshProUGUI _value;
    
    private void OnEnable()
    {
        if (_inventory != null)
        {
            _inventory.onInventoryChange += Refresh;
        }
    }
    private void OnDisable()
    {
        if (_inventory != null)
        {
            _inventory.onInventoryChange -= Refresh;
        }
    }
    public void Refresh()
    {
        List<KeyValuePair<RessourceSO, int>> inventoryContent = _inventory.GetAllContent();

        int inventoryContentLength = inventoryContent.Count;

        for (int i = 0; i < _ressourcesAndAmountUI.Length; i++)
        {
            RessourceAndAmountUI ressourceAndAmountUI = _ressourcesAndAmountUI[i];

            if (i < inventoryContentLength)
            {
                KeyValuePair<RessourceSO, int> ressourcesAndAmountInvetory = inventoryContent[i];
                // show all the ingredient for the ressource
                ressourceAndAmountUI.gameObject.SetActive(true);
                ressourceAndAmountUI.DisplayRessourceAndAmount(ressourcesAndAmountInvetory.Key, ressourcesAndAmountInvetory.Value);
            }
            else
            {
                // when we found one that was unactive, we know that the other are
                // also inactive
                if (!ressourceAndAmountUI.gameObject.activeSelf) break;
                ressourceAndAmountUI.gameObject.SetActive(false);
            }
        }

        if (_weight != null) _weight.text = inventory.weight + "";
        if (_value != null) _value.text = inventory.value + "";
    }
}
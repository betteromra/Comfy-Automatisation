using UnityEngine;
using System.Collections.Generic;
using TMPro;
using System.Linq;

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
        else
        {
            Debug.LogError("InventoryUI not linked to Inventory");
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
        List<RessourceSO> whiteListed = _inventory.ressourcesWhiteListed;
        RessourceAndAmount[] inventoryContent = _inventory.ressourcesStored.Select(kvp => new RessourceAndAmount(kvp)).ToArray();

        int whiteListedLength = whiteListed.Count;
        int inventoryContentLength = inventoryContent.Length;

        for (int i = 0; i < _ressourcesAndAmountUI.Length; i++)
        {
            RessourceAndAmountUI ressourceAndAmountUI = _ressourcesAndAmountUI[i];

            if (i < inventoryContentLength)
            {
                RessourceAndAmount ressourceAndAmountInvetory = inventoryContent[i];

                // Is the inventory white listed
                if (whiteListedLength == 0)
                {
                    ressourceAndAmountUI.DisplayRessourceAndAmount(ressourceAndAmountInvetory);
                }
                else
                {
                    // if it is then we show the element only if it is white listed
                    if (whiteListed.Contains(ressourceAndAmountInvetory.ressourceSO))
                    {
                        ressourceAndAmountUI.DisplayRessourceAndAmount(ressourceAndAmountInvetory);
                        whiteListed.Remove(ressourceAndAmountInvetory.ressourceSO);
                    }
                }

                // show all the ingredient for the ressource
                ressourceAndAmountUI.gameObject.SetActive(true);
            }
            else if (i < whiteListedLength)
            {
                RessourceSO whiteListedRessource = whiteListed[i - inventoryContentLength];
                // if the white listed is not contain, the we ghost it
                ressourceAndAmountUI.DisplayRessourceAndAmount(new RessourceAndAmount(whiteListedRessource, -1));
                ressourceAndAmountUI.gameObject.SetActive(true);
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
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    [SerializeField] float percentageValueSell;
    int goldStored = 0;
    public void Sell(RessourceAndAmount ressourceAndAmount)
    {

    }
    public int CanBuyHowMany(RessourceSO ressourceSO)
    {
        return Mathf.FloorToInt(goldStored / (ressourceSO.value * percentageValueSell));
    }
    public bool Buy(RessourceAndAmount ressourceAndAmount)
    {
        if (goldStored - ressourceAndAmount.value >= 0)
        {
            goldStored -= ressourceAndAmount.value;
            return true;
        }
        return false;
    }
}

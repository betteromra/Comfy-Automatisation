using UnityEngine;

public class ShopManager : MonoBehaviour
{
    [SerializeField] float percentageValueSell;
    int goldStored = 0;
    public void Sell(RessourceAndAmount ressourceAndAmount)
    {
        goldStored += Mathf.FloorToInt(ressourceAndAmount.value * percentageValueSell);
    }
    public int CanBuyHowMany(RessourceSO ressourceSO)
    {
        if (ressourceSO == null) return 0;
        return Mathf.FloorToInt(goldStored / ressourceSO.value);
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

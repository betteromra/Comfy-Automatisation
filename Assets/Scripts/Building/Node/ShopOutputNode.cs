using UnityEngine;

public class ShopOutputNode : OutputNode
{
    public override int Output(RessourceAndAmount ressourceAndAmountOutput)
    {
        int howManyCanBuy = GameManager.instance.shopManager.CanBuyHowMany(ressourceAndAmountOutput.ressourceSO);
        int howManyBuy = base.Output(new RessourceAndAmount(ressourceAndAmountOutput.ressourceSO, howManyCanBuy));

        GameManager.instance.shopManager.Buy(new RessourceAndAmount(ressourceAndAmountOutput.ressourceSO, howManyBuy));

        return howManyBuy;
    }
}

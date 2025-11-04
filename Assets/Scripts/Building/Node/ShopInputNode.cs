using UnityEngine;

public class ShopInputNode : InputNode
{
    public override int Input(RessourceAndAmount ressourceAndAmountOutput, bool verify = true)
    {
        int howManySold = base.Input(ressourceAndAmountOutput, verify);

        GameManager.instance.shopManager.Sell(new RessourceAndAmount(ressourceAndAmountOutput.ressourceSO, howManySold));

        return howManySold;
    }
}

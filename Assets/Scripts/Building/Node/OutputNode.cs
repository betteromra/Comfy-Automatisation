using UnityEngine;

public class OutputNode : BuildingNode
{
  bool CanOutput(RessourceAndAmount ressourceAndAmountOutput)
  {
    return _inventory.ContainsAmount(ressourceAndAmountOutput);
  }

  public bool Output(RessourceAndAmount ressourceAndAmountOutput)
  {
    if (!CanOutput(ressourceAndAmountOutput)) return false;

    _inventory.Remove(ressourceAndAmountOutput);

    return true;
  }
}

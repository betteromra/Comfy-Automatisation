using UnityEngine;

public class InputNode : BuildingNode
{
  bool CanInput(RessourceAndAmount ressourceAndAmountOutput)
  {
    return _inventory.CanAdd(ressourceAndAmountOutput);
  }

  public bool Input(RessourceAndAmount ressourceAndAmountOutput)
  {
    if (!CanInput(ressourceAndAmountOutput)) return false;

    return _inventory.Add(ressourceAndAmountOutput);
  }
}

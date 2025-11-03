using UnityEngine;

public class OutputNode : BuildingNode
{

  bool CanOutput(RessourceAndAmount ressourceAndAmountOutput)
  {
    return _inventory.ContainsAmount(ressourceAndAmountOutput);
  }
  
  public RessourceAndAmount RessourceAccesibleFromList(RessourceAndAmount[] priorityArray)
  {
    if (priorityArray == null) return null;

    foreach (RessourceAndAmount ressourceAndAmount in priorityArray)
    {
      if (_inventory.ContainsAmount(ressourceAndAmount))
      {
        return ressourceAndAmount;
      }
    }

    return null;
  }

  public bool Output(RessourceAndAmount ressourceAndAmountOutput, bool verify = true)
  {
    if (verify && !CanOutput(ressourceAndAmountOutput)) return false;

    _inventory.Remove(ressourceAndAmountOutput);

    return true;
  }
}

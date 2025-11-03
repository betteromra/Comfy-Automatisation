using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class InputNode : BuildingNode
{
  RecipeSO _recipeSO;
  public RecipeSO recipeSO { get => _recipeSO; set => _recipeSO = value; }
  bool CanInput(RessourceAndAmount ressourceAndAmountOutput)
  {
    return _inventory.CanAdd(ressourceAndAmountOutput);
  }

  // highest priority is going to be at the top of this list
  public RessourceAndAmount[] PriorityNeeds()
  {
    if (_recipeSO == null) return new RessourceAndAmount[0];

    RessourceAndAmount[] ressourcesNeeded = _recipeSO.ingredientsInput;
    Dictionary<RessourceSO, float> _percentageRessourceComplete = new Dictionary<RessourceSO, float>();

    foreach (RessourceAndAmount ressourceNeeded in ressourcesNeeded)
    {
      int amountContained = _inventory.Contains(ressourceNeeded.ressourceSO);
      float clampedPercentage = Mathf.Clamp(amountContained / ressourceNeeded.amount, 0, 1);
      _percentageRessourceComplete.Add(ressourceNeeded.ressourceSO, clampedPercentage);
    }

    return ressourcesNeeded.OrderBy(ra => _percentageRessourceComplete[ra.ressourceSO]).ToArray();
  }

  public bool Input(RessourceAndAmount ressourceAndAmountOutput, bool verify = true)
  {
    if (verify && !CanInput(ressourceAndAmountOutput)) return false;

    return _inventory.Add(ressourceAndAmountOutput);
  }
}

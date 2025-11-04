using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class InputNode : BuildingNode
{
  protected Dictionary<OutputNode, int> _linkedPath = new Dictionary<OutputNode, int>();
  RecipeSO _recipeSO;
  public RecipeSO recipeSO { get => _recipeSO; set => _recipeSO = value; }
  int HowMuchCanInput(RessourceSO ressourceSO)
  {
    return _inventory.CanAddHowMany(ressourceSO);
  }

  bool CanLink(OutputNode outputNode)
  {
    return _linkedPath.Count < _maxPath;
  }

  public bool Link(OutputNode outputNode)
  {
    if (!CanLink(outputNode)) return false;
    _linkedPath[outputNode] = _linkedPath.GetValueOrDefault(outputNode) + 1;

    return true;
  }

  public void Unlink(OutputNode outputNode)
  {
    if (_linkedPath.ContainsKey(outputNode))
    {
      _linkedPath[outputNode]--;
      if (_linkedPath[outputNode] == 0) _linkedPath.Remove(outputNode);
    }
  }

  bool CanInput(RessourceSO ressourceSO)
  {
    return _inventory.CanAdd(new RessourceAndAmount(ressourceSO, 1));
  }

  // highest priority is going to be at the top of this list
  public RessourceAndAmount[] PriorityNeeds()
  {
    if (_recipeSO == null) return new RessourceAndAmount[0];

    RessourceAndAmount[] ressourcesNeeded = _recipeSO.ingredientsInput;
    Dictionary<RessourceSO, float> _percentageRessourceComplete = new Dictionary<RessourceSO, float>();

    // Check percentage of ressource needed to craft
    foreach (RessourceAndAmount ressourceNeeded in ressourcesNeeded)
    {
      int amountContained = _inventory.ContainsHowMany(ressourceNeeded.ressourceSO);
      float clampedPercentage = Mathf.Clamp(amountContained / ressourceNeeded.amount, 0, 1);
      _percentageRessourceComplete.Add(ressourceNeeded.ressourceSO, clampedPercentage);
    }

    // order list by highest to lowest priority
    return ressourcesNeeded.OrderBy(ra => _percentageRessourceComplete[ra.ressourceSO]).ToArray();
  }

  public virtual int Input(RessourceAndAmount ressourceAndAmountOutput, bool verify = true)
  {
    if (verify && !CanInput(ressourceAndAmountOutput.ressourceSO)) return 0;
    int howManyAdded = Mathf.Min(HowMuchCanInput(ressourceAndAmountOutput.ressourceSO), ressourceAndAmountOutput.amount);

    _inventory.Add(new RessourceAndAmount(ressourceAndAmountOutput.ressourceSO, howManyAdded));

    return howManyAdded;
  }
}

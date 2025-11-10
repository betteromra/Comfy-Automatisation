using System.Collections.Generic;
using UnityEngine;

public class OutputNode : BuildingNode
{
  public int HowMuchCanOutput(RessourceSO ressourceSO)
  {
    return _inventory.ContainsHowMany(ressourceSO);
  }

  public RessourceAndAmount[] RessourceAccesibleFromList(InputNode nodeThatWeWantToDrop)
  {
    RessourceAndAmount[] priorityArray = nodeThatWeWantToDrop.PriorityNeeds();

    if (priorityArray.Length == 0)
    {
      RessourceSO mostRessource = _inventory.MostRessourceInside();
      if (mostRessource == null) return new RessourceAndAmount[0];
      return new RessourceAndAmount[] { new RessourceAndAmount(mostRessource, _inventory.ContainsHowMany(mostRessource)) };
    }

    List<RessourceAndAmount> priorityArrayWithWhatAvailable = new List<RessourceAndAmount>();
    foreach (RessourceAndAmount ressourceAndAmount in priorityArray)
    {
      if (_inventory.ContainsAmount(ressourceAndAmount))
      {
        priorityArrayWithWhatAvailable.Add(ressourceAndAmount);
      }
    }

    return priorityArrayWithWhatAvailable.ToArray();
  }

  public virtual int Output(RessourceAndAmount ressourceAndAmountOutput)
  {
    int howManyRemoved = Mathf.Min(HowMuchCanOutput(ressourceAndAmountOutput.ressourceSO), ressourceAndAmountOutput.amount);

    _inventory.Remove(ressourceAndAmountOutput);

    return howManyRemoved;
  }
}

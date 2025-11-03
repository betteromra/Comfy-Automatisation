using System.Collections.Generic;
using UnityEngine;

public class OutputNode : BuildingNode
{
  protected Dictionary<InputNode, int> _linkedPath = new Dictionary<InputNode, int>();
  public int HowMuchCanOutput(RessourceSO ressourceSO)
  {
    return _inventory.ContainsHowMany(ressourceSO);
  }

  bool CanLink(InputNode inputNode)
  {
    return _linkedPath.Count < _maxPath;
  }

  public bool Link(InputNode inputNode)
  {
    if (!CanLink(inputNode)) return false;
    _linkedPath[inputNode] = _linkedPath.GetValueOrDefault(inputNode) + 1;

    return true;
  }
  public void Unlink(InputNode inputNode)
  {
    if (_linkedPath.ContainsKey(inputNode))
    {
      _linkedPath[inputNode]--;
      if (_linkedPath[inputNode] == 0) _linkedPath.Remove(inputNode);
    }
  }
  public int PeopleOnPath(InputNode inputNode)
  {
    if (_linkedPath.ContainsKey(inputNode))
    {
      return _linkedPath[inputNode];
    }

    return 0;
  }

  public RessourceAndAmount[] RessourceAccesibleFromList(InputNode previousNode)
  {
    RessourceAndAmount[] priorityArray = previousNode.PriorityNeeds();

    if (priorityArray.Length == 0)
    {
      RessourceSO mostRessource = _inventory.MostRessourceInside();
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

  public int Output(RessourceAndAmount ressourceAndAmountOutput)
  {
    int howManyRemoved = Mathf.Min(HowMuchCanOutput(ressourceAndAmountOutput.ressourceSO), ressourceAndAmountOutput.amount);

    _inventory.Remove(ressourceAndAmountOutput);

    return howManyRemoved;
  }
}

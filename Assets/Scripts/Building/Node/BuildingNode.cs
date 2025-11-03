using System.Collections.Generic;
using UnityEngine;

public class BuildingNode : MonoBehaviour
{
  [SerializeField] protected Inventory _inventory;
  public Inventory inventory { get => _inventory; }
  [SerializeField] protected int _maxPath;
}

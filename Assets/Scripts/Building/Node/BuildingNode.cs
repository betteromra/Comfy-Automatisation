using UnityEngine;

public class BuildingNode : MonoBehaviour
{
  [SerializeField] Inventory _inventory;
  public Inventory inventory { get => _inventory; }
  RessourceSO _ressourceSO;
  public RessourceSO ressourceSO { get => _ressourceSO; set => _ressourceSO = value; }
}

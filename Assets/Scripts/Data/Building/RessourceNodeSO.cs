using UnityEngine;

[CreateAssetMenu(fileName = "RessourceNode", menuName = "Scriptable Objects/RessourceNode")]
public class RessourceNodeSO : BuildingSO
{
  [SerializeField] RessourceAndAmount _ressourceAndAmount;
  public RessourceAndAmount ressourceAndAmount { get => _ressourceAndAmount; }
  [SerializeField] float _ressourcePerMinute;
  public float ressourcePerMinute { get => _ressourcePerMinute; }
}

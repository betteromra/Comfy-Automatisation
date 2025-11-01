using UnityEngine;

[CreateAssetMenu(fileName = "RessourceNode", menuName = "Scriptable Objects/RessourceNode")]
public class RessourceNodeSO : BuildingSO
{
  [SerializeField] RessourceAndAmount _ressourceAndAmount;
  public RessourceAndAmount ressourceAndAmount { get => _ressourceAndAmount; }
  [SerializeField] float _extractionPerMinute;
  public float extractionPerMinute { get => _extractionPerMinute; }
}

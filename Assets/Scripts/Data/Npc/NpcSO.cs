using UnityEngine;

[CreateAssetMenu(fileName = "Npc", menuName = "Scriptable Objects/Npc")]
public class NpcSO : ScriptableObject, Makeable
{
  [SerializeField] RecipeSO _recipe;
  public RecipeSO recipe { get => _recipe; }
}

using UnityEngine;

[CreateAssetMenu(fileName = "Npc", menuName = "Scriptable Objects/Npc")]
public class Npc : ScriptableObject, Makeable
{
  [SerializeField] Ingredient[] _ingredients;
  public Ingredient[] ingredients { get => _ingredients; }
}

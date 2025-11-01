using UnityEngine;

[CreateAssetMenu(fileName = "Npc", menuName = "Scriptable Objects/Npc")]
public class NpcSO : ScriptableObject
{
  [SerializeField] GameObject _prefab;
  public GameObject prefab { get => _prefab; }
}

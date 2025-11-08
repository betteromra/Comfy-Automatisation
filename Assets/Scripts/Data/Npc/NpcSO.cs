using UnityEngine;

[CreateAssetMenu(fileName = "Npc", menuName = "Scriptable Objects/Npc")]
public class NpcSO : ScriptableObject
{
  [SerializeField] GameObject _prefab;
  public GameObject prefab { get => _prefab; }
  [SerializeField] float _speed;
  public float Speed { get => _speed; }
  [SerializeField] RessourceSO _ressourceSO;
  public RessourceSO ressourceSO { get => _ressourceSO; }
  [SerializeField] int _maxSameRessourceSpace;
  public int maxSameRessourceSpace { get => _maxSameRessourceSpace; }
  [SerializeField] int _maxDifferentRessource;
  public int maxDifferentRessource { get => _maxDifferentRessource; }
  [Tooltip("Time NPC waits after dropping reaching target.")]
  [SerializeField] float _waitDuration;
  public float WaitDuration { get => _waitDuration; }
}

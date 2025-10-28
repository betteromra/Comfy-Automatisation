using UnityEngine;

[CreateAssetMenu(fileName = "RessourceNode", menuName = "Scriptable Objects/RessourceNode")]
public class RessourceNode : ScriptableObject
{
  [SerializeField] Sprite _sprite;
  public Sprite sprite { get => _sprite; }
  [SerializeField] RessourceSO _ressource;
  public RessourceSO ressource { get => _ressource; }
  [SerializeField] float _ressourcePerMinute;
  public float ressourcePerMinute { get => _ressourcePerMinute; }
}

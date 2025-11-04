using UnityEngine;

[CreateAssetMenu(fileName = "Building", menuName = "Scriptable Objects/Building")]
public class BuildingSO : ScriptableObject
{
  [SerializeField] GameObject _prefab;
  public GameObject prefab { get => _prefab; }
  [SerializeField] RessourceSO _ressourceSO;
  public RessourceSO ressourceSO { get => _ressourceSO; }
}


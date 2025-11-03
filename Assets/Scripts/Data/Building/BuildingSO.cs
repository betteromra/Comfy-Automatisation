using UnityEngine;

[CreateAssetMenu(fileName = "Building", menuName = "Scriptable Objects/Building")]
public class BuildingSO : ScriptableObject
{
  // Name to be fetched when needing the name used to display
  // in game, else use name
  [SerializeField] string _actualName;
  public string actualName
  {
    get
    {
      if (_actualName == "") return name;
      else return _actualName;
    }
  }
  [SerializeField] Sprite _icon;
  public Sprite icon { get => _icon; }
  [SerializeField] RecipeSO _recipe; // what it take to make itself
  public RecipeSO recipe { get => _recipe; }
  [SerializeField] int _npcSpace;
  public int npcSpace { get => _npcSpace; }

  [SerializeField] GameObject _prefab;
  public GameObject prefab { get => _prefab; }
  [SerializeField] RessourceSO _ressourceSO;
  public RessourceSO ressourceSO { get => _ressourceSO; }
}


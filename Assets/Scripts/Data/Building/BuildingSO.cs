using UnityEngine;

[CreateAssetMenu(fileName = "Building", menuName = "Scriptable Objects/Building")]
public class BuildingSO : ScriptableObject, Makeable
{
  [SerializeField] Sprite _icon;
  public Sprite icon { get => _icon; }
  [SerializeField] RecipeSO _recipe; // what it take to make itself
  public RecipeSO recipe { get => _recipe; }
}


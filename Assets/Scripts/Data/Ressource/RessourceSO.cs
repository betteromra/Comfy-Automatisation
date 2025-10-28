using UnityEngine;

[CreateAssetMenu(fileName = "Ressource", menuName = "Scriptable Objects/Ressource")]
public class RessourceSO : ScriptableObject, Makeable
{
  [SerializeField] RecipeSO _recipe;
  public RecipeSO recipe { get => _recipe; }
}

using UnityEngine;

[CreateAssetMenu(fileName = "Building", menuName = "Scriptable Objects/Building")]
public class BuildingSO : ScriptableObject, Makeable
{
  [SerializeField] IngredientSO[] _ingredients;
  public IngredientSO[] ingredients { get => _ingredients; }
}


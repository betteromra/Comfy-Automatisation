using UnityEngine;

[CreateAssetMenu(fileName = "Recipe", menuName = "Scriptable Objects/Recipe")]
public class RecipeSO : ScriptableObject
{
  [SerializeField] Ingredient[] _ingredientsInput;
  public Ingredient[] ingredientsInput { get => _ingredientsInput; }
  [SerializeField] Ingredient[] _ingredientsOutput;
  public Ingredient[] ingredientsOutput { get => _ingredientsOutput; }
}

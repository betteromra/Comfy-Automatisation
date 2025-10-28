using UnityEngine;

public abstract class Building : MonoBehaviour
{
    [SerializeField] protected BuildingSO _buildingSO;
    [SerializeField] protected MeshRenderer _meshRenderer;
    [SerializeField] protected BoxCollider _collider;
    [SerializeField] protected InputNode[] _inputNode;
    [SerializeField] protected OutputNode[] _outputNode;
    [SerializeField] protected Ingredient _ingredientToDisplay;
    [SerializeField] protected IngredientUI _ingredientUI;
    protected virtual void Awake()
    {

    }
}

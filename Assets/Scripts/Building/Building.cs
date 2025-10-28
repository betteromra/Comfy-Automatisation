using UnityEngine;

public class Building : MonoBehaviour
{
    [SerializeField] protected BuildingSO _buildingSO;
    [SerializeField] protected MeshRenderer _meshRenderer;
    [SerializeField] protected BoxCollider _collider;
    [SerializeField] protected InputNode[] _inputNode;
    [SerializeField] protected OutputNode[] _outputNode;
    [SerializeField] protected IngredientUI _ingredientUI;
}

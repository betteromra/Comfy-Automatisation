using UnityEngine;

public abstract class Building : MonoBehaviour
{
    [SerializeField] protected BuildingSO _buildingSO;
    public BuildingSO buildingSO { get => _buildingSO; set => _buildingSO = value; }
    [SerializeField] protected MeshRenderer _meshRenderer;
    [SerializeField] protected BoxCollider _collider;
    [SerializeField] protected InputNode[] _inputNode;
    [SerializeField] protected OutputNode[] _outputNode;
    [SerializeField] protected BuildingUI _buildingUI;
    [SerializeField] protected RessourceAndAmountToDisplayUI _ressourceAndAmountToDisplayUI;
    public RessourceAndAmountToDisplayUI ressourceAndAmountToDisplayUI { get => _ressourceAndAmountToDisplayUI; }
    protected virtual void Awake()
    {

    }

    protected void UpdateIngredientToDisplay(RessourceSO ressourceSO, int amount)
    {
        if (_ressourceAndAmountToDisplayUI != null)
        {
            _ressourceAndAmountToDisplayUI.ingredientUI.DisplayRessourceAndAmount(ressourceSO, amount);
        }
    }
}

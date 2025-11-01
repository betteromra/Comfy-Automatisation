using UnityEngine;

public abstract class Building : MonoBehaviour
{
    protected BuildingSO _buildingSO;
    public BuildingSO buildingSO { get => _buildingSO; }
    [SerializeField] protected bool _destructable = true;
    public bool destructable { get => _destructable; }
    [SerializeField] protected MeshRenderer _meshRenderer;
    [SerializeField] protected BoxCollider _boxCollider;
    public BoxCollider boxCollider { get => _boxCollider; }
    [SerializeField] protected InputNode[] _inputNode;
    [SerializeField] protected OutputNode[] _outputNode;
    [SerializeField] protected BuildingUI _buildingUI;
    [SerializeField] protected RessourceAndAmountToDisplayUI _ressourceAndAmountToDisplayUI;
    public RessourceAndAmountToDisplayUI ressourceAndAmountToDisplayUI { get => _ressourceAndAmountToDisplayUI; }
    protected virtual void Awake()
    {
    }

    protected void UpdateIngredientToDisplay(RessourceAndAmount ressourceAndAmount)
    {
        if (_ressourceAndAmountToDisplayUI != null)
        {
            _ressourceAndAmountToDisplayUI.ingredientUI.DisplayRessourceAndAmount(ressourceAndAmount);
        }
    }

    protected virtual void OnEnable()
    {
    }

    protected virtual void OnDisable()
    {
        _buildingUI.gameObject.SetActive(false);
        _ressourceAndAmountToDisplayUI.gameObject.SetActive(false);
        _boxCollider.gameObject.SetActive(false);
    }
}

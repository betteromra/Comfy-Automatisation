using UnityEngine;

public abstract class Building : MonoBehaviour
{
    protected BuildingSO _buildingSO;
    public BuildingSO buildingSO { get => _buildingSO; }
    [SerializeField] protected bool _destructable = true;
    public bool destructable { get => _destructable; }
    [SerializeField] protected MeshRenderer _meshRenderer;
    [SerializeField] protected BoxCollider _collider;
    [SerializeField] protected InputNode[] _inputNode;
    [SerializeField] protected OutputNode[] _outputNode;
    [SerializeField] protected BuildingUI _buildingUI;
    [SerializeField] protected RessourceAndAmountToDisplayUI _ressourceAndAmountToDisplayUI;
    public RessourceAndAmountToDisplayUI ressourceAndAmountToDisplayUI { get => _ressourceAndAmountToDisplayUI; }
    Selectable _selectable;
    protected virtual void Awake()
    {
        _selectable = GetComponent<Selectable>();
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
    }
}

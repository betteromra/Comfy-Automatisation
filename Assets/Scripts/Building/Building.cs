using UnityEngine;

public abstract class Building : MonoBehaviour
{
    protected BuildingSO _buildingSO;
    public BuildingSO buildingSO { get => _buildingSO; }
    [SerializeField] protected bool _destructable = true;
    public bool destructable { get => _destructable; }
    [SerializeField] protected MeshRenderer _meshRenderer;
    public MeshRenderer meshRenderer { get => _meshRenderer; }
    [SerializeField] protected BoxCollider _boxCollider;
    public BoxCollider boxCollider { get => _boxCollider; }
    [SerializeField] protected Light[] _lights;
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
        if (_ressourceAndAmountToDisplayUI != null) _ressourceAndAmountToDisplayUI.transform.parent.rotation = Quaternion.Euler(60, 315, 0);
    }

    protected virtual void OnDisable()
    {
        if (_buildingUI != null) _buildingUI.gameObject.SetActive(false);
        if (_ressourceAndAmountToDisplayUI != null) _ressourceAndAmountToDisplayUI.gameObject.SetActive(false);
        _boxCollider.gameObject.SetActive(false);
        foreach (Light light in _lights)
        {
            light.gameObject.SetActive(false);
        }
    }
}

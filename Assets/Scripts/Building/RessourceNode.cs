using Unity.VisualScripting;
using UnityEngine;

public class RessourceNode : Building
{
    [SerializeField] Inventory _inventory;
    RessourceNodeSO _ressourceNodeSO;
    override protected void Awake()
    {
        base.Awake();
        _ressourceNodeSO = _buildingSO as RessourceNodeSO;
        _ingredientToDisplay.ressource = _ressourceNodeSO.ressource;
    }
}

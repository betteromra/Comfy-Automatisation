using Unity.VisualScripting;
using UnityEngine;

public class RessourceNode : Building
{
    RessourceNodeSO _ressourceNodeSO;
    void Awake()
    {
        _ressourceNodeSO = _buildingSO.GetComponent<RessourceNodeSO>();
    }
}

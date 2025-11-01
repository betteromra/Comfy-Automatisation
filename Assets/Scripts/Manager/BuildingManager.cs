using System;
using UnityEngine;
using System.Collections.Generic;

public class BuildingManager : MonoBehaviour
{
    [SerializeField] RessourceNode[] _ressourcesNode;
    public RessourceNode[] ressourcesNode { get => _ressourcesNode; set => _ressourcesNode = value; }
    [SerializeField] StorageBuilding _barn;
    public StorageBuilding barn { get => _barn; set => _barn = value; }
    List<Building> _buildings = new List<Building>();
    public List<Building> buildings { get => _buildings; }
    BuildingSO _buildingSOToolBarSelected;
    public BuildingSO buildingSOToolBarSelected
    {
        get => _buildingSOToolBarSelected; 
        set {
            _buildingSOToolBarSelected = value;
            OnBuildingSelected();
        }
    }

    [SerializeField] public LayerMask BlockingLayers;
    [SerializeField] GameObject BuildingsParent;

    void Awake()
    {
        foreach (RessourceNode ressourceNode in _ressourcesNode)
        {
            _buildings.Add(ressourceNode);
        }
        _buildings.Add(_barn);
    }

    // // These two functions just enable us to preview our buildings.
    // void OnPreviewKeep()
    // {
    //     Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
    //     RaycastHit RayHit;
    //     if (Physics.Raycast(ray, out RayHit, 1000f, ~BlockingLayers))
    //     {
    //         GameObject targetHit = RayHit.transform.gameObject;
    //         Vector3 hitPos = RayHit.point;

    //         if (temp != null)
    //             Destroy(temp);
    //         temp = Instantiate(Keep, hitPos, Quaternion.identity);

    //     }
    // }

    // void OnPreviewRefinery()
    // {
    //     Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
    //     RaycastHit RayHit;
    //     if (Physics.Raycast(ray, out RayHit, 1000f, ~BlockingLayers))
    //     {
    //         GameObject targetHit = RayHit.transform.gameObject;
    //         Vector3 hitPos = RayHit.point;

    //         if (temp != null)
    //             Destroy(temp);
    //         temp = Instantiate(Refinery, hitPos, Quaternion.identity);
    //     }
    // }


    //This continues to show us our preview, so long as our temp variable contains something.
    private void Update()
    {
        // if (temp != null)
        // {
        //     Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        //     RaycastHit RayHit;
        //     if (Physics.Raycast(ray, out RayHit, 1000f, ~BlockingLayers))
        //     {
        //         GameObject targetHit = RayHit.transform.gameObject;
        //         Vector3 hitPos = RayHit.point;
        //         temp.transform.position = hitPos;
        //     }
        // }
    }


    //This places our building, but first has to check if temp exists. It reparents temp, then sets it to null, essentially detatching our prefab to the scene.
    private void OnBuildingSelected()
    {
        //     if (temp != null && CheckPlacementCollision(temp.GetComponentInChildren<BoxCollider>()))
        //     {
        //         temp.transform.SetParent(BuildingParent.transform);
        //         temp = null;
        //     }
    }

    //This is a kinda shitty way to do this. It's just +2 in each direction at this point. May change to boxcast. 2 Collisions are expected, itself and the floor.
    private bool CheckPlacementCollision(BoxCollider col)
    {
        Collider[] collisions = Physics.OverlapBox(col.transform.position, new Vector3((col.size.x / 2) + 2, col.size.y / 2, (col.size.z / 2) + 2), Quaternion.identity);

        if (collisions.Length > 2)
            return false;
        return true;

    }

    void OnEnable()
    {
        // GameManager.instance.player.onPressedBuild += function here;
        // GameManager.instance.player.onPressedCancelBuild += function here;
    }

    void OnDisable()
    {
        // GameManager.instance.player.onPressedBuild -= function here;
        // GameManager.instance.player.onPressedCancelBuild -= function here;
    }
}

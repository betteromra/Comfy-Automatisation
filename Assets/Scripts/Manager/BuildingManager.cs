using System;
using UnityEngine;

public class BuildingManager : MonoBehaviour
{
    [SerializeField] Building[] _buildings;
    public Building[] buildings { get => _buildings; }
    [SerializeField] BuildingSO _buildingSOToolBarSelected;
    public BuildingSO buildingSOToolBarSelected { get => _buildingSOToolBarSelected; set => _buildingSOToolBarSelected = value; }

    public LayerMask BlockingLayers;
    public GameObject Keep;
    public GameObject Refinery;
    public GameObject BuildingParent;

    private GameObject temp;


    public event Action onPlaceKeep;


    private void Start()
    {
        temp = null;
    }

    // These two functions just enable us to preview our buildings.
    void OnPreviewKeep()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit RayHit;
        if (Physics.Raycast(ray, out RayHit, 1000f, ~BlockingLayers))
        {
            GameObject targetHit = RayHit.transform.gameObject;
            Vector3 hitPos = RayHit.point;

            if (temp != null)
                Destroy(temp);
            temp = Instantiate(Keep, hitPos, Quaternion.identity);

        }
    }

    void OnPreviewRefinery()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit RayHit;
        if (Physics.Raycast(ray, out RayHit, 1000f, ~BlockingLayers))
        {
            GameObject targetHit = RayHit.transform.gameObject;
            Vector3 hitPos = RayHit.point;

            if (temp != null)
                Destroy(temp);
            temp = Instantiate(Refinery, hitPos, Quaternion.identity);
        }
    }


    //This continues to show us our preview, so long as our temp variable contains something.
    private void Update()
    {
        if (temp != null)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit RayHit;
            if (Physics.Raycast(ray, out RayHit, 1000f, ~BlockingLayers))
            {
                GameObject targetHit = RayHit.transform.gameObject;
                Vector3 hitPos = RayHit.point;
                temp.transform.position = hitPos;
            }
        }
    }


    //This places our building, but first has to check if temp exists. It reparents temp, then sets it to null, essentially detatching our prefab to the scene.
    private void OnSelect()
    {
        if (temp != null && CheckPlacementCollision(temp.GetComponentInChildren<BoxCollider>()))
        {
            temp.transform.SetParent(BuildingParent.transform);
            temp = null;
        }
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

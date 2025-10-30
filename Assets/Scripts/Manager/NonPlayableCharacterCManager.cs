using System.Collections.Generic;
using UnityEngine;

public class NonPlayableCharacterCManager : MonoBehaviour
{
    [SerializeField] private LayerMask clickableLayers = -1;
    [SerializeField] private GameObject _npcPrefab;
    private List<GameObject> _npcs = new();
    private List<NPC> _currentSelectedNPCs = new();
    private GameObject _tempTarget; //Needed because NPC only takes GameObject, not transform.

    void Start()
    {
        _tempTarget = new("ClickTarget");
        InstansiateNewNPC(new(-55.9f, 6, 1.5f)); //TEMP
    }

    void OnEnable()
    {
        GameManager.instance.player.onPressedSelect += HandleClick;
    }

    void OnDisable()
    {
        GameManager.instance.player.onPressedSelect -= HandleClick;
    }

    public void InstansiateNewNPC(Vector3 position)
    {
        GameObject newNPC = Instantiate(_npcPrefab, position, new());

        _npcs.Add(newNPC);

        newNPC.GetComponent<NPC>().OnSelfSelected += HandleNPCSelected;
    }

    private void HandleNPCSelected(NPC npc, bool isSelected)
    {
        if (isSelected)
        {
            _currentSelectedNPCs.Add(npc);
            return;
        }

        _currentSelectedNPCs.Remove(npc);
    }

    private void HandleClick()
    {
        if (_currentSelectedNPCs.Count <= 0)
            return;

        Camera playerCamera = GameManager.instance.cameraManager.mainCamera;
        if (playerCamera == null)
            playerCamera = Camera.main;

        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, clickableLayers))
        {
            GameObject target;

            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Ground"))
            {
                _tempTarget.transform.position = hit.point;
                target = _tempTarget;
            }
            else
            {
                target = hit.collider.gameObject;
            }

            foreach(NPC npc in _currentSelectedNPCs)
            {
                npc.Link(target);
            }
        }
    }
}

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
        InstantiateNewNPC(new(-55.9f, 6, 1.5f)); //TEMP
        InstantiateNewNPC(new(-52.9f, 6, 1.5f));
    }

    void OnDestroy()
    {
        if (_tempTarget != null)
            Destroy(_tempTarget);
    }

    void OnEnable()
    {
        GameManager.instance.player.onPressedSelect += HandleClick;
    }

    void OnDisable()
    {
        GameManager.instance.player.onPressedSelect -= HandleClick;
    }

    public void InstantiateNewNPC(Vector3 position)
    {
        GameObject newNPC = Instantiate(_npcPrefab, position, Quaternion.identity);

        _npcs.Add(newNPC);

        newNPC.GetComponent<NPC>().OnSelfSelected += HandleNPCSelected;
    }

    private void HandleNPCSelected(NPC npc, bool isSelected)
    {
        if (isSelected)
        {
            if (!_currentSelectedNPCs.Contains(npc))
                _currentSelectedNPCs.Add(npc);
        }
        else
            _currentSelectedNPCs.Remove(npc);
    }

    private void HandleClick()
    {
        if (_currentSelectedNPCs.Count <= 0)
            return;

        bool isMultiSelect = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl) ||
                    Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        if (isMultiSelect) //Solving the problem where multiselecting NPCs would make one of them walk torwards the other.
            return;

        Camera playerCamera = GameManager.instance.cameraManager.mainCamera;
        if (playerCamera == null)
            playerCamera = Camera.main;

        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, clickableLayers))
        {
            if (hit.collider.TryGetComponent<NPC>(out _)) //Needed so that it doesn't instaselect the ground behind the NPC
                return;

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
                Debug.Log($"Linked {target.name} ({target.transform.position}) to {npc.name}");
            }
        }
    }
}

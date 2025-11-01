using System.Collections.Generic;
using UnityEngine;

public class NonPlayableCharacterManager : MonoBehaviour
{
    [SerializeField] private LayerMask clickableLayers = -1;
    [SerializeField] private NpcSO _basicNpcSO;
    private List<Npc> _npcs = new();
    private List<Npc> _currentSelectedNPCs = new();
    private GameObject _tempTarget; //Needed because NPC only takes GameObject, not transform.

    void Start()
    {
        _tempTarget = new("ClickTarget");
        InstantiateNewNPC(_basicNpcSO, new(-55.9f, 6, 1.5f)); //TEMP
        InstantiateNewNPC(_basicNpcSO, new(-52.9f, 6, 1.5f));
        InstantiateNewNPC(_basicNpcSO, new(-49.9f, 6, 1.5f));
        InstantiateNewNPC(_basicNpcSO, new(-47.9f, 6, 1.5f));
        InstantiateNewNPC(_basicNpcSO, new(-45.9f, 6, 1.5f));
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

    public void InstantiateNewNPC(NpcSO npcSO, Vector3 position)
    {
        Npc newNPC = Instantiate(npcSO.prefab, position, Quaternion.identity).GetComponent<Npc>();

        _npcs.Add(newNPC);

        newNPC.OnSelfSelected += HandleNPCSelected;
    }

    private void HandleNPCSelected(Npc npc, bool isSelected)
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
            if (hit.collider.TryGetComponent<Npc>(out _)) //Needed so that it doesn't instaselect the ground behind the NPC
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

            foreach(Npc npc in _currentSelectedNPCs)
            {
                npc.Link(target);
                Debug.Log($"Linked {target.name} ({target.transform.position}) to {npc.name}");
            }
        }
    }
}

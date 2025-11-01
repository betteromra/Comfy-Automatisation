using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Inventory))]
public class InventoryEditor : Editor
{
    RessourceSO selectedRessource;
    int amountToAdd = 1;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        Inventory inventory = (Inventory)target;

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Add Resource", EditorStyles.boldLabel);

        selectedRessource = (RessourceSO)EditorGUILayout.ObjectField("Ressource", selectedRessource, typeof(RessourceSO), false);
        amountToAdd = EditorGUILayout.IntField("Amount", amountToAdd);

        if (GUILayout.Button("Add"))
        {
            if (selectedRessource != null)
            {
                inventory.Add(new RessourceAndAmount(selectedRessource, amountToAdd));
            }
        }
        if (GUILayout.Button("Remove"))
        {
            if (selectedRessource != null)
            {
                inventory.Remove(new RessourceAndAmount(selectedRessource, amountToAdd));
            }
        }
    }
}

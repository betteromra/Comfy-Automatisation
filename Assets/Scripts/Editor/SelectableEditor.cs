using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(Selectable))]
public class SelectableEditor : Editor
{
    private SerializedProperty outlineWidth;
    private SerializedProperty hoverTint;
    private SerializedProperty hoverIntensity;
    private SerializedProperty useOutline;
    private SerializedProperty isParent;
    private SerializedProperty specificRenderers;

    private bool showChildRenderers = false;
    private List<Renderer> availableRenderers = new List<Renderer>();

    private void OnEnable()
    {
        outlineWidth = serializedObject.FindProperty("outlineWidth");
        hoverTint = serializedObject.FindProperty("hoverTint");
        hoverIntensity = serializedObject.FindProperty("hoverIntensity");
        useOutline = serializedObject.FindProperty("useOutline");
        isParent = serializedObject.FindProperty("isParent");
        specificRenderers = serializedObject.FindProperty("specificRenderers");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        Selectable selectable = (Selectable)target;

        // Visual Feedback Header
        EditorGUILayout.LabelField("Visual Feedback", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(outlineWidth);
        EditorGUILayout.PropertyField(hoverTint, new GUIContent("Hover Tint", "Color tint applied when hovering over the object"));
        EditorGUILayout.PropertyField(hoverIntensity, new GUIContent("Hover Intensity", "Intensity of the hover effect"));

        EditorGUILayout.Space();

        // Selection Settings Header
        EditorGUILayout.LabelField("Selection Settings", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(useOutline);

        EditorGUILayout.Space();

        // Parent/Prefab Settings Header
        EditorGUILayout.LabelField("Parent/Prefab Settings", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(isParent, new GUIContent("Is Parent", "Enable this if this is a parent prefab with multiple child meshes"));

        if (isParent.boolValue)
        {
            EditorGUILayout.HelpBox(
                "Parent Mode: You can specify which child renderers to include in selection.\n" +
                "Leave the array empty to use all child renderers.",
                MessageType.Info
            );

            EditorGUILayout.Space();

            // Specific Renderers Array
            EditorGUILayout.PropertyField(specificRenderers, new GUIContent("Specific Renderers", "Drag renderers here to include only specific children. Leave empty for all."), true);

            EditorGUILayout.Space();

            // Helper Section - Show available child renderers
            showChildRenderers = EditorGUILayout.Foldout(showChildRenderers, "Available Child Renderers", true);
            if (showChildRenderers)
            {
                EditorGUI.indentLevel++;
                
                // Get all child renderers
                availableRenderers.Clear();
                availableRenderers.AddRange(selectable.GetComponentsInChildren<Renderer>());

                if (availableRenderers.Count == 0)
                {
                    EditorGUILayout.HelpBox("No child renderers found.", MessageType.Warning);
                }
                else
                {
                    EditorGUILayout.LabelField($"Found {availableRenderers.Count} renderer(s):", EditorStyles.miniLabel);
                    
                    foreach (var renderer in availableRenderers)
                    {
                        if (renderer == null) continue;

                        EditorGUILayout.BeginHorizontal();
                        
                        // Show renderer info
                        EditorGUILayout.ObjectField(renderer, typeof(Renderer), true);
                        
                        // Check if already in the specific renderers array
                        bool isIncluded = false;
                        for (int i = 0; i < specificRenderers.arraySize; i++)
                        {
                            if (specificRenderers.GetArrayElementAtIndex(i).objectReferenceValue == renderer)
                            {
                                isIncluded = true;
                                break;
                            }
                        }

                        // Add/Remove button
                        if (isIncluded)
                        {
                            if (GUILayout.Button("Remove", GUILayout.Width(70)))
                            {
                                RemoveRendererFromArray(renderer);
                            }
                        }
                        else
                        {
                            if (GUILayout.Button("Add", GUILayout.Width(70)))
                            {
                                AddRendererToArray(renderer);
                            }
                        }

                        EditorGUILayout.EndHorizontal();
                    }
                }

                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space();

            // Quick action buttons
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Select All Child Renderers"))
            {
                SelectAllRenderers(selectable);
            }
            if (GUILayout.Button("Clear Renderers"))
            {
                specificRenderers.ClearArray();
            }
            EditorGUILayout.EndHorizontal();

            // Show current count
            int currentCount = specificRenderers.arraySize;
            if (currentCount > 0)
            {
                EditorGUILayout.HelpBox($"Currently using {currentCount} specific renderer(s).", MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox("Using all child renderers (no specific selection).", MessageType.Info);
            }
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void AddRendererToArray(Renderer renderer)
    {
        specificRenderers.InsertArrayElementAtIndex(specificRenderers.arraySize);
        specificRenderers.GetArrayElementAtIndex(specificRenderers.arraySize - 1).objectReferenceValue = renderer;
        serializedObject.ApplyModifiedProperties();
    }

    private void RemoveRendererFromArray(Renderer renderer)
    {
        for (int i = specificRenderers.arraySize - 1; i >= 0; i--)
        {
            if (specificRenderers.GetArrayElementAtIndex(i).objectReferenceValue == renderer)
            {
                specificRenderers.DeleteArrayElementAtIndex(i);
                break;
            }
        }
        serializedObject.ApplyModifiedProperties();
    }

    private void SelectAllRenderers(Selectable selectable)
    {
        specificRenderers.ClearArray();
        var renderers = selectable.GetComponentsInChildren<Renderer>();
        
        foreach (var renderer in renderers)
        {
            AddRendererToArray(renderer);
        }
    }
}

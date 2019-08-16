using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
class HierarchyLock
{
#if UNITY_2019_1_OR_NEWER
    static readonly Color darkGray = new Color(0.1058824f, 0.1058824f, 0.1058824f);
    static readonly float lockIconXPos = 24;
#else
    static readonly Color darkGray = new Color(0.4549019f, 0.4549019f, 0.4549019f);
    static readonly float lockIconXPos = 4;
#endif

    static readonly float lockIconYOffset = 1;
    static readonly float lockIconWidth = 18;

    static GUIContent lockIconOnContent;
    static GUIContent lockIconContent;

    static List<GameObject> descendants;

    static HierarchyLock()
    {
        EditorApplication.hierarchyWindowItemOnGUI += HierarchyWindowItemOnGUI;
        lockIconOnContent = EditorGUIUtility.IconContent("LockIcon-On");
        lockIconContent = EditorGUIUtility.IconContent("LockIcon");
        descendants = new List<GameObject>();
    }

    static void HierarchyWindowItemOnGUI(int instanceID, Rect selectionRect)
    {
#if UNITY_2018_3_OR_NEWER
        if (UnityEditor.Experimental.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage() != null)
        {
            return;
        }
#endif

        GameObject hierarchyGameObject = EditorUtility.InstanceIDToObject(instanceID) as GameObject;

        if (!hierarchyGameObject)
        {
            return;
        }

        Color originalContentColor = GUI.contentColor;

        Rect hierarchyRowRect = new Rect(
            0, 
            selectionRect.y, 
            selectionRect.x + selectionRect.width,
            selectionRect.height
        );

        Rect lockIconRect = new Rect(
            lockIconXPos,
            selectionRect.y + lockIconYOffset,
            lockIconWidth,
            selectionRect.height
        );

        Vector2 mousePos = Event.current.mousePosition;
        bool mouseWithinHierarchyRowRect = hierarchyRowRect.Contains(mousePos);
        bool mouseWithinLockIconRect = lockIconRect.Contains(mousePos);

        GUIStyle guiStyle = new GUIStyle(GUI.skin.label);
        GUI.skin.button = guiStyle;

        if (EditorGUIUtility.isProSkin)
        {
            if (mouseWithinLockIconRect)
            {
                GUI.contentColor = Color.white;
            }
            else
            {
                GUI.contentColor = darkGray;
            }
        } else {
            if (mouseWithinLockIconRect)
            {
                GUI.contentColor = darkGray;
            }
            else
            {
                GUI.contentColor = Color.gray;
            }
        }

        if (hierarchyGameObject.hideFlags == HideFlags.NotEditable)
        {
            if (GUI.Button(lockIconRect, lockIconOnContent))
            {
                Undo.RegisterCompleteObjectUndo(hierarchyGameObject, "GameObject made NotEditable");
                hierarchyGameObject.hideFlags = HideFlags.None;
                EditorUtility.SetDirty(hierarchyGameObject);

                descendants.Clear();
                descendants = GetDescendants(hierarchyGameObject, descendants);

                foreach (GameObject go in descendants)
                {
                    go.hideFlags = HideFlags.None;
                }
            }
        }
        else
        {
            if (mouseWithinHierarchyRowRect)
            {
                if (GUI.Button(lockIconRect, lockIconContent))
                {
                    Undo.RegisterCompleteObjectUndo(hierarchyGameObject, "GameObject made Editable");
                    hierarchyGameObject.hideFlags = HideFlags.NotEditable;
                    EditorUtility.SetDirty(hierarchyGameObject);

                    descendants.Clear();
                    descendants = GetDescendants(hierarchyGameObject, descendants);
               
                    foreach (GameObject go in descendants)
                    {
                        go.hideFlags = HideFlags.NotEditable;
                    }
                }
            }
        }

        GUI.contentColor = originalContentColor;

        EditorApplication.RepaintHierarchyWindow();
    }

    static List<GameObject> GetDescendants(GameObject parent, List<GameObject> descendants)
    {
        int childCount = parent.transform.childCount;
        if (parent.transform.childCount > 0)
        {
            for (int i = 0; i < childCount; i++)
            {
                descendants.Add(parent.transform.GetChild(i).gameObject);
                GetDescendants(descendants[descendants.Count - 1], descendants);
            }
        }

        return descendants;
    }
}

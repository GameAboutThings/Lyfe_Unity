using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Designer_CellEditor : MonoBehaviour
{
    private static List<Node_CellEditor> selectedNodes;
    private static bool editorInputEnabled = true;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public static void AddSelectedNode(Node_CellEditor _node)
    {
        if (_node != null)
        {
            if (selectedNodes == null)
                selectedNodes = new List<Node_CellEditor>();

            selectedNodes.Add(_node);
        }
    }

    public static void RemoveSelectedNode(Node_CellEditor _node)
    {
        selectedNodes.Remove(_node);
    }

    public static Node_CellEditor GetLastSelectedNode()
    {
        if (selectedNodes.Count > 0)
            return selectedNodes[selectedNodes.Count - 1];
        else
            return null;
    }

    public static bool GetEditorInputEnabled()
    {
        return editorInputEnabled;
    }
}

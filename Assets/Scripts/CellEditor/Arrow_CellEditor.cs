using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Meta_CellEditor.SCULPTING.ARROW;

public class Arrow_CellEditor : MonoBehaviour
{
    private bool isMouseOver;
    private Material arrowMaterial;

    // Start is called before the first frame update
    void Start()
    {
        arrowMaterial = GetComponent<MeshRenderer>().material;
        arrowMaterial.color = COLOR_BASE;
    }

    void Update()
    {
        CheckClick();
    }

    void OnMouseOver()
    {
        isMouseOver = true;
        arrowMaterial.color = COLOR_HOVER;
    }

    void OnMouseExit()
    {
        isMouseOver = false;
        arrowMaterial.color = COLOR_BASE;
    }

    private void CheckClick()
    {
        if (isMouseOver)
        {
            if (Input.GetMouseButtonDown(0))
                OnClicked();
        }
    }

    private void OnClicked()
    {
        GameObject node = transform.parent.gameObject;
        node.GetComponent<Node_CellEditor>().OnArrowClicked(this);
    }
}

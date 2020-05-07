using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexBehaviour : MonoBehaviour
{
    private Color baseColor;
    private Color movableColor;
    private List<Material> materials;

    private bool isMouseOver = false;

    void Start()
    {
        materials = new List<Material>();
        GetComponent<MeshRenderer>().GetMaterials(materials);
        baseColor = materials[0].GetColor("Color_8F328A1B");
        movableColor = baseColor + new Color(0.5f, 0f, 0f, 1);
    }

    void OnMouseOver()
    {
        isMouseOver = true;
        Player_World.SetCurrentlySelectedHex(gameObject);
    }

    void OnMouseExit()
    {
        isMouseOver = false;
    }

    public void SetMovable()
    {
        materials[0].SetColor("Color_8F328A1B", movableColor);
    }

    public void ResetStatus()
    {
        materials[0].SetColor("Color_8F328A1B", baseColor);
    }
}

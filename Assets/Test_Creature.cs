using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test_Creature : MonoBehaviour
{
    private Vector3 targetPosition;
    private float speed = 30f;
    private bool resting = true;
    private float moveRadius = 10f;

    //caches
    private List<HexBehaviour> movableHexes;

    void Update()
    {
        Move();
    }

    private void Move()
    {
        if (Input.GetMouseButtonDown(0))
        {
            GameObject target = Player_World.GetCurrentlySelectedHex();

            if (target != null && movableHexes.IndexOf(target.GetComponent<HexBehaviour>()) != -1)
                targetPosition = target.transform.position + new Vector3(0, 2.5f, 0);

           

            Debug.Log(targetPosition);
        }

        if (targetPosition != transform.position)
        {
            resting = false;
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
        }       
        else
        {
            if (!resting)
            {
                ColorPossibleTiles();

                resting = true;
            }
        }
    }

    private void ColorPossibleTiles()
    {
        ResetPreviousHexes();
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, moveRadius);

        for (int i = 0; i < hitColliders.Length; i++)
        {
            GameObject obj = hitColliders[i].gameObject;
            HexBehaviour hex = obj.GetComponent<HexBehaviour>();
            if (hex == null)
                continue;
            hex.SetMovable();

            if (movableHexes == null)
                movableHexes = new List<HexBehaviour>();

            movableHexes.Add(hex);
        }
    }

    private void ResetPreviousHexes()
    {
        if (movableHexes == null)
            return;

        while (movableHexes.Count != 0)
        {
            movableHexes[0].ResetStatus();
            movableHexes.Remove(movableHexes[0]);
        }
    }
}

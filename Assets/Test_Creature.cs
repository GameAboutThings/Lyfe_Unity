using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test_Creature : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        Move();
    }

    private void Move()
    {
        if (Input.GetMouseButtonDown(0))
        {
            GameObject target = Player_World.GetCurrentlySelectedHex();
            //if(target != null)
                gameObject.transform.position = target.transform.position + new Vector3(0, 2.5f, 0);
        }
    }
}

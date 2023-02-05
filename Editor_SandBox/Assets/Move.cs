using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move : MonoBehaviour
{
    
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            transform.Translate(0, 0.1f, 0);
        }
        else if (Input.GetKeyDown(KeyCode.B))
        {
            transform.Translate(-0.1f, 0, 0);
        }
        else if (Input.GetKeyDown(KeyCode.C))
        {
            transform.Translate(0, -0.1f, 0);
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            transform.Translate(0.1f, 0, 0);
        }
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            transform.Translate(0.1f, 0, 0);
        }
    }
}

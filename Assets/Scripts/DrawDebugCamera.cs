using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class DrawDebugCamera : MonoBehaviour
{
    // Start is called before the first frame update
    //void Start()
    //{
    //    cam = GetComponent<Camera>();
    //}

    // Update is called once per frame
    void Update()
    {
        Debug.DrawLine(this.transform.position, this.transform.position + transform.forward * 3f, Color.green);
    }
}

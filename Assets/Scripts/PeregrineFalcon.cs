using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PeregrineFalcon : MonoBehaviour {

    /*
     * Startup movements:
     * 1. Rise circlingHeight in Y direction 
     * 2. Go forward circlingRadius in Z direction (head should be pointing in - X direction) 
     */
    Transform transform;
    public float circlingRadius;
    public float speed;
    public float circlingHeight;
    float original_x;
    float original_z;
    float theta;

    // Start is called before the first frame update
    void Start()
    {
        transform = gameObject.transform;
        transform.position = new Vector3(transform.position.x, transform.position.y + circlingHeight, transform.position.z);
        original_x = transform.position.x;
        original_z = transform.position.z;
        theta = 0;
    }

    private void FlyInCircle(Vector3 center)
    {
        float x_offset = circlingRadius * Mathf.Cos(theta);
        float z_offset = circlingRadius * Mathf.Sin(theta);
        theta += speed;
        if (theta > 360)
        {
            theta = 0;
        }

        transform.eulerAngles = new Vector3(
            transform.eulerAngles.x,
            -theta * 180f / (float)Math.PI + 90f,
            transform.eulerAngles.z
        );

        transform.position = new Vector3(center.x + x_offset, center.y, center.z + z_offset);

    }

    // Update is called once per frame
    void Update()
    {
        Vector3 falconCircleCenter = new Vector3(original_x, transform.position.y, original_z);
        FlyInCircle(falconCircleCenter);
    }

    
}

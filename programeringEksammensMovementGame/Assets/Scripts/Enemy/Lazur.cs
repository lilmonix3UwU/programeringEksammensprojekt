using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lazur : MonoBehaviour
{
    Vector3 d = Vector3.zero;
    float s = 0;


    void Start()
    {
        
    }
    void Update()
    {
        transform.position += d * s * Time.deltaTime;
    }


    public void Launch(Vector3 direction, float speed)
    {
        d = direction;
        s = speed;
    }
}

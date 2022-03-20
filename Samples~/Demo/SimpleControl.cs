using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleControl : MonoBehaviour
{
    public float speed = 1f;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.position += speed * Time.deltaTime * new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));
    }
}

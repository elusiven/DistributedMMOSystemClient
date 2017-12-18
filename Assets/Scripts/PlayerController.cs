using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    public float moveSpeed;

    // Use this for initialization
    void Start()
    {
        moveSpeed = 8f;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.anyKey)
        {
            transform.Translate(moveSpeed * Input.GetAxis("Horizontal") * Time.deltaTime, 0f, moveSpeed * Input.GetAxis("Vertical") * Time.deltaTime);
        }   
    }

}

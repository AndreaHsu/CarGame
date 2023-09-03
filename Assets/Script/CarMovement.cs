using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarMovement : MonoBehaviour
{
    public Rigidbody rb;
    public float speed = 3;
    public float rotate_speed = 15;
    private Vector3 initialPos;
    // Start is called before the first frame update
    void Start()
    {
        initialPos =  transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKey("w") || Input.GetKey("up") ){
            transform.Translate(-speed * Time.deltaTime, 0, 0);
        }
        if(Input.GetKey("s") || Input.GetKey("down")){
            transform.Translate(speed * Time.deltaTime, 0, 0);
        }
        if(Input.GetKey("a") || Input.GetKey("left")){
            transform.Rotate(0, -rotate_speed * Time.deltaTime, 0);
        }
        if(Input.GetKey("d") || Input.GetKey("right")){
            transform.Rotate(0, rotate_speed * Time.deltaTime, 0);
        }
    }

    void OnTriggerEnter(Collider other)
    {
    　　if(other.tag == "endline"){
            print("Achieve endline");
            transform.position = initialPos;
        }
        else if(other.tag == "outofbound"){
            print("OutOfBound");
            transform.position = initialPos;
        }
    }

    void OnCollisionEnter(Collision other)
    {
        if(other.gameObject.tag == "obticle"){
            print("collide with a obticle");
            transform.position = initialPos;
        }
    }
}

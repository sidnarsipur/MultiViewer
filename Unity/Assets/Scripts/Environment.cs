using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Environment : MonoBehaviour
{

    public float scale;
    public float objScale;
    // public GameObject gb;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnCollisionEnter(Collision collision)
    {
        //Prevent GameObjet from moving

        // Debug.Log(gameObject.name + " Collided with: " + collision.gameObject.name);
        
        // gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
        // gameObject.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;

        // Debug.Log(gameObject.name + " Collided with: " + collision.gameObject.name);

        // // Rigidbody rb = gameObject.GetComponent<Rigidbody>();

        // // rb.velocity = Vector3.zero; // Stop the object
        // // rb.angularVelocity = Vector3.zero; // Stop any rotation
        // // rb.isKinematic = true; // Make it kinematic to stop further movement
    }
}

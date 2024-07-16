using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Object : MonoBehaviour
{
    MultiViewer m;
    
    private bool collided = false;
    
    void Start(){
        GameObject multiViewerGameObject = GameObject.Find("MultiViewer");
        m = multiViewerGameObject.GetComponent<MultiViewer>();

        scale = this.transform.localScale.x;
    }

    public void selectChild(GameObject g){
        m.setSelectedGameObject(g);
    }

    public void setCollided(bool collided){
        this.collided = collided;
    }

    public bool getCollided(){
        return collided;
    }

    void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Boundary"){
            setCollided(true);
        }
    }
    
    void OnCollisionExit(Collision collision)
    {
        if(collision.gameObject.tag == "Boundary"){
            setCollided(false);
        }
    }
}
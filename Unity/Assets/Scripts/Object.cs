using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Object : MonoBehaviour
{
    MultiViewer m;

    void Start(){
        GameObject multiViewerGameObject = GameObject.Find("MultiViewer");
        m = multiViewerGameObject.GetComponent<MultiViewer>();
    }

    void Update(){
        if(OVRInput.Get(OVRInput.RawButton.LThumbstickUp)){
            moveObject();
        }
    }

    public void selectChild(GameObject g){
        Debug.Log(g.name + " SELECTED");
        m.setSelectedGameObject(g);
    }

    public void unselectChild(){
        m.setSelectedGameObject(null);
    }

    public void moveObject(){
        gameObject.transform.position += new Vector3(0, 0, 0.01f);
    }

}
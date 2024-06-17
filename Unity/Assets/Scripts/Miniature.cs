using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Miniature : MonoBehaviour
{
    public void selectChild(){

        Debug.Log("Selecting Child: " + gameObject.name);
        // g.transform.position += new Vector3(0, 0, 100f);

        MultiViewer m = gameObject.GetComponent<MultiViewer>();
        m.setSelectedGameObject(gameObject);
    }

    public void unselectChild(){

        MultiViewer m = gameObject.GetComponent<MultiViewer>();
        m.setSelectedGameObject(null);
    }

    public void moveChild(){
        gameObject.transform.position += new Vector3(0, 0, 100f);
    }
}
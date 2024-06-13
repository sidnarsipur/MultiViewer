using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Miniature : MonoBehaviour
{
    public void selectChild(GameObject g){

        MultiViewer m = GameObject.Find("MultiViewer").GetComponent<MultiViewer>();
        m.setSelectedGameObject(g);
    }

    public void unselectChild(){

        MultiViewer m = GameObject.Find("MultiViewer").GetComponent<MultiViewer>();
        m.setSelectedGameObject(null);
    }
}
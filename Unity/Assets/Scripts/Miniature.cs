using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Miniature : MonoBehaviour
{
    public void selectChild(GameObject g){
        Debug.Log("Selected");

        MultiViewer m = GameObject.Find("MultiViewer").GetComponent<MultiViewer>();
        m.setSelectedChild(g);
    }

    public void unselectChild(){
        Debug.Log("Unselected");

        MultiViewer m = GameObject.Find("MultiViewer").GetComponent<MultiViewer>();
        m.setSelectedChild(null);
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiViewer : MonoBehaviour
{
    public List<Transform> m_environments;
    public enum Env {Bedroom, Carlson, StudyRoom, Izone}

    [Header("MultiViewer")]                
    public Env parent;
    public List<Env> children;

    [HideInInspector]            
    public GameObject parentEnv;

    [HideInInspector] 
    public List<GameObject> childEnvs = new List<GameObject>();

    public void clearEnv()
    {
        foreach (Transform env in m_environments)
        {
            env.gameObject.SetActive(false);
        }
    }

    public void setEnv()
    {
        clearEnv(); 

        parentEnv = m_environments[(int)parent].gameObject;
        parentEnv.SetActive(true);

        GameObject parentCopy = Instantiate(parentEnv);
        parentCopy.transform.parent = transform;
        parentCopy.name = "parentCopy";

        GameObject objects = transform.Find("objects").gameObject;
        objects.transform.parent = parentEnv.transform;

        disableInteraction(parentEnv);

        foreach(Env env in children)
        {
            GameObject childEnv = m_environments[(int)env].gameObject;
            childEnvs.Add(childEnv);

            GameObject objectCopy = Instantiate(objects);
            objectCopy.transform.parent = childEnv.transform;
            objectCopy.name = "objects";
        }

        childEnvs.Add(parentCopy);

        GameObject copy = Instantiate(objects); //To be cleaned up!
        copy.transform.parent = parentCopy.transform;
        copy.name = "objects";
    }

   public void placeChildren()
    {

        int numObjects = childEnvs.Count;

        Vector3 size = parentEnv.GetComponent<BoxCollider>().size;
        float width = size.x / (numObjects * 1.5f);

        for (int i = 0; i < numObjects; i++)
        {
            GameObject g = childEnvs[i];
            g.SetActive(true);

            Environment e = g.GetComponent<Environment>();

            if (e != null)
            {
                g.transform.localScale = new Vector3(e.scale, e.scale, e.scale);

                float x = parentEnv.transform.position.x - (size.x/4) + width * (i+1);
                Vector3 objectPosition = new Vector3(x, 0.0f, 0.5f);              
                g.transform.position = objectPosition;
            }

        }
    }

    public void changeParent()
    {
        
    }

    public void disableInteraction(GameObject g)
    {
        GameObject RayGrabInteraction = g.transform.Find("ISDK_RayGrabInteraction").gameObject;
        RayGrabInteraction?.SetActive(false);

    }

     public void enableInteraction(GameObject g)
    {
        GameObject RayGrabInteraction = g.transform.Find("ISDK_RayGrabInteraction").gameObject;
        RayGrabInteraction?.SetActive(true);

    }

    // Start is called before the first frame update
    void Start()
    {
        setEnv();
        placeChildren();            
    }

    // Update is called once per frame
    void Update()
    {
        
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiViewer : MonoBehaviour
{
    [Header("MultiViewer")]                
    public GameObject parent;
    public List<GameObject> children;

    private GameObject parentCopy;
    private GameObject parentObjects;
   
    // private Dictionary<GameObject, ObjectState> originalStates = new Dictionary<GameObject, ObjectState>();

    // private class ObjectState
    // {
    //     public Vector3 position;
    //     public Vector3 scale;

    //     public ObjectState(Vector3 pos, Vector3 scl)
    //     {
    //         position = pos;
    //         scale = scl;
    //     }
    // }

    public void clearEnv()
    {
        foreach (GameObject g in children)
        {
            g.SetActive(false);
            enableInteraction(g);
        }

        parent.SetActive(false);
        enableInteraction(parent);
    }

    public void setEnv()
    {
        clearEnv(); 

        parent.SetActive(true);

        parentCopy = Instantiate(parent);
        parentCopy.transform.parent = transform;
        parentCopy.name = "parentCopy";

        disableInteraction(parent);

        parentObjects = transform.Find("objects").gameObject;
        parentObjects.transform.parent = parent.transform;

        foreach(GameObject child in children)
        {
            GameObject objectCopy = Instantiate(parentObjects);
            objectCopy.transform.parent = child.transform;
            objectCopy.name = child.name + "objects";

            foreach(Transform t in objectCopy.transform){
                t.name = child.name + t.name;
            }
        }

        children.Add(parentCopy);

        GameObject copy = Instantiate(parentObjects); //To be cleaned up!
        copy.transform.parent = parentCopy.transform;
        copy.name = parentCopy.name + "objects";

        foreach(Transform t in copy.transform){
            t.name = "parentCopy" + t.name;
        }
    }

   public void placeChildren()
    {
        int numObjects = children.Count;

        Vector3 size = parent.GetComponent<BoxCollider>().size;
        float width = size.x / (numObjects * 1.5f);

        for (int i = 0; i < numObjects; i++)
        {
            GameObject g = children[i];
            g.SetActive(true);

            float scale = getScale(g);

            g.transform.localScale = new Vector3(scale, scale, scale);

            float x = parent.transform.position.x - (size.x/4) + width * (i+1);
            Vector3 objectPosition = new Vector3(x, 0.0f, 0.5f);              
            g.transform.position = objectPosition;
        }
    }

    private float getScale(GameObject g){
         Environment e = g.GetComponent<Environment>();

          if (e != null)
            {
                return e.scale;
            }
            return 0;
    }

    public void changeParent(GameObject g)
    {
        // if(OVRInput.Get(OVRInput.RawButton.LIndexTrigger)){
        //     ResetObjectState(parent);

        //     // children.Add(parent);
        //     // children.Remove(parentCopy);
        //     // children.Remove(g);
        //     // Destroy(parentCopy);

        //     // parent = g;

        //     // foreach(GameObject child in children){
        //     //     Destroy(transform.Find(child.name + "objects"));
        //     //     ResetObjectState(child);
        //     // }
            
        //     // setEnv();
        // }
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

    public Vector3 getAnchor(GameObject g){
        BoxCollider boxCollider = g.GetComponent<BoxCollider>();

        Vector3 center = boxCollider.center;
        Vector3 size = boxCollider.size;

        Vector3 leftAnchor = center - new Vector3(size.x / 2, 0, 0);

        return leftAnchor;
    }

    public Vector3 sizeCompare(GameObject object1, GameObject object2){
        Vector3 size1 = object1.GetComponent<BoxCollider>().bounds.size;
        Vector3 size2 = object2.GetComponent<BoxCollider>().bounds.size;

        Vector3 scale1 = object1.transform.localScale;
        Vector3 scale2 = object2.transform.localScale;

        Vector3 relativeSize = new Vector3(size1.x / scale1.x / (size2.x / scale2.x),
                                        size1.y / scale1.y / (size2.y / scale2.y),
                                        size1.z / scale1.z / (size2.z / scale2.z));
        
        return relativeSize;
}

public void updateObjects()
{ 
    // Vector3 parentAnchor = getAnchor(parent); 

    // Dictionary<string, (float, Vector3)> objectPositions = new Dictionary<string, (float, Vector3)>();

    // foreach(Transform t in parentObjects.transform){
    //     Vector3 position = t.position;

    //     float dist = (parentAnchor - position).magnitude;
    //     Vector3 direction = (parentAnchor - position).normalized;
        
    //     objectPositions.Add(t.name, (dist, direction));
    // }

    // foreach(GameObject child in children){              
    //     Vector3 relativeSize = sizeCompare(child, parent);
    //     Vector3 childAnchor = getAnchor(child);

    //     GameObject childObjects = child.transform.Find((child.name + "objects")).gameObject;

    //     foreach(Transform obj in childObjects.transform){
    //         string objectName = (obj.name).Replace(child.name, "");

    //         if(objectPositions.ContainsKey(objectName)){
    //             Debug.Log(obj.name);

    //             var objectPosition = objectPositions[objectName];

    //             float objDist = objectPosition.Item1 * relativeSize.magnitude;
    //             Vector3 objDir = objectPosition.Item2;

    //             obj.position = parentAnchor;
    //         }
    //     }
    // }
 }

    //  private void StoreOriginalState(GameObject obj) 
    // {
    //     if (!originalStates.ContainsKey(obj))
    //     {
    //         Vector3 originalPosition = obj.transform.position;
    //         Vector3 originalScale = obj.transform.localScale;
    //         originalStates.Add(obj, new ObjectState(originalPosition, originalScale));
    //     }
    // }

    // private void ResetObjectState(GameObject obj)
    // {
    //     if (originalStates.ContainsKey(obj))
    //     {
    //         ObjectState state = originalStates[obj];
    //         obj.transform.position = state.position;
    //         obj.transform.localScale = state.scale;
    //     }
    // }

    // Start is called before the first frame update
    void Start()
    {
        // StoreOriginalState(parent);
        
        // foreach(GameObject child in children){
        //     StoreOriginalState(child);
        // }

        setEnv();
        placeChildren();            
    }

    // Update is called once per frame
    void Update()
    {
        updateObjects();
    }

}

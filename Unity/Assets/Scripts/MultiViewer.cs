using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiViewer : MonoBehaviour
{
    [Header("MultiViewer")]                
    public GameObject parent;
    public List<GameObject> children;

    private GameObject parentObjects;
    
    private GameObject parentCopy;

    private GameObject selectedChild = null;
   
    private Dictionary<GameObject, ObjectState> originalStates = new Dictionary<GameObject, ObjectState>();

    private class ObjectState
    {
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;

        public ObjectState(Vector3 pos, Quaternion rot, Vector3 scl)
        {
            position = pos;
            rotation = rot;
            scale = scl;
        }
    }

    public void clearEnv()
    {
        foreach (GameObject g in children)
        {
            g.SetActive(false);
            enableInteraction(g);
        }

        parent.SetActive(false);
        enableInteraction(parent);

        Debug.Log("Clear Env Finished");
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

        children.Add(parentCopy);

        GameObject copy = Instantiate(parentObjects); //To be cleaned up!
        copy.transform.parent = parentCopy.transform;
        copy.name = parentCopy.name + "objects";

        foreach(Transform t in copy.transform){
            t.name = "parentCopy" + t.name;
        }

        Debug.Log("Set Env Finished");
    }

    public void createChildObjects()
    {
        foreach(GameObject child in children)
        {
            GameObject objectCopy = Instantiate(parentObjects);
            objectCopy.transform.parent = child.transform;
            objectCopy.name = child.name + "objects";

            foreach(Transform t in objectCopy.transform){
                t.name = child.name + t.name;
            }
        }
    }

    public void setSelectedChild(GameObject g){
        selectedChild = g;
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

            Debug.Log("Set active" + g.name);

            float scale = getScale(g);

            g.transform.localScale = new Vector3(scale, scale, scale);

            float x = parent.transform.position.x - (size.x/4) + width * (i+1);
            Vector3 objectPosition = new Vector3(x, 0.0f, 0.5f);              
            g.transform.position = objectPosition;
        }
    }

    public void placeChildObjects(){

        Dictionary<string, (Vector3, float)> parentObjectLocations = new Dictionary<string, (Vector3, float)>();

        GameObject parentTopLeft = parent.Find("TopLeft");

        foreach(Transform object in parent.transform){
            
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

    public void changeParent()
    {   
        children.Remove(parentCopy);
        Destroy(parentCopy);

        parentObjects.transform.parent = this.transform;
        
        foreach(GameObject child in children){
            Destroy(transform.Find(child.name + "objects"));
            ResetObjectState(child);
        }

        ResetObjectState(parent);
        children.Add(parent);

        parent = selectedChild;

        setSelectedChild(null);
        
        setEnv();

        placeChildren();
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
    
    private void StoreOriginalState(GameObject obj) 
    {
        if (!originalStates.ContainsKey(obj))
        {
            Vector3 originalPosition = obj.transform.position;
            Quaternion originalRotation = obj.transform.rotation;
            Vector3 originalScale = obj.transform.localScale;
        
            originalStates.Add(obj, new ObjectState(originalPosition, originalRotation, originalScale));
        }
    }
    
    private void ResetObjectState(GameObject obj)
    {
        if (originalStates.ContainsKey(obj))
        {
            ObjectState state = originalStates[obj];

            obj.transform.position = state.position;
            obj.transform.rotation = state.rotation;
            obj.transform.localScale = state.scale;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        StoreOriginalState(parent);
        
        foreach(GameObject child in children){
            StoreOriginalState(child);
        }

        setEnv();
        createChildObjects();
        placeChildren();            
    }

    // Update is called once per frame
    void Update()
    {
        if(OVRInput.Get(OVRInput.RawButton.X)){
            if(selectedChild != null){
                changeParent();
            }
        }
    }

}

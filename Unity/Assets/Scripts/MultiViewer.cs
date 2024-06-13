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

    private GameObject selectedGameObject = null;
   
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

        parentObjects = this.transform.Find("objects").gameObject;

        parentCopy = Instantiate(parent);
        parentCopy.transform.parent = transform;
        parentCopy.name = "parentCopy";

        GameObject parentAvatar = parent.transform.Find("Avatar").gameObject;
        parentAvatar.SetActive(false);

        GameObject copy = Instantiate(parentObjects); //To be cleaned up!
        copy.transform.parent = parentCopy.transform;
        copy.name = parentCopy.name + "objects";

        foreach(Transform t in copy.transform){
            t.name = "parentCopy" + t.name;
        }
        Destroy(copy);

        disableInteraction(parent);

        parentObjects.transform.parent = parent.transform;

        children.Add(parentCopy);


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

    public void setSelectedGameObject(GameObject g){
        selectedGameObject = g;
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

     public void placeChildObjects()
    {
        GameObject parentLeftAnchor = parent.transform.Find("TopLeft").gameObject;
        GameObject parentRightAnchor = parent.transform.Find("BottomRight").gameObject;

        float parentWidth = parentRightAnchor.transform.position.x - parentLeftAnchor.transform.position.x;
        float parentHeight = parentRightAnchor.transform.position.y - parentLeftAnchor.transform.position.y;
        float parentDepth = parentRightAnchor.transform.position.z - Camera.main.transform.position.z;

        Dictionary<string, (Vector3, Quaternion)> parentObjectLocations = new Dictionary<string, (Vector3, Quaternion)>();

        foreach (Transform obj in parentObjects.transform)
        {
            Vector3 distance = obj.position - parentLeftAnchor.transform.position;
            Quaternion rotation = obj.rotation;

            parentObjectLocations.Add(obj.name, (distance, rotation));
        }

        foreach (GameObject child in children)
        {
            GameObject childLeftAnchor = child.transform.Find("TopLeft").gameObject;
            GameObject childRightAnchor = child.transform.Find("BottomRight").gameObject;

            GameObject childAvatar = child.transform.Find("Avatar").gameObject;

            float childWidth = childRightAnchor.transform.position.x - childLeftAnchor.transform.position.x;
            float childHeight = childRightAnchor.transform.position.y - childLeftAnchor.transform.position.y;
            float childDepth = childRightAnchor.transform.position.z - childAvatar.transform.position.z;

            float widthScale = childWidth / parentWidth;
            float heightScale = childHeight / parentHeight;
            float depthScale = childDepth / parentDepth;

            Transform childObjects = child.transform.Find((child.name + "objects"));

            foreach (Transform obj in childObjects)
            {
                var parentObject = parentObjectLocations[obj.name.Replace(child.name, "")];

                Vector3 distance = parentObject.Item1;

                // Debug.Log("Distance: " + obj.name + " " + distance);

                obj.position = new Vector3(
                    childLeftAnchor.transform.position.x + distance.x * widthScale,
                    childLeftAnchor.transform.position.y + distance.y * heightScale,
                    childLeftAnchor.transform.position.z + distance.z * depthScale
                );

                obj.rotation = parentObject.Item2;
            }
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
        // children.Remove(parentCopy);
        // Destroy(parentCopy);

        // parentObjects.transform.parent = this.transform;
        
        // foreach(GameObject child in children){
        //     Destroy(child.GetComponent(child.name + "objects"));
        //     ResetObjectState(child);
        // }

        // ResetObjectState(parent);
        // children.Add(parent);

        // parent = selectedGameObject; //Add Check
        
        // setEnv();

        // placeChildren();
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

    public void moveObject(bool dir){
        if(dir){
            print(selectedGameObject.transform.position);
            selectedGameObject.transform.position = new Vector3(100, 100, 10f);
            print(selectedGameObject.transform.position);
        }
        else{
            selectedGameObject.transform.position = new Vector3(10990, 10000, -10f);
        }
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

    // private void ResetRotation(GameObject obg)

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
        // if(OVRInput.Get(OVRInput.RawButton.X)){
        //     if(selectedGameObject != null){
        //         changeParent();
        //     }
        // }

        if(OVRInput.Get(OVRInput.RawButton.LThumbstickUp)){
            GameObject test = this.transform.Find("StudyRoom").gameObject;
            selectedGameObject.transform.position += new Vector3(0, 0, 0.2f);
            if(selectedGameObject != null){
                moveObject(true);
            }
        }

        if(OVRInput.Get(OVRInput.RawButton.LThumbstickDown)){
            if(selectedGameObject != null){
                moveObject(false);
            }
        }

        placeChildObjects();
    }

}

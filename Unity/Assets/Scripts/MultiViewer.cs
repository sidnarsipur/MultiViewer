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
   
    private Dictionary<string, ObjectState> originalStates = new Dictionary<string, ObjectState>();

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
    
    public void placeChildren()
    {
        GameObject parentLeftAnchor = parent.transform.Find("TopLeft").gameObject;
        GameObject parentRightAnchor = parent.transform.Find("BottomRight").gameObject;
        
        float minX = parentLeftAnchor.transform.position.x + 0.25f;
        float maxX = parentRightAnchor.transform.position.x;
        float width = maxX - minX;

        Camera mainCamera = Camera.main;
        
        float centerX = mainCamera.transform.position.x + 0.25f;
        float centerZ = mainCamera.transform.position.z;

        float b = 0.2f;

        float angleStep = -Mathf.PI / 8; 
        float startAngle = 2.35619f;  

        float prevX = Mathf.Max(minX, centerX - 0.5f); 

        for (int i = 0; i < children.Count; i++)
        {
            GameObject g = children[i];
            g.SetActive(true);
            
            float scale = getScale(g);
            g.transform.localScale = new Vector3(scale, scale, scale);

            float objScale = getObjectScale(g);

            if(objScale != 0){
                GameObject childObjects = g.transform.Find((g.name + "objects")).gameObject;
                
                childObjects.transform.localScale = new Vector3(objScale, objScale, objScale);
            }

            float angle = startAngle + angleStep * i;

            float prevChildWidth;
            
            if(i != 0){
                prevChildWidth = getWidth(children[i-1]);
            }
            else{
                prevChildWidth = 0;
            }

            float x = prevX + prevChildWidth;
            float z = centerZ + b * Mathf.Sin(angle);
            
            x = Mathf.Clamp(x, minX, maxX);
            
            // Vector3 objectPosition = new Vector3(x, mainCamera.transform.position.y - 0.1f,  mainCamera.transform.position.z + 0.25f);
            Vector3 objectPosition = new Vector3(x, mainCamera.transform.position.y + 0.5f,  mainCamera.transform.position.z + 0.3f);
            g.transform.position = objectPosition;

            prevX = getRightAnchor(g).transform.position.x;
        }
    }

     public void placeChildObjects()
    {
        GameObject parentLeftAnchor = parent.transform.Find("TopLeft").gameObject;
        GameObject parentRightAnchor = parent.transform.Find("BottomRight").gameObject;

        float parentWidth = parentRightAnchor.transform.position.x - parentLeftAnchor.transform.position.x;
        float parentHeight = parentRightAnchor.transform.position.y - parentLeftAnchor.transform.position.y;
        float parentDepth = parentRightAnchor.transform.position.z - Camera.main.transform.position.z; //Should be based on parent avatar, not camera. Smaller active region?

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
            if(childObjects == null){
                Debug.Log("Could not find for " + child.name);
            }

            foreach (Transform t in childObjects)
            {
                Object obj = t.gameObject.GetComponent<Object>();

                var parentObject = parentObjectLocations[t.name.Replace(child.name, "")];
                Vector3 distance = parentObject.Item1;
            
                Vector3 newPosition = new Vector3(
                    childLeftAnchor.transform.position.x + distance.x * widthScale,
                    childLeftAnchor.transform.position.y + distance.y * heightScale,
                    childLeftAnchor.transform.position.z + distance.z * depthScale
                );

                Quaternion newRotation = parentObject.Item2;

                BoxCollider boxCollider = t.gameObject.GetComponent<BoxCollider>();

                t.rotation = newRotation;

                if(!IsCollidingAtPosition(boxCollider, newPosition, newRotation, t.gameObject.name)){
                    t.position = newPosition;
                }

            }
        }
    }

    bool IsCollidingAtPosition(BoxCollider boxCollider, Vector3 position, Quaternion rotation, string name)
    {
        Vector3 boxSize = Vector3.Scale(boxCollider.size, boxCollider.transform.lossyScale) / 2;
        Collider[] hitColliders = Physics.OverlapBox(position, boxSize, rotation);

        foreach (Collider hitCollider in hitColliders)
        {
            if (hitCollider != boxCollider && hitCollider.tag == "Boundary")
            {
                return true;
            }
        }
        return false;
    }

    private float getScale(GameObject g){
         Environment e = g.GetComponent<Environment>();

          if (e != null)
            {
                return e.scale;
            }
            return 0;
    }

    private float getWidth(GameObject g){
        GameObject leftAnchor = g.transform.Find("TopLeft").gameObject;
        GameObject rightAnchor = g.transform.Find("BottomRight").gameObject;

        return (rightAnchor.transform.position.x - leftAnchor.transform.position.x);
    }

    private GameObject getRightAnchor(GameObject g){
        GameObject rightAnchor = g.transform.Find("BottomRight").gameObject;

        return rightAnchor;
    }

    private float getObjectScale(GameObject g){
         Environment e = g.GetComponent<Environment>();
         
         return e.objScale;
    }

    public void setSelectedGameObject(GameObject g){ 
        if (g != null){
        Debug.Log("Selecting Child: " + g.name);
        }
        selectedGameObject = g;
    }

    public void changeParent(GameObject newParent)
    {   
        if(string.Equals(newParent.name, "parentCopy")){
            return;
        }

        //Reset Interactions
        disableInteraction(newParent);
        enableInteraction(parent);
        
        //Reset Children
        children.Remove(newParent);
        children.Remove(parentCopy);

        Destroy(parentCopy);

        //Reset original parent's scale, objects and avatar
        float scale = getScale(parent);
        parent.transform.localScale = new Vector3(scale, scale, scale);
        children.Add(parent);

        GameObject parentAvatar = parent.transform.Find("Avatar").gameObject;
        parentAvatar.SetActive(true);

        //Reset original parent's objects
        parentObjects.name = parent.name + "objects";

        foreach(Transform obj in parentObjects.transform){
            obj.name = parent.name + obj.name;
        }

        //Create a copy of the new parent
        parentCopy = Instantiate(newParent);
        parentCopy.name = "parentCopy";
        parentCopy.transform.parent = transform;

        enableInteraction(parentCopy);

        GameObject parentCopyObjects = parentCopy.transform.Find((newParent.name + "objects")).gameObject;
        parentCopyObjects.name = "parentCopyobjects";

        foreach(Transform obj in parentCopyObjects.transform){
            obj.name = parentCopy.name + obj.name.Replace(newParent.name, "");
        }

        children.Add(parentCopy);

        //Make the new parent larger, rename its objects and disable its avatar
        ResetObjectState(newParent);

        parent = newParent;

        parentAvatar = parent.transform.Find("Avatar").gameObject;
        parentAvatar.SetActive(false);

        parentObjects = parent.transform.Find((parent.name + "objects")).gameObject;
        parentObjects.name = "objects";

        foreach(Transform obj in parentObjects.transform){
            obj.name = obj.name.Replace(parent.name, "");
        }

        Camera.main.transform.position = new Vector3(0, 0, 0); 

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
        if (!originalStates.ContainsKey(obj.name))
        {
            Vector3 originalPosition = obj.transform.position;
            Quaternion originalRotation = obj.transform.rotation;
            Vector3 originalScale = obj.transform.localScale;
        
            originalStates.Add(obj.name, new ObjectState(originalPosition, originalRotation, originalScale));
        }
    }
    
    private void ResetObjectState(GameObject obj)
    {
        if (originalStates.ContainsKey(obj.name))
        {
            ObjectState state = originalStates[obj.name];

            obj.transform.position = state.position;
            obj.transform.rotation = state.rotation;
            obj.transform.localScale = state.scale;
        }
        else{
            Debug.Log("Object not found in original states");
        }
    }

    private void moveObject(bool dir){
        if(selectedGameObject != null){
            GameObject g = selectedGameObject;
        
            disableInteraction(g);
            Rigidbody rb = g.GetComponent<Rigidbody>();

            Camera camera = Camera.main;
            Vector3 cameraDir = (camera.transform.position - g.transform.position).normalized;

            float moveStep = 0.4f;
            // Vector3 newPosition = g.transform.position + cameraDir * moveStep;

            if(dir){
                rb.MovePosition(g.transform.position + cameraDir * -moveStep);
            }
            else{
                rb.MovePosition(g.transform.position + cameraDir * moveStep);
            }

            enableInteraction(g);
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
            if(selectedGameObject != null){
                changeParent(selectedGameObject);
            }
        }

        if(OVRInput.Get(OVRInput.RawButton.LThumbstickUp) || OVRInput.Get(OVRInput.RawButton.RThumbstickUp)){
            moveObject(true);
        }

        if(OVRInput.Get(OVRInput.RawButton.LThumbstickDown) || OVRInput.Get(OVRInput.RawButton.RThumbstickDown)){
            moveObject(false);
        }

        placeChildObjects();
    }
}

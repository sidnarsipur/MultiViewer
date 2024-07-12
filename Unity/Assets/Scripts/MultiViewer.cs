using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiViewer : MonoBehaviour
{
    [Header("MultiViewer")]                
    public GameObject parent; //Parent environment
    public List<GameObject> children; //List of children environments

    public enum Scenario { Leisure, Productivity };  
    public Scenario scenario;    

    public int logNumber = 1;

    private GameObject parentObjects; //Objects in the parent environment
    private GameObject parentCopy; //Copy of the parent environment
    private Vector3 parentAvatarPosition; //Avatar in the parent environment

    private GameObject selectedGameObject = null; //Game object selected by user
   
    private Dictionary<string, ObjectState> originalStates = new Dictionary<string, ObjectState>(); //Original states of objects

    float initObjectDistance;

    private Logger logger;

    private string[] leisureObjects = {"MusicPlayer", "Messenger", "Instagram", "VideoPlayer", "News"};
    private string[] prodObjects = {"Slack", "Calendar", "Word", "MusicPlayer", "Research", "Mail"};

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

    //Clears the environment by disabling all objects and enabling interactions
    public void clearEnv()
    {
        logger.log("MultiViewer", "Clearing Environment - CLEARENV");

        foreach(GameObject g in children)
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

        logger.log("MultiViewer", "Initializing Environment - SETENV");

        logger.log("MultiViewer", "Setting Parent To " + parent.name + " - SETENV");

        Environment e = parent.GetComponent<Environment>();
        e.id = 0;

        for(int i = 0; i < children.Count; i++)
        {
            logger.log("MultiViewer", "Adding Child " + children[i].name + " - SETENV");

            Environment childEnv = children[i].GetComponent<Environment>();
            childEnv.id = i + 1;
        }

        parent.SetActive(true);

        parentObjects = this.transform.Find("objects").gameObject; 

        initObjectDistance = Vector3.Distance(parent.transform.Find("Height").transform.position, parentObjects.transform.position);

        parentCopy = Instantiate(parent);
        parentCopy.transform.parent = transform;
        parentCopy.name = "parentCopy";

        GameObject parentAvatar = parent.transform.Find("Avatar").gameObject;
        parentAvatarPosition = parentAvatar.transform.position;
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

        Camera.main.transform.position = parentAvatarPosition;
        Camera.main.transform.rotation = Quaternion.identity;

        Debug.Log("Set Env Finished");
        logger.log("MultiViewer", "Environment Setup Complete - SETENV");
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
        children.Sort((x, y) => string.Compare(getID(x).ToString(), getID(y).ToString()));

        GameObject parentLeftAnchor = parent.transform.Find("TopLeft").gameObject;
        GameObject parentRightAnchor = parent.transform.Find("BottomRight").gameObject;

        GameObject heightAnchor = parent.transform.Find("Height").gameObject;
        
        float minX = parentLeftAnchor.transform.position.x;
        float maxX = parentRightAnchor.transform.position.x;
        float width = maxX - minX;
        
        float centerX = parentAvatarPosition.x + 0.25f;
        float centerY = (parentLeftAnchor.transform.position.y - parentRightAnchor.transform.position.y) / 2;
        float centerZ = parentAvatarPosition.z;

        float b = 0.2f;

        float angleStep = -Mathf.PI / 8; 
        float startAngle = 2.35619f;  

        float prevX = Mathf.Max(minX + 0.1f, centerX - 0.75f); 

        for (int i = 0; i < children.Count; i++)
        {
            GameObject g = children[i];
            g.SetActive(true);
            
            float scale = getMiniatureScale(g);
            g.transform.localScale = new Vector3(scale, scale, scale);

            children[i].transform.Find("Wall")?.gameObject.SetActive(false);

            float angle = startAngle + angleStep * i;

            float prevChildWidth;
            
            if(i != 0){
                prevChildWidth = getWidth(children[i-1]) + 0.1f;
            }
            else{
                prevChildWidth = 0;
            }

            float x = prevX + prevChildWidth;
            float z = centerZ + b * Mathf.Sin(angle);
            
            x = Mathf.Clamp(x, minX, maxX);
            
            Vector3 objectPosition = new Vector3(x, heightAnchor.transform.position.y,  parentAvatarPosition.z + 0.3f);
            g.transform.position = objectPosition;

            prevX = getRightAnchor(g).transform.position.x;

            logger.log("MultiViewer", "Child " + g.name + " Placed at " + objectPosition + " - PLACECHILDREN");
        }
    }

     public void placeChildObjects()
    {
        GameObject parentLeftAnchor = parent.transform.Find("TopLeft").gameObject;
        GameObject parentRightAnchor = parent.transform.Find("BottomRight").gameObject;

        GameObject parentAvatar = parent.transform.Find("Avatar").gameObject;

        float parentWidth;
        float parentHeight = parentLeftAnchor.transform.position.y - parentAvatar.transform.position.y;
        float parentDepth = parentLeftAnchor.transform.position.z - parentAvatar.transform.position.z; 

        Dictionary<string, (Vector3, Quaternion)> parentObjectLocations = new Dictionary<string, (Vector3, Quaternion)>();

        foreach (Transform obj in parentObjects.transform)
        {

            logger.log("Objects", "Object " + obj.name + " is  at " + obj.position + " - PLACECHILDOBJECTS");

            Vector3 distance = new Vector3(
                obj.position.x - parentAvatar.transform.position.x,
                obj.position.y - parentRightAnchor.transform.position.y,
                obj.position.z - parentAvatar.transform.position.z
            );

            Quaternion rotation = obj.rotation;

            parentObjectLocations.Add(obj.name, (distance, rotation));

            float scaleDist = Vector3.Distance(parent.transform.Find("Height").transform.position, obj.position);

            GameObject bounds = obj.Find("bounds").gameObject;
            GameObject window = obj.Find("window").gameObject;

            Vector3 originalScale = getOriginalScale(obj.gameObject);  

            Vector3 scale = originalScale * (Mathf.Abs(scaleDist) / Mathf.Abs(initObjectDistance));

            Debug.Log("Original Scale: " + originalScale + " - New Scale: " + scale + " - Distance: " + scaleDist + " - Init Distance: " + initObjectDistance);
                
            // Apply the new scale
            bounds.transform.localScale = new Vector3(scale.x, scale.y, originalScale.z);
            window.transform.localScale = new Vector3(scale.x, scale.y, originalScale.z);
        }

        foreach (GameObject child in children)
        {

            GameObject childLeftAnchor = child.transform.Find("TopLeft").gameObject;
            GameObject childRightAnchor = child.transform.Find("BottomRight").gameObject;

            GameObject childAvatar = child.transform.Find("Avatar").gameObject;

            float childWidth;
            float childHeight = childLeftAnchor.transform.position.y - childAvatar.transform.position.y;
            float childDepth = childLeftAnchor.transform.position.z - childAvatar.transform.position.z;

            float widthScale;
            float heightScale = childHeight / parentHeight;
            float depthScale = childDepth / parentDepth;

            float minX = childLeftAnchor.transform.position.x;
            float maxX = childRightAnchor.transform.position.x;

            float minY = childRightAnchor.transform.position.y;
            float maxY = childLeftAnchor.transform.position.y;

            float maxZ = childLeftAnchor.transform.position.z;

            Transform childObjects = child.transform.Find((child.name + "objects"));
            if(childObjects == null){
                Debug.Log("Could not find objects for " + child.name);
            }

            foreach (Transform t in childObjects)
            {
                Object obj = t.gameObject.GetComponent<Object>();

                var parentObject = parentObjectLocations[t.name.Replace(child.name, "")];
                Vector3 distance = parentObject.Item1;

                if (distance.x > 0f){
                    parentWidth = parentRightAnchor.transform.position.x - parentAvatar.transform.position.x;
                    childWidth = childRightAnchor.transform.position.x - childAvatar.transform.position.x;
                }
                else{
                    parentWidth = parentAvatar.transform.position.x - parentLeftAnchor.transform.position.x;
                    childWidth = childAvatar.transform.position.x - childLeftAnchor.transform.position.x;
                }

                widthScale = childWidth / parentWidth;

                float xCoord = Mathf.Min(childAvatar.transform.position.x + distance.x * widthScale, maxX);
                float yCoord = Mathf.Min(childRightAnchor.transform.position.y + distance.y * heightScale, maxY);
                float zCoord = Mathf.Min(childAvatar.transform.position.z + distance.z * depthScale, maxZ);
            
                Vector3 newPosition = new Vector3(
                    xCoord,
                    yCoord,
                    zCoord
                );

                Quaternion newRotation = parentObject.Item2;

                BoxCollider boxCollider = t.transform.Find("bounds").gameObject.GetComponent<BoxCollider>();

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


    public void changeParent(GameObject newParent)
    {   
        if(string.Equals(newParent.name, "parentCopy")){
            Debug.Log("Environment is already the parent.");
            return;
        }

        Debug.Log("Changing parent to " + newParent.name);
        logger.log("MultiViewer", "Changing Parent To - " + newParent.name + " - CHANGEPARENT");

        //Reset Interactions
        disableInteraction(newParent);
        enableInteraction(parent);
        
        //Reset Children
        children.Remove(newParent);
        children.Remove(parentCopy);

        Destroy(parentCopy);

        //Reset original parent's scale, objects and avatar
        float scale = getMiniatureScale(parent);
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
        parent.transform.Find("Wall")?.gameObject.SetActive(true);

        Camera.main.transform.position = parentAvatar.transform.position;
        Camera.main.transform.rotation = Quaternion.identity;

        parentAvatar.SetActive(false);

        parentObjects = parent.transform.Find((parent.name + "objects")).gameObject;
        parentObjects.name = "objects";

        foreach(Transform obj in parentObjects.transform){
            obj.name = obj.name.Replace(parent.name, "");

            GameObject frame = obj.transform.Find("Frame").gameObject;
        }

        placeChildren();

        selectedGameObject = null;

        logger.log("MultiViewer", "Parent Set To - " + newParent.name + " - CHANGEPARENT");
    }

    private float getMiniatureScale(GameObject g){
         Environment e = g.GetComponent<Environment>();

          if (e != null)
            {
                return e.scale;
            }
            return 0;
    }

    private int getID(GameObject g){
        Environment e = g.GetComponent<Environment>();

        return e.id;
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

    public void setSelectedGameObject(GameObject g){ 
        if (g != null){
            Debug.Log("Selecting: " + g.name);
            logger.log("Objects", "Selected Object " + g.name + " - SETSELECTEDOBJECT");

            selectedGameObject = g;
        }
    }

    public void disableInteraction(GameObject g)
    {
        GameObject RayGrabInteraction = g.transform.Find("ISDK_RayGrabInteraction").gameObject;
        RayGrabInteraction?.SetActive(false);
    }

    public void enableInteraction(GameObject g)
     {
        GameObject RayGrabInteraction = g.transform.Find("ISDK_RayGrabInteraction").gameObject;

        if(RayGrabInteraction != null){
            RayGrabInteraction.SetActive(true);
        }
        else{
            Debug.Log("RayGrabInteraction not found for " + g.name);}
            RayGrabInteraction?.SetActive(true);
    }
    
    private void StoreOriginalState(string name, GameObject obj) 
    {
        if (!originalStates.ContainsKey(name))
        {
            Vector3 originalPosition = obj.transform.position;
            Quaternion originalRotation = obj.transform.rotation;
            Vector3 originalScale = obj.transform.localScale;
        
            originalStates.Add(name, new ObjectState(originalPosition, originalRotation, originalScale));
        }
        else{
            Debug.Log("Object original state already stored");
        }
    }

    private Vector3 getOriginalScale(GameObject obj){
        if (originalStates.ContainsKey(obj.name))
        {
            ObjectState state = originalStates[obj.name];

            return state.scale;
        }
        else{
            Debug.Log("Object original state not found");
            return new Vector3(0, 0, 0);
        }
    }
    
    private void ResetObjectState(GameObject obj)
    {
        if (originalStates.ContainsKey(obj.name))
        {
            ObjectState state = originalStates[obj.name];

            if(obj.tag == "Object"){
                GameObject bounds = obj.transform.Find("bounds").gameObject;
                GameObject window = obj.transform.Find("window").gameObject;

                bounds.transform.localScale = state.scale;
                window.transform.localScale = state.scale;
            }
            else{
                obj.transform.localScale = state.scale;
            }

            obj.transform.position = state.position;
            obj.transform.rotation = state.rotation;

            logger.log("Objects", obj.name + " position moved to " + obj.transform.position + " - RESETOBJECTSTATE");
        }
        else{
            Debug.Log("Object original state not found");
        }
    }

    private void moveObject(bool dir){
        if(selectedGameObject != null){
            GameObject g = selectedGameObject;
        
            disableInteraction(g);
            Rigidbody rb = g.GetComponent<Rigidbody>();

            Camera camera = Camera.main;
            Vector3 cameraDir = (camera.transform.position - g.transform.position).normalized;

            float moveStep = 0.03f;

            if(dir){
                rb.MovePosition(g.transform.position + cameraDir * -moveStep);
            }
            else{
                rb.MovePosition(g.transform.position + cameraDir * moveStep);
            }

            // if(g.tag == "Object"){
            //     float distance = Vector3.Distance(parent.transform.Find("Height").transform.position, g.transform.position);

            //     GameObject bounds = g.transform.Find("bounds").gameObject;
            //     GameObject window = g.transform.Find("window").gameObject;

            //     Vector3 originalScale = getOriginalScale(g);  

            //     Vector3 scale = originalScale * (Mathf.Abs(distance) / Mathf.Abs(initObjectDistance));

            //     Debug.Log("Original Scale: " + originalScale + " - New Scale: " + scale + " - Distance: " + distance + " - Init Distance: " + initObjectDistance);
                
            //     // Apply the new scale
            //     bounds.transform.localScale = new Vector3(scale.x, scale.y, originalScale.z);
            //     window.transform.localScale = new Vector3(scale.x, scale.y, originalScale.z);
            // }

            enableInteraction(g);

            logger.log("Objects", g.name + " position moved to " + g.transform.position + " - MOVEOBJECT");
        }
        else{
            Debug.Log("No object selected");
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        logger = new Logger(logNumber);

        logger.createLog("Objects");
        logger.createLog("MultiViewer");
        logger.createLog("Controller");

        logger.log("MultiViewer", "Starting MultiViewer - START");
        
        StoreOriginalState(parent.name, parent);
        
        foreach(GameObject child in children){
            StoreOriginalState(child.name, child);
        }

        setEnv();
        createChildObjects();
        placeChildren();   

        foreach(Transform t in parentObjects.transform){
            StoreOriginalState(t.gameObject.name, t.Find("bounds").gameObject);
        }   
    }

    // Update is called once per frame
    void Update()
    {
        if(OVRInput.Get(OVRInput.RawButton.X)){
            logger.log("Controller", "X Button");
            if(selectedGameObject != null){
                changeParent(selectedGameObject);
            }
        }

        if(OVRInput.Get(OVRInput.RawButton.B)){
            logger.log("Controller", "B Button");
            logger.log("MultiViewer", "Resetting Objects - RESETENV");
            foreach(Transform t in parentObjects.transform){
                ResetObjectState(t.gameObject);
            }
        }

        if(OVRInput.Get(OVRInput.RawButton.LThumbstickUp) || OVRInput.Get(OVRInput.RawButton.RThumbstickUp)){
            logger.log("Controller", "Thumbstick Up");
            moveObject(true);
        }

        if(OVRInput.Get(OVRInput.RawButton.LThumbstickDown) || OVRInput.Get(OVRInput.RawButton.RThumbstickDown)){
            logger.log("Controller", "Thumbstick Down");
            moveObject(false);
        }

        placeChildObjects();

        Vector3 leftControllerPosition = OVRInput.GetLocalControllerPosition(OVRInput.Controller.LTouch);
        Quaternion leftControllerRotation = OVRInput.GetLocalControllerRotation(OVRInput.Controller.LTouch);

        Vector3 rightControllerPosition = OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch);
        Quaternion rightControllerRotation = OVRInput.GetLocalControllerRotation(OVRInput.Controller.RTouch);

        logger.log("Controller", "Controller Position is (" + leftControllerPosition + " " + rightControllerPosition + ") - UPDATE");
        logger.log("Controller", "Controller Rotation is (" + leftControllerRotation + " " + rightControllerRotation + ") - UPDATE");
    }

    void OnApplicationQuit()
    {
        logger.log("MultiViewer", "Quitting MultiViewer - APPLICATIONQUIT");
    }
}

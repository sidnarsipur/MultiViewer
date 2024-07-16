using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiViewer : MonoBehaviour
{                
    private GameObject parent; //Parent environment
    private List<GameObject> children; //List of children environments

    public enum Environments { Bedroom, Carlson, Studyroom, Izone, Livingroom, Gamingroom };

    [Header("Environment Setup")]
    public List<Environments> environments;
 
    public enum Scenario { Leisure, Productivity };  

    [Header("Options")]
    public Scenario scenario;    
    public string logString = "1";
    public bool randomizeEnvironments = false;
    public bool randomizeObjects = false;

    private GameObject parentObjects; //Objects in the parent environment
    private GameObject parentCopy; //Copy of the parent environment
    private Vector3 parentAvatarPosition; //Avatar in the parent environment

    private GameObject selectedGameObject = null; //Game object selected by user
   
    private Dictionary<string, ObjectState> originalStates = new Dictionary<string, ObjectState>(); //Original states of objects

    private Logger logger;

    private List<string> leisureObjects = new List<string> {"MusicPlayer", "Messenger", "Instagram", "VideoPlayer", "News", "Game", "Clock", "Weather"};
    private List<string> prodObjects = new List<string> {"Slack", "Word", "Calendar", "MusicPlayer", "Research", "Mail", "Clock", "Weather"};

    private class ObjectState
    {
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;
        public Vector3 windowScale;

        public ObjectState(Vector3 pos, Quaternion rot, Vector3 scl, Vector3 winScl)
        {
            position = pos;
            rotation = rot;
            scale = scl;
            windowScale = winScl;
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

    public void setObjects()
    {
        parentObjects = this.transform.Find("objects").gameObject; 

        foreach(Transform t in parentObjects.transform){
            if(scenario == Scenario.Leisure){
                if(!leisureObjects.Contains(t.gameObject.name)){
                    t.gameObject.SetActive(false);
                    continue;
                }
            }
            else{
                if(!prodObjects.Contains(t.gameObject.name)){
                    t.gameObject.SetActive(false);
                    continue;
                }
            }
            Debug.Log("Storing original state for " + t.gameObject.name);
            StoreOriginalState(t.gameObject.name, t.gameObject);
        }

        if(randomizeObjects){
            int numObjects = 2;

            while(numObjects > 0){
                int index = Random.Range(0, 13);
                Transform t = parentObjects.transform.GetChild(index);

                if(t.gameObject.activeSelf == true){
                    t.gameObject.SetActive(false);
                    numObjects--;
                }
            }
        } 
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

        parentCopy = Instantiate(parent);
        parentCopy.transform.parent = transform;
        parentCopy.name = "parentCopy";

        GameObject parentAvatar = parent.transform.Find("Avatar").gameObject;
        parentAvatarPosition = parentAvatar.transform.position;
        parentAvatar.SetActive(false);

        GameObject copy = Instantiate(parentObjects);
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

            Transform childObjects = g.transform.Find((g.name + "objects"));

            foreach(Transform obj in childObjects){
                StoreOriginalState(obj.name, obj.gameObject);
            }
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

        Dictionary<string, (Vector3, Quaternion)> parentObjectLocations = new Dictionary<string, (Vector3, Quaternion)>(); //Location of objects in parent environment

        foreach (Transform obj in parentObjects.transform)
        {
            if(obj.gameObject.activeSelf == false){
                continue;
            }            

            logger.log("Objects", "Object " + obj.name + " is  at " + obj.position + " - PLACECHILDOBJECTS");

            GameObject window = obj.Find("window").gameObject;
            
            Vector3 distance = new Vector3(
                    obj.position.x - parentAvatar.transform.position.x,
                    obj.position.y - parentRightAnchor.transform.position.y,
                    obj.position.z - parentAvatar.transform.position.z
                );

            Quaternion rotation = obj.rotation;
           
            parentObjectLocations.Add(obj.name, (distance, rotation));

            float scaleDist = Vector3.Distance(parent.transform.Find("Height").transform.position, obj.position);
        
            Vector3 windowScale = getWindowScale(obj.gameObject);
            Vector3 updatedWindowScale;
            
            float initObjectDistance = Mathf.Abs(getOriginalDistance(obj.gameObject));
            float ratio = Mathf.Abs(scaleDist) / initObjectDistance;

            if(isWidget(obj.gameObject)){
                updatedWindowScale = new Vector3 (
                    Mathf.Max(windowScale.x * ratio * 0.5f, windowScale.x), 
                    Mathf.Max(windowScale.y * ratio * 0.5f, windowScale.y),
                    Mathf.Max(windowScale.z * ratio * 0.5f, windowScale.z)
                    );
            }
            else{
                GameObject bounds = obj.Find("bounds").gameObject;
                Vector3 boundScale = getOriginalScale(obj.gameObject);

                Vector3 updatedBoundScale = new Vector3 (
                    Mathf.Max(boundScale.x * ratio * 0.75f, boundScale.x), 
                    Mathf.Max(boundScale.y * ratio * 0.75f, boundScale.y),
                    boundScale.z
                    );

                updatedWindowScale = new Vector3(
                    Mathf.Max(windowScale.x * ratio * 0.75f, windowScale.x), 
                    Mathf.Max(windowScale.y * ratio * 0.75f, windowScale.y),
                    windowScale.z
                    ); 
                
                bounds.transform.localScale = updatedBoundScale;
            }
                
            window.transform.localScale = updatedWindowScale;
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
                if(t.gameObject.activeSelf == false){
                    continue;
                }

                Object obj = t.gameObject.GetComponent<Object>();

                string parentObjectName = t.name.Replace(child.name, "");
                
                var parentObjectLocation = parentObjectLocations[parentObjectName];
                Vector3 distance = parentObjectLocation.Item1;

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

                Quaternion newRotation = parentObjectLocation.Item2;

                GameObject parentObject = parentObjects.transform.Find(parentObjectName).gameObject;

                GameObject window = t.Find("window").gameObject;

                Vector3 windowScale = getWindowScale(parentObject);
                Vector3 updatedWindowScale;

                float originalDistance = Mathf.Abs(getOriginalDistance(parentObject));
                float miniDistance = Mathf.Abs(Vector3.Distance(child.transform.Find("Height").transform.position, getOriginalPosition(t.gameObject)));

                float scaleDist = Mathf.Abs(Vector3.Distance(child.transform.Find("Height").transform.position, t.position));

                float ratio = scaleDist / miniDistance;

                Debug.Log("Ratio for " + t.gameObject.name + " is " + ratio);

                BoxCollider boxCollider;

                if(isWidget(t.gameObject)){
                    boxCollider = window.GetComponent<BoxCollider>();

                    updatedWindowScale = new Vector3 (
                        Mathf.Max(windowScale.x * ratio * 0.5f, windowScale.x), 
                        Mathf.Max(windowScale.y * ratio * 0.5f, windowScale.y),
                        Mathf.Max(windowScale.z * ratio * 0.5f, windowScale.z)
                    );
                }
                else{
                    boxCollider = t.transform.Find("bounds").gameObject.GetComponent<BoxCollider>();

                    GameObject bounds = t.transform.Find("bounds").gameObject;
                    Vector3 boundScale = bounds.transform.localScale;

                    Vector3 updatedBoundScale = new Vector3 (
                        Mathf.Max(boundScale.x * ratio * 0.75f, boundScale.x), 
                        Mathf.Max(boundScale.y * ratio * 0.75f, boundScale.y),
                        boundScale.z
                    );

                    updatedWindowScale = new Vector3(
                        Mathf.Max(windowScale.x * ratio * 0.75f, windowScale.x), 
                        Mathf.Max(windowScale.y * ratio * 0.75f, windowScale.y),
                        windowScale.z
                    );

                    bounds.transform.localScale = updatedBoundScale;
                }

                window.transform.localScale = updatedWindowScale;

                if(!IsCollidingAtPosition(boxCollider, newPosition, newRotation, t.gameObject.name)){
                    if(isWidget(t.gameObject)){
                        window.transform.position = newPosition;
                        window.transform.rotation = newRotation;
                    }
                    else{
                        t.rotation = newRotation;
                        t.position = newPosition;
                    }
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

    private bool isWidget(GameObject g){
        GameObject window = g.transform.Find("window").gameObject;

        if(window.tag == "Widget"){
            return true;
        }
        return false;
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

            Vector3 originalScale;
            Vector3 windowScale;

            if(obj.tag == "Object"){
                if(!isWidget(obj)){
                  originalScale = obj.transform.Find("bounds").transform.localScale;
                }
                else{
                    originalScale = obj.transform.Find("window").transform.localScale;
                }
                windowScale = obj.transform.Find("window").transform.localScale;
            }
            else
            {
                originalScale = obj.transform.localScale;
                windowScale = new Vector3(0, 0, 0);
            }
        
            originalStates.Add(name, new ObjectState(originalPosition, originalRotation, originalScale, windowScale));
        }
        else{
            Debug.Log("Object original state already stored");
        }
    }

    private Vector3 getOriginalPosition(GameObject obj){
        if (originalStates.ContainsKey(obj.name))
        {
            ObjectState state = originalStates[obj.name];

            return state.position;
        }
        else{
            Debug.Log("Object original state not found for " + obj.name);
            return new Vector3(0, 0, 0);
        }
    }

    private Vector3 getOriginalScale(GameObject obj){
        if (originalStates.ContainsKey(obj.name))
        {
            ObjectState state = originalStates[obj.name];

            return state.scale;
        }
        else{
            Debug.Log("Object original state not found for " + obj.name);
            return new Vector3(0, 0, 0);
        }
    }

    private Vector3 getWindowScale(GameObject obj){
        if (originalStates.ContainsKey(obj.name))
        {
            ObjectState state = originalStates[obj.name];

            return state.windowScale;
        }
        else{
            Debug.Log("Object original state not found for " + obj.name);
            return new Vector3(0, 0, 0);
        }
    }

    private float getOriginalDistance(GameObject obj){
        if (originalStates.ContainsKey(obj.name))
        {
            ObjectState state = originalStates[obj.name];

            return Vector3.Distance(state.position, parent.transform.Find("Height").transform.position);
        }
        else{
            Debug.Log("Object original state not found");
            return 0;
        }
    }
    
    private void ResetObjectState(GameObject obj)
    {
        if (originalStates.ContainsKey(obj.name))
        {
            ObjectState state = originalStates[obj.name];

            if(obj.tag == "Object"){
                if(!isWidget(obj)){
                    GameObject bounds = obj.transform.Find("bounds").gameObject;
                    bounds.transform.localScale = state.scale;
                }
                GameObject window = obj.transform.Find("window").gameObject;
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
            Debug.Log("Object original state not found for " + obj.name);
        }
    }

    private void moveObject(bool dir){
        if(selectedGameObject != null && selectedGameObject.tag == "Object"){
            GameObject g = selectedGameObject;
        
            disableInteraction(g);
            Rigidbody rb = g.GetComponent<Rigidbody>();

            Camera camera = Camera.main;
            Vector3 cameraDir = (camera.transform.position - g.transform.position).normalized;

            float moveStep = dir ? 0.03f : -0.03f;
            
            if(!isWidget(g)){
                rb.MovePosition(g.transform.position + cameraDir * -moveStep);
            }
            else{
                GameObject window = g.transform.Find("window").gameObject;
                window.transform.position = new Vector3(window.transform.position.x, window.transform.position.y, window.transform.position.z + cameraDir.z * -moveStep);
            }
           
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
        parent = this.transform.Find(environments[0].ToString()).gameObject;
        children = new List<GameObject>();


        if(!randomizeEnvironments){
            children = new List<GameObject>();

            for(int i = 1; i < 4; i++){
                children.Add(this.transform.Find(environments[i].ToString()).gameObject);
            }
        }
        else{
            int count = 0;

            while(count < 3){
                int index = Random.Range(1, 6);
                GameObject child = this.transform.Find(environments[index].ToString()).gameObject;

                if(!children.Contains(child)){
                    children.Add(child);
                    count++;
                }
            }
        }
        

        logger = new Logger(logString);

        logger.createLog("Objects");
        logger.createLog("MultiViewer");
        logger.createLog("Controller");

        logger.log("MultiViewer", "Starting MultiViewer - START");
        
        StoreOriginalState(parent.name, parent);
        
        foreach(GameObject child in children){
            StoreOriginalState(child.name, child);
        }

        setObjects();
        setEnv();

        createChildObjects(); 
        placeChildren();
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

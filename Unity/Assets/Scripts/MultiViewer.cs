using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiViewer : MonoBehaviour
{
    public List<Transform> m_environments;
    public enum Environment {Bedroom, Carlson, CoffeeShop, Izone}

    [Header("Environments")]
    public Environment parent;
    public Environment childOne;
    public Environment childTwo;
    public Environment childThree;

    public void clearEnv()
    {
        // foreach (Transform env in m_environments)
        // {
        //     env.gameObject.SetActive(false);
        // }
    }

    public void setParent()
    {
        clearEnv(); 

        GameObject parentEnv = m_environments[(int)parent].gameObject;
        parentEnv.SetActive(true);
    }

    // Start is called before the first frame update
    void Start()
    {
        setParent();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvStack : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        int level = 0;

        foreach (Transform chield in transform)
        {
            chield.position = new Vector3(chield.position.x, level * 30, chield.position.z);
            level++;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

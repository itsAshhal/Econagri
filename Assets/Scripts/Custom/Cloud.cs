using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cloud : MonoBehaviour
{
    
    public float CloudChangeInPosition = 2f;
    private void Start()
    {
        //Destroy(this.gameObject, DestroyAfter);
    }

    private void Update()
    {
        // now keep this cloud moving from right to left
        transform.position = new Vector3(transform.position.x + CloudChangeInPosition,
            transform.position.y, transform.position.z
            );
    }


}

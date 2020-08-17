using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{

    public float speed = 1;
    public static GameObject Target;
    private Camera cam;
    public Transform camTransform;
    // Start is called before the first frame update
    void Start()
    {
        camTransform = transform;
        cam = Camera.main;
        Target = this.gameObject;
    }

    
    void Update()
    {
        Move();
    }

    public void Move()
    {

        transform.rotation = Quaternion.Slerp(transform.rotation, Target.transform.rotation, Time.deltaTime*2);

        transform.position = Vector3.Slerp(transform.position, Target.transform.position, Time.deltaTime*2);
    }
}

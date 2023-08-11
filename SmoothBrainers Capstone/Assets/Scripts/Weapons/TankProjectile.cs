using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankProjectile : BaseProjectile
{
    Rigidbody rb;
    public GameObject entity;
    Vector3 targetPos;

    public float h = 25;
    public float gravity = -18;

    private bool launched = false;


    // Start is called before the first frame update
    protected override void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;

        //entity = GameObject.Find("Tank");

        base.Start();
        
    }

    // Update is called once per frame
    void Update()
    {
        if(entity.GetComponent<Entity>().target)
        {
            targetPos = entity.GetComponent<Entity>().target.transform.position;
        }
        if (!launched && targetPos!= null)
        {
            Launch();
            launched = true;
        }
    }

    private void Launch()
    {
        Physics.gravity = Vector3.up * gravity;
        rb.useGravity = true;
        rb.velocity = CalculateLaunchVelocity();
        Debug.Log(CalculateLaunchVelocity());
    }

    private Vector3 CalculateLaunchVelocity()
    {
        float displacementY = targetPos.y - rb.position.y;
        Vector3 displaycementXZ = new Vector3(targetPos.x - rb.position.x, 0, 
            targetPos.z - rb.position.z);

        Vector3 velocityY = Vector3.up * Mathf.Sqrt(-2 * gravity * h);
        Vector3 velocityXZ = displaycementXZ / (Mathf.Sqrt(-2 * h / gravity)
            + Mathf.Sqrt(2 * (displacementY - h) / gravity));

        return velocityXZ + velocityY;
    }
}


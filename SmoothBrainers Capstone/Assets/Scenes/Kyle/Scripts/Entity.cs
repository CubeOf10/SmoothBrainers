using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
    //Rigidbody rb;

    // Entity Health
    public float health = 100.0f;
    public GameObject deathEffect;
    public GameObject deathSound;

    // Movement and Rotation
    public float speed = 50.0f;
    private float rotationSpeed = 5.0f;
    private float adjRotSpeed;
    private Quaternion targetRotation;
    public GameObject target;
    public float targetRadius = 200.0f;
    public float acceleration = 10.0f;
    public float fuel = 50.0f;

    public float attackRange = 20.0f;


    //// Entity FSM Enumerator
    //public enum EntityBehaviours
    //{
    //    Idle,
    //    Scouting,
    //    Attacking,
    //    Fleeing
    //}

    //public EntityBehaviours entityBehaviour;


    // Start is called before the first frame update
    void Start()
    {
        //rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        //// Entity Behaviours - State Switching
        //switch (entityBehaviour)
        //{
        //    case EntityBehaviours.Idle:
        //        Idle();
        //        break;
        //    case EntityBehaviours.Scouting:
        //        Scouting();
        //        break;
        //    case EntityBehaviours.Attacking:
        //        Attacking();
        //        break;
        //    case EntityBehaviours.Fleeing:
        //        Fleeing();
        //        break;
        //}
    }

    public void MoveTowardsTarget(Vector3 targetPos, Rigidbody rb)
    {
        Debug.Log("Moving");
        //Rotate and move towards target if out of range
        if (Vector3.Distance(targetPos, transform.position) > targetRadius)
        {
            //Lerp Towards target
            targetRotation = Quaternion.LookRotation(targetPos - transform.position);
            adjRotSpeed = Mathf.Min(rotationSpeed * Time.deltaTime, 1);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, adjRotSpeed);

            rb.AddRelativeForce(Vector3.forward * speed * 20 * Time.deltaTime);
        }
    }

    //private void Idle()
    //{
    //    Debug.Log("Idle");
    //    target = testTarget;
    //    MoveTowardsTarget(target.transform.position);
    //    Debug.DrawLine(transform.position, target.transform.position, Color.white);
    //}

    //private void Scouting()
    //{
    //    Debug.Log("Scouting");
    //}

    //private void Attacking()
    //{
    //    Debug.Log("Attacking");
    //}

    //private void Fleeing()
    //{
    //    Debug.Log("Fleeing");
    //}
}

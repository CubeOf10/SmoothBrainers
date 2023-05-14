using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tank : Entity
{
    Rigidbody rb;

    // Tank FSM Enumerator
    public enum TankBehaviours
    {
        Idle,
        Scouting,
        Attacking,
        Fleeing
    }

    public TankBehaviours tankBehaviours;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        // Entity Behaviours - State Switching
        switch (tankBehaviours)
        {
            case TankBehaviours.Idle:
                Idle();
                break;
            case TankBehaviours.Scouting:
                Scouting();
                break;
            case TankBehaviours.Attacking:
                Attacking();
                break;
            case TankBehaviours.Fleeing:
                Fleeing();
                break;
        }
    }

    private void Idle()
    {
        if (Vector3.Distance(transform.position, target.transform.position) > 1)
        {
            Debug.Log("Idle");
            MoveTowardsTarget(target.transform.position, rb);
            Debug.DrawLine(transform.position, target.transform.position, Color.green);
        }
    }

    private void Scouting()
    {
        Debug.Log("Scouting");
    }

    private void Attacking()
    {
        Debug.Log("Attacking");
    }

    private void Fleeing()
    {
        Debug.Log("Fleeing");
    }
}

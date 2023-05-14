using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Helicopter : Entity
{
    Rigidbody rb;

    // Helicopter FSM Enumerator
    public enum HelicopterBehaviours
    {
        Idle,
        Scouting,
        Attacking,
        Fleeing
    }

    public HelicopterBehaviours helicopterBehaviour;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        // Entity Behaviours - State Switching
        switch (helicopterBehaviour)
        {
            case HelicopterBehaviours.Idle:
                Idle();
                break;
            case HelicopterBehaviours.Scouting:
                Scouting();
                break;
            case HelicopterBehaviours.Attacking:
                Attacking();
                break;
            case HelicopterBehaviours.Fleeing:
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
            Debug.DrawLine(transform.position, target.transform.position, Color.blue);
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

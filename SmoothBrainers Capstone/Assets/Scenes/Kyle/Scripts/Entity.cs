using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Entity : MonoBehaviour
{
    Rigidbody rb;
    NavMeshAgent navMesh;

    // Entity Health
    public float health = 100.0f;
    public GameObject deathEffect;
    public GameObject deathSound;

    // Movement
    public GameObject target;
    public float fuel = 50.0f;

    public float attackRange = 20.0f;


    // Entity FSM Enumerator
    public enum EntityBehaviours
    {
        Idle,
        Attacking,
        Fleeing
    }

    public EntityBehaviours entityBehaviour;


    // Start is called before the first frame update
    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody>();
        navMesh = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        // Entity Behaviours - State Switching
        switch (entityBehaviour)
        {
            case EntityBehaviours.Idle:
                Idle();
                break;
            case EntityBehaviours.Attacking:
                Attacking();
                break;
            case EntityBehaviours.Fleeing:
                Fleeing();
                break;
        }
    }

    protected virtual void Idle()
    {
        Debug.Log("Idle");
        if (Vector3.Distance(transform.position, target.transform.position) > 1)
        {
            Debug.Log("Idle");
            navMesh.destination = target.transform.position;
            Debug.DrawLine(transform.position, target.transform.position, Color.red);
        }
    }

    // User chooses their entity -> chooses an enemy entity to attack -> move towards
    // -> once in attackRange it will attack -> once enemy entity destroyed it will be Idle
    protected virtual void Attacking()
    {
        Debug.Log("Attacking");
        // Gets transform.position of enemy entity
        // If in attackRange
        // Attack
        // Else
        // Move towards enemy entity's position

    }

    // User chooses their entity -> chooses somewhere on the terrain -> move towards
    // -> once reached destination -> change behaviour to Idle
    protected virtual void Fleeing()
    {
        Debug.Log("Fleeing");
        // Gets location where user selects and applies navMesh.destination = target.transform.position;
        // If not reached destination
            // Move towards position
        // Else
            // Change to Idle
    }
}

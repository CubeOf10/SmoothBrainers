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
    public bool inRange = false;


    // Entity FSM Enumerator
    public enum EntityBehaviours
    {
        Idle,
        Attacking,
        Relocating
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
            case EntityBehaviours.Relocating:
                Relocating();
                break;
        }
    }

    protected virtual void Idle()
    {

    }

    // User chooses their entity -> chooses an enemy entity to attack -> move towards
    // -> once in attackRange it will attack -> once enemy entity destroyed it will be Idle
    protected virtual void Attacking()
    {
        // Gets transform.position of enemy entity
        // If not in attackRange
            // Move towards enemy entity's position
        // Else
            // Attack

        
        // Move towards target until in range to attack
        if (Vector3.Distance(transform.position, target.transform.position) > attackRange)
        {
            Debug.Log("Moving");
            navMesh.destination = target.transform.position;
            Debug.DrawLine(transform.position, target.transform.position, Color.red);
        }

        // In attacking range
        else
        {
            Debug.Log("In Range");
            navMesh.speed = 0;
            inRange = true;
        }
    }

    // User chooses their entity -> chooses somewhere on the terrain -> move towards
    // -> once reached destination -> change behaviour to Idle
    protected virtual void Relocating()
    {
        // Gets location where user selects and applies navMesh.destination;
        // If not reached destination
            // Move towards position
        // Else
            // Change to Idle

        
        // Move towards destination
        if (Vector3.Distance(transform.position, target.transform.position) > 1)
        {
            navMesh.destination = target.transform.position;
            Debug.DrawLine(transform.position, target.transform.position, Color.yellow);
        }

        // Change behaviour to Idle
        else
            entityBehaviour = EntityBehaviours.Idle;
    }
}

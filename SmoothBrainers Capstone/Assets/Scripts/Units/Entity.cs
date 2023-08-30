using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Entity : MonoBehaviour
{
    GameManager gameManager;
    Rigidbody rb;
    NavMeshAgent navMesh;

    // Entity Health
    public float health = 100.0f;
    float maxHealth;
    public GameObject deathEffect;
    public GameObject deathSound;

    // Movement
    public GameObject target;
    public float fuel = 50.0f;

    public float attackRange = 20.0f;

    [Header("Projectiles")]
    public GameObject projectilePrefab;
    public GameObject projectileSpawnPos;
    public float projectileAmount = 10;
    float projectileMaxAmount;
    public float projectileFireRate = 7f;
    private float projectileFireTime;
    public float projectileRegenRate = 5.0f;
    //private float projectileRegenTimer;
    //public float projectileMaxAmount = 10;

    UnitDisplayController unitDisplay;
    public Canvas unitDisplayCanvas;
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
        maxHealth = health;
        projectileMaxAmount = projectileAmount;

        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

        target = gameManager.targets[0];
        rb = GetComponent<Rigidbody>();
        navMesh = GetComponent<NavMeshAgent>();

        unitDisplay = unitDisplayCanvas.GetComponent<UnitDisplayController>();
        unitDisplay.setAction(EntityBehaviours.Idle.ToString());
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

        unitDisplay.transform.LookAt(gameManager.Player.transform.position);
        transform.Rotate(0, 180, 0);
        unitDisplay.setAcceleration(navMesh.acceleration);
    }

    protected virtual void Idle()
    {
        unitDisplay.setAction(EntityBehaviours.Idle.ToString());
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
        if(target)
        {

            unitDisplay.setAction(EntityBehaviours.Attacking.ToString());

            if (Vector3.Distance(transform.position, target.transform.position) > attackRange)
            {
                if(navMesh.isOnNavMesh)
                {
                    navMesh.destination = target.transform.position;
                    Debug.DrawLine(transform.position, target.transform.position, Color.red);
                }
            }

            // In attacking range
            else
            {
                //navMesh.speed = 0;
                navMesh.destination = transform.position;

                //Fire Missile
                if (Time.time > projectileFireTime && projectileAmount > 0)
                {
                    GameObject newProjectile = Instantiate(projectilePrefab, projectileSpawnPos.transform.position, projectileSpawnPos.transform.rotation);
                    newProjectile.GetComponent<TankProjectile>().entity = gameObject;
                    projectileAmount--;
                    projectileFireTime = Time.time + projectileFireRate;

                    unitDisplay.setAmmo(projectileAmount, projectileMaxAmount);
                    unitDisplay.setFirepower(projectileAmount * projectilePrefab.GetComponent<BaseProjectile>().damage);
                }

                ////Regenerate Missiles
                //if (Time.time > projectileRegenTimer && projectileAmount < projectileMaxAmount)
                //{
                //    projectileAmount++;
                //    projectileRegenTimer = Time.time + projectileRegenRate;
                //}
            }
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
            unitDisplay.setAction(EntityBehaviours.Relocating.ToString());

            navMesh.destination = target.transform.position;
            Debug.DrawLine(transform.position, target.transform.position, Color.yellow);
        }

        // Change behaviour to Idle
        else
            entityBehaviour = EntityBehaviours.Idle;
    }

    protected virtual void TakeDamage(float incomingDamage)
    {
        // Reduces entity health by incoming damage value
        // If no health remains, remove entity

        health -= incomingDamage;
        if(health <= 0)
        {
            Instantiate(deathEffect);
            Instantiate(deathSound);
            Destroy(gameObject);
        } 
        
        unitDisplay.setHealth(health / maxHealth);
    }
}

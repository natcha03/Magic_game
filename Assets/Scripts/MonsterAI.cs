using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MonsterAI : MonoBehaviour
{
    ///change every level
    [SerializeField] float RunAwayHealthLimit = 1000f;

    public bool BehindExitDoor = false;

    [SerializeField] public LayerMask groundMask;




    public Animator animator;
    public bool isDead = false;
    public float lastDidSomething = 0;
    public float pauseTime = 3f;

    public UnityEngine.AI.NavMeshAgent agent;

    public Transform player;

    public LayerMask whatIsGround, whatIsPlayer;

    public float health;

    //Patroling
    public Vector3 walkPoint;
    bool walkPointSet;
    public float walkPointRange;

    //Attacking
    public float timeBetweenAttacks;
    bool alreadyAttacked;
    //public GameObject projectile;

    //States
    public float sightRange, attackRange;
    public bool playerInSightRange, playerInAttackRange;

    //check movement
    private float prev_movement;

    private void Awake()
    {
        player = GameObject.Find("mainCharacter").transform;
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        animator.SetBool("die",false);
        animator.SetBool("isWalking",false);
        animator.SetBool("isRunning",false);
        prev_movement = transform.position.x + transform.position.z;

    }

    private void Update()
    {
        if (isDead) {return;}
        if (health <= RunAwayHealthLimit) {runAway();}
        if (Time.time < lastDidSomething + pauseTime) return; 
        //Check for sight and attack range
        playerInSightRange = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer);
        playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer);


        if (!playerInSightRange && !playerInAttackRange)  Patroling();
        if (playerInSightRange && !playerInAttackRange) ChasePlayer();
        if (playerInAttackRange && playerInSightRange) AttackPlayer();
    }

    private void runAway() //run away at the end of each level
    {
        animator.SetBool("isRunning",true);
        //not yet complimented on where to head to 
            //agent.SetDestination(ExitToNextLevel.position);
        
    }


    private void Patroling()
    {

        if (isDead) {return;}
        else if (prev_movement == transform.position.x + transform.position.z) {
             animator.SetBool("isWalking",false);
        } else {
            animator.SetBool("isWalking",true);
        }
        prev_movement = transform.position.x + transform.position.z;
        
        if (!walkPointSet) SearchWalkPoint();

        if (walkPointSet) agent.SetDestination(walkPoint);

        Vector3 distanceToWalkPoint = transform.position - walkPoint;

        //Walkpoint reached
        if (distanceToWalkPoint.magnitude < 1f) walkPointSet = false;

        lastDidSomething  = Time.time; 
        
    }

    private void SearchWalkPoint()
    {
        //Calculate random point in range
        float randomZ = Random.Range(-walkPointRange, walkPointRange);
        float randomX = Random.Range(-walkPointRange, walkPointRange);

        walkPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

        if (Physics.Raycast(walkPoint, -transform.up, 2f, whatIsGround))
            walkPointSet = true;
    }

    private void ChasePlayer()
    {
        if (isDead) {return;}
        else if (prev_movement == transform.position.x+ transform.position.z) {
             animator.SetBool("isWalking",false);
        } else {
            animator.SetBool("isWalking",true);
        }
        prev_movement = transform.position.x+ transform.position.z;
        
        agent.SetDestination(player.position);

        lastDidSomething  = Time.time; 
        
    }

    private void AttackPlayer()
    {
        if (isDead) {return;}
        else if (prev_movement == transform.position.x+ transform.position.z) {
             animator.SetBool("isWalking",false);
        } else {
            animator.SetBool("isWalking",true);
        }
        prev_movement = transform.position.x+ transform.position.z;

        //Make sure enemy doesn't move
        agent.SetDestination(transform.position);

        transform.LookAt(player);
       
        if (!alreadyAttacked)
        {
            ///Attack code here
            AttackRandomly();
            ///End of attack code

            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }
        lastDidSomething  = Time.time; 
    }
    private void ResetAttack()
    {
        alreadyAttacked = false;
    }

    public void TakeDamage(int damage)
    {
        if (isDead) {return;}

        health -= damage;

        if (health <= 0) isDead = true; 
        animator.SetBool("die",true);
    }

    private void AttackRandomly()
    {
        int randomNumber = Random.Range(0,2);

        if (randomNumber == 0) { animator.SetTrigger("punch");}
        else if (randomNumber ==1) {animator.SetTrigger("pound");}
        else {animator.SetTrigger("punch");}
    }


}

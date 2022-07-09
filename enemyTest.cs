using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class enemyTest : MonoBehaviour
{

    public float idleStateTimer = 5;   
    public float idleStateTimerCount;
    bool runIdleTimer = false;    

    public Transform[] waypoints;
    public int FirstWayPoint = 0;
    Vector3 NextPos;
    bool StartPatrolMoving = false;

    public float MaxSearchDistance = 5;

    public float searchTimer = 5;
    public float searchTimerCount;
    bool runSearchTimer = false; 

    public float speed;
    public float rotationSpeed;
    public float searchSpeed;

    Transform player;
    public bool CanSeePlayer = false;
    bool startChasing = false;
    bool startSearching = false;    
    bool startReturing = false;

    public PolygonCollider2D pc2d;
    public bool isSearching = false;


    public bool canShoot = false;
    public GameObject bullet;
    public GameObject firePos;
    public float bulletSpeed;
    public Color[] colors;
    public LayerMask lm;

    public float shootTimer = 2;
    public float shootTimerCounter = 0;

    public string gameState;


    void Start()
    { 
      
        player = GameObject.Find("Player").GetComponent<Transform>();  // get the player object 
        idleState();  // call for idle state 
    }

    // Update is called once per frame
    void Update()
    {

        CheckCanPlayerShoot();

        if(runIdleTimer){   // idle state 
            if(idleStateTimerCount > 0){  // if the counter is greater then 0 then starting counting it down
                idleStateTimerCount -= Time.deltaTime;
            }  
            if(idleStateTimerCount <= 0){
                runIdleTimer = false;  // set the boolean to false so that the statement stops
                PatrolState();  // if time is over then we go to patrol state
            }
            if(CanSeePlayer){
                runIdleTimer = false;  // set the boolean to false so that the statement stops
                chaseState();  // if we see the player then go to chase state
            }
        }

        if(StartPatrolMoving){  // patrol
            transform.up = Vector3.Lerp(transform.up, (NextPos - transform.position), rotationSpeed);   // turn towards the next position
            transform.position = Vector3.MoveTowards(transform.position, NextPos, speed * Time.deltaTime); // move to the next position 
            if(transform.position == NextPos){  // if we reach the next position
                FirstWayPoint += 1;
                if(FirstWayPoint > 2){   // we chagne the next position and 
                    FirstWayPoint = 0;
                }
                idleState();  // go to idle state
            }
            if(CanSeePlayer){
                chaseState();  // if we can see the player go to chase state
            }
        }

        if(startChasing){  // chase 
            if(CanSeePlayer){  // if we still see the player then 
                transform.up = Vector3.Lerp(transform.up, (player.position - transform.position), rotationSpeed);  // turn towards the player
                transform.position = Vector3.MoveTowards(transform.position, player.position, speed * Time.deltaTime); // move towards the player
            }
            else{ // if we did not see the player
                searchState();  // go to search state
            }
          
        }

        if(startSearching){  // search
            if(searchTimerCount > 0){  // if timer is greater then 0 then start counting it down
                searchTimerCount -= Time.deltaTime;
            } 
            if(CanSeePlayer == true){  // if we see the player
                startSearching = false; // set the boolean to false so that the statement stops
                chaseState(); // and go to the chase state
            }
            else if(searchTimerCount <= 0){  // or else if the timer ends 
                startSearching = false; // set the boolean to false so that the statement stops
                retrunState(); // and go to the return state
            }
            else if(CanSeePlayer == false){  // or else if we do not see the player and the timer is still running then
                transform.Rotate( new Vector3(0,0,-360) * searchSpeed * Time.deltaTime);  // rotate 360 degrees to search for the player
            }
            
        }

        if(startReturing == true){  // return 
            if(transform.position != NextPos && CanSeePlayer == false){  // if we are not on the next position and we cannot see the player then
                transform.up = Vector3.Lerp(transform.up, (NextPos - transform.position), rotationSpeed); // turn towards the next position
                transform.position = Vector3.MoveTowards(transform.position, NextPos, speed * Time.deltaTime); // move towards the next position
            }
            else if(transform.position == NextPos){  // if we are at the next position
                idleState(); // go to idle state
            }
            else if(CanSeePlayer){  // if we can see the player then
                startReturing = false; // stop the statement
                chaseState(); // go to chase state
            }
           
        }

        if(canShoot){  // shooting 
            if(shootTimerCounter > 0){ // if the timer is greater then 0 we start counting it down
                shootTimerCounter -= Time.deltaTime;
            }
            else if(shootTimerCounter <= 0){ // if the timer is 0 then we shoot and set the timer back to some seconds 
                int ranCol = Random.Range(0,colors.Length);
                GameObject bulletObject = Instantiate(bullet, firePos.transform.position, firePos.transform.rotation);
                bulletObject.GetComponent<SpriteRenderer>().color = colors[ranCol];
                bulletObject.GetComponent<Rigidbody2D>().AddForce(firePos.transform.up * bulletSpeed, ForceMode2D.Impulse);
                StartCoroutine(DestoryBullet(bulletObject));
                shootTimerCounter = shootTimer;
            }

        }

    }

    IEnumerator DestoryBullet(GameObject obj){  // destory bullet after some seconds
        yield return new WaitForSeconds(4);
        Destroy(obj);
    }

    void idleState(){
        Debug.Log("Idle");
        gameState = "IDLE"; // setting the game state to idle 
        idleStateTimerCount = idleStateTimer; // setting the timercount back to 5 so it can start counting again
        StartPatrolMoving = false; // stoping patrolling
        startReturing = false;
        runIdleTimer = true;    // starting idle timer  
    }

    void PatrolState(){
        Debug.Log("Patrol");
        gameState = "PATROL";
        idleStateTimerCount = idleStateTimer; // setting the timercount back to 5 so it can start counting again
        NextPos = waypoints[FirstWayPoint].position; // setting the next pos;
        StartPatrolMoving = true;  // starting the patrol
        
    }


    void chaseState(){
        Debug.Log("Chase");
        gameState = "CHASE";
        isSearching = false;
        StartPatrolMoving = false; // stoping the patrol 
        if(CanSeePlayer == false){ // if we can't see the player then
            searchState(); // go to search state
        }
        else{
            startChasing = true; // or else if we can see the player start searching
        }
    }

    void searchState(){
        Debug.Log("Search");
        gameState = "SEARCH";
        isSearching = true;
        searchTimerCount = searchTimer;
        startChasing = false;
        startSearching = true;
    }

    void retrunState(){
        Debug.Log("Return");
        gameState = "RETURN";
        isSearching = false;
        FirstWayPoint += 1;
        if(FirstWayPoint > 2){
            FirstWayPoint = 0;
        }
        NextPos = waypoints[FirstWayPoint].position;
        startReturing = true;
    }


    // player checking 

    void OnTriggerEnter2D(Collider2D col){
        if(col.gameObject.tag == "Player"){
            CanSeePlayer = true;
        }
    }

    void OnTriggerExit2D(Collider2D col){
        if(col.gameObject.tag == "Player"){
            CanSeePlayer = false;
        }
    }
    
    void CheckCanPlayerShoot(){
        RaycastHit2D hit = Physics2D.Raycast(transform.position, transform.up, MaxSearchDistance, lm);
        if(hit.collider != null){
            canShoot = true;
        }
        else {
            canShoot = false;
        }
    }

    void OnDrawGizmos(){
        Debug.DrawRay(transform.position, transform.up * MaxSearchDistance, Color.red);
    }
}

using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.AI;

public class NPCController : MonoBehaviour
{
    public TMP_Text plan;
    private Planner planner;
    private Domain domain;
    public NPCState currentState;
    private GameState gameState;
    public Animator animator;

    public NavMeshAgent agent;
    
    private List<PrimitiveTask> currentPlan;
    private int currentTaskIndex = 0;
    public GameObject currentTarget;
    public GameObject targetRock;
    
    public float replanInterval = 2f;
    private float replanTimer = 0f;
    
    void Awake()
    {
        //agent = GetComponent<NavMeshAgent>();
    }

    void Start()
    {
        gameState = FindFirstObjectByType<GameState>();
        planner = new Planner();
        
        currentState = new NPCState
        {
            position = transform.position,
            hunger = 0f,
            takenTreasure = false,
            seesPlayer = false,
            hasTarget = false,
            atTarget = false, 
            hasRock = false
        };
        
        domain = GetComponent<Domain>();
        if (domain == null)
        {
            domain = gameObject.AddComponent<Domain>();
        }
        domain.Initialize(this);
        
        domain.RegisterTasks(planner);
        
        Replan();
    }
    public void RunToTarget()
    {
        animator.SetBool("run", true);
        if (currentTarget != null)
        {

            agent.isStopped = false;
            agent.SetDestination(currentTarget.transform.position);
        }
    }
     public void MoveToTarget()
    {
        animator.SetBool("moving", true);
        if (currentTarget != null)
        {

            agent.isStopped = false;
            agent.SetDestination(currentTarget.transform.position);
        }
    }
    
    void Update()
    {

        
        UpdateState();
        
        ExecutePlan();
        
        // Only periodic replan if we have no plan
        if (currentPlan == null || currentPlan.Count == 0 || currentTaskIndex >= currentPlan.Count)
        {
            replanTimer += Time.deltaTime;
            if (replanTimer >= replanInterval)
            {
                replanTimer = 0f;
                Replan();
            }
        }
        else
        {
            replanTimer = 0f; // Reset timer while executing
        }
    }

     public void Stop()
    {
        agent.isStopped = true;
        agent.ResetPath();
        animator.SetBool("run", false);
    }
    public void killPlayer()
    {
        Debug.Log("NPC has killed the player!");
        gameState.Attacked();
    }

    public void throwRock()
    {
        if (targetRock != null)
        {
            StartCoroutine(SimpleThrowCoroutine(targetRock));
        }
    }

private IEnumerator<object> SimpleThrowCoroutine(GameObject rock)
{
    Vector3 startPos = rock.transform.position;
    float throwHeight = 3f;
    float throwSpeed = 6f;
    
    // up
    while (rock.transform.position.y < startPos.y + throwHeight)
    {
        rock.transform.Translate(Vector3.up * throwSpeed * Time.deltaTime);
        yield return null;
    }
    
    // back down
    while (rock.transform.position.y > startPos.y)
    {
        rock.transform.Translate(Vector3.down * throwSpeed * Time.deltaTime);
        yield return null;
    }
    
    
    //Destroy(rock);
    targetRock = null;
}
    void UpdateState()
    {
        // Update position
        currentState.position = transform.position;
        
        // Update hunger (increases over time)
        currentState.hunger += Time.deltaTime * 1f; 
    
        
        
    }
    
    void ExecutePlan()
    {
        // If no plan, try to replan
        if (currentPlan == null || currentPlan.Count == 0)
        {
            Replan();
            return;
        }
        
        // If finished all tasks, replan
        if (currentTaskIndex >= currentPlan.Count)
        {
            Replan();
            return;
        }
        
        // Execute current task
        PrimitiveTask currentTask = currentPlan[currentTaskIndex];
        plan.text = "Plan:\n";
        plan.text += $"{currentTask.Name}\n";
        
        
        // Execute the task (this updates the world)
        currentTask.Execute?.Invoke(currentState);
        
        // Check if task is complete
        if (IsTaskComplete(currentTask))
        {
            //Debug.Log($"Task '{currentTask.Name}' completed");
            currentTaskIndex++;
        }
    }
    
    void Replan()
    {
        
        // Get root task from domain
        ITask rootTask = domain.GetRootTask();
        
        // Generate new plan
        List<PrimitiveTask> newPlan = planner.GeneratePlan(currentState, rootTask);
        
        if (newPlan != null && newPlan.Count > 0)
        {
            currentPlan = newPlan;
            currentTaskIndex = 0;
            
            foreach (var task in newPlan)
            {
                //Debug.Log($"  - {task.Name}");
            }
        }
        else
        {
           // Debug.LogWarning("Planning failed - no valid plan found");
            currentPlan = null;
        }
    }
    
    bool IsTaskComplete(PrimitiveTask task)
    {
        // Define completion conditions for each task type
        switch (task.Name)
        {
            case "FindFood":
                // Complete when we have a target
                return currentState.hasTarget;
            
            case "MoveTo":
                // Complete when we're at the target
                return currentState.atTarget;
            
            case "EatMushroom":
                
                return true; 
           
             case "Sleep":
                return true;
            case "FindPlayer":
                return currentState.seesPlayer;
            case "Angry":
                return true;
            case "RunToPlayer":
                return currentState.atTarget;
            case "AttackPlayer":
                return true;
            case "FindRock":
                return currentState.hasRock;
            default:
                return true;
        }
    }
    
    public void OnTreasureTaken()
    {
        currentState.takenTreasure = true;
        Debug.Log("NPC noticed treasure taken!");
        Replan(); 
    }
    
    
}

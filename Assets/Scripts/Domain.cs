using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public interface ITask
{
    string Name { get; }
    bool IsValid(NPCState state);
}

// Prim 
public class PrimitiveTask : ITask
{
    public string Name { get; set; }
    public System.Func<NPCState, bool> Precondition;
    public System.Action<NPCState> Effect;
    public System.Action<NPCState> Execute;
    
    public bool IsValid(NPCState state)
    {
        return Precondition == null || Precondition(state);
    }
}

// Compound
public class CompoundTask : ITask
{
    public string Name { get; set; }
    public List<Method> Methods = new List<Method>();
    
    public bool IsValid(NPCState state)
    {
        return Methods.Any(m => m.IsValid(state));
    }
}

public class Method
{
    public string Name;
    public System.Func<NPCState, bool> Precondition;
    public List<ITask> Subtasks = new List<ITask>();
    
    public bool IsValid(NPCState state)
    {
        return Precondition == null || Precondition(state);
    }
}

// World state
public class NPCState
{
    public Vector3 position;
    public float hunger;
    public bool takenTreasure;
    public bool seesPlayer = true;
    public bool hasTarget;
    public bool atTarget;
    public bool hasRock;
    
    public NPCState Clone()
    {
        return (NPCState)this.MemberwiseClone();
    }
}

public class Domain : MonoBehaviour
{
    private NPCController npc;
    private CompoundTask hungryTask;
    private CompoundTask angryTask;
    private CompoundTask rootTask;
    
    private CompoundTask rockFight;
    public void Initialize(NPCController controller)
    {
        npc = controller;
    }
    
    public void RegisterTasks(Planner planner)
    {
        var findFood = FindFoodTask();
        var moveTo = MoveToTask();
        var eatMushroom = EatMushroomTask();
        
        var angry = AngryTask();     
        var attack = CreateAttackTask();
        var run = CreateRunTask();
        var findPlayer = CreateFindTask();

        var rockThrow = ThrowRockTask();
        var findRock = FindRock();

        var sleep = SleepTask();

        planner.RegisterPrimitiveTask(attack);
        planner.RegisterPrimitiveTask(run);
        planner.RegisterPrimitiveTask(findPlayer);

        planner.RegisterPrimitiveTask(rockThrow);
        planner.RegisterPrimitiveTask(findRock);
        rockFight = RockAttackTask(findRock, rockThrow);

        angryTask = CreateAngryTask(angry, findPlayer, run, attack, rockFight);
        
        planner.RegisterPrimitiveTask(findFood);
        planner.RegisterPrimitiveTask(moveTo);
        planner.RegisterPrimitiveTask(eatMushroom);
        planner.RegisterPrimitiveTask(sleep);
        planner.RegisterPrimitiveTask(angry);
        
        
        hungryTask = CreateHungryTask(findFood, moveTo, eatMushroom, sleep);
        planner.RegisterCompoundTask(hungryTask);
        planner.RegisterCompoundTask(angryTask);
        rootTask = CreateRootTask(angryTask, hungryTask);
        planner.RegisterCompoundTask(rootTask);

    }

     private CompoundTask CreateRootTask(CompoundTask angerTask, CompoundTask hungryTask)
    {
        var compound = new CompoundTask { Name = "Root" };
        
        compound.Methods.Add(new Method
        {
            Name = "GetAngry",
            Precondition = state => state.takenTreasure,
            Subtasks = new List<ITask> { angerTask }
        });
        
        // Method 2: If hungry, eat
        compound.Methods.Add(new Method
        {
            Name = "EatIfHungry",
            Precondition = state => state.hunger > 10 && !state.takenTreasure,
            Subtasks = new List<ITask> { hungryTask }
        });
        
        compound.Methods.Add(new Method
        {
            Name = "Idle",
            Precondition = state => true, // Always valid as fallback
            Subtasks = new List<ITask>() // Do nothing
        });
        
        return compound;
    }
    
    private CompoundTask CreateAngryTask(PrimitiveTask angry, PrimitiveTask find, PrimitiveTask run, PrimitiveTask attack, CompoundTask rockFight)
    {
        var compound = new CompoundTask { Name = "AngryBehavior" };
               
        //sees player, runs to player  if not already in range
        compound.Methods.Add(new Method
        {
                   
            Name = "RunToPlayer",
            Precondition = state => state.seesPlayer && !state.atTarget,
            Subtasks = new List<ITask> { find, run}
            
               
        });

        // in range, attack with rock if close by
        compound.Methods.Add(new Method
        {
            
            Name = "AttackPlayerWithRock",
            Precondition = state => state.seesPlayer && state.atTarget,
            Subtasks = new List<ITask> { rockFight }
                    
        });

        // In range already, no rock, just attack
        compound.Methods.Add(new Method
        {
            
            Name = "AttackPlayer",
            Precondition = state => state.seesPlayer && state.atTarget,
            Subtasks = new List<ITask> { attack }
                    
        });
         
        //just angry, cant see player
        compound.Methods.Add(new Method
        {
            Name = "AngryAtPlayer",
            Precondition = state => state.seesPlayer || state.takenTreasure,
            Subtasks = new List<ITask> { angry }
        });

 
        return compound;
    }

    private PrimitiveTask FindRock()
    {
        return new PrimitiveTask
        {
            Name = "FindRock",
            Precondition = state => true,
            Execute = state => 
            {
                GameObject rock = GameObject.FindGameObjectWithTag("boulder");
                float distance = Vector3.Distance(npc.transform.position, rock.transform.position);

                if (rock != null )//&& distance < 2.0f)
                {
                    state.hasRock = true;
                    npc.targetRock = rock;
                    Debug.Log($"Found rock at {rock.transform.position}");
                }
            },
            Effect = state => 
            {
                state.hasRock = true;
            }
        };
    }

    private PrimitiveTask ThrowRockTask()
    {
        return new PrimitiveTask
        {
            Name = "ThrowRock",
            Precondition = state => state.seesPlayer && state.atTarget && state.hasRock,
            Execute = state => 
            {
                Debug.Log("NPC is throwing a rock at the player!");
                npc.animator.SetBool("throw", true);
                npc.throwRock();
   
                AnimatorStateInfo stateInfo = npc.animator.GetCurrentAnimatorStateInfo(0);
                
                Debug.Log("Rock thrown at player!");
                npc.killPlayer();
                state.hasRock = false;
                state.takenTreasure = false;                        
                
                          
              
            },
            Effect = state => 
            {
                state.hasRock = false;
                state.takenTreasure = false;
            }
        };
    }

    private CompoundTask RockAttackTask(PrimitiveTask findRock, PrimitiveTask throwRock)
    {
        var compound = new CompoundTask { Name = "RockAttack" };
        
        compound.Methods.Add(new Method
        {
            Name = "FindRock",
            Precondition = state => state.seesPlayer && state.atTarget && !state.hasRock,
            Subtasks = new List<ITask> { findRock}
        });

        compound.Methods.Add(new Method
        {
            Name = "ThrowRock",
            Precondition = state => state.seesPlayer && state.atTarget && state.hasRock,
            Subtasks = new List<ITask> { throwRock }
        });


        return compound;
    }

    private PrimitiveTask CreateAttackTask()
    {
        return new PrimitiveTask
        {
            Name = "AttackPlayer",
            Precondition = state => state.seesPlayer && state.atTarget,
            Execute = state => 
            {
                Debug.Log("NPC is attacking the player!");
                npc.animator.SetBool("attack", true);
                AnimatorStateInfo stateInfo = npc.animator.GetCurrentAnimatorStateInfo(0);
                if (stateInfo.IsName("Attack") && stateInfo.normalizedTime >= 0.8f)
                {
                    npc.killPlayer();
                    state.takenTreasure = false;
                }
            },
            Effect = state => 
            {
                //npc.killPlayer();
                state.takenTreasure = false;

            }
        };
    }
        private PrimitiveTask CreateRunTask()
    {
        return new PrimitiveTask
        {
            Name = "RunToPlayer",
            Precondition = state => state.seesPlayer && state.takenTreasure,
            Execute = state =>
            {
                if (npc.currentTarget != null)
                {
                    npc.RunToTarget();

                    float distance = Vector3.Distance(
                        npc.transform.position,
                        npc.currentTarget.transform.position
                    );

                    state.atTarget = distance < 2f;
                }
            },
            Effect = state =>
            {
                state.atTarget = true;
            }
        };
    }

    private PrimitiveTask AngryTask()
    {
        return new PrimitiveTask
        {
            Name = "Angry",
            Precondition = state => state.seesPlayer,
            Execute = state => 
            {
                //Debug.Log("NPC is angry at the player!");
                npc.animator.SetBool("Combat Idle", true);
            },
            Effect = state => 
            {
                // No immediate effect
            }
        };
    }

    private PrimitiveTask CreateFindTask()
    {
        return new PrimitiveTask
        {
            Name = "FindPlayer",
            Precondition = state => true,
            Execute = state => 
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    state.hasTarget = true;
                    npc.currentTarget = player;
                    Debug.Log($"Found player at {player.transform.position}");
                }
            },
            Effect = state => 
            {
                state.hasTarget = true;
            }
        };
    }
    private CompoundTask CreateHungryTask(PrimitiveTask find, PrimitiveTask move, PrimitiveTask eat, PrimitiveTask sleep)
    {
        var compound = new CompoundTask { Name = "Eat" };
        
        // Method 1: If hungry, find food, go to it, and eat it
        compound.Methods.Add(new Method
        {
            Name = "GoToFood",
            Precondition = state => state.hunger > 10,
            Subtasks = new List<ITask> { find, move, eat }
        });
        
        // Method 2: If not hungry, do nothing
        compound.Methods.Add(new Method
        {
            Name = "Idle",
            Precondition = state => state.hunger <= 10,
            Subtasks = new List<ITask>{sleep}
        });
        
        return compound;
    }

    private PrimitiveTask FindFoodTask()
    {
        return new PrimitiveTask
        {
            Name = "FindFood",
            Precondition = state => state.hunger > 10,
            Execute = state => 
            {
                GameObject mushroom = GameObject.FindGameObjectWithTag("Food");
                if (mushroom != null)
                {
                    state.hasTarget = true;
                    npc.currentTarget = mushroom;
                }
            },
            Effect = state => 
            {
                state.hasTarget = true;

            }
        };
    }

    private PrimitiveTask MoveToTask()
    {
        return new PrimitiveTask
        {
            Name = "MoveTo",
            Precondition = state => state.hasTarget,
            Execute = state => 
            {
                if (npc.currentTarget != null) 
                {
                    //npc.animator.SetBool("moving", true);
                    npc.MoveToTarget();
                    Vector3 targetPos = npc.currentTarget.transform.position;
                    // npc.transform.position = Vector3.MoveTowards(
                    //     npc.transform.position,
                    //     targetPos,
                    //     Time.deltaTime * 3f
                    // );
                    
                    float distance = Vector3.Distance(npc.transform.position, targetPos);
                    state.atTarget = distance < 1.5f;
                    
                    if (state.atTarget)
                    {
                        //Debug.Log("Arrived at mushroom!");
                    }
                }
            },
            Effect = state => 
            {
                state.atTarget = true;
                // Debug.Log("Effect of", npc.currentTarget); 

                // state.position = npc.currentTarget.transform.position;
            }
        };
    }

    private PrimitiveTask EatMushroomTask()
    {
        return new PrimitiveTask
        {
            Name = "EatMushroom",
            Precondition = state => state.atTarget,
            Execute = state => 
            {
                npc.animator.SetBool("Eat", true);
                
                // Destroy mushroom
                if (npc.currentTarget != null)
                {
                    Destroy(npc.currentTarget);
                }
                
                // Reset state
                state.hunger = 0;
                state.hasTarget = false;
                state.atTarget = false;
                npc.currentTarget = null;
                //npc.animator.SetBool("Eat", false);
            },
            Effect = state => 
            {
                state.hunger = 0;
                state.hasTarget = false;
                state.atTarget = false;
            }
        };
    }

    private PrimitiveTask SleepTask()
    {
        return new PrimitiveTask
        {
            Name = "Sleep",
            Precondition = state => state.hunger <= 10,
            Execute = state => 
            {
                // Play sleep animation
                // npc.GetComponent<Animator>()?.SetTrigger("Sleep");
                
                //Debug.Log("NPC is sleeping...");
                
                // Reduce hunger slowly while sleeping
                state.hunger += Time.deltaTime * 0.5f; 
            },
            Effect = state => 
            {
                // No immediate effect
            }
        };
    }
 
    public ITask GetRootTask()
    {
        return rootTask;
    }
}
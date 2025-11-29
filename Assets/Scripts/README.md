COMP 521
Sophia Kyrychenko, 261099949
Nov 26, 2025

CONTROLS:
Space for invis for 10s

RUNNING THE GAME: Since it's quite challenging to make it through
To test I recommend running in scene view and moving the player obj in hierarchy controls
Walk through all the caves, try hiding behind rocks
Make it back to spawn point (black dot) before getting mauled too many times

PLEASE ensure screen is large enough to see UI to check bear moves etc
Console log also good to keep track of actions

IGNORE errors about read/writes with navmesh, had issues with version compatibilities so i think it's fine in game editor just not as a downloaded game, as far as I understand

DESIGN CHOICES
- not finding a good animated ogre, so bear instead
- camera to help fov of each bear

LIMITATIONS

- difficulties with making tasks last long enough, i dont know how to do a good timer 
- animations and short tasks therefore not very visible, can only tell through console at times
- bears go to the same mushroom, it's ok they can share


########### HTN doc

HTN STRUCTURE

Root Task: Root
Method 1: GetAngry (Highest Priority)
  Precondition: takenTreasure == true
  Subtasks: [AngryBehavior]
Method 2: EatIfHungry 
  Precondition: hunger > 10 && !takenTreasure
  Subtasks: [Eat]
Method 3: Idle
  Precondition: always true
  Subtasks: []

Compound Task: AngryBehavior
Handles all combat-reactive behaviors.
Method 1: RunToPlayer
  Precondition: seesPlayer && !atTarget
  Subtasks:
  [FindPlayer, RunToPlayer]
Method 2: AttackPlayerWithRock
  Precondition: seesPlayer && atTarget && hasRock
  Subtasks:[RockAttack]
Method 3: AttackPlayer
  Precondition: seesPlayer && atTarget && !hasRock
  Subtasks:[AttackPlayer]
Method 4: AngryAtPlayer
  Precondition: seesPlayer || takenTreasure
  Subtasks: [Angry]

Compound Task: RockAttack
Method 1: FindRock
  Precondition: seesPlayer && atTarget && !hasRock
  Subtasks: [FindRock]
Method 2: ThrowRock
  Precondition: seesPlayer && atTarget && hasRock
  Subtasks: [ThrowRock]

Compound Task: Eat
Handles mushroom-seeking behavior.
Method 1: GoToFood
  Precondition: hunger > 10
  Subtasks:
  [FindFood, MoveTo, EatMushroom]

Method 2: Idle
Precondition: hunger <= 10
Subtasks:
[Sleep]

PRIMITIVE TASKS

Combat Tasks////
  FindPlayer
  Pre: always valid
  Post: hasTarget = true
  Effect: sets NPC target to player

  RunToPlayer
  Pre: seesPlayer && takenTreasure
  Post: atTarget = true
  Effect: approach player at 5 u/s

  AttackPlayer
  Pre: seesPlayer && atTarget
  Post: takenTreasure = false
  Effect: melee attack animation + kill logic

  Angry
  Pre: seesPlayer
  Post: none
  Effect: combat idle animation
  
Rock-Related Tasks/////
  
  FindRock
  Pre: always true
  Post: hasRock = true
  Effect: sets target rock

  ThrowRock
  Pre: seesPlayer && atTarget && hasRock
  Post:
  hasRock = false
  takenTreasure = false
  Effect: throw animation, kill player

Food Tasks/////////

  FindFood
  Pre: hunger > 10
  Post: hasTarget = true
  Effect: sets mushroom target
  
  MoveTo
  Pre: hasTarget
  Post: atTarget = true
  Effect: move toward target 
  
  EatMushroom
  Pre: atTarget
  Post:
  hunger = 0
  hasTarget = false
  atTarget = false
  Effect: destroy mushroom and reset state

  Sleep
  Pre: hunger <= 10
  Post: none
  Effect: idle behavior while hunger slowly increases

Approach HTN:
public class NPCState
{
    public Vector3 position;        
    public float hunger;           
    public bool takenTreasure;      
    public bool seesPlayer;      
    public bool hasRock; // found rock to throw   
    public bool hasTarget;          // Whether NPC has identified a target (food/player)
    public bool atTarget;           // Whether NPC has reached their current target
}
```

- `position`: Updated every frame, used for spatial reasoning
- `hunger`: Increases at 1.0/second, triggers food-seeking behavior at >10
- `takenTreasure`: Set by treasure cave trigger, switches NPC to aggressive mode
- `seesPlayer`: Controlled by FOV camera system, enables chase/attack
- `hasTarget`: Set by "Find" tasks, enables movement tasks
- `atTarget`: Set by "Move" tasks when within range, enables interaction tasks
- `hasRock` : Set by Find task



using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Planner
{
    private Dictionary<string, CompoundTask> compoundTasks = new Dictionary<string, CompoundTask>();
    private Dictionary<string, PrimitiveTask> primitiveTasks = new Dictionary<string, PrimitiveTask>();
    
    public List<PrimitiveTask> GeneratePlan(NPCState currentState, ITask rootTask)
    {
        // task stack to work through
        Stack<ITask> taskStack = new Stack<ITask>();
        // copy WS state
        NPCState workingState = currentState.Clone();
        
        // resutling plan
        List<PrimitiveTask> plan = new List<PrimitiveTask>();
        
        // Push root task to stack
        taskStack.Push(rootTask);
        
        // While stack not empty
        while (taskStack.Count > 0)
        {
            ITask currentTask = taskStack.Pop();
            
            // If primitive task
            if (currentTask is PrimitiveTask primitive)
            {
                // If valid in world state
                if (primitive.IsValid(workingState))
                {
                    // Add to plan
                    plan.Add(primitive);
                    
                    // Apply effects to world state
                    primitive.Effect?.Invoke(workingState);
                }
                else
                {
                    // Fail - primitive not valid
                    //Debug.LogWarning($"Primitive task '{primitive.Name}' failed precondition");
                    return null;
                }
            }
            // Else if compound task
            else if (currentTask is CompoundTask compound)
            {
                
                Method validMethod = FindValidMethod(compound, workingState);
                if (validMethod != null)
                {
                    // Push subtasks to stack in reverse order
                    for (int i = validMethod.Subtasks.Count - 1; i >= 0; i--)
                    {
                        taskStack.Push(validMethod.Subtasks[i]);
                    }
                }
                else
                {
                    // Fail - no valid method found
                    //Debug.LogWarning($"Compound task '{compound.Name}' has no valid method");
                    return null;
                }
            }
        }
        
        return plan;
    }
    
    private Method FindValidMethod(CompoundTask compound, NPCState state)
    {
        foreach (var method in compound.Methods)
        {
           
            //Debug.Log($"Checking method '{method.Name}' for compound task '{compound.Name}' at state hunger={state.hunger}, hasTarget={state.hasTarget}");

            if (method.IsValid(state))
            {
                return method;
            }
        }
        return null;
    }
    
    private bool Decompose(ITask task, NPCState state, List<PrimitiveTask> plan)
    {
        if (task is PrimitiveTask primitive)
        {
            if (!primitive.IsValid(state))
                return false;
                
            plan.Add(primitive);
            primitive.Effect?.Invoke(state); 
            return true;
        }
        
        if (task is CompoundTask compound)
        {
            foreach (var method in compound.Methods)
            {
                if (!method.IsValid(state))
                    continue;
                
                NPCState testState = state.Clone();
                List<PrimitiveTask> tempPlan = new List<PrimitiveTask>();
                bool success = true;
                
                foreach (var subtask in method.Subtasks)
                {
                    if (!Decompose(subtask, testState, tempPlan))
                    {
                        success = false;
                        break;
                    }
                }
                
                if (success)
                {
                    plan.AddRange(tempPlan);
                    foreach (var key in testState.GetType().GetFields())
                    {
                        key.SetValue(state, key.GetValue(testState));
                    }
                    return true;
                }
            }
            
            return false; 
        }
        
        return false;
    }
    
    public void RegisterCompoundTask(CompoundTask task)
    {
        compoundTasks[task.Name] = task;
    }
    
    public void RegisterPrimitiveTask(PrimitiveTask task)
    {
        primitiveTasks[task.Name] = task;
    }
    
    public CompoundTask GetCompoundTask(string name)
    {
        return compoundTasks.ContainsKey(name) ? compoundTasks[name] : null;
    }
    
    public PrimitiveTask GetPrimitiveTask(string name)
    {
        return primitiveTasks.ContainsKey(name) ? primitiveTasks[name] : null;
    }
}
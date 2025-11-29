using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;
using Unity.AI.Navigation;


public class Environment : MonoBehaviour
{
    public GameObject boulder;
    public GameObject mushroom;
    public int rocks = 10;
    public int shrooms = 20;

    public NavMeshSurface surface;
    
    // Area bounds
    public Vector3 minBounds;
    public Vector3 maxBounds;
    
    // Spacing
    public float rockRadius; // Collision radius for rocks
    public float mushroomRadius; // Collision radius for mushrooms
    public int maxAttempts = 50; // Max tries to place each object
    
    private Vector3[] rockPositions;
    private List<Vector3> occupiedPositions = new List<Vector3>();



    public void Start()
    {

        // LayerMask blockMask = LayerMask.GetMask("Block");

        // GameObject[] blocks = FindFirstObjectByType<GameObject>();

        // foreach (GameObject obj in blocks)
        // {
        //     if (((1 << obj.layer) & blockMask) != 0)
        //     {
        //         occupiedPositions.Add(obj.transform.position);
        //     }
        // }

        // bear or Player or Cave tag
        string[] tags = { "bear", "Player", "Cave" };

        foreach (string tag in tags)
        {
            GameObject[] objs = GameObject.FindGameObjectsWithTag(tag);

            foreach (GameObject obj in objs)
            {
                occupiedPositions.Add(obj.transform.position);
            }
        }
        GenerateRocks();
        GenerateShrooms();
        surface.BuildNavMesh();
    }

    public void GenerateRocks()
    {
        for (int i = 0; i < rocks; i++)
        {
            Vector3 position = FindValidPosition(rockRadius);
            
            if (position != Vector3.zero)
            {
                occupiedPositions.Add(position);
                GameObject rock = Instantiate(boulder, position, Quaternion.Euler(0, Random.Range(0, 360), 0));
                rock.tag = "boulder";
            }
        }
    }

    public void GenerateShrooms()
    {
        int placed = 0;
        
        for (int i = 0; i < shrooms; i++)
        {
            Vector3 position = FindValidPosition(mushroomRadius);
            
            if (position != Vector3.zero)
            {
                GameObject shroom = Instantiate(mushroom, position, Quaternion.Euler(0, Random.Range(0, 360), 0));
                shroom.tag = "Food";
                placed++;
            }
        }
    }
    private Vector3 FindValidPosition(float objectRadius)
    {
        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            // Generate random position within bounds
            Vector3 position = new Vector3(
                Random.Range(minBounds.x, maxBounds.x),
                minBounds.y,
                Random.Range(minBounds.z, maxBounds.z)
            );
            
            // Check if position is valid (doesn't overlap with existing objects)
            if (IsPositionValid(position, objectRadius))
            {
                return position;
            }
        }
        
        // Failed to find valid position after max attempts
        Debug.LogWarning($"Could not find valid position after {maxAttempts} attempts");
        return Vector3.zero;
    }

    private bool IsPositionValid(Vector3 position, float objectRadius)
    {
        // Check against all occupied positions (rocks)
        foreach (Vector3 occupied in occupiedPositions)
        {
            // Calculate minimum distance needed (sum of both radii)
            float minDistance = rockRadius + objectRadius;
            float distance = Vector3.Distance(position, occupied);
            
            // If too close, position is invalid
            if (distance < minDistance)
            {
                return false;
            }
        }
        
        return true; // Position is valid
    }
}
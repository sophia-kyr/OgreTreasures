using UnityEngine;

public class fov : MonoBehaviour
{
    public Camera bearCam;
    public Collider playerCollider;
    public LayerMask obstructionMask; // Assign in Inspector (e.g., walls, terrain)
    public NPCController npcController;
    private Plane[] cameraFrustumPlanes;
    private GameState 
    gameState;
    private bool canSeePlayer = false;
    
    void Start()
    {
        gameState = FindFirstObjectByType<GameState>();
    }
    void Update()
    {
        canSeePlayer = CheckPlayerVisible();
        
        if (canSeePlayer)
        {
            // Debug.Log("Player is visible!");
            npcController.currentState.seesPlayer = true;
        
        }
    }
    
    bool CheckPlayerVisible()
    {
        if (!gameState.visibility)
        {
            return false;
        }
        // Step 1: Check if player is in FOV
        var bounds = playerCollider.bounds;
        cameraFrustumPlanes = GeometryUtility.CalculateFrustumPlanes(bearCam);
        
        if (!GeometryUtility.TestPlanesAABB(cameraFrustumPlanes, bounds))
        {
            // Player not in FOV
            return false;
        }
        
        // Step 2: Raycast to check for obstructions
        Vector3 bearPosition = bearCam.transform.position;
        Vector3 playerPosition = playerCollider.bounds.center;
        Vector3 directionToPlayer = playerPosition - bearPosition;
        float distanceToPlayer = directionToPlayer.magnitude;
        
        // Cast ray from bear to player
        if (Physics.Raycast(bearPosition, directionToPlayer, out RaycastHit hit, distanceToPlayer, obstructionMask))
        {
            // Something is blocking the view
            Debug.DrawRay(bearPosition, directionToPlayer * hit.distance, Color.red);
            return false;
        }
        
        Debug.DrawRay(bearPosition, directionToPlayer, Color.green);
        return true;
    }
    
    
}
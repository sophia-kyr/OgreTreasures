using UnityEngine;
public class Cave : MonoBehaviour
{
    public int caveIndex;
    private GameState stateScript;
    public NPCController guardian;
    
    void Start()
    {
        // Find the State script in the scene
        stateScript = FindFirstObjectByType<GameState>();
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player entered cave " + caveIndex);
            stateScript.SetCaveTaken(caveIndex);
            guardian.OnTreasureTaken();// Notify guardian NPC of treasure taken
        }
    }
}
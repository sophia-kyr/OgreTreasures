using UnityEngine;

public class SafeZone : MonoBehaviour
{
    private GameState stateScript;
    void Start()
    {
        stateScript = FindFirstObjectByType<GameState>();
        
    }

    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player entered safe zone.");
            stateScript.SafeZoneEntered();
        }
    }
}

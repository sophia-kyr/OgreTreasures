using UnityEngine;
using System.Collections;
// script at Player obj next to camera and body

// toggle visibility - cooldown, indication UI
// liveness 0 indication UI

//You have a pool of 10 seconds of invisibility for the whole run.

public class Player : MonoBehaviour
{
    public Vector3 respawnPoint;

    private bool isVisible = true;
    private float invisibilityDuration = 10.0f;
    private Coroutine invisibilityCoroutine;
    private bool isAlive = true;

    private KeyCode left = KeyCode.A;
    private KeyCode right = KeyCode.D;
    private KeyCode forward = KeyCode.W;
    private KeyCode back = KeyCode.S;
    private KeyCode toggleVisKey = KeyCode.Space;
    public int livesLeft = 2;

    public Transform playerTransform; 
    
    private float pitch = 30f; // vertical look (X axis)
    private float yaw = 0f;   // horizontal look (Y axis)

    public Camera playerCamera;
    private float speed = 8.0f;
    private bool treasureCollected = false;
    private GameState gameState;

    void Start()
    {
        respawn();
        gameState = FindFirstObjectByType<GameState>();
    }

    void Update()
    {

         if (Input.GetKey(left))
        {
            playerTransform.Translate(Vector3.left * speed * Time.deltaTime);
        }
        if (Input.GetKey(right))
        {
            playerTransform.Translate(Vector3.right * speed * Time.deltaTime);
        }
        if (Input.GetKey(forward))
        {
            playerTransform.Translate(Vector3.forward * speed * Time.deltaTime);
        }
        if (Input.GetKey(back))
        {
            playerTransform.Translate(Vector3.back * speed * Time.deltaTime);
        }
          if (Input.GetKeyDown(toggleVisKey))
        {
            toggleVisibility();
        }

        
        
        float mouseX = Input.GetAxis("Mouse X") * 400f * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * 400f * Time.deltaTime;

        yaw += mouseX;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, -90f, 90f);

        playerCamera.transform.localRotation = Quaternion.Euler(pitch, 0, 0);
        playerTransform.rotation = Quaternion.Euler(0, yaw, 0);
        
      
        
    }
    
    private IEnumerator InvisibilityTimer()
    {
    
        Debug.Log($"Invisible! Time remaining: {invisibilityDuration:F1}s");
        
        // Count down while invisible
        while (invisibilityDuration > 0f)
        {
            invisibilityDuration -= Time.deltaTime;
            
            
            yield return null; 
        }
        
        invisibilityDuration = 0f;
        invisibilityCoroutine = null;
        
        Debug.Log("Invisibility time depleted!");
        turnVisible();


    }

    private void turnVisible()
    {
        isVisible = true;
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer rend in renderers)
        {
            rend.enabled = isVisible;
        }
        gameState.ToggleVisibility();
    }

    private void turnInvisible()
    {
        isVisible = false;
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer rend in renderers)
        {
            rend.enabled = isVisible;
        }
        gameState.ToggleVisibility();
    }
   
    private void toggleVisibility()
    {

        if(isVisible && invisibilityDuration <= 0f)
        {
            Debug.Log("No invisibility time left!");
            return;
        }
        else if (isVisible)
        {// turning invisible
            StartCoroutine(InvisibilityTimer());
            turnInvisible();
        }
        else
        {//turning visible
            turnVisible();
            StopCoroutine(InvisibilityTimer());
            invisibilityCoroutine = null;
            
        }  
    }

    public bool getLiveness()
    {
        return(isAlive);
    }
    public bool getVisibility()
    {
        return(isVisible);
    }

    public void respawn()
    {
        this.playerTransform.position = respawnPoint;
        isAlive = true;
    }
}

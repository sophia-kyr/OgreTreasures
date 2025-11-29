using UnityEngine;
using TMPro;
// tags with cave .. check their ontriggers -> set those treasures as taken
public class GameState : MonoBehaviour
{
    private GameObject safeZone;
    public bool isInSafeZone = false;
    public GameObject[] caves;
    public bool[] cavesTaken;
    private int numCavesTaken = 0;
    public bool allTreasures = false;
    public GameObject winCanvas;
    public GameObject loseCanvas;
    public GameObject stateCanvas;
    private TMP_Text liveText;
    private TMP_Text invisText;
    private TMP_Text caveText;
    public bool visibility = true;
    public int livesLeft = 2;
    private Player player;

    
    void Start()
    {
        cavesTaken = new bool[3];
        //stateCanvas.GetComponentsInChildren<TextMesh>(); //this canvas has 3 kids textmeshes
        //this has not been correct below
        liveText = stateCanvas.transform.Find("LiveText").GetComponent<TMP_Text>();
        invisText = stateCanvas.transform.Find("VisText").GetComponent<TMP_Text>();    
        caveText = stateCanvas.transform.Find("TreasuresText").GetComponent<TMP_Text>();
        player = FindFirstObjectByType<Player>();
        
    }

    void Update()
    {
        
    }

    public void ToggleVisibility()
    {
        visibility = !visibility;
        if (visibility)
        {
            invisText.text = "Visibility: ON";
        }
        else
        {
            invisText.text = "Visibility: OFF";
        }
    }
    public bool SetCaveTaken(int index)
    {
       
        if (index >= 0 && index < cavesTaken.Length && !cavesTaken[index])
        {
            cavesTaken[index] = true;
            Debug.Log("Cave " + index + " taken.");
            numCavesTaken++;
            Debug.Log("Num caves taken: " + caveText.text);
            caveText.text = "Treasures: " + numCavesTaken + " / " + cavesTaken.Length;
            // Check if all caves are taken
            allTreasures = true;
            for (int i = 0; i < cavesTaken.Length; i++)
            {
                if (!cavesTaken[i])
                {
                    allTreasures = false;
                }
            }
            if (allTreasures)
            {
                Debug.Log("All treasures collected!");
            }
            return true;
        }
        return false;
    }

    public void SafeZoneEntered()
    {
        isInSafeZone = true;
        
        if (allTreasures)
        {
            Debug.Log("WIN CONDITION!");
            
            winCanvas.SetActive(true);
            Time.timeScale = 0f; 

            
        }
        else
        {
            Debug.Log("Player has entered the safe zone but is missing some treasures.");
        }
    }

    public void Attacked()
    {
        livesLeft--;
        liveText.text = "Lives: " + livesLeft;
        player.GetComponent<Player>().respawn();
        player.livesLeft = livesLeft;
        if (livesLeft <= 0)
        {
            Debug.Log("Player has no lives left. Game Over!");
            loseCanvas.SetActive(true);
            Time.timeScale = 0f;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // Include the TextMeshPro namespace
using UnityEngine.InputSystem;

public class ARGameManager : MonoBehaviour
{
    public enum GameStage
    {
        Scan,
        Gameplay,
        GameOver,
        Format // New GameStage: Format
    }

    public enum GameMode
    {
        Easy,
        Medium,
        Hard
    }

    // Define a variable to store the current game mode
    private GameMode currentGameMode;

    // Method to set the game mode
    private void SetGameMode(GameMode mode)
    {
        currentGameMode = mode;
        Debug.Log("Game Mode set to: " + mode);
    }

    public GameStage currentStage;

    // Reference to the SurfaceCharacter script
    public scc surfaceCharacter;
    public CharacterController characterController;
    public GameObject[] controls;
    public GameObject[] uiObjects; // Declare the GameObject field
    private int score = 0; // Score counter
    private int health = 3; // Health counter
    public TextMeshProUGUI scoreText; // Reference to the TextMeshPro score text component
    public TextMeshProUGUI healthText; // Reference to the TextMeshPro health text component
    public TextMeshProUGUI timerText; // Reference to the TextMeshPro timer text component
    private float timer = 15f; // Timer for the countdown
    public GameObject[] difficultyObject;
    public GameObject[] gameOver;
    public characterInputAction actionMap;
    public GameObject Tracker;

    private static ARGameManager instance; // Static instance of ARGameManager

    void Awake()
    {
        instance = this; // Set the static instance to this instance
    }

    public static ARGameManager Instance
    {
        get { return instance; }
    }

    // Start is called before the first frame update
    void Start()
    {
        actionMap = new characterInputAction();

        // UIs
        controls = GameObject.FindGameObjectsWithTag("control");
        uiObjects = GameObject.FindGameObjectsWithTag("UI");
        gameOver = GameObject.FindGameObjectsWithTag("gameOver");
        difficultyObject = GameObject.FindGameObjectsWithTag("difficulty");

        // Set everything to false
        setActiveObjects(controls, false);
        setActiveObjects(uiObjects, false);
        setActiveObjects(gameOver, false);
        setActiveObjects(difficultyObject, false);

        // Scan Stage
        currentStage = GameStage.Format;
    }

    // Update is called once per frame
    void Update()
    {
        // Update the timer if the game stage is Gameplay or Format
        if (currentStage == GameStage.Gameplay)
        {
            timer -= Time.deltaTime; // Decrease the timer by the time passed in one frame

            // Check if the timer has reached zero
            if (timer <= 0f)
            {
                // Trigger game over logic when timer reaches zero
                GameOver();
            }

            // Update the timer text
            UpdateTimerText();
        }

        // You can check the current stage in Update method and perform actions accordingly
        switch (currentStage)
        {
            case GameStage.Scan:
                // Activate the SurfaceCharacter script when in the Scan stage
                surfaceCharacter.activate = true;
                break;

            case GameStage.Gameplay:
                actionMap.Disable();
                // Perform actions for Gameplay stage

                setActiveObjects(controls, true);
                setActiveObjects(uiObjects, true);
                // Optionally, find the scoreText, healthText, and timerText components if not assigned in the inspector
                if (scoreText == null)
                {
                    scoreText = GameObject.Find("score").GetComponent<TextMeshProUGUI>();
                }

                if (healthText == null)
                {
                    healthText = GameObject.Find("health").GetComponent<TextMeshProUGUI>();
                }

                if (timerText == null)
                {
                    timerText = GameObject.Find("time").GetComponent<TextMeshProUGUI>();
                }

                // Update the score, health, and timer texts initially
                UpdateScoreText();
                UpdateHealthText();
                UpdateTimerText();
                break;

            case GameStage.GameOver:

                actionMap.Enable();
                // Perform actions for GameOver stage
                setActiveObjects(controls, false);
                setActiveObjects(uiObjects, false);
                setActiveObjects(gameOver, true);

                // Destroy the "boundaries" and "plane" objects if they exist
                GameObject boundariesObject = GameObject.FindGameObjectWithTag("boundaries");
                if (boundariesObject != null)
                {
                    Destroy(boundariesObject);
                }

                GameObject planeObject = GameObject.FindGameObjectWithTag("plane");
                if (planeObject != null)
                {
                    Destroy(planeObject);
                }

                if (Keyboard.current.tKey.wasPressedThisFrame)
                {
                    // Transition to the Format stage
                    currentStage = GameStage.Format;
                    Debug.Log("Transitioning to Format stage.");
                }
                break;

            case GameStage.Format:
                // Perform actions for Format stage

                setActiveObjects(gameOver, false);
                setActiveObjects(difficultyObject, true);

                // Check if key 'B' is pressed
                if (Keyboard.current.bKey.wasPressedThisFrame)
                {
                    // Set the game mode to Easy
                    SetGameMode(GameMode.Easy);
                }
                // Check if key 'N' is pressed
                else if (Keyboard.current.nKey.wasPressedThisFrame)
                {
                    // Set the game mode to Medium
                    SetGameMode(GameMode.Medium);
                }
                // Check if key 'M' is pressed
                else if (Keyboard.current.mKey.wasPressedThisFrame)
                {
                    // Set the game mode to Hard
                    SetGameMode(GameMode.Hard);
                }
                // Check if key 'O' is pressed
                else if (Keyboard.current.oKey.wasPressedThisFrame)
                {
                    // Change aircraft to F16
                    ChangeAircraft("F16");
                }
                // Check if key 'P' is pressed
                else if (Keyboard.current.pKey.wasPressedThisFrame)
                {
                    // Change aircraft to UFO
                    ChangeAircraft("UFO");
                }
                // Check if key 'I' is pressed
                else if (Keyboard.current.iKey.wasPressedThisFrame)
                {
                    // Reset game to Scan phase
                    currentStage = GameStage.Scan;
                    switch (currentGameMode)
                    {
                        case GameMode.Easy:
                            // Set health to 10 for Easy mode
                            health = 10;
                            break;
                        case GameMode.Medium:
                            // Set health to 5 for Medium mode
                            health = 5;
                            break;
                        case GameMode.Hard:
                            // Set health to 1 for Hard mode
                            health = 1;
                            break;
                    }
                    // Reset score, health, and timer
                    score = 0;
                    timer = 15f;

                    // Update UI elements
                    //UpdateScoreText();
                    //UpdateHealthText();
                    //UpdateTimerText();
                    setActiveObjects(difficultyObject, false);
                }
                break;
                
        }
    }

    void setActiveObjects(GameObject[] objects, bool setActive)
    {
        foreach (GameObject obj in objects)
        {
            obj.SetActive(setActive);
        }
    }

    // Method to change the aircraft type
    void ChangeAircraft(string aircraftType)
    {
        // Check if the surfaceCharacter script is available
        if (surfaceCharacter != null)
        {
            // Call a method in the surfaceCharacter script to change the aircraft type
            surfaceCharacter.ChangeAircraftType(aircraftType);
        }
        else
        {
            Debug.LogWarning("SurfaceCharacter script not found or assigned!");
        }
    }


    // Method to increase score when an enemy is shot
    public void IncreaseScore(int points)
    {
        score += points; // Increase score by the given points
        UpdateScoreText(); // Update the score text
        Debug.Log("Score: " + score); // Optionally, you can log the score for debugging
    }

    // Method to decrement health
    public void TakeDamage(int damage)
    {
        health -= damage; // Decrease health by the given damage
        UpdateHealthText(); // Update the health text
        Debug.Log("Health: " + health); // Optionally, you can log the health for debugging

        if (health <= 0)
        {
            // Trigger game over logic when health reaches zero or below
            GameOver();
        }
    }

    // Method to update the score text
    void UpdateScoreText()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + score.ToString(); // Update the text with the current score
        }
    }

    // Method to update the health text
    void UpdateHealthText()
    {
        if (healthText != null)
        {
            healthText.text = "Health: " + health.ToString(); // Update the text with the current health
        }
    }

    // Method to update the timer text
    void UpdateTimerText()
    {
        if (timerText != null)
        {
            timerText.text = "Time: " + Mathf.CeilToInt(timer).ToString(); // Update the text with the current time (rounded up)
        }
    }

    // Method to handle game over
    void GameOver()
    {
        // Implement game over logic here
        currentStage = GameStage.GameOver;
        Debug.Log("Game Over");
    }
}

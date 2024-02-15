using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

public class CharacterController : MonoBehaviour
{
    [SerializeField] private float speed = 1f; // Adjust as needed
    [SerializeField] private GameObject missilePrefab; // Reference to the missile prefab
    [SerializeField] private Vector3[] missileSpawnPositions; // Array of spawn positions for missiles
    [SerializeField] private Vector2 directionValue;
    [SerializeField] private GameObject enemyPrefab; // Reference to the enemy prefab
    public Rigidbody rb;
    public characterInputAction actionMap;
    public GameObject plane;

    private GameObject[] planes; // Reference to the plane object
    private GameObject[] missiles; // Array to store instantiated missiles
    private GameObject[] enemies;

    private ARGameManager.GameStage currentStage;
    private ARGameManager gameManager;

    private bool timerStarted = false;
    public AudioSource soundController;

    void Start()
    {
        //controlObject.enabled = true;
        actionMap = new characterInputAction();
        actionMap.Enable();
        gameManager = FindObjectOfType<ARGameManager>();
        // Find the plane object in the scene
        planes = GameObject.FindGameObjectsWithTag("plane");
        foreach (GameObject p in planes)
        {
            if (p.activeSelf)
            {
                plane = p;
            }
        }
        // Instantiate missiles at spawn positions
        InstantiateMissiles();

        // Instatntiate Enemies
        InstantiateEnemies();
    }

    void Update()
    {
        // Check if the current stage is Gameplay
        if (gameManager != null && gameManager.currentStage != ARGameManager.GameStage.Gameplay)
        {
            foreach (GameObject enemy in enemies)
            {
                enemy.SetActive(false);
            }

            foreach (GameObject missile in missiles)
            {
                missile.SetActive(false);
            }
            return; // Exit the method early if not in Gameplay stage
        }

        // Read the joystick input
        directionValue = actionMap.player.joystick.ReadValue<Vector2>();

        // If F key is pressed, fire missiles
        if (Keyboard.current.fKey.wasPressedThisFrame)
        {
            FireMissile();
        }

        // If Q key is pressed, move up
        if (Keyboard.current.qKey.wasPressedThisFrame)
        {
            MoveUp();
        }

        // If E key is pressed, move down
        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            MoveDown();
        }

        missiles = RemoveNullEntries(missiles);

        foreach (GameObject missile in missiles)
        {
            // Debug.Log(missile);
            if (!missile.activeSelf)
            {
                // If timer not started, start it
                if (!timerStarted)
                {
                    StartCoroutine(MoveMissileWithDelay(missile)); // Start coroutine
                }
            }
        }

        foreach (GameObject enemy in enemies)
        {
            if (!enemy.activeSelf)
            {
                // If timer not started, start it
                if (!timerStarted)
                {
                    float spawnTime = Random.Range(1f, 5f);
                    StartCoroutine(SpawnEnemyWithDelay(enemy, spawnTime)); // Start coroutine
                }
            }
        }
    }

    IEnumerator SpawnEnemyWithDelay(GameObject enemy, float spawnTime)
    {
        timerStarted = true;
        yield return new WaitForSeconds(spawnTime);
        timerStarted = false; // Reset timer flag

        GameObject spawnEnemy = GameObject.FindGameObjectWithTag("enemySpawner");
        if (spawnEnemy != null)
        {
            // Get the width, length, and height of the enemySpawner cube
            Renderer spawnRenderer = spawnEnemy.GetComponent<Renderer>();
            if (spawnRenderer != null)
            {
                float spawnWidth = spawnRenderer.bounds.size.x;
                float spawnHeight = spawnRenderer.bounds.size.y;
                float spawnLength = spawnRenderer.bounds.size.z;

                // Calculate the position of the top-left corner of the enemySpawner
                Vector3 spawnTopLeftCorner = spawnEnemy.transform.position - new Vector3(spawnWidth / 2, spawnHeight / 2, spawnLength / 2);
                Debug.Log("Top-left corner position of enemySpawner: " + spawnTopLeftCorner);

                // Generate random position within the specified range
                float randomX = Random.Range(-spawnWidth / 2, spawnWidth / 2); // Half width to ensure position within range
                float randomY = Random.Range(-spawnHeight / 2, spawnHeight / 2); // Half height to ensure position within range
                float randomZ = Random.Range(-spawnLength / 2, spawnLength / 2); // Half length to ensure position within range

                // Calculate the spawn position relative to the spawner
                Vector3 spawnPosition = spawnEnemy.transform.position + new Vector3(randomX, randomY, randomZ);

                // Move the enemy to the random position inside the spawn area
                enemy.transform.position = spawnPosition;

                // Activate the enemy
                enemy.SetActive(true);

                // Debug log the random positions
                Debug.Log("Random X: " + randomX + ", Random Y: " + randomY + ", Random Z: " + randomZ);
            }
            else
            {
                Debug.LogError("Renderer component not found on enemySpawner object.");
            }
        }
        else
        {
            Debug.LogError("No object with tag 'enemySpawner' found.");
        }

        // Leave out other conditions as requested
    }

    // Method to remove null entries from an array
    GameObject[] RemoveNullEntries(GameObject[] array)
    {
        List<GameObject> list = new List<GameObject>(array);
        list.RemoveAll(item => item == null);
        return list.ToArray();
    }

    IEnumerator MoveMissileWithDelay(GameObject missile)
    {
        // Set timer flag to prevent starting multiple coroutines for the same missile
        timerStarted = true;

        // Wait for 3 seconds
        yield return new WaitForSeconds(3f);

        // Move the missile to an empty cube
        MoveMissileToEmptyCube(missile);

        // Set Active True
        missile.SetActive(true);

        // Reset timer flag
        timerStarted = false;
    }

    void MoveMissileToEmptyCube(GameObject missile)
    {
        // Retrieve the missile's behavior script
        missileBehavior missileBehavior = missile.GetComponent<missileBehavior>();

        // Check if the missile behavior script exists
        if (missileBehavior != null)
        {
            // Retrieve the cube the missile is attached to
            GameObject cube = missileBehavior.cube;

            // Check if the cube reference exists
            if (cube != null)
            {
                // Reset the velocity of the missile
                Rigidbody missileRb = missile.GetComponent<Rigidbody>();
                missileRb.velocity = Vector3.zero;

                // Move the missile to the position of the cube
                missile.transform.position = cube.transform.position;

                // Reset the parent of the missile
                missile.transform.parent = cube.transform;

                // Toggle following
                missile.GetComponent<missileBehavior>().ToggleFollowing(true);
            }
            else
            {
                Debug.LogWarning("Cube reference not found for the missile!");
            }
        }
        else
        {
            Debug.LogWarning("Missile behavior script not found!");
        }
    }




    void FixedUpdate()
    {
        GameObject spawnEnemy = GameObject.Find("spawnEnemy");
        ApplyForce();
    }


    void ApplyForce()
    {
        // Create a direction vector based on the joystick input
        Vector3 applyingForce = new Vector3(directionValue.x, 0f, directionValue.y) * speed;

        // Adjust y-component based on q and e key inputs
        applyingForce.y += (Keyboard.current.qKey.isPressed ? 1f : 0f) * speed; // Move up if Q is pressed
        applyingForce.y -= (Keyboard.current.eKey.isPressed ? 1f : 0f) * speed; // Move down if E is pressed

        // Transform the direction vector to world space and apply it as force
        Vector3 forceInWorldSpace = transform.TransformDirection(applyingForce);
        rb.AddForce(forceInWorldSpace, ForceMode.Impulse);
    }

    void MoveUp()
    {
        // Move up
        rb.AddForce(Vector3.up * speed, ForceMode.Impulse);
    }

    void MoveDown()
    {
        // Move down
        rb.AddForce(Vector3.down * speed, ForceMode.Impulse);
    }


    private Vector3[] initialMissilePositions;

    void InstantiateMissiles()
    {
        // Check if missilePrefab is assigned
        if (missilePrefab == null)
        {
            Debug.LogError("Missile Prefab is not assigned!");
            return;
        }

        // Check if the missileSpawnPositions array is not null and contains at least 4 positions
        if (missileSpawnPositions == null || missileSpawnPositions.Length < 4)
        {
            Debug.LogError("Invalid missile spawn positions array! Please assign at least 4 spawn positions.");
            return;
        }

        // Find all cubes under the plane prefab
        Transform[] childTransforms = plane.GetComponentsInChildren<Transform>();
        GameObject[] cubes = new GameObject[childTransforms.Length];
        int cubeIndex = 0;
        foreach (Transform childTransform in childTransforms)
        {
            if (childTransform.CompareTag("Cube"))
            {
                cubes[cubeIndex++] = childTransform.gameObject;
            }
        }

        // Instantiate 4 missiles under each cube and make them follow
        missiles = new GameObject[cubeIndex * 4];
        initialMissilePositions = new Vector3[cubeIndex * 4]; // Store initial positions
        int missileIndex = 0;
        for (int i = 0; i < Mathf.Min(4, cubeIndex); i++)
        {
            Vector3 spawnPosition = cubes[i].transform.position;

            Quaternion spawnRotation = Quaternion.LookRotation(-cubes[i].transform.forward);

            GameObject missile = Instantiate(missilePrefab, spawnPosition, spawnRotation);
            // Set the layer of the missile to the "plane" layer
            //missile.layer = LayerMask.NameToLayer("plane");

            // Make the missile a child of the cube
            missile.transform.parent = cubes[i].transform;

            // Attach MissileController script to the instantiated missile
            missileBehavior missileBehavior = missile.AddComponent<missileBehavior>();
            // Set the cube reference for the missile
            missileBehavior.SetCube(cubes[i]);

            // Store the initial position of the missile
            initialMissilePositions[missileIndex] = spawnPosition;

            // Add the missile to the missiles array
            missiles[missileIndex++] = missile;
        }
    }



    void FireMissile()
    {
        // Check if there are missiles instantiated
        if (missiles == null || missiles.Length == 0)
        {
            Debug.LogWarning("No missiles instantiated!");
            return;
        }

        // Find the soundController GameObject in the scene
        soundController = FindObjectOfType<AudioSource>();

        // Check if soundController is found
        if (soundController == null)
        {
            Debug.LogWarning("Sound controller not found in the scene!");
            return;
        }

        // Initial force magnitude and increment
        float initialForceMagnitude = 2f;
        float forceIncrement = 1.11f;

        // Iterate through the missiles array
        foreach (GameObject missile in missiles)
        {
            // Check if the missile is enabled
            if (missile.activeSelf)
            {
                Rigidbody missileRb = missile.GetComponent<Rigidbody>();

                // Apply force to the missile to propel it forward
                missileRb.AddForce(-missile.transform.forward * initialForceMagnitude, ForceMode.Impulse);

                // Get the particle system component named "flame" from the missile's children
                ParticleSystem flameParticle = missile.GetComponentInChildren<ParticleSystem>();

                // Check if the particle system component is found
                if (flameParticle != null)
                {
                    // Play the flame particle effect
                    flameParticle.Play();
                }
                else
                {
                    Debug.LogWarning("Flame particle system not found!");
                }

                // Play sound effect
                if (soundController != null)
                {
                    // Assuming you have a sound effect attached to the AudioSource component
                    soundController.Play();
                }
                else
                {
                    Debug.LogWarning("Sound controller not assigned!");
                }

                // Gradually increase force magnitude for next missile
                initialForceMagnitude *= forceIncrement;

                // Disable following behavior
                missile.GetComponent<missileBehavior>().ToggleFollowing(false);

                // Break the loop after firing the first enabled missile
                break;
            }
        }
    }




    void InstantiateEnemies()
    {
        // Define the number of enemies to instantiate
        int numberOfEnemies = 5;
        enemies = new GameObject[numberOfEnemies];

        // Find the enemy spawner by tag
        GameObject enemySpawner = GameObject.FindGameObjectWithTag("enemySpawner");

        if (enemySpawner != null)
        {
            // Spawn each enemy at the position (10, 10, 10)
            for (int i = 0; i < numberOfEnemies; i++)
            {
                // Set spawn position to (10, 10, 10)
                Vector3 spawnPosition = new Vector3(10f, 10f, 10f);

                // Instantiate enemy at the spawn position
                GameObject enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);

                // Set the enemy's parent to the enemy spawner
                enemy.transform.parent = enemySpawner.transform;

                // Add the enemy to the array
                enemies[i] = enemy;

                // Hide the enemy
                enemy.SetActive(false);
            }
        }
        else
        {
            Debug.LogError("No object with tag 'enemySpawner' found.");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if the collision is with an object tagged as "Ground"
        if (other.CompareTag("floor"))
        {
            // Get the ARGameManager component if not assigned
            if (gameManager == null)
            {
                gameManager = FindObjectOfType<ARGameManager>();
            }

            // Trigger game over in the ARGameManager
            if (gameManager != null)
            {
                gameManager.currentStage = ARGameManager.GameStage.GameOver;
            }
            else
            {
                Debug.LogWarning("ARGameManager not found!");
            }
        }

        if (other.CompareTag("enemy"))
        {
            rb.freezeRotation = true;
            // Deactivate the enemy
            other.gameObject.SetActive(false);

            // Decrease Health
            ARGameManager gameManager = FindObjectOfType<ARGameManager>(); // Find ARGameManager in the scene
            if (gameManager != null)
            {
                gameManager.TakeDamage(1); // Increase score by 1 (or any other value you want)
            }
            else
            {
                Debug.LogWarning("ARGameManager not found in the scene!");
            }
        }
    }


}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class missileBehavior : MonoBehaviour
{
    public GameObject cube; // Reference to the cube this missile is following
    private Vector3 offset = new Vector3(0f, -0.02f, 0f); // Offset downwards
    private bool isFollowing = true; // Indicates whether the missile is following the cube

    // Set the cube reference
    public void SetCube(GameObject cube)
    {
        this.cube = cube;
    }

    void Update()
    {
        if (isFollowing && cube != null)
        {
            transform.position = cube.transform.position + offset;
        }
    }

    // Method to toggle following behavior
    public void ToggleFollowing(bool follow)
    {
        isFollowing = follow;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "enemy")
        {
            Debug.Log("Downed an Enemy");
            // Deactivate the enemy
            other.gameObject.SetActive(false);

            // Deactivate the missile
            gameObject.SetActive(false);

            // Increase score
            ARGameManager gameManager = FindObjectOfType<ARGameManager>(); // Find ARGameManager in the scene
            if (gameManager != null)
            {
                gameManager.IncreaseScore(1); // Increase score by 1 (or any other value you want)
            }
            else
            {
                Debug.LogWarning("ARGameManager not found in the scene!");
            }
        }

        if (other.gameObject.tag == "missileDespawn")
        {
            // Deactivate the missile
            gameObject.SetActive(false);
        }
    }
}


using UnityEngine;

public class enemyBehaviour : MonoBehaviour
{
    public float minSpeed = 1f; // Minimum speed
    public float maxSpeed = 2f; // Maximum speed
    public int damage = 1; // Damage inflicted on collision with despawn zone

    // Called when the Collider other enters the trigger zone
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("deSpawn"))
        {
            // Deactivate the enemy
            gameObject.SetActive(false);

            // Decrease health score in ARGameManager
            ARGameManager.Instance.TakeDamage(damage);
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Move towards the despawn cube
        MoveTowardsDespawn();
    }

    void MoveTowardsDespawn()
    {
        // Get the position of the "despawn" cube
        GameObject despawnCube = GameObject.Find("deSpawn");
        if (despawnCube != null)
        {
            // Calculate the direction vector from the enemy to the despawn cube
            Vector3 direction = (despawnCube.transform.position - transform.position).normalized;

            // Generate a random speed within the specified range
            float speed = Random.Range(minSpeed, maxSpeed);

            // Move the enemy towards the despawn cube with the random speed
            transform.Translate(direction * speed * Time.deltaTime);
        }
        else
        {
            Debug.LogError("No GameObject with name 'despawn' found.");
        }
    }
}

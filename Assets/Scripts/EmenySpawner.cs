//using System.Collections;
//using UnityEngine;

//public class EmenySpawner : MonoBehaviour
//{
//    public GameObject enemyPrefab;
//    public Transform[] spawnPoints;
//    public Transform player;


//    public float activationDistance = 25f;
//    public float spawnCooldown = 5f;
//    public int maxEnemies = 3;
//    private int spawnedEnemies = 0;
//    private bool canSpawn = true;

//    void Update()
//    {
//        float distance = Vector3.Distance(transform.position, player.position);

//        if (distance <= activationDistance && canSpawn && spawnedEnemies < maxEnemies)
//        {
//            StartCoroutine(SpawnEnemy());
//        }
//    }

//    IEnumerator SpawnEnemy()
//    {
//        canSpawn = false;

//        GameObject enemy = Instantiate(enemyPrefab, transform.position, Quaternion.identity);
//        spawnedEnemies++;

//        enemy.GetComponent<CheedoorController>().OnDeath += EnemyDied;

//        yield return new WaitForSeconds(spawnCooldown);
//        canSpawn = true;
//    }

//    void EnemyDied()
//    {
//        spawnedEnemies--;
//    }
//}

using UnityEngine;
using System.Collections;

public class EnemySpawner : MonoBehaviour
{
    [Header("References")]
    public GameObject cheedoorPrefab;
    public Transform player;

    [Header("Spawn Settings")]
    public float activationDistance = 25f;
    public float spawnCooldown = 5f;
    public int maxEnemiesAlive = 3;

    private int currentEnemies = 0;
    private bool canSpawn = true;

    void Update()
    {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= activationDistance && canSpawn && currentEnemies < maxEnemiesAlive)
        {
            StartCoroutine(SpawnEnemy());
        }
    }

    IEnumerator SpawnEnemy()
    {
        canSpawn = false;

        GameObject enemy = Instantiate(
            cheedoorPrefab,
            transform.position,
            Quaternion.identity
        );

        CheedoorController cheedoor = enemy.GetComponent<CheedoorController>();

        if (cheedoor != null)
        {
            cheedoor.OnDeath += EnemyDied;
        }

        currentEnemies++;

        yield return new WaitForSeconds(spawnCooldown);
        canSpawn = true;
    }

    void EnemyDied()
    {
        currentEnemies--;
    }
}


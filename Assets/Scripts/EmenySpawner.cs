//using UnityEngine;
//using System.Collections;
//using UnityEngine.AI;

//public class EnemySpawner : MonoBehaviour
//{
//    [Header("References")]
//    public GameObject cheedoorPrefab;
//    public GameObject ratbootPrefab;
//    public Transform player;
//    private bool spawnCheedoor = true; 

//    [Header("Spawn Settings")]
//    public float activationDistance = 25f;
//    public float spawnCooldown = 5f;
//    public int maxEnemiesAlive = 3;

//    private int currentEnemies = 0;
//    private bool canSpawn = true;

//    void Update()
//    {
//        if (player == null) return;

//        float distance = Vector3.Distance(transform.position, player.position);

//        if (distance <= activationDistance && canSpawn && currentEnemies < maxEnemiesAlive)
//        {
//            StartCoroutine(SpawnEnemy());
//        }
//    }

//    //IEnumerator SpawnEnemy()
//    //{
//    //    canSpawn = false;

//    //    // 1. Seleccionar un punto de spawn aleatorio de tu lista
//    //    Vector3 targetSpawnPos = transform.position; // Posición de respaldo
//    //    if (spawnPoints != null && spawnPoints.Length > 0)
//    //    {
//    //        // Elige uno de los puntos que arrastraste al Inspector
//    //        int randomIndex = UnityEngine.Random.Range(0, spawnPoints.Length);
//    //        targetSpawnPos = spawnPoints[randomIndex].position;
//    //    }

//    //    GameObject prefabToSpawn = spawnCheedoor ? cheedoorPrefab : ratbootPrefab;
//    //    spawnCheedoor = !spawnCheedoor;

//    //    NavMeshHit hit;
//    //    // 2. Buscamos el NavMesh cerca del punto ELEGIDO (targetSpawnPos)
//    //    if (NavMesh.SamplePosition(targetSpawnPos, out hit, 10f, NavMesh.AllAreas))
//    //    {
//    //        // Instanciamos en la posición del hit encontrado
//    //        GameObject enemy = Instantiate(prefabToSpawn, hit.position, Quaternion.identity);

//    //        // 3. Forzamos al agente a estar en esa posición para evitar el (0,0,0)
//    //        NavMeshAgent agent = enemy.GetComponent<NavMeshAgent>();
//    //        if (agent != null)
//    //        {
//    //            agent.Warp(hit.position);
//    //        }

//    //        // 4. Suscribir eventos de muerte
//    //        MouseEnemy mouse = enemy.GetComponent<MouseEnemy>();
//    //        CheedoorController cheedoor = enemy.GetComponent<CheedoorController>();

//    //        if (mouse != null) mouse.OnDeath += EnemyDied;
//    //        if (cheedoor != null) cheedoor.OnDeath += EnemyDied;

//    //        currentEnemies++;
//    //    }
//    //    else
//    //    {
//    //        Debug.LogWarning($"No se encontró NavMesh cerca de {targetSpawnPos}. Revisa si hay suelo azul ahí.");
//    //    }

//    //    yield return new WaitForSeconds(spawnCooldown);
//    //    canSpawn = true;
//    //}

//    void EnemyDied()
//    {
//        currentEnemies--;
//    }
//}

using UnityEngine;
using System.Collections;
using UnityEngine.AI;

public class EnemySpawner : MonoBehaviour
{
    [Header("References")]
    public GameObject cheedoorPrefab;
    public GameObject ratbootPrefab;
    public Transform player;
    private bool spawnCheedoor = true;

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

        Vector3 targetSpawnPos = transform.position;

        GameObject prefabToSpawn = spawnCheedoor ? cheedoorPrefab : ratbootPrefab;
        spawnCheedoor = !spawnCheedoor;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(targetSpawnPos, out hit, 10f, NavMesh.AllAreas))
        {
            GameObject enemy = Instantiate(prefabToSpawn, hit.position, Quaternion.identity);

            NavMeshAgent agent = enemy.GetComponent<NavMeshAgent>();
            if (agent != null)
                agent.Warp(hit.position);

            MouseEnemy mouse = enemy.GetComponent<MouseEnemy>();
            CheedoorController cheedoor = enemy.GetComponent<CheedoorController>();

            if (mouse != null) mouse.OnDeath += EnemyDied;
            if (cheedoor != null) cheedoor.OnDeath += EnemyDied;

            currentEnemies++;
        }
        else
        {
            Debug.LogWarning($"[EnemySpawner] No se encontró NavMesh cerca de {targetSpawnPos}.");
        }

        yield return new WaitForSeconds(spawnCooldown);
        canSpawn = true;
    }

    void EnemyDied()
    {
        currentEnemies--;
    }
}
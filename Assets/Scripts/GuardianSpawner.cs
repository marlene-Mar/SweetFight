using UnityEngine;
using UnityEngine.AI;

public class GuardianSpawner : MonoBehaviour
{
    public GameObject guardianPrefab;

    public MeshCollider[] validSurfaces;
    public float maxSlopeAngle = 30f;
    public float groundOffset = 0.1f;

    public int numberOfGuardians = 2;
    public float minDistanceBetweenGuardians = 20f;
    public int maxSpawnAttempts = 100;

    public int patrolPointsPerGuardian = 3;
    public float patrolRadius = 15f;
    public int maxPatrolAttempts = 30;

    public bool showGizmos = true;
    public Color guardian1Color = Color.green;
    public Color guardian2Color = Color.cyan;

    private GuardianData[] guardians;

    [System.Serializable]
    private class GuardianData
    {
        public GameObject guardianObject;
        public Vector3 spawnPoint;
        public Transform[] patrolPoints;
        public Color gizmoColor;
    }

    void Start()
    {
        SpawnGuardians();
    }

    public void SpawnGuardians()
    {
        CleanupPreviousGuardians();

        guardians = new GuardianData[numberOfGuardians];
        Color[] colors = { guardian1Color, guardian2Color };

        int guardiansSpawned = 0;
        int attempts = 0;

        while (guardiansSpawned < numberOfGuardians && attempts < maxSpawnAttempts)
        {
            attempts++;

            // Seleccionar superficie aleatoria
            MeshCollider surface = validSurfaces[Random.Range(0, validSurfaces.Length)];
            Bounds bounds = surface.bounds;

            // Generar posición aleatoria en la superficie
            float randomX = Random.Range(bounds.min.x, bounds.max.x);
            float randomZ = Random.Range(bounds.min.z, bounds.max.z);

            Vector3 rayOrigin = new Vector3(
                randomX,
                bounds.max.y + 10f,
                randomZ
            );

            Ray ray = new Ray(rayOrigin, Vector3.down);
            RaycastHit hit;

            if (!Physics.Raycast(ray, out hit, Mathf.Infinity))
                continue;

            if (!IsValidSurface(hit.collider))
                continue;

            if (Vector3.Angle(hit.normal, Vector3.up) > maxSlopeAngle)
                continue;

            Vector3 spawnPos = hit.point + hit.normal * groundOffset;

            if (!IsValidDistanceFromOtherGuardians(spawnPos, guardiansSpawned))
                continue;

            NavMeshHit navHit;
            if (!NavMesh.SamplePosition(spawnPos, out navHit, 5f, NavMesh.AllAreas))
                continue;

            guardians[guardiansSpawned] = new GuardianData
            {
                spawnPoint = navHit.position,
                gizmoColor = colors[guardiansSpawned % colors.Length]
            };

            InstantiateGuardian(guardiansSpawned);
            AssignSurfacesToGuardian(guardiansSpawned); 

            guardiansSpawned++;
        }
    }

    bool IsValidSurface(Collider hitCollider)
    {
        foreach (MeshCollider surface in validSurfaces)
        {
            if (hitCollider == surface)
                return true;
        }
        return false;
    }

    bool IsValidDistanceFromOtherGuardians(Vector3 candidatePos, int currentGuardianCount)
    {
        for (int i = 0; i < currentGuardianCount; i++)
        {
            if (guardians[i] == null) continue;

            float distance = Vector3.Distance(candidatePos, guardians[i].spawnPoint);
            if (distance < minDistanceBetweenGuardians)
                return false;
        }
        return true;
    }

    void CleanupPreviousGuardians()
    {
        if (guardians != null)
        {
            foreach (var guardian in guardians)
            {
                if (guardian?.guardianObject != null)
                {
                    Destroy(guardian.guardianObject);
                }
            }
        }

        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }

    void InstantiateGuardian(int guardianIndex)
    {
        Vector3 spawnPoint = guardians[guardianIndex].spawnPoint;

        NavMeshHit navHit;
        if (NavMesh.SamplePosition(spawnPoint, out navHit, 5f, NavMesh.AllAreas))
        {
            spawnPoint = navHit.position; 
        }

        GameObject guardian = Instantiate(guardianPrefab, spawnPoint, Quaternion.identity);
        guardian.name = $"Guardian_{guardianIndex + 1}";

        guardians[guardianIndex].guardianObject = guardian;
    }

    void AssignSurfacesToGuardian(int guardianIndex)
    {
        GameObject guardianObj = guardians[guardianIndex].guardianObject;
        if (guardianObj == null)
        {
            Debug.LogError($"⚠ Guardian {guardianIndex + 1} objeto es nulo");
            return;
        }

        GuardianController guardian = guardianObj.GetComponent<GuardianController>();
        if (guardian != null)
        {
            // Encontrar el player
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            Transform playerTransform = player != null ? player.transform : null;

            guardian.Initialize(validSurfaces, playerTransform);

            Debug.Log($"✓ Guardian {guardianIndex + 1}: inicializado y listo para patrullar");
        }
        else
        {
            Debug.LogError($"⚠ Guardian {guardianIndex + 1} no tiene GuardianController");
        }
    }

    void OnDrawGizmos()
    {
        if (!showGizmos) return;

        if (validSurfaces != null)
        {
            Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
            foreach (MeshCollider surface in validSurfaces)
            {
                if (surface != null)
                {
                    Gizmos.DrawWireCube(surface.bounds.center, surface.bounds.size);
                }
            }
        }

        if (guardians != null)
        {
            for (int i = 0; i < guardians.Length; i++)
            {
                if (guardians[i] == null) continue;

                GuardianData guardian = guardians[i];
                Gizmos.color = guardian.gizmoColor;

                Gizmos.DrawSphere(guardian.spawnPoint, 1.5f);
                Gizmos.DrawWireSphere(guardian.spawnPoint, patrolRadius);

#if UNITY_EDITOR
                UnityEditor.Handles.Label(
                    guardian.spawnPoint + Vector3.up * 2f, 
                    $"Guardian {i + 1}",
                    new GUIStyle() { normal = new GUIStyleState() { textColor = guardian.gizmoColor } }
                );
#endif
            }
        }
    }   
}

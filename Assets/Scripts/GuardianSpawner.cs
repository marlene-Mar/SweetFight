using UnityEngine;
using UnityEngine.AI;

public class GuardianSpawner : MonoBehaviour
{
    [Header("Prefab del Guardian")]
    public GameObject guardianPrefab;

    [Header("Superficies Válidas")]
    public MeshCollider[] validSurfaces; // Arrastra aquí: Terreno y Caminos
    public float maxSlopeAngle = 30f;
    public float groundOffset = 0.1f;

    [Header("Configuración de Spawn")]
    public int numberOfGuardians = 2;
    public float minDistanceBetweenGuardians = 20f;
    public int maxSpawnAttempts = 100;

    [Header("Configuración de Patrullaje")]
    public int patrolPointsPerGuardian = 3;
    public float patrolRadius = 15f;
    public int maxPatrolAttempts = 30;

    [Header("Visualización")]
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

    [ContextMenu("Spawn Guardians")]
    public void SpawnGuardians()
    {
        if (validSurfaces == null || validSurfaces.Length == 0)
        {
            Debug.LogError("¡No hay superficies válidas asignadas! Arrastra Terreno y Caminos al array.");
            return;
        }

        if (guardianPrefab == null)
        {
            Debug.LogError("¡No se ha asignado el prefab del Guardian!");
            return;
        }

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

            // Verificar que la superficie es válida
            if (!IsValidSurface(hit.collider))
                continue;

            // Verificar pendiente
            if (Vector3.Angle(hit.normal, Vector3.up) > maxSlopeAngle)
                continue;

            Vector3 spawnPos = hit.point + hit.normal * groundOffset;

            // Verificar distancia con otros guardianes
            if (!IsValidDistanceFromOtherGuardians(spawnPos, guardiansSpawned))
                continue;

            // Verificar que hay NavMesh cerca
            NavMeshHit navHit;
            if (!NavMesh.SamplePosition(spawnPos, out navHit, 5f, NavMesh.AllAreas))
                continue;

            // Spawn exitoso
            guardians[guardiansSpawned] = new GuardianData
            {
                spawnPoint = navHit.position,
                gizmoColor = colors[guardiansSpawned % colors.Length]
            };

            InstantiateGuardian(guardiansSpawned);
            GeneratePatrolPoints(guardiansSpawned);
            AssignPatrolPointsToGuardian(guardiansSpawned);

            guardiansSpawned++;
            Debug.Log($"Guardian {guardiansSpawned} spawneado en {navHit.position} (intentos: {attempts})");
        }

        if (guardiansSpawned < numberOfGuardians)
        {
            Debug.LogWarning($"Solo se spawnearon {guardiansSpawned} de {numberOfGuardians} guardianes después de {attempts} intentos.");
        }
        else
        {
            Debug.Log($"¡Se spawnearon {numberOfGuardians} guardianes exitosamente!");
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
        GameObject guardian = Instantiate(guardianPrefab, spawnPoint, Quaternion.identity);
        guardian.name = $"Guardian_{guardianIndex + 1}";

        guardians[guardianIndex].guardianObject = guardian;
    }

    void GeneratePatrolPoints(int guardianIndex)
    {
        Transform[] patrolPoints = new Transform[patrolPointsPerGuardian];
        Vector3 centerPoint = guardians[guardianIndex].spawnPoint;

        GameObject patrolParent = new GameObject($"PatrolPoints_Guardian{guardianIndex + 1}");
        patrolParent.transform.parent = this.transform;

        int pointsGenerated = 0;
        int attempts = 0;

        while (pointsGenerated < patrolPointsPerGuardian && attempts < maxPatrolAttempts * patrolPointsPerGuardian)
        {
            attempts++;

            // Seleccionar superficie aleatoria
            MeshCollider surface = validSurfaces[Random.Range(0, validSurfaces.Length)];
            Bounds bounds = surface.bounds;

            // Generar punto aleatorio cerca del guardian
            Vector2 randomCircle = Random.insideUnitCircle * patrolRadius;
            float randomX = centerPoint.x + randomCircle.x;
            float randomZ = centerPoint.z + randomCircle.y;

            // Clampear a los límites de la superficie
            randomX = Mathf.Clamp(randomX, bounds.min.x, bounds.max.x);
            randomZ = Mathf.Clamp(randomZ, bounds.min.z, bounds.max.z);

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

            Vector3 patrolPos = hit.point + hit.normal * groundOffset;

            // Verificar que está en NavMesh
            NavMeshHit navHit;
            if (!NavMesh.SamplePosition(patrolPos, out navHit, 5f, NavMesh.AllAreas))
                continue;

            // Verificar distancia al centro (no muy lejos)
            if (Vector3.Distance(navHit.position, centerPoint) > patrolRadius)
                continue;

            GameObject patrolObj = new GameObject($"Patrol_{pointsGenerated}");
            patrolObj.transform.position = navHit.position;
            patrolObj.transform.parent = patrolParent.transform;

            patrolPoints[pointsGenerated] = patrolObj.transform;
            pointsGenerated++;
        }

        // Si no se generaron suficientes puntos, rellenar con el centro
        for (int i = pointsGenerated; i < patrolPointsPerGuardian; i++)
        {
            GameObject patrolObj = new GameObject($"Patrol_{i}_Fallback");
            patrolObj.transform.position = centerPoint;
            patrolObj.transform.parent = patrolParent.transform;
            patrolPoints[i] = patrolObj.transform;
            Debug.LogWarning($"Punto de patrullaje {i} usa posición de spawn como fallback.");
        }

        guardians[guardianIndex].patrolPoints = patrolPoints;
        Debug.Log($"Guardian {guardianIndex + 1}: {pointsGenerated}/{patrolPointsPerGuardian} puntos de patrullaje generados.");
    }

    void AssignPatrolPointsToGuardian(int guardianIndex)
    {
        GameObject guardianObj = guardians[guardianIndex].guardianObject;
        if (guardianObj == null) return;

        GuardianController guardian = guardianObj.GetComponent<GuardianController>();
        if (guardian != null)
        {
            guardian.patrolPoints = guardians[guardianIndex].patrolPoints;
        }
        else
        {
            Debug.LogError($"Guardian {guardianIndex + 1} no tiene el componente GuardianController.");
        }
    }

    void OnDrawGizmos()
    {
        if (!showGizmos) return;

        // Dibujar bounds de superficies válidas
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

        // Dibujar guardianes y rutas
        if (guardians != null)
        {
            for (int i = 0; i < guardians.Length; i++)
            {
                if (guardians[i] == null) continue;

                GuardianData guardian = guardians[i];
                Gizmos.color = guardian.gizmoColor;

                // Spawn point
                Gizmos.DrawSphere(guardian.spawnPoint, 1.5f);
                Gizmos.DrawWireSphere(guardian.spawnPoint, patrolRadius);

                // Puntos de patrullaje
                if (guardian.patrolPoints != null)
                {
                    for (int j = 0; j < guardian.patrolPoints.Length; j++)
                    {
                        if (guardian.patrolPoints[j] != null)
                        {
                            Gizmos.DrawSphere(guardian.patrolPoints[j].position, 0.5f);

                            if (j < guardian.patrolPoints.Length - 1 && guardian.patrolPoints[j + 1] != null)
                            {
                                Gizmos.DrawLine(guardian.patrolPoints[j].position, guardian.patrolPoints[j + 1].position);
                            }
                        }
                    }

                    if (guardian.patrolPoints.Length > 1 &&
                        guardian.patrolPoints[0] != null &&
                        guardian.patrolPoints[guardian.patrolPoints.Length - 1] != null)
                    {
                        Gizmos.DrawLine(
                            guardian.patrolPoints[guardian.patrolPoints.Length - 1].position,
                            guardian.patrolPoints[0].position
                        );
                    }
                }

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
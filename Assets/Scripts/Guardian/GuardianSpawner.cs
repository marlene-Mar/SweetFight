using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Clase encargada de instanciar Guardianes de forma aleatoria sobre superficies válidas.
/// Asegura que no se amontonen y que queden correctamente posicionados en el NavMesh.
/// </summary>
public class GuardianSpawner : MonoBehaviour
{
    [Header("Prefabs y Configuración")]
    public GameObject guardianPrefab;       // El modelo del Guardián a instanciar
    public MeshCollider[] validSurfaces;    // Lista de suelos/plataformas donde pueden aparecer

    [Header("Restricciones de Posicionamiento")]
    public float maxSlopeAngle = 30f;       // Ángulo máximo de inclinación permitido para aparecer
    public float groundOffset = 0.1f;       // Pequeña elevación sobre el suelo al aparecer
    public int numberOfGuardians = 2;       // Cuántos guardianes queremos en total
    public float minDistanceBetweenGuardians = 20f; // Distancia mínima entre ellos para evitar solapamientos
    public int maxSpawnAttempts = 100;      // Límite de intentos para encontrar un sitio válido

    [Header("Configuración de Patrulla")]
    public int patrolPointsPerGuardian = 3;
    public float patrolRadius = 15f;
    public int maxPatrolAttempts = 30;

    [Header("Visualización (Editor)")]
    public bool showGizmos = true;
    public Color guardian1Color = Color.green;
    public Color guardian2Color = Color.cyan;

    private GuardianData[] guardians; // Almacén interno de los datos de cada guardián

    /// <summary>
    /// Estructura interna para agrupar los datos de un guardián instanciado.
    /// </summary>
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

    /// <summary>
    /// Lógica principal para generar a los guardianes en el mapa.
    /// </summary>
    public void SpawnGuardians()
    {
        CleanupPreviousGuardians(); // Limpiar si había guardianes de una ejecución anterior

        guardians = new GuardianData[numberOfGuardians];
        Color[] colors = { guardian1Color, guardian2Color };

        int guardiansSpawned = 0;
        int attempts = 0;

        // Bucle de intentos: busca posiciones aleatorias hasta llenar el cupo o agotar intentos
        while (guardiansSpawned < numberOfGuardians && attempts < maxSpawnAttempts)
        {
            attempts++;

            // 1. Seleccionar una superficie (suelo) al azar de la lista
            MeshCollider surface = validSurfaces[Random.Range(0, validSurfaces.Length)];
            Bounds bounds = surface.bounds;

            // 2. Generar coordenadas aleatorias dentro de los límites de esa superficie
            float randomX = Random.Range(bounds.min.x, bounds.max.x);
            float randomZ = Random.Range(bounds.min.z, bounds.max.z);

            // 3. Lanzar un rayo desde arriba hacia abajo para encontrar el punto exacto de contacto
            Vector3 rayOrigin = new Vector3(randomX, bounds.max.y + 10f, randomZ);
            Ray ray = new Ray(rayOrigin, Vector3.down);
            RaycastHit hit;

            if (!Physics.Raycast(ray, out hit, Mathf.Infinity))
                continue;

            // 4. Validaciones de seguridad: ¿Es una superficie permitida? ¿Está muy inclinado?
            if (!IsValidSurface(hit.collider))
                continue;

            if (Vector3.Angle(hit.normal, Vector3.up) > maxSlopeAngle)
                continue;

            Vector3 spawnPos = hit.point + hit.normal * groundOffset;

            // 5. Validar distancia con respecto a otros guardianes ya creados
            if (!IsValidDistanceFromOtherGuardians(spawnPos, guardiansSpawned))
                continue;

            // 6. Asegurar que la posición sea accesible para el NavMesh (IA de movimiento)
            NavMeshHit navHit;
            if (!NavMesh.SamplePosition(spawnPos, out navHit, 5f, NavMesh.AllAreas))
                continue;

            // 7. Guardar datos e instanciar
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

    // Comprueba si el colisionador impactado está en nuestra lista de superficies válidas
    bool IsValidSurface(Collider hitCollider)
    {
        foreach (MeshCollider surface in validSurfaces)
        {
            if (hitCollider == surface) return true;
        }
        return false;
    }

    // Evita que los guardianes aparezcan uno encima de otro
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

    // Destruye los guardianes existentes para reiniciar el spawner
    void CleanupPreviousGuardians()
    {
        if (guardians != null)
        {
            foreach (var guardian in guardians)
            {
                if (guardian?.guardianObject != null)
                    Destroy(guardian.guardianObject);
            }
        }

        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }

    // Crea el objeto en la escena y le asigna un nombre único
    void InstantiateGuardian(int guardianIndex)
    {
        Vector3 spawnPoint = guardians[guardianIndex].spawnPoint;

        GameObject guardian = Instantiate(guardianPrefab, spawnPoint, Quaternion.identity);
        guardian.name = $"Guardian_{guardianIndex + 1}";

        guardians[guardianIndex].guardianObject = guardian;
    }

    /// <summary>
    /// Configura el script GuardianController del objeto recién creado.
    /// </summary>
    void AssignSurfacesToGuardian(int guardianIndex)
    {
        GameObject guardianObj = guardians[guardianIndex].guardianObject;
        if (guardianObj == null) return;

        GuardianController guardian = guardianObj.GetComponent<GuardianController>();
        if (guardian != null)
        {
            // Busca al jugador para que el guardián sepa a quién seguir/atacar
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            Transform playerTransform = player != null ? player.transform : null;

            // Inicializa la IA del guardián
            guardian.Initialize(validSurfaces, playerTransform);
            Debug.Log($"✓ Guardian {guardianIndex + 1}: inicializado y listo para patrullar");
        }
        else
        {
            Debug.LogError($"⚠ Guardian {guardianIndex + 1} no tiene GuardianController");
        }
    }

    // Dibuja esferas de colores en el editor de Unity para previsualizar los puntos de aparición
    void OnDrawGizmos()
    {
        if (!showGizmos) return;

        if (validSurfaces != null)
        {
            Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
            foreach (MeshCollider surface in validSurfaces)
            {
                if (surface != null)
                    Gizmos.DrawWireCube(surface.bounds.center, surface.bounds.size);
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
            }
        }
    }
}
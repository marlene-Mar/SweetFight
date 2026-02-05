using UnityEngine;

public class PatrolPointsGenerator : MonoBehaviour
{
    [Header("Configuración de Puntos de Patrullaje")]
    public int numberOfPoints = 3;
    public float spawnRadius = 20f;
    public LayerMask walkableLayer; // Capa del NavMesh
    
    [Header("Visualización")]
    public bool showGizmos = true;
    public Color gizmoColor = Color.green;
    
    private Transform[] patrolPoints;
    
    void Start()
    {
        GeneratePatrolPoints();
    }
    
    public void GeneratePatrolPoints()
    {
        // Limpiar puntos anteriores si existen
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        
        patrolPoints = new Transform[numberOfPoints];
        
        for (int i = 0; i < numberOfPoints; i++)
        {
            Vector3 randomPoint = GetRandomNavMeshPoint();
            
            // Crear un GameObject vacío para el punto de patrullaje
            GameObject patrolPoint = new GameObject($"PatrolPoint_{i}");
            patrolPoint.transform.position = randomPoint;
            patrolPoint.transform.parent = this.transform;
            
            patrolPoints[i] = patrolPoint.transform;
        }
        
        // Asignar puntos al Guardian
        GuardianController guardian = FindObjectOfType<GuardianController>();
        if (guardian != null)
        {
            guardian.patrolPoints = patrolPoints;
            Debug.Log($"Se asignaron {numberOfPoints} puntos de patrullaje al Guardian.");
        }
    }
    
    Vector3 GetRandomNavMeshPoint()
    {
        Vector3 randomDirection = Random.insideUnitSphere * spawnRadius;
        randomDirection += transform.position;
        
        UnityEngine.AI.NavMeshHit navHit;
        UnityEngine.AI.NavMesh.SamplePosition(randomDirection, out navHit, spawnRadius, UnityEngine.AI.NavMesh.AllAreas);
        
        return navHit.position;
    }
    
    void OnDrawGizmos()
    {
        if (!showGizmos) return;
        
        Gizmos.color = gizmoColor;
        
        // Dibujar radio de spawn
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
        
        // Dibujar puntos de patrullaje
        if (patrolPoints != null)
        {
            for (int i = 0; i < patrolPoints.Length; i++)
            {
                if (patrolPoints[i] != null)
                {
                    Gizmos.DrawSphere(patrolPoints[i].position, 0.5f);
                    
                    // Dibujar líneas entre puntos
                    if (i < patrolPoints.Length - 1 && patrolPoints[i + 1] != null)
                    {
                        Gizmos.DrawLine(patrolPoints[i].position, patrolPoints[i + 1].position);
                    }
                }
            }
            
            // Conectar último con primero
            if (patrolPoints.Length > 1 && patrolPoints[0] != null && patrolPoints[patrolPoints.Length - 1] != null)
            {
                Gizmos.DrawLine(patrolPoints[patrolPoints.Length - 1].position, patrolPoints[0].position);
            }
        }
    }
    
    // Método para regenerar puntos en el editor
    [ContextMenu("Regenerar Puntos de Patrullaje")]
    public void RegeneratePoints()
    {
        GeneratePatrolPoints();
    }
}

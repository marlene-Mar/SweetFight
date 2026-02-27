using UnityEngine;

// Se encarga de generar flanes y caramelos en superficies válidas al inicio del juego
public class SimpleObjectSpawner : MonoBehaviour
{
    // Prefabs de flan y caramelo a instanciar
    public GameObject flanPrefab;
    public GameObject candyPrefab;

    // Superficies válidas donde se pueden generar los objetos
    public MeshCollider[] validSurfaces;

    public int maxFlanes = 15;
    public int maxCandies = 40;
    public float maxSlopeAngle = 40f;
    public float groundOffset = 0.3f;

    private int currentFlans = 0;
    private int currentCandies = 0;

    void Start()
    {
        SpawnInitialFlanes();
        SpawnInitialCandy();
    }

    // Genera flanes en superficies válidas al inicio del juego
    void SpawnInitialFlanes()
    {
        int attempts = 0;

        while (currentFlans < maxFlanes && attempts < maxFlanes * 15)
        {
            attempts++;

            MeshCollider surface = validSurfaces[Random.Range(0, validSurfaces.Length)];
            Bounds bounds = surface.bounds;

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

            Quaternion alignToGround = Quaternion.FromToRotation(Vector3.up, hit.normal); 
            Quaternion randomYaw = Quaternion.Euler(0, Random.Range(0f, 360f), 0);

            Vector3 spawnPos = hit.point + hit.normal * groundOffset;

            Instantiate(flanPrefab, spawnPos, alignToGround * randomYaw);
            currentFlans++;
        }
    }

    // Genera caramelos en superficies válidas al inicio del juego
    void SpawnInitialCandy()
    {
        int attempts = 0;

        while (currentCandies < maxCandies && attempts < maxCandies * 30)
        {
            attempts++;

            MeshCollider surface = validSurfaces[Random.Range(0, validSurfaces.Length)];
            Bounds bounds = surface.bounds;

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

            Quaternion alignToGround = Quaternion.FromToRotation(Vector3.up, hit.normal);
            Quaternion randomYaw = Quaternion.Euler(0, Random.Range(0f, 360f), 0);

            Vector3 spawnPos = hit.point + hit.normal * groundOffset;

            Instantiate(candyPrefab, spawnPos, alignToGround * randomYaw);
            currentCandies++;
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

    // Reinicia el conteo de flanes y caramelos y vuelve a generar los objetos al inicio del juego
    public void RespawnAll()
    {
        currentFlans = 0;
        currentCandies = 0;
        SpawnInitialFlanes();
        SpawnInitialCandy();
    }

}

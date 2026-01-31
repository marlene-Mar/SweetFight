using UnityEngine;

public class SimpleObjectSpawner : MonoBehaviour
{
    public GameObject zonaPasto;
    public int cantidad = 100;
    public Vector2 area;

    void Start()
    {
        for (int i = 0; i < cantidad; i++)
        {
            Vector3 pos = new Vector3(
                Random.Range(-area.x, area.x),
                0,
                Random.Range(-area.y, area.y)
            );

            Instantiate(zonaPasto, pos, Quaternion.Euler(0, Random.Range(0, 360), 0));
        }
    }


}
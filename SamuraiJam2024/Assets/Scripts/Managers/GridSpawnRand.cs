using UnityEngine;

public class GridSpawnRand : MonoBehaviour
{
    public GameObject objectToSpawn;
    public int rows = 10;
    public int columns = 10;

    void Start()
    {
        Vector3 size = GetComponent<Renderer>().bounds.size;
        Vector3 startPosition = transform.position - size / 2;

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                Vector3 spawnPosition = new Vector3(startPosition.x + (size.x / rows) * i, startPosition.y, startPosition.z + (size.z / columns) * j);

                float coinFlip = Random.Range(0, 1);

                Quaternion rotation = Quaternion.identity;
                if (coinFlip == 1)
                {
                    rotation = Quaternion.Euler(0, 90f, 0);
                }


                Instantiate(objectToSpawn, spawnPosition, rotation);
            }
        }
    }
}
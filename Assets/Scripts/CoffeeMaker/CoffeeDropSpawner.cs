using System.Collections;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class CoffeeDropSpawner : MonoBehaviour
{
    [Header("References")]
    public GridMesh gridMesh;
    public GameObject coffeeDropPrefab;
    public CoffeeGameplayManager gameplayManager;

    [Header("Spawn Settings")]
    public float spawnHeight = 5f;   // height above the grid base
    public float spawnInterval = 1f;
    public float totalTime = 10f;

    void Start()
    {
        if (gridMesh == null || coffeeDropPrefab == null || gameplayManager == null)
        {
            Debug.LogWarning("CoffeeDropSpawner is missing references!");
            return;
        }

        StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        float elapsed = 0f;

        while (elapsed < totalTime)
        {
            SpawnDrop();
            yield return new WaitForSeconds(spawnInterval);
            elapsed += spawnInterval;
        }
    }

    void SpawnDrop()
    {
        int randX = Random.Range(0, gridMesh.gridSize);
        int randZ = Random.Range(0, gridMesh.gridSize);

        Vector3 spawnPos = GetCellCenter(randX, randZ);
        spawnPos.y = gridMesh.transform.position.y + spawnHeight;

        GameObject newDropObj = Instantiate(coffeeDropPrefab, spawnPos, Quaternion.identity);
        CoffeeDrop drop = newDropObj.GetComponent<CoffeeDrop>();

        if (drop != null)
            gameplayManager.RegisterDrop(drop);
    }

    Vector3 GetCellCenter(int x, int z)
    {
        Vector3 origin = gridMesh.transform.position;

        float worldX = origin.x + (x + 0.5f) * gridMesh.cellSize;
        float worldZ = origin.z + (z + 0.5f) * gridMesh.cellSize;

        return new Vector3(worldX, origin.y, worldZ);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (gridMesh == null) return;

        Vector3 origin = gridMesh.transform.position;
        float y = origin.y + spawnHeight;

        for (int x = 0; x < gridMesh.gridSize; x++)
        {
            for (int z = 0; z < gridMesh.gridSize; z++)
            {
                Vector3 cellCenter = GetCellCenter(x, z);
                Vector3 spawnPos = new Vector3(cellCenter.x, y, cellCenter.z);

                // Draw cube normally
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireCube(spawnPos, new Vector3(gridMesh.cellSize * 0.9f, 0.1f, gridMesh.cellSize * 0.9f));

                // Draw line with transparency using Handles
                Handles.color = new Color(0f, 1f, 1f, 0.2f); // cyan, 20% alpha
                Vector3 basePos = new Vector3(cellCenter.x, origin.y, cellCenter.z);
                Handles.DrawLine(basePos, spawnPos);
            }
        }
    }
#endif
}

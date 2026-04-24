using UnityEngine;

public class FoodSpawner : MonoBehaviour
{
    [Header("Food")]
    public GameObject FoodPrefab;
    public float FoodSize = 0.5f;

    [Header("Spawn Area")]
    public float SpawnRangeX = 12f;
    public float SpawnRangeZ = 12f;
    public float SpawnY = 0.5f;

    private GameObject currentFood;

    private void Start()
    {
        SpawnFood();
    }

    public void SpawnFood()
    {
        if (FoodPrefab == null)
        {
            Debug.LogError("FoodPrefab is missing.");
            return;
        }

        Vector3 spawnPosition = new Vector3(
            Random.Range(-SpawnRangeX, SpawnRangeX),
            SpawnY,
            Random.Range(-SpawnRangeZ, SpawnRangeZ)
        );

        currentFood = Instantiate(FoodPrefab, spawnPosition, Quaternion.identity);
        currentFood.name = "Food";

        currentFood.transform.localScale = new Vector3(FoodSize, FoodSize, FoodSize);

        Renderer renderer = currentFood.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = Color.red;
        }

        BoxCollider boxCollider = currentFood.GetComponent<BoxCollider>();
        if (boxCollider == null)
        {
            boxCollider = currentFood.AddComponent<BoxCollider>();
        }

        boxCollider.isTrigger = true;
    }

    public bool IsFood(GameObject obj)
    {
        return obj == currentFood;
    }

    public void CollectFood(GameObject food)
    {
        if (food != currentFood)
        {
            return;
        }

        Destroy(currentFood);
        currentFood = null;

        SpawnFood();
    }
}
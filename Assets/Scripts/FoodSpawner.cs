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

    [Header("Floating Effect")]
    public float FloatHeight = 0.25f;
    public float FloatSpeed = 3f;
    public float RotationSpeed = 90f;

    private GameObject currentFood;
    private Vector3 foodBasePosition;

    private void Update()
    {
        AnimateFood();
    }

    public void ResetFood()
    {
        if (currentFood != null)
        {
            Destroy(currentFood);
            currentFood = null;
        }

        SpawnFood();
    }

    public void SpawnFood()
    {
        if (FoodPrefab == null)
        {
            Debug.LogError("FoodPrefab is missing.");
            return;
        }

        foodBasePosition = new Vector3(
            Random.Range(-SpawnRangeX, SpawnRangeX),
            SpawnY,
            Random.Range(-SpawnRangeZ, SpawnRangeZ)
        );

        currentFood = Instantiate(FoodPrefab, foodBasePosition, Quaternion.identity);
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

    private void AnimateFood()
    {
        if (currentFood == null)
        {
            return;
        }

        float floatOffset = Mathf.Sin(Time.time * FloatSpeed) * FloatHeight;

        currentFood.transform.position = foodBasePosition + new Vector3(0f, floatOffset, 0f);

        currentFood.transform.Rotate(Vector3.up * RotationSpeed * Time.deltaTime);
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
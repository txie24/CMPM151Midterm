using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SnakeController : MonoBehaviour
{
    [Header("Settings")]
    public float MoveSpeed = 5f;
    public float SteerSpeed = 180f;
    public float BodySpeed = 5f;
    public int Gap = 10;

    [Header("References")]
    public GameObject BodyPrefab;
    public FoodSpawner FoodSpawner;

    private List<GameObject> BodyParts = new List<GameObject>();
    private List<Vector3> PositionsHistory = new List<Vector3>();

    private InputAction steerAction;

    private void Awake()
    {
        steerAction = new InputAction(
            name: "Steer",
            type: InputActionType.Value,
            expectedControlType: "Axis"
        );

        steerAction.AddCompositeBinding("1DAxis")
            .With("Negative", "<Keyboard>/a")
            .With("Negative", "<Keyboard>/leftArrow")
            .With("Positive", "<Keyboard>/d")
            .With("Positive", "<Keyboard>/rightArrow");

        steerAction.AddBinding("<Gamepad>/leftStick/x");
    }

    private void OnEnable()
    {
        steerAction.Enable();
    }

    private void OnDisable()
    {
        steerAction.Disable();
    }

    private void Start()
    {
        if (FoodSpawner == null)
        {
            FoodSpawner = FindFirstObjectByType<FoodSpawner>();
        }

        GrowSnake(); // only one body attached at the start
    }

    private void Update()
    {
        MoveForward();
        Steer();
        SavePositionHistory();
        MoveBodyParts();
    }

    private void MoveForward()
    {
        transform.position += transform.forward * MoveSpeed * Time.deltaTime;
    }

    private void Steer()
    {
        float steerDirection = steerAction.ReadValue<float>();
        transform.Rotate(Vector3.up * steerDirection * SteerSpeed * Time.deltaTime);
    }

    private void SavePositionHistory()
    {
        PositionsHistory.Insert(0, transform.position);
    }

    private void MoveBodyParts()
    {
        int index = 0;

        foreach (GameObject body in BodyParts)
        {
            Vector3 point = PositionsHistory[
                Mathf.Clamp(index * Gap, 0, PositionsHistory.Count - 1)
            ];

            Vector3 moveDirection = point - body.transform.position;
            body.transform.position += moveDirection * BodySpeed * Time.deltaTime;

            if (moveDirection != Vector3.zero)
            {
                body.transform.LookAt(point);
            }

            index++;
        }
    }

    private void GrowSnake()
    {
        if (BodyPrefab == null)
        {
            Debug.LogError("BodyPrefab is missing.");
            return;
        }

        GameObject body = Instantiate(BodyPrefab, transform.position, transform.rotation);
        BodyParts.Add(body);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (FoodSpawner == null)
        {
            return;
        }

        if (FoodSpawner.IsFood(other.gameObject))
        {
            GrowSnake(); // grow one body part
            FoodSpawner.CollectFood(other.gameObject);

            Debug.Log("snake ate food");
        }
    }

    private void OnDestroy()
    {
        steerAction?.Dispose();
    }
}
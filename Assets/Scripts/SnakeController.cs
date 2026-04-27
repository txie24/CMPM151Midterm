using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class SnakeController : MonoBehaviour
{
    [Header("Movement")]
    public float MoveSpeed = 5f;
    public float SteerSpeed = 180f;
    public float BodySpeed = 8f;
    public float BodySpacing = 1.3f;
    public float HistoryRecordSpacing = 0.05f;
    public float BodyRotationSpeed = 720f;

    [Header("Speed Increase")]
    public float SpeedIncreasePerFood = 0.5f;
    public float MaxMoveSpeed = 15f;
    public float BodySpeedMultiplier = 2.5f;

    [Header("Wall Collision")]
    public LayerMask WallLayer;
    public float WallCheckDistance = 0.8f;

    [Header("Self Collision")]
    public float SelfCollisionDistance = 0.75f;
    public int SelfCollisionStartIndex = 2;

    [Header("References")]
    public GameObject BodyPrefab;
    public FoodSpawner FoodSpawner;

    [Header("UI")]
    public TMP_Text TimeText;
    public TMP_Text AppleText;
    public GameObject GameOverPanel;
    public TMP_Text GameOverReasonText;

    private List<GameObject> BodyParts = new List<GameObject>();
    private List<Vector3> PositionsHistory = new List<Vector3>();
    private List<Quaternion> RotationsHistory = new List<Quaternion>();

    private InputAction steerAction;

    private bool isGameOver = false;
    private int applesEaten = 0;
    private float gameTime = 0f;

    private Vector3 startPosition;
    private Quaternion startRotation;
    private float startingMoveSpeed;

    private SnakeSoundManager soundManager;

    private void Awake()
    {
        startPosition = transform.position;
        startRotation = transform.rotation;
        startingMoveSpeed = MoveSpeed;

        soundManager = FindFirstObjectByType<SnakeSoundManager>(); 

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

        NewGame();
    }

    private void Update()
    {
        if (isGameOver)
        {
            if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
            {
                NewGame();
            }

            return;
        }

        gameTime += Time.deltaTime;
        UpdateUI();

        MoveForward();
        Steer();
        SavePositionHistory();
        MoveBodyParts();
        CheckSelfCollision();
    }

    private void NewGame()
    {
        isGameOver = false;

        applesEaten = 0;
        gameTime = 0f;

        MoveSpeed = startingMoveSpeed;
        BodySpeed = MoveSpeed * BodySpeedMultiplier;

        transform.position = startPosition;
        transform.rotation = startRotation;

        foreach (GameObject body in BodyParts)
        {
            Destroy(body);
        }

        BodyParts.Clear();

        SetupStartingHistory();

        GrowSnake(); // starts with only one body cube

        if (FoodSpawner != null)
        {
            FoodSpawner.ResetFood();
        }

        if (GameOverPanel != null)
        {
            GameOverPanel.SetActive(false);
        }

        UpdateUI();

        Debug.Log("Game restarted.");
    }

    private void MoveForward()
    {
        if (Physics.Raycast(transform.position, transform.forward, WallCheckDistance, WallLayer))
        {
            GameOver("Game Over: snake hit the wall.");
            return;
        }

        transform.position += transform.forward * MoveSpeed * Time.deltaTime;
    }

    private void Steer()
    {
        float steerDirection = steerAction.ReadValue<float>();
        transform.Rotate(Vector3.up * steerDirection * SteerSpeed * Time.deltaTime);
    }

    private void SetupStartingHistory()
    {
        PositionsHistory.Clear();
        RotationsHistory.Clear();

        PositionsHistory.Add(transform.position);
        RotationsHistory.Add(transform.rotation);

        for (int i = 1; i < 500; i++)
        {
            Vector3 pointBehindHead = transform.position - transform.forward * HistoryRecordSpacing * i;

            PositionsHistory.Add(pointBehindHead);
            RotationsHistory.Add(transform.rotation);
        }
    }

    private void SavePositionHistory()
    {
        if (PositionsHistory.Count == 0)
        {
            PositionsHistory.Insert(0, transform.position);
            RotationsHistory.Insert(0, transform.rotation);
            return;
        }

        float distanceFromLastPoint = Vector3.Distance(transform.position, PositionsHistory[0]);

        if (distanceFromLastPoint >= HistoryRecordSpacing)
        {
            PositionsHistory.Insert(0, transform.position);
            RotationsHistory.Insert(0, transform.rotation);
        }

        int maxHistory = 2000;

        if (PositionsHistory.Count > maxHistory)
        {
            PositionsHistory.RemoveAt(PositionsHistory.Count - 1);
            RotationsHistory.RemoveAt(RotationsHistory.Count - 1);
        }
    }

    private void MoveBodyParts()
    {
        for (int i = 0; i < BodyParts.Count; i++)
        {
            GameObject body = BodyParts[i];

            float followDistance = (i + 1) * BodySpacing;

            Vector3 targetPoint = GetPointBehindHead(followDistance);
            Quaternion targetRotation = GetRotationBehindHead(followDistance);

            body.transform.position = Vector3.MoveTowards(
                body.transform.position,
                targetPoint,
                BodySpeed * Time.deltaTime
            );

            body.transform.rotation = Quaternion.RotateTowards(
                body.transform.rotation,
                targetRotation,
                BodyRotationSpeed * Time.deltaTime
            );
        }
    }

    private Vector3 GetPointBehindHead(float distanceBehindHead)
    {
        if (PositionsHistory.Count == 0)
        {
            return transform.position;
        }

        float distanceSoFar = 0f;

        for (int i = 1; i < PositionsHistory.Count; i++)
        {
            Vector3 newerPoint = PositionsHistory[i - 1];
            Vector3 olderPoint = PositionsHistory[i];

            float distanceBetweenPoints = Vector3.Distance(newerPoint, olderPoint);

            if (distanceSoFar + distanceBetweenPoints >= distanceBehindHead)
            {
                float remainingDistance = distanceBehindHead - distanceSoFar;
                float t = remainingDistance / distanceBetweenPoints;

                return Vector3.Lerp(newerPoint, olderPoint, t);
            }

            distanceSoFar += distanceBetweenPoints;
        }

        return PositionsHistory[PositionsHistory.Count - 1];
    }

    private Quaternion GetRotationBehindHead(float distanceBehindHead)
    {
        if (RotationsHistory.Count == 0)
        {
            return transform.rotation;
        }

        float distanceSoFar = 0f;

        for (int i = 1; i < PositionsHistory.Count; i++)
        {
            Vector3 newerPoint = PositionsHistory[i - 1];
            Vector3 olderPoint = PositionsHistory[i];

            float distanceBetweenPoints = Vector3.Distance(newerPoint, olderPoint);

            if (distanceSoFar + distanceBetweenPoints >= distanceBehindHead)
            {
                float remainingDistance = distanceBehindHead - distanceSoFar;
                float t = remainingDistance / distanceBetweenPoints;

                return Quaternion.Slerp(RotationsHistory[i - 1], RotationsHistory[i], t);
            }

            distanceSoFar += distanceBetweenPoints;
        }

        return RotationsHistory[RotationsHistory.Count - 1];
    }

    private void GrowSnake()
    {
        if (BodyPrefab == null)
        {
            Debug.LogError("BodyPrefab is missing.");
            return;
        }

        float spawnDistance = (BodyParts.Count + 1) * BodySpacing;
        Vector3 spawnPosition = GetPointBehindHead(spawnDistance);
        Quaternion spawnRotation = GetRotationBehindHead(spawnDistance);

        GameObject body = Instantiate(BodyPrefab, spawnPosition, spawnRotation);
        BodyParts.Add(body);
    }

    private void IncreaseSpeed()
    {
        MoveSpeed = Mathf.Min(MoveSpeed + SpeedIncreasePerFood, MaxMoveSpeed);
        BodySpeed = MoveSpeed * BodySpeedMultiplier;
    }

    private void CheckSelfCollision()
    {
        for (int i = SelfCollisionStartIndex; i < BodyParts.Count; i++)
        {
            float distance = Vector3.Distance(transform.position, BodyParts[i].transform.position);

            if (distance <= SelfCollisionDistance)
            {
                GameOver("Game Over: snake hit its own body.");
                return;
            }
        }
    }

    private void GameOver(string reason)
    {
        isGameOver = true;

        if (soundManager != null)
            soundManager.TriggerGameOverSound(); 

        MoveSpeed = 0f;
        BodySpeed = 0f;

        Debug.Log(reason);

        if (GameOverPanel != null)
        {
            GameOverPanel.SetActive(true);
        }

        if (GameOverReasonText != null)
        {
            GameOverReasonText.text = reason + "\nPress R to Restart";
        }
    }

    private void UpdateUI()
    {
        if (TimeText != null)
        {
            TimeText.text = "Time: " + gameTime.ToString("F1");
        }

        if (AppleText != null)
        {
            AppleText.text = "Apples: " + applesEaten;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isGameOver)
        {
            return;
        }

        if (FoodSpawner != null && FoodSpawner.IsFood(other.gameObject))
        {
            applesEaten++;

            // 1. Define the channels (swapped "noise" for "pulse1")
            string[] extraChannels = { "pulse1", "pulse2", "tri" };

            // 2. Pick one at random
            string chosenChannel = extraChannels[Random.Range(0, extraChannels.Length)];

            // 3. Generate a purely random volume between 0 and the 0.25 cap
            float randomizedVolume = Random.Range(0f, 0.25f);

            // 4. Send the new volume to Pure Data
            if (soundManager != null)
            {
                soundManager.SetChannelVolume(chosenChannel, randomizedVolume);
                Debug.Log("Randomly shifted " + chosenChannel + " to " + randomizedVolume);
            }

            soundManager.TriggerAppleEatSound();

            GrowSnake();
            IncreaseSpeed();
            FoodSpawner.CollectFood(other.gameObject);

            UpdateUI();

            Debug.Log("snake ate food, apples eaten: " + applesEaten);
        }
    }

    private void OnDestroy()
    {
        steerAction?.Dispose();
    }
}
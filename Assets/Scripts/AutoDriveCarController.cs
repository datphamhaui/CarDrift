using UnityEngine;

public class AutoDriveCarController : MonoBehaviour
{
    [Header("References")]
    public PrometeoCarController carController;
    public SteeringWheelUI steeringWheel;

    [Header("Auto-Drift Settings")]
    [Range(0.3f, 0.8f)]
    public float driftThreshold = 0.5f;

    [Header("Stuck Detection")]
    [Tooltip("Check interval in seconds")]
    public float stuckCheckInterval = 2f;
    [Tooltip("If car moves less than this distance in the interval, it's stuck")]
    public float stuckDistanceThreshold = 3f;

    bool isActive;
    float stuckCheckTimer;
    Vector3 lastCheckPosition;

    // Fake touch input objects - we control these to feed input to PrometeoCarController
    PrometeoTouchInput throttlePTI;
    PrometeoTouchInput reversePTI;
    PrometeoTouchInput turnLeftPTI;
    PrometeoTouchInput turnRightPTI;
    PrometeoTouchInput handbrakePTI;

    void Awake()
    {
        if (carController == null)
            carController = GetComponent<PrometeoCarController>();

        // Create hidden fake UI buttons for PrometeoCarController's touch control system
        CreateFakeTouchButtons();
    }

    void CreateFakeTouchButtons()
    {
        // We need a Canvas for RectTransform to work (PrometeoTouchInput.Start requires it)
        var canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            var canvasGO = new GameObject("FakeCanvas");
            canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        }

        carController.throttleButton = CreateFakeButton("FakeThrottle", canvas.transform);
        carController.reverseButton = CreateFakeButton("FakeReverse", canvas.transform);
        carController.turnLeftButton = CreateFakeButton("FakeTurnLeft", canvas.transform);
        carController.turnRightButton = CreateFakeButton("FakeTurnRight", canvas.transform);
        carController.handbrakeButton = CreateFakeButton("FakeHandbrake", canvas.transform);

        // Enable touch control mode on PrometeoCarController
        carController.useTouchControls = true;
    }

    GameObject CreateFakeButton(string name, Transform parent)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();
        go.AddComponent<PrometeoTouchInput>();
        go.SetActive(true);
        // Keep it invisible (no Image component, size 0)
        var rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = Vector2.zero;
        return go;
    } 

    void Start()
    {
        // Cache the PTI references (PrometeoCarController.Start will also grab them)
        throttlePTI = carController.throttleButton.GetComponent<PrometeoTouchInput>();
        reversePTI = carController.reverseButton.GetComponent<PrometeoTouchInput>();
        turnLeftPTI = carController.turnLeftButton.GetComponent<PrometeoTouchInput>();
        turnRightPTI = carController.turnRightButton.GetComponent<PrometeoTouchInput>();
        handbrakePTI = carController.handbrakeButton.GetComponent<PrometeoTouchInput>();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStart += OnGameStart;
            GameManager.Instance.OnGameOver += OnGameStop;
            GameManager.Instance.OnGameWin += OnGameStop;
        }
        else
        {
            OnGameStart();
        }
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStart -= OnGameStart;
            GameManager.Instance.OnGameOver -= OnGameStop;
            GameManager.Instance.OnGameWin -= OnGameStop;
        }
    }

    void OnGameStart()
    {
        isActive = true;
        stuckCheckTimer = 0f;
        lastCheckPosition = transform.position;

    }

    void OnGameStop()
    {
        isActive = false;
        ReleaseAllButtons();
    }

    void Update()
    {
        if (!isActive)
        {
            ReleaseAllButtons();
            return;
        }

        // --- STUCK DETECTION: game over if car barely moved ---
        stuckCheckTimer += Time.deltaTime;
        if (stuckCheckTimer >= stuckCheckInterval)
        {
            float movedDistance = Vector3.Distance(transform.position, lastCheckPosition);
            if (movedDistance < stuckDistanceThreshold && GameManager.Instance != null)
            {
                GameManager.Instance.TriggerGameOver();
                return;
            }
            lastCheckPosition = transform.position;
            stuckCheckTimer = 0f;
        }

        // --- NORMAL DRIVING ---
        float steer = 0f;
        if (steeringWheel != null)
            steer = steeringWheel.SteeringAmount;

        float absSteer = Mathf.Abs(steer);

        throttlePTI.buttonPressed = true;
        reversePTI.buttonPressed = false;

        turnLeftPTI.buttonPressed = steer < -0.05f;
        turnRightPTI.buttonPressed = steer > 0.05f;

        handbrakePTI.buttonPressed = absSteer > driftThreshold;
    }

    void ReleaseAllButtons()
    {
        if (throttlePTI != null) throttlePTI.buttonPressed = false;
        if (reversePTI != null) reversePTI.buttonPressed = false;
        if (turnLeftPTI != null) turnLeftPTI.buttonPressed = false;
        if (turnRightPTI != null) turnRightPTI.buttonPressed = false;
        if (handbrakePTI != null) handbrakePTI.buttonPressed = false;
    }
}

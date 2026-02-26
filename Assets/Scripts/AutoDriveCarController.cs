using UnityEngine;

public class AutoDriveCarController : MonoBehaviour
{
    [Header("References")]
    public PrometeoCarController carController;
    public SteeringWheelUI steeringWheel;

    [Header("Auto-Drift Settings")]
    [Range(0.3f, 0.8f)]
    public float driftThreshold = 0.5f;

    bool isActive;

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

        if (ScoreManager.Instance != null)
            ScoreManager.Instance.StartScoring();
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

        float steering = 0f;
        if (steeringWheel != null)
            steering = steeringWheel.SteeringAmount;

        float absSteering = Mathf.Abs(steering);

        // --- THROTTLE: always pressed (auto-drive forward) ---
        throttlePTI.buttonPressed = true;
        reversePTI.buttonPressed = false;

        // --- STEERING: left or right based on steering wheel ---
        turnLeftPTI.buttonPressed = steering < -0.05f;
        turnRightPTI.buttonPressed = steering > 0.05f;

        // --- HANDBRAKE: auto-drift when steering hard ---
        handbrakePTI.buttonPressed = absSteering > driftThreshold;
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

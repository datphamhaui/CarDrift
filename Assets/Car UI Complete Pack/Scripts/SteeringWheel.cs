using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

namespace CarUICompletePack
{
    public class SteeringWheel : MonoBehaviour
    {

        [Range(-1f, 1f)] public float steeringInput = 0f;
        [Min(0f)] public float maxSteerAngle = 270f;
        [Min(0f)] public float resetSpeed = 5f;
        [Min(0f)] public float deadZoneRadius = 5f;

        private RectTransform wheelTransform;
        private Image wheelImage;
        private CanvasGroup canvasGroup;
        private EventTrigger eventHandler;

        private float currentAngle = 0f, tempAngle = 0f, newAngle = 0f;
        private bool isSteering = false;
        private Vector2 centerPosition, touchPosition;

        void Awake()
        {
            InitializeWheel();
        }

        void OnEnable()
        {
            isSteering = false;
            steeringInput = 0f;
        }

        void LateUpdate()
        {
            HandleWheelRotation();
            steeringInput = CalculateSteeringInput();
        }

        private void InitializeWheel()
        {
            wheelImage = GetComponent<Image>();
            if (!wheelImage) return;

            wheelTransform = wheelImage.rectTransform;
            canvasGroup = wheelImage.GetComponent<CanvasGroup>();
            centerPosition = wheelTransform.position;
            SetupEventTriggers();
        }

        private void SetupEventTriggers()
        {
            eventHandler = wheelImage.GetComponent<EventTrigger>() ?? wheelImage.gameObject.AddComponent<EventTrigger>();
            eventHandler.triggers = new List<EventTrigger.Entry>();

            AddEvent(EventTriggerType.PointerDown, data =>
            {
                isSteering = true;
                touchPosition = ((PointerEventData)data).position;
                tempAngle = Vector2.Angle(Vector2.up, touchPosition - centerPosition);
            });

            AddEvent(EventTriggerType.Drag, data =>
            {
                touchPosition = ((PointerEventData)data).position;
            });

            AddEvent(EventTriggerType.EndDrag, data =>
            {
                isSteering = false;
            });
        }

        private void AddEvent(EventTriggerType type, System.Action<BaseEventData> action)
        {
            var entry = new EventTrigger.Entry { eventID = type };
            entry.callback.AddListener(new UnityEngine.Events.UnityAction<BaseEventData>(action));
            eventHandler.triggers.Add(entry);
        }

        private float CalculateSteeringInput()
        {
            return Mathf.Round(currentAngle / maxSteerAngle * 100) / 100;
        }

        private void HandleWheelRotation()
        {
            if (!canvasGroup || !wheelTransform) return;

            if (isSteering)
            {
                newAngle = Vector2.Angle(Vector2.up, touchPosition - centerPosition);

                if (Vector2.Distance(touchPosition, centerPosition) > deadZoneRadius)
                {
                    currentAngle += (touchPosition.x > centerPosition.x) ? (newAngle - tempAngle) : -(newAngle - tempAngle);
                }

                currentAngle = Mathf.Clamp(currentAngle, -maxSteerAngle, maxSteerAngle);
                tempAngle = newAngle;
            }
            else
            {
                currentAngle = Mathf.MoveTowards(currentAngle, 0f, resetSpeed * Time.deltaTime * 100f);
            }

            wheelTransform.eulerAngles = new Vector3(0f, 0f, -currentAngle);
        }

        void OnDisable()
        {
            isSteering = false;
            steeringInput = 0f;
        }
    }
}

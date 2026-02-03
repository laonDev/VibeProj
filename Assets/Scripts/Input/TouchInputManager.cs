using System;
using UnityEngine;

namespace AnimalKitchen
{
    /// <summary>
    /// 모바일 터치 입력 처리
    /// </summary>
    public class TouchInputManager : MonoBehaviour
    {
        public static TouchInputManager Instance { get; private set; }

        [Header("Settings")]
        [SerializeField] private float tapThreshold = 0.2f;
        [SerializeField] private float dragThreshold = 10f;

        public event Action<Vector2> OnTap;
        public event Action<Vector2> OnDragStart;
        public event Action<Vector2> OnDrag;
        public event Action<Vector2> OnDragEnd;
        public event Action<float> OnPinch;

        private Vector2 touchStartPos;
        private float touchStartTime;
        private bool isDragging;
        private float lastPinchDistance;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Update()
        {
            if (GameManager.Instance?.CurrentState != GameState.Playing) return;

#if UNITY_EDITOR || UNITY_STANDALONE
            HandleMouseInput();
#else
            HandleTouchInput();
#endif
        }

        private void HandleMouseInput()
        {
            if (Input.GetMouseButtonDown(0))
            {
                touchStartPos = Input.mousePosition;
                touchStartTime = Time.time;
                isDragging = false;
            }
            else if (Input.GetMouseButton(0))
            {
                Vector2 currentPos = Input.mousePosition;
                float distance = Vector2.Distance(touchStartPos, currentPos);

                if (!isDragging && distance > dragThreshold)
                {
                    isDragging = true;
                    OnDragStart?.Invoke(touchStartPos);
                }

                if (isDragging)
                {
                    OnDrag?.Invoke(currentPos);
                }
            }
            else if (Input.GetMouseButtonUp(0))
            {
                Vector2 endPos = Input.mousePosition;
                float duration = Time.time - touchStartTime;
                float distance = Vector2.Distance(touchStartPos, endPos);

                if (isDragging)
                {
                    OnDragEnd?.Invoke(endPos);
                }
                else if (duration < tapThreshold && distance < dragThreshold)
                {
                    OnTap?.Invoke(endPos);
                }

                isDragging = false;
            }

            // Mouse scroll for pinch simulation
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.01f)
            {
                OnPinch?.Invoke(1f + scroll);
            }
        }

        private void HandleTouchInput()
        {
            if (Input.touchCount == 1)
            {
                Touch touch = Input.GetTouch(0);

                switch (touch.phase)
                {
                    case TouchPhase.Began:
                        touchStartPos = touch.position;
                        touchStartTime = Time.time;
                        isDragging = false;
                        break;

                    case TouchPhase.Moved:
                        float distance = Vector2.Distance(touchStartPos, touch.position);
                        if (!isDragging && distance > dragThreshold)
                        {
                            isDragging = true;
                            OnDragStart?.Invoke(touchStartPos);
                        }
                        if (isDragging)
                        {
                            OnDrag?.Invoke(touch.position);
                        }
                        break;

                    case TouchPhase.Ended:
                    case TouchPhase.Canceled:
                        float duration = Time.time - touchStartTime;
                        float endDistance = Vector2.Distance(touchStartPos, touch.position);

                        if (isDragging)
                        {
                            OnDragEnd?.Invoke(touch.position);
                        }
                        else if (duration < tapThreshold && endDistance < dragThreshold)
                        {
                            OnTap?.Invoke(touch.position);
                        }
                        isDragging = false;
                        break;
                }
            }
            else if (Input.touchCount == 2)
            {
                // Pinch to zoom
                Touch touch0 = Input.GetTouch(0);
                Touch touch1 = Input.GetTouch(1);

                float currentDistance = Vector2.Distance(touch0.position, touch1.position);

                if (touch1.phase == TouchPhase.Began)
                {
                    lastPinchDistance = currentDistance;
                }
                else if (touch0.phase == TouchPhase.Moved || touch1.phase == TouchPhase.Moved)
                {
                    float delta = currentDistance / lastPinchDistance;
                    OnPinch?.Invoke(delta);
                    lastPinchDistance = currentDistance;
                }
            }
        }

        /// <summary>
        /// 스크린 좌표를 월드 좌표로 변환
        /// </summary>
        public Vector3 ScreenToWorld(Vector2 screenPos)
        {
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 10f));
            worldPos.z = 0;
            return worldPos;
        }
    }
}

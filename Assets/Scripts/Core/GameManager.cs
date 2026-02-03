using System;
using UnityEngine;

namespace AnimalKitchen
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Game State")]
        [SerializeField] private GameState currentState = GameState.Loading;

        [Header("References")]
        [SerializeField] private Restaurant currentRestaurant;

        public GameState CurrentState => currentState;
        public Restaurant CurrentRestaurant => currentRestaurant;

        public event Action<GameState> OnGameStateChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            Initialize();
        }

        private void Initialize()
        {
            SetGameState(GameState.Playing);
        }

        public void SetGameState(GameState newState)
        {
            if (currentState == newState) return;

            currentState = newState;
            OnGameStateChanged?.Invoke(currentState);

            Debug.Log($"[GameManager] State changed to: {currentState}");
        }

        public void PauseGame()
        {
            if (currentState == GameState.Playing)
            {
                SetGameState(GameState.Paused);
                Time.timeScale = 0f;
            }
        }

        public void ResumeGame()
        {
            if (currentState == GameState.Paused)
            {
                SetGameState(GameState.Playing);
                Time.timeScale = 1f;
            }
        }

        public void SetRestaurant(Restaurant restaurant)
        {
            currentRestaurant = restaurant;
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                SaveManager.Instance?.SaveGame();
            }
        }

        private void OnApplicationQuit()
        {
            SaveManager.Instance?.SaveGame();
        }
    }
}

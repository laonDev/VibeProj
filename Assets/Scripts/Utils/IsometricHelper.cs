using UnityEngine;

namespace AnimalKitchen
{
    /// <summary>
    /// 아이소메트릭 좌표 변환 및 정렬 유틸리티
    /// </summary>
    public static class IsometricHelper
    {
        // 2:1 아이소메트릭 비율
        private const float ISO_RATIO = 0.5f;

        /// <summary>
        /// 월드 좌표를 아이소메트릭 스크린 좌표로 변환
        /// </summary>
        public static Vector2 WorldToIso(Vector3 worldPos)
        {
            float isoX = worldPos.x - worldPos.y;
            float isoY = (worldPos.x + worldPos.y) * ISO_RATIO;
            return new Vector2(isoX, isoY);
        }

        /// <summary>
        /// 아이소메트릭 좌표를 월드 좌표로 변환
        /// </summary>
        public static Vector3 IsoToWorld(Vector2 isoPos)
        {
            float worldX = (isoPos.x / 2f) + isoPos.y;
            float worldY = isoPos.y - (isoPos.x / 2f);
            return new Vector3(worldX, worldY, 0);
        }

        /// <summary>
        /// 스크린 좌표(터치/클릭)를 아이소메트릭 그리드 좌표로 변환
        /// </summary>
        public static Vector2Int ScreenToGridPosition(Vector3 screenPos, Camera camera, float cellSize = 1f)
        {
            Vector3 worldPos = camera.ScreenToWorldPoint(screenPos);
            return WorldToGridPosition(worldPos, cellSize);
        }

        /// <summary>
        /// 월드 좌표를 그리드 좌표로 변환
        /// </summary>
        public static Vector2Int WorldToGridPosition(Vector3 worldPos, float cellSize = 1f)
        {
            int gridX = Mathf.RoundToInt(worldPos.x / cellSize);
            int gridY = Mathf.RoundToInt(worldPos.y / cellSize);
            return new Vector2Int(gridX, gridY);
        }

        /// <summary>
        /// 그리드 좌표를 월드 좌표로 변환
        /// </summary>
        public static Vector3 GridToWorldPosition(Vector2Int gridPos, float cellSize = 1f)
        {
            return new Vector3(gridPos.x * cellSize, gridPos.y * cellSize, 0);
        }

        /// <summary>
        /// 스프라이트 정렬 순서 계산 (Y축 기반)
        /// 화면 아래쪽 오브젝트가 앞에 보이도록
        /// </summary>
        public static int CalculateSortingOrder(Vector3 position, int baseOrder = 0)
        {
            // Y값이 작을수록 (화면 아래) 순서가 높음
            return baseOrder - Mathf.RoundToInt(position.y * 100);
        }
    }

    /// <summary>
    /// 자동으로 Y축 기준 스프라이트 정렬
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class IsometricSorting : MonoBehaviour
    {
        [SerializeField] private int baseOrder = 0;
        [SerializeField] private bool updateEveryFrame = true;

        private SpriteRenderer spriteRenderer;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void LateUpdate()
        {
            if (updateEveryFrame)
            {
                UpdateSortingOrder();
            }
        }

        public void UpdateSortingOrder()
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.sortingOrder = IsometricHelper.CalculateSortingOrder(transform.position, baseOrder);
            }
        }
    }
}

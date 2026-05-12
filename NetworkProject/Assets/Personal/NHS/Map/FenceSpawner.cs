using UnityEngine;

public class StageBuilder : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject _fencePrefab;
    [SerializeField] private GameObject _wallPrefab;

    [Header("Settings")]
    [SerializeField] private float _mapSize = 49f;
    [SerializeField] private float _fenceSpacing = 1.1f;
    [SerializeField] private float _wallSize = 5f; // 벽 한 칸의 가로 길이

    [ContextMenu("Build Stage")]
    public void BuildStage()
    {
        ClearStage();

        float halfSize = _mapSize / 2f;

        // 1. 펜스 설치 (기존 로직)
        int fenceCount = Mathf.FloorToInt(_mapSize / _fenceSpacing);
        for (int i = 0; i <= fenceCount; i++)
        {
            float pos = -halfSize + (i * _fenceSpacing);
            if (pos > halfSize + 0.01f) break;

            CreateObject(_fencePrefab, new Vector3(pos, 0, +halfSize), Quaternion.identity);
            CreateObject(_fencePrefab, new Vector3(pos, 0, -halfSize), Quaternion.identity);
            CreateObject(_fencePrefab, new Vector3(-halfSize, 0, pos), Quaternion.Euler(0, 90, 0));
            CreateObject(_fencePrefab, new Vector3(+halfSize, 0, pos), Quaternion.Euler(0, 90, 0));
        }

        // 2. 벽 설치 (펜스 바로 뒤쪽)
        // 펜스 위치에서 벽 두께의 절반만큼 바깥으로 밀어냅니다.
        float wallOffset = halfSize + (_wallSize / 2f);
        int wallCount = Mathf.CeilToInt((_mapSize + _wallSize * 2) / _wallSize);

        for (int i = 0; i < wallCount; i++)
        {
            // 벽의 중심점 기준으로 배치 좌표 계산
            float pos = -(wallOffset) + (i * _wallSize);

            // 상단 벽
            CreateObject(_wallPrefab, new Vector3(pos, _wallSize / 2f, wallOffset), Quaternion.identity);
            // 하단 벽
            CreateObject(_wallPrefab, new Vector3(pos, _wallSize / 2f, -wallOffset), Quaternion.identity);
            // 좌측 벽
            CreateObject(_wallPrefab, new Vector3(-wallOffset, _wallSize / 2f, pos), Quaternion.Euler(0, 90, 0));
            // 우측 벽
            CreateObject(_wallPrefab, new Vector3(wallOffset, _wallSize / 2f, pos), Quaternion.Euler(0, 90, 0));
        }
    }

    private void CreateObject(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (prefab == null) return;
        GameObject obj = Instantiate(prefab, position, rotation);
        obj.transform.parent = this.transform;
    }

    [ContextMenu("Clear Stage")]
    public void ClearStage()
    {
        for (int i = this.transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(this.transform.GetChild(i).gameObject);
        }
    }
}
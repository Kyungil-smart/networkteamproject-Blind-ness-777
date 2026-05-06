using Unity.Netcode;
using UnityEngine;

public class MapLoader : NetworkBehaviour
{
    [SerializeField] private GameObject[] _mapPrefabs;
    private NetworkObject _currentMap;
    public Transform[] AISpawnPoints { get; private set; }

    public void LoadMap()
    {
        if (!IsServer) return;
        
        int _random = Random.Range(0, _mapPrefabs.Length);
        GameObject _mapPrefab = Instantiate(_mapPrefabs[_random]);
        _currentMap = _mapPrefab.GetComponent<NetworkObject>();
        _currentMap.Spawn();
        
        Transform aiSpawnPoint = _currentMap.transform.Find("AiSpawnPoint");
        AISpawnPoints = new Transform[aiSpawnPoint.childCount];
        for (int i = 0; i < aiSpawnPoint.childCount; i++)
            AISpawnPoints[i] = aiSpawnPoint.GetChild(i);
    }
    
    public void DestroyMap()
    {
        // if (!IsServer) return;
        if (_currentMap == null) return;
        _currentMap.Despawn(true); 
        _currentMap = null;
    }
}

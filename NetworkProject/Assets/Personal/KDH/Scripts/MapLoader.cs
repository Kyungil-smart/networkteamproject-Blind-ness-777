using Unity.AI.Navigation;
using Unity.Netcode;
using UnityEngine;

public class MapLoader : NetworkBehaviour
{
    [SerializeField] private GameObject[] _mapPrefabs;
    private NetworkObject _currentMap;

    public void LoadMap()
    {
        if (!IsServer) return;
        
        int _random = Random.Range(0, _mapPrefabs.Length);
        GameObject _mapPrefab = Instantiate(_mapPrefabs[_random]);
        _currentMap = _mapPrefab.GetComponent<NetworkObject>();
        _currentMap.Spawn();
        
        NavMeshSurface _nms = _mapPrefab.GetComponent<NavMeshSurface>();
        _nms.BuildNavMesh();
    }
    
    public void DestroyMap()
    {
        // if (!IsServer) return;
        if (_currentMap == null) return;
        _currentMap.Despawn(true); 
        _currentMap = null;
    }
}

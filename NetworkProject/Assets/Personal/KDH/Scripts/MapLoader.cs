using System.Collections;
using Unity.AI.Navigation;
using Unity.Netcode;
using UnityEngine;

public class MapLoader : NetworkBehaviour
{
    [SerializeField] private GameObject[] _mapPrefabs;
    private NetworkObject _currentMap;

    public IEnumerator LoadMap()
    {
        if (!IsServer) yield break;
        Debug.Log("Loading map");
        int _random = Random.Range(0, _mapPrefabs.Length);
        GameObject _mapPrefab = Instantiate(_mapPrefabs[_random]);
        _currentMap = _mapPrefab.GetComponent<NetworkObject>();
        _currentMap.Spawn();
        
        yield return new WaitForSeconds(2f);
        
        NavMeshSurface _nms = _mapPrefab.GetComponent<NavMeshSurface>();
        if (_nms != null)
        {
            _nms.BuildNavMesh();
            yield return null;
        }
    }
    
    public void DestroyMap()
    {
        // if (!IsServer) return;
        if (_currentMap == null) return;
        _currentMap.Despawn(true); 
        _currentMap = null;
    }
}

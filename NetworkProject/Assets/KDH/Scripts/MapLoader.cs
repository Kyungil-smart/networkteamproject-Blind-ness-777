using Unity.Netcode;
using UnityEngine;

public class MapLoader : NetworkBehaviour
{
    [SerializeField] private GameObject[] _mapPrefabs;

    public void LoadMap()
    {
        if (!IsServer) return;
        
        int _random = Random.Range(0, _mapPrefabs.Length);
        GameObject _mapPrefab = Instantiate(_mapPrefabs[_random]);
        _mapPrefab.GetComponent<NetworkObject>().Spawn();
    }
}

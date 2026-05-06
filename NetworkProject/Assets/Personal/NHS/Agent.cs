using Unity.Netcode;
using UnityEngine;

public class Agent : NetworkBehaviour
{
    private float _greetCoolTime  = 10;
    private float _greetTimer     = 0;
    private bool  _isGreet        = false;
    public  bool  isGreet { get => _isGreet; set => _isGreet = value; }


    void Update()
    {
        if (isGreet == false) return;

        if(_greetTimer < _greetCoolTime)
        {
            _greetTimer += Time.deltaTime;
        }
        else
        {
            _greetTimer = 0;
            isGreet = false;
        }
    }
}
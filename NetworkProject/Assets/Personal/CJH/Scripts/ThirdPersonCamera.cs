using UnityEngine;
using Unity.Netcode;

/// <summary>
/// FreeLook Camera 오브젝트에 부착.
/// Cinemachine 제거 후 이 스크립트 단독으로 카메라 제어.
/// PlayerController에서 IsOwner 아닐 시 이 컴포넌트 비활성화할 것.
/// </summary>
public class ThirdPersonCamera : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform _target; // TargetPoint

    [Header("Distance")]
    [SerializeField] private float _distance    = 4f;
    [SerializeField] private float _minDistance = 1.5f;
    [SerializeField] private float _maxDistance = 6f;

    [Header("Vertical Angle")]
    [SerializeField] private float _defaultPitch = 20f;
    [SerializeField] private float _minPitch     = 5f;  // 발밑 안 보이게
    [SerializeField] private float _maxPitch     = 70f;

    [Header("Sensitivity")]
    [SerializeField] private float _sensitivityX = 3f;
    [SerializeField] private float _sensitivityY = 2f;
    [SerializeField] private float _smoothing    = 10f;

    [Header("Occlusion")]
    [SerializeField] private LayerMask _obstacleMask;
    [SerializeField] private float     _occlusionPullSpeed = 15f;

    private float _yaw;
    private float _pitch;
    private float _currentDistance;

    private void Start()
    {
        _yaw             = transform.eulerAngles.y;
        _pitch           = _defaultPitch;
        _currentDistance = _distance;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;
    }

    private void LateUpdate()
    {
        if (_target == null) return;
        
        HandleRotation();
        HandleOcclusion();
        ApplyTransform();
    }
    
    private void HandleRotation()
    {
        float mouseX = Input.GetAxis("Mouse X") * _sensitivityX;
        float mouseY = Input.GetAxis("Mouse Y") * _sensitivityY;

        _yaw   += mouseX;
        _pitch  = Mathf.Clamp(_pitch - mouseY, _minPitch, _maxPitch);
    }

    private void HandleOcclusion()
    {
        Vector3 desiredCamPos = GetCameraPosition(_distance);

        if (Physics.Linecast(_target.position, desiredCamPos, out RaycastHit hit, _obstacleMask))
        {
            float safeDistance = Mathf.Max(hit.distance - 0.2f, _minDistance);
            _currentDistance = Mathf.Lerp(_currentDistance, safeDistance, _occlusionPullSpeed * Time.deltaTime);
        }
        else
        {
            _currentDistance = Mathf.Lerp(_currentDistance, _distance, _occlusionPullSpeed * Time.deltaTime);
        }
    }

    private void ApplyTransform()
    {
        Vector3 camPos = GetCameraPosition(_currentDistance);

        transform.position = Vector3.Lerp(transform.position, camPos, _smoothing * Time.deltaTime);
        transform.LookAt(_target.position);
    }

    private Vector3 GetCameraPosition(float dist)
    {
        Quaternion rotation = Quaternion.Euler(_pitch, _yaw, 0f);
        return _target.position + rotation * new Vector3(0f, 0f, -dist);
    }

    // PlayerController에서 호출 — 오너 아닌 플레이어 카메라 비활성화
    public void SetActive(bool active)
    {
        enabled = active;
        GetComponent<Camera>()?.gameObject.SetActive(active);
    }
}
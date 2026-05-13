using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour, IPhaseChangeable
{
    [Header("Target")]
    [SerializeField] private Transform _target;
    [SerializeField] private float _heightOffset = 1.5f;

    [Header("Distance")]
    [SerializeField] private float _distance    = 4f;
    [SerializeField] private float _minDistance = 1.5f;
    [SerializeField] private float _maxDistance = 6f;

    [Header("Vertical Angle")]
    [SerializeField] private float _defaultPitch = 20f;
    [SerializeField] private float _minPitch     = 5f;
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
    private bool  _isActive = true;

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
        if (_target == null || !_isActive) return;

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
        Vector3 targetPos    = _target.position + Vector3.up * _heightOffset;
        Vector3 desiredCamPos = GetCameraPosition(_distance);

        if (Physics.Linecast(targetPos, desiredCamPos, out RaycastHit hit, _obstacleMask))
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
        transform.LookAt(_target.position + Vector3.up * _heightOffset);
    }

    private Vector3 GetCameraPosition(float dist)
    {
        Vector3    targetPos = _target.position + Vector3.up * _heightOffset;
        Quaternion rotation  = Quaternion.Euler(_pitch, _yaw, 0f);
        return targetPos + rotation * new Vector3(0f, 0f, -dist);
    }

    public void SetTarget(Transform target) => _target = target;

    public void SetActive(bool active) => _isActive = active;

    public void OnPhaseChanged(GamePhase phase)
    {
        switch (phase)
        {
            case GamePhase.HideAndSeek:
                _isActive            = true;
                gameObject.SetActive(true);
                Cursor.lockState     = CursorLockMode.Locked;
                Cursor.visible       = false;
                break;

            case GamePhase.Shooting:
                _isActive = false;
                // 탑뷰 전환 완료 후 TopViewCamera에서 비활성화 호출
                break;
        }
    }
}
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public Transform target; // the target to follow and rotate around

    const string INPUT_MOUSE_SCROLLWHEEL = "Mouse ScrollWheel";
    const string INPUT_MOUSE_X = "Mouse X";
    const string INPUT_MOUSE_Y = "Mouse Y";
    const float MIN_CAM_DISTANCE = 1f;
    const float MAX_CAM_DISTANCE = 200f;

    // how fast the camera orbits
    [Range(2f, 15f)]
    public float orbitSpeed = 6f;

    // how fast the camera zooms in and out
    [Range(.3f, 50f)]
    public float zoomSpeed = .8f;

    [Range(0.1f, 1f)]
    public float dragSpeed = 0.2f;

    [Range(0.1f, 1.0f)]
    public float fovRatio = 0.5f;

    // the current distance from pivot point (locked to Vector3.zero)
    float distance = 0f;
    private Vector3 mouseOrigin;
    private Vector3 currentPosition;
    private bool isDragging;
    private Vector3 originPosition;
    private Quaternion originRotate;
    private Vector3 origin;

    private Camera controlCam;
    private float originSize;
    private bool onMouse = false;
    internal bool isRotate = true;

    private void Start()
    {
        controlCam = gameObject.GetComponent<Camera>();
    }

    void LateUpdate()
    {
        IsMouseOnCamera();

        if (onMouse)
        {
            // orbits
            if (Input.GetMouseButton(0))
            {
                float rot_x = Input.GetAxis(INPUT_MOUSE_X);
                float rot_y = -Input.GetAxis(INPUT_MOUSE_Y);

                Vector3 eulerRotation = transform.localRotation.eulerAngles;

                if (isRotate)
                {
                    eulerRotation.x += rot_y * orbitSpeed;
                    eulerRotation.y += rot_x * orbitSpeed;

                    eulerRotation.z = 0f;
                }

                Vector3 currentPosition = target.position - (transform.localRotation * (Vector3.forward * distance));
                transform.SetPositionAndRotation(currentPosition, Quaternion.Euler(eulerRotation));

            }

            if (Input.GetMouseButtonDown(1))
            {
                isDragging = true;
                mouseOrigin = Input.mousePosition;
            }

            if (Input.GetMouseButtonUp(1))
            {
                isDragging = false;
            }

            if (isDragging)
            {
                currentPosition = Input.mousePosition;
                Vector3 moveVector = (currentPosition - mouseOrigin) * dragSpeed * Time.deltaTime;
                transform.Translate(moveVector);
            }

            if (Input.GetAxis(INPUT_MOUSE_SCROLLWHEEL) != 0f)
            {
                float delta = Input.GetAxis(INPUT_MOUSE_SCROLLWHEEL);

                if (controlCam.orthographic)
                {
                    distance = controlCam.orthographicSize;
                    distance -= delta * zoomSpeed;

                    controlCam.orthographicSize = distance < MAX_CAM_DISTANCE && distance > MIN_CAM_DISTANCE ? distance : MIN_CAM_DISTANCE;
                }
                else
                {
                    // 새로운 카메라 거리 계산
                    distance -= delta * zoomSpeed;

                    // 카메라 거리를 최소값과 최대값 사이로 제한
                    distance = Mathf.Clamp(distance, MIN_CAM_DISTANCE, MAX_CAM_DISTANCE);

                    // 현재 카메라의 방향 계산
                    Vector3 cameraDirection = transform.position - target.position;

                    // 카메라의 새 위치 계산 
                    Vector3 newCameraPosition = target.position + cameraDirection.normalized * distance;

                    // 카메라 위치 업데이트
                    transform.position = newCameraPosition;
                }
            }
        }
        else
        {
            isDragging = false;
        }
    }

    public void RecalculateCamera()
    {
        transform.SetPositionAndRotation(originPosition, originRotate);
    }

    public void InitCameraPosition()
    {
        origin = transform.position;

        float maxDimension = Mathf.Max(origin.x, origin.y, origin.z) * 0.5f;

        float cameraDistance = maxDimension / Mathf.Tan(Mathf.Deg2Rad * GetComponent<Camera>().fieldOfView * fovRatio);
        distance = Mathf.Clamp(cameraDistance, MIN_CAM_DISTANCE, MAX_CAM_DISTANCE);

        originPosition = target.position - (transform.localRotation * (Vector3.forward * distance));
        originRotate = Quaternion.Euler(transform.localRotation.eulerAngles);
    }

    public void SetOrthographicSize(Bounds bounds)
    {
        float requiredSize = (bounds.size.y + bounds.size.x + bounds.size.z) / 3; //Mathf.Max(bounds.extents.y, bounds.extents.x, bounds.extents.z);

        originSize = requiredSize; // *1.3f;
        controlCam.orthographicSize = originSize;
    }

    public void GetOriginOrthographicSize()
    {
        if (controlCam.orthographic)
            controlCam.orthographicSize = originSize;
    }

    private void IsMouseOnCamera()
    {
        Vector3 mousePosition = Input.mousePosition;

        Camera camera = GetComponent<Camera>();

        Vector3 viewportPoint = camera.ScreenToViewportPoint(mousePosition);

        if (viewportPoint.x >= 0 && viewportPoint.x <= 1 &&
            viewportPoint.y >= 0 && viewportPoint.y <= 1)
        {
            onMouse = true;
        }
        else
        {
            onMouse = false;
        }
    }
}

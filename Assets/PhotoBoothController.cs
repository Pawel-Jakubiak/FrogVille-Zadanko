using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhotoBoothController : MonoBehaviour
{
    [Header("Model Settings")]
    [SerializeField] private GameObject modelParent;
    public float rotationSpeed;
    public float dragSpeed;
    public float zoomSpeed;

    [SerializeField] private List<GameObject> models;
    private GameObject currentModel;
    private int modelIndex = 0;

    [Header("Camera Settings")]
    public float maxZoomIn = .1f;
    public float maxZoomOut = 5;

    private Camera cam;

    [Header("UI Settings")]
    [SerializeField] private Canvas canvas;
    [SerializeField] private GameObject missingModelsText;

    private void Awake()
    {
        GameObject[] loadedModels = Resources.LoadAll<GameObject>("Input");

        models.AddRange(loadedModels);
    }

    private void Start()
    {
        cam = Camera.main;

        if (models.Count == 0) missingModelsText.SetActive(true);

        SpawnModel();
    }

    private void Update()
    {
        HandleRotation();
        HandleZoom();
    }

    private void HandleRotation()
    {
        Vector2 mouseAxis = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

        if (Input.GetMouseButton(0))
        {
            modelParent.transform.Rotate(Vector3.down, mouseAxis.x * rotationSpeed, Space.World);
            modelParent.transform.Rotate(Vector3.right, mouseAxis.y * rotationSpeed, Space.World);
        }

        if (Input.GetMouseButton(1))
        {
            modelParent.transform.Translate(mouseAxis * (dragSpeed * cam.orthographicSize) * Time.deltaTime, Space.World);
        }
    }

    private void HandleZoom()
    {
        float scrollWheelAxis = Input.GetAxis("Mouse ScrollWheel");

        if (scrollWheelAxis != 0)
        {
            Vector3 direction = cam.ScreenToWorldPoint(Input.mousePosition);
            float newZoomDistance = cam.orthographicSize - (Input.GetAxis("Mouse ScrollWheel") * zoomSpeed);

            newZoomDistance = Mathf.Clamp(newZoomDistance, maxZoomIn, maxZoomOut);

            cam.orthographicSize = newZoomDistance;

            if (cam.orthographicSize != maxZoomIn && scrollWheelAxis > 0)
            {
                cam.transform.position = Vector3.MoveTowards(cam.transform.position, direction, 10f * Time.deltaTime);
            }
        }
    }

    public void GetNextModel(bool previous = false)
    {
        modelIndex = previous ? modelIndex -= 1 : modelIndex += 1;
        modelIndex = Mathf.Clamp(modelIndex, 0, models.Count - 1);

        SpawnModel();
    }

    public void PhotoButton() => StartCoroutine(TakePhoto());

    public IEnumerator TakePhoto()
    {
        if (models.Count == 0) yield break;

        if (!Directory.Exists(Application.dataPath + "/Output"))
            Directory.CreateDirectory(Application.dataPath + "/Output");

        canvas.enabled = false;

        string screenshotName = DateTime.UtcNow.ToLocalTime().ToString("dd-MM-yyyy_hh-mm-ss-ff");

        ScreenCapture.CaptureScreenshot(Application.dataPath + $"/Output/{screenshotName}.png");

        yield return new WaitForFixedUpdate();

        canvas.enabled = true;
    }

    private void SpawnModel()
    {
        if (models.Count == 0) return;

        if (currentModel) Destroy(currentModel);
        currentModel = Instantiate(models[modelIndex], modelParent.transform);
    }
}

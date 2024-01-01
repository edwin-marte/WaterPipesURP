using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SpawnSystem : MonoBehaviour
{
    public GameObject objectPrefab; // The prefab of the object to be spawned
    public GameObject elbowPrefab;
    public Material objectMaterial; // The material for the spawned object
    public Material ghostMaterial;
    public float rotationSpeed = 40f; // The speed at which the object rotates
    public LayerMask objectLayerMask;
    public LayerMask groundLayerMask;
    public GridSystem gridSystem;
    public SpawnSystemUI spawnSystemUI;

    public static event Action OnExitedBuildMode;

    private bool isRotating; // Flag to indicate if the object is rotating
    private GameObject ghostObject; // Reference to the ghost object
    private float floorYOffset = 0.26f; // Offset to ensure the object is slightly above the floor
    private bool isDragging = false;
    private bool hasDragged = false;
    private bool rightClickHeld = false;
    private GameObject firstSpawn = null;
    private bool isFirstSpawnDone = false;
    private Vector3 startDragPosition = new Vector3(float.MinValue, float.MinValue, float.MinValue);

    private void Start()
    {
        SpawnSystemUI.OnPipeSelected += EnterBuildMode;
    }

    private void OnDisable()
    {
        SpawnSystemUI.OnPipeSelected -= EnterBuildMode;
    }

    private void EnterBuildMode()
    {
        InitGhostObject();
    }

    private void ExitBuildMode()
    {
        if (ghostObject != null)
        {
            Destroy(ghostObject);
        }
    }

    private void InitGhostObject()
    {
        if (ghostObject == null && ghostMaterial != null)
        {
            ghostObject = Instantiate(objectPrefab, Vector3.zero, transform.rotation);
            GhostObjectSetup();

            int ghostLayerIndex = LayerMask.NameToLayer("Ghost Object");

            if (ghostLayerIndex > 0)
            {
                ghostObject.layer = ghostLayerIndex;
            }
        }
    }

    private void Update()
    {
        HandleBuildMode();
        UpdateGhostTransform();
        HandleMouseInput();
        HandleRotationInput();
        HandleObjectInteraction();
    }

    private void HandleBuildMode()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            spawnSystemUI.ExitBuildMode();
            ExitBuildMode();
            OnExitedBuildMode?.Invoke();
        }
    }

    private void HandleObjectInteraction()
    {
        var isOverObject = IsOverObject();

        if (isOverObject.Item1)
        {
            SpawnObject();
        }
        else
        {
            DeleteObject(isOverObject.Item2);
        }
    }

    private void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0) && !spawnSystemUI.IsInteractingWithUI())
        {
            isDragging = true;
            hasDragged = false;
            startDragPosition = transform.position;
        }
        else if (Input.GetMouseButton(0))
        {
            if (isDragging)
            {
                hasDragged = true;
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
            firstSpawn = null;
            isFirstSpawnDone = false;

            if (hasDragged)
            {
                hasDragged = false;
            }
        }

        rightClickHeld = Input.GetMouseButton(1);
    }

    private void HandleRotationInput()
    {
        if (!isRotating && Input.GetKeyDown(KeyCode.R))
        {
            StartCoroutine(RotateObject());
        }
    }

    private void DeleteObject(GameObject collidingObject)
    {
        if (rightClickHeld && collidingObject != null)
        {
            var pipeSegment = collidingObject.GetComponent<PipeSegment>();

            if (pipeSegment == null) return;

            ghostObject.transform.rotation = collidingObject.transform.rotation;

            if (pipeSegment.fatherLine != null)
            {
                if (pipeSegment.fatherLine.GetPipeSegments().Contains(pipeSegment))
                {
                    pipeSegment.previousSegment?.SetNextSegment(null);
                    pipeSegment.nextSegment?.SetPreviousSegment(null);
                    pipeSegment.fatherLine.GetPipeSegments().Remove(pipeSegment);
                }

                pipeSegment.ClearExtractor();
            }
            Destroy(collidingObject);
        }
    }

    private IEnumerator RotateObject()
    {
        isRotating = true;

        Quaternion startRotation = transform.rotation;
        Quaternion targetRotation = transform.rotation * Quaternion.Euler(0f, 90f, 0f);
        float totalRotation = 90f;
        float elapsedTime = 0f;

        while (elapsedTime < totalRotation)
        {
            float rotationAmount = (rotationSpeed * 20) * Time.deltaTime;
            elapsedTime += rotationAmount;

            // Adjust the rotation amount to ensure accurate rotation
            if (elapsedTime > totalRotation)
            {
                rotationAmount -= (elapsedTime - totalRotation);
            }

            transform.rotation *= Quaternion.Euler(0f, rotationAmount, 0f);
            yield return null;
        }

        ghostObject.transform.rotation = targetRotation;
        isRotating = false;
    }

    private (bool, GameObject) IsOverObject()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, objectLayerMask))
        {
            Debug.DrawLine(ray.origin, hit.point, Color.blue);
            return (false, hit.collider.gameObject);
        }
        return (true, null);
    }

    private void SpawnObject()
    {
        if (hasDragged)
        {
            SpawnSetup();
        }
    }

    GameObject previousSpawn = null;
    private void SpawnSetup()
    {
        if (objectPrefab != null && ghostObject != null)
        {
            Vector3 spawnPosition = ghostObject.transform.position;
            spawnPosition.y = floorYOffset;
            GameObject lastSpawn = Instantiate(objectPrefab, gridSystem.SetCell(spawnPosition), transform.rotation);

            Vector3 direction = gridSystem.CurrentCell - gridSystem.PreviousCell;

            SpawnedObjectSetup(lastSpawn);
            SetDirection(direction, lastSpawn);

            if (firstSpawn == null)
            {
                firstSpawn = lastSpawn;
            }
            else if (!isFirstSpawnDone)
            { 
                // If direction changes after the first spawn, corrent the first spawned segment angle
                var pipeSegment = firstSpawn.GetComponent<PipeSegment>();

                pipeSegment?.CorrectSegment(lastSpawn);

                isFirstSpawnDone = true;
            }

            previousSpawn = lastSpawn;
        }
    }

    private bool VectorIsNull()
    {
        return startDragPosition == NullVector();
    }

    private Vector3 NullVector()
    {
        return new Vector3(float.MinValue, float.MinValue, float.MinValue);
    }

    public enum Direction
    {
        None,
        Up,
        Down,
        Left,
        Right
    }

    Direction previousDirection = Direction.None;
    Direction newDirection = Direction.None;

    public void SetDirection(Vector3 dragDirection, GameObject spawnedObject)
    {
        // Default, no drag
        if (!VectorIsNull() && startDragPosition == transform.position)
        {
            spawnedObject.transform.rotation = ghostObject.transform.rotation;
            transform.rotation = ghostObject.transform.rotation;
            startDragPosition = NullVector();

            previousDirection = Direction.None;
            newDirection = Direction.None;
        }
        else
        {
            if (Mathf.Abs(dragDirection.x) > Mathf.Abs(dragDirection.z))
            {
                if (dragDirection.x > 0)
                {
                    // Right
                    CalculateDirection(spawnedObject, 0f);
                    newDirection = Direction.Right;
                }
                else if (dragDirection.x < 0)
                {
                    // Left
                    CalculateDirection(spawnedObject, 180f);
                    newDirection = Direction.Left;
                }
            }
            else
            {
                if (dragDirection.z > 0)
                {
                    // Up
                    CalculateDirection(spawnedObject, 270f);
                    newDirection = Direction.Up;
                }
                else if (dragDirection.z < 0)
                {
                    // Down
                    CalculateDirection(spawnedObject, 90f);
                    newDirection = Direction.Down;
                }
            }
        }

        if (newDirection != previousDirection)
        {
            if (previousDirection != Direction.None && previousSpawn != null)
            {
                var elbowPosition = gridSystem.SetCell(previousSpawn.transform.position);
                elbowPosition.y = 0.278f;

                var elbow = Instantiate(elbowPrefab, elbowPosition, transform.rotation);
                SetupElbow(elbow, previousDirection, newDirection);
                Destroy(previousSpawn);
            }

            previousDirection = newDirection;
        }
    }

    private void SetupElbow(GameObject elbow, Direction previousDir, Direction newDir)
    {
        if (previousDir == Direction.Right && newDir == Direction.Down || previousDir == Direction.Up && newDir == Direction.Left)
        {
            elbow.transform.rotation = Quaternion.Euler(-90f, 180f, transform.position.z);
        }
        else if (previousDir == Direction.Left && newDir == Direction.Down || previousDir == Direction.Up && newDir == Direction.Right)
        {
            elbow.transform.rotation = Quaternion.Euler(-90f, 90f, transform.position.z);
        }
        else if (previousDir == Direction.Right && newDir == Direction.Up || previousDir == Direction.Down && newDir == Direction.Left)
        {
            elbow.transform.rotation = Quaternion.Euler(-90f, -90f, transform.position.z);
        }
        else if (previousDir == Direction.Left && newDir == Direction.Up || previousDir == Direction.Down && newDir == Direction.Right)
        {
            elbow.transform.rotation = Quaternion.Euler(-90f, 0f, transform.position.z);
        }
    }

    private void CalculateDirection(GameObject spawnedObject, float value)
    {
        Quaternion targetRotation = Quaternion.Euler(0f, value, 0f);
        transform.rotation = targetRotation;
        spawnedObject.transform.rotation = targetRotation;
        ghostObject.transform.rotation = targetRotation;
    }

    private void SpawnedObjectSetup(GameObject obj)
    {
        Vector3 spawnScale = ghostObject.transform.localScale;
        obj.transform.localScale = spawnScale;

        Destroy(obj.transform.Find("Arrow_Start")?.gameObject);
        Destroy(obj.transform.Find("Arrow_End")?.gameObject);

        if (obj.TryGetComponent(out PipeSegment pipeSegment))
        {
            pipeSegment.isGhostSegment = false;
        }

        Renderer objectRenderer = obj.GetComponent<Renderer>();
        if (objectRenderer != null)
        {
            objectRenderer.material = objectMaterial;
        }
    }

    private void GhostObjectSetup()
    {
        ghostObject.GetComponent<Renderer>().material = ghostMaterial;
        ghostObject.GetComponent<PipeSegment>().isGhostSegment = true;
        Vector3 ghostPosition = ghostObject.transform.position;
        ghostPosition = new Vector3(ghostPosition.x, floorYOffset, ghostPosition.z);
    }

    private void UpdateGhostTransform()
    {
        if (ghostObject != null)
        {
            if (isRotating)
                ghostObject.transform.rotation = transform.rotation;

            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, groundLayerMask))
            {
                Vector3 newPosition = new Vector3(hit.point.x, hit.point.y + floorYOffset, hit.point.z);
                ghostObject.transform.position = gridSystem.AdjustToGrid(newPosition);

                Debug.DrawLine(ray.origin, hit.point, Color.red);
            }
        }
    }
}

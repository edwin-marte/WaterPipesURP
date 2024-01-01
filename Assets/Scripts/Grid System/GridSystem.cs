using UnityEngine;

public class GridSystem : MonoBehaviour
{
    public float cellSize = 1f; // Size of each grid cell
    private Vector3 previousCell;
    private Vector3 currentCell;

    public Vector3 PreviousCell { get { return previousCell; } }
    public Vector3 CurrentCell { get { return currentCell; } }

    public Vector3 AdjustToGrid(Vector3 position)
    {
        // Round the position to the nearest multiple of cellSize
        float x = Mathf.Round(position.x / cellSize) * cellSize;
        float y = position.y;
        float z = Mathf.Round(position.z / cellSize) * cellSize;

        return new Vector3(x, y, z);
    }

    public Vector3 SetCell(Vector3 position)
    {
        previousCell = currentCell;
        currentCell = AdjustToGrid(position);

        return currentCell;
    }
}

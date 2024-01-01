using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PipeSegment : MonoBehaviour
{
    public List<Transform> segmentPath;
    public float maxCapacity = 3f;
    public PipesLine fatherLine;
    public bool isGhostSegment = false;
    public PipeSegment nextSegment = null;
    public PipeSegment previousSegment = null;
    public WaterExtractor extractor = null;

    public bool corrected = false;

    private void Start()
    {
        if (!isGhostSegment)
        {
            // Checks for collisions multiple times to increase accuracy
            StartCoroutine(ExecuteMethods());
        }
    }

    public void SetNextSegment(PipeSegment segment)
    {
        nextSegment = segment;
    }

    public void SetPreviousSegment(PipeSegment segment)
    {
        previousSegment = segment;
    }


    // Corrects a segment angle and updates its connected pipes
    public void CorrectSegment(GameObject nextObject)
    {
        float angle = Quaternion.Angle(this.transform.rotation, nextObject.transform.rotation);
        float threshold = 0.1f;

        if (angle >= threshold)
        {
            this.transform.rotation = nextObject.transform.rotation;
            nextSegment?.SetPreviousSegment(null);
            previousSegment?.SetNextSegment(null);
            ClearExtractor(nextObject.GetComponent<PipeSegment>());
            SetNextSegment(null);
            SetPreviousSegment(null);
        }
    }

    public void ClearExtractor()
    {
        if (extractor != null)
        {
            fatherLine = null;
            extractor.connectedSegment = null;
            extractor = null;
        }
    }

    public void ClearExtractor(PipeSegment nextSeg)
    {
        if (extractor != null)
        {
            corrected = true;
            fatherLine = null;

            extractor.connectedSegment = null;
            extractor = null;
        }
    }

    private IEnumerator ExecuteMethods()
    {
        int executionCount = 0;
        int maxExecutionCount = 3; // Number of executions

        while (executionCount < maxExecutionCount)
        {
            if (previousSegment == null && extractor == null)
                CheckStartPointCollisions();

            if (nextSegment == null)
                CheckEndPointCollisions();

            executionCount++;
            yield return new WaitForSeconds(0.5f);
        }
    }

    private void CheckStartPointCollisions()
    {
        var collisions = CheckCollisions(segmentPath[0]);

        if (collisions.Item1 && collisions.Item2 != null)
        {
            var pipeSegment = collisions.Item2.GetComponent<PipeSegment>();

            // Rotations are approximately equal within the threshold
            float angle = Quaternion.Angle(collisions.Item2.transform.rotation, this.gameObject.transform.rotation);
            float threshold = 0.1f; // Your desired threshold

            if (pipeSegment != null && !pipeSegment.isGhostSegment && (angle < threshold))
            {
                previousSegment = pipeSegment;
                previousSegment.nextSegment = this;
                if (previousSegment.fatherLine != null)
                {
                    fatherLine = previousSegment.fatherLine;

                    if (!fatherLine.GetPipeSegments().Contains(this))
                        fatherLine.GetPipeSegments().Add(this);
                }
            }
            else if (collisions.Item2.name == "Connector")
            {
                extractor = collisions.Item2.GetComponentInParent<WaterExtractor>();
                extractor.connectedSegment = this;
                if (!extractor.connectedPipesLine.GetPipeSegments().Contains(this))
                    extractor.connectedPipesLine.GetPipeSegments().Add(this);
                fatherLine = extractor.connectedPipesLine;
            }
        }
    }

    private void CheckEndPointCollisions()
    {
        var collisions = CheckCollisions(segmentPath[segmentPath.Count - 1]);

        if (collisions.Item1 && collisions.Item2 != null)
        {
            var pipeSegment = collisions.Item2.GetComponent<PipeSegment>();

            if (pipeSegment != null && !pipeSegment.isGhostSegment &&
                    collisions.Item2.transform.rotation == this.gameObject.transform.rotation)
            {

                nextSegment = pipeSegment;
                nextSegment.previousSegment = this;
            }
        }
    }

    private (bool, GameObject) CheckCollisions(Transform point)
    {
        // Draw the raycast in the Scene view for visualization
        Debug.DrawRay(point.position, point.forward * 0.3f, Color.red, 1f);

        RaycastHit hit;
        if (Physics.Raycast(point.position, point.forward, out hit, 0.3f))
        {
            return (true, hit.collider.gameObject);
        }
        return (false, null);
    }
}

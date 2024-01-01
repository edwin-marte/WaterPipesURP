using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PipesLine : MonoBehaviour
{
    private float key = 0f;
    private List<PipeSegment> pipeSegments = new List<PipeSegment>();
    private List<PipeSegment> newSegments = new List<PipeSegment>();

    private void Update()
    {
        // Connect missing segments
        foreach (PipeSegment pipeSegment in pipeSegments)
        {
            if (pipeSegment.previousSegment != null)
            {
                if (!pipeSegments.Contains(pipeSegment.previousSegment))
                {
                    pipeSegment.previousSegment.fatherLine = this;
                    newSegments.Add(pipeSegment.previousSegment);
                }
            }

            if (pipeSegment.nextSegment != null)
            {
                if (!pipeSegments.Contains(pipeSegment.nextSegment) && !pipeSegment.corrected)
                {
                    pipeSegment.nextSegment.fatherLine = this;
                    newSegments.Add(pipeSegment.nextSegment);
                }
            }
        }

        pipeSegments.AddRange(newSegments);
        newSegments.Clear();
    }

    public void SetKey(float newKey)
    {
        key = newKey;
    }

    public float GetKey()
    {
        return key;
    }

    public List<PipeSegment> GetPipeSegments()
    {
        return pipeSegments;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class WaterExtractor : MonoBehaviour
{
    public PipeSegment connectedSegment;

    public PipesLinesManager pipeLinesManager;
    public PipesLinesKeys pipeLinesKeys;

    public PipesLine connectedPipesLine;

    private void Start()
    {
        CreatePipesLine();
    }

    private void CreatePipesLine()
    {
        connectedPipesLine = Instantiate(pipeLinesManager.pipesLinePrefab, Vector3.zero, Quaternion.identity).GetComponent<PipesLine>();
        connectedPipesLine.SetKey(pipeLinesKeys.GetNewKey());
        pipeLinesManager.pipeLines.Add(connectedPipesLine.GetKey(), connectedPipesLine);
    }
}
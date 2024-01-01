using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PipesLinesManager : MonoBehaviour
{
    public GameObject pipesLinePrefab;
    public Dictionary<float, PipesLine> pipeLines;

    private void Awake()
    {
        pipeLines = new Dictionary<float, PipesLine>();
    }
}

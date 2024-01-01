using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PipesLinesKeys : MonoBehaviour
{
    private float nextKey = 1f;

    // Generate Key
    public float GetNewKey()
    {
        var value = nextKey;
        nextKey += 1f;
        return value;
    }
}

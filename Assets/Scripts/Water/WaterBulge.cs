using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterBulge : MonoBehaviour
{
    public bool isMoving = true;
    public int pointsIndex = 0;
    public WaterBulge nextWaterBulge;

    public void SetIsMoving(bool isMoving)
    {
        this.isMoving = isMoving;
    }

    public bool IsMoving()
    {
        return isMoving;
    }
}

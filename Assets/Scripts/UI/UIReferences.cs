using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIReferences : MonoBehaviour
{
    public static UIReferences Instance;

    [SerializeField] private List<Button> pipeButtons;
    [SerializeField] private GameObject overlay;

    void Awake()
    {
        Instance = this;
    }

    public List<Button> GetPipeButtons()
    {
        return pipeButtons;
    }

    public GameObject GetOverlay()
    {
        return overlay;
    }
}

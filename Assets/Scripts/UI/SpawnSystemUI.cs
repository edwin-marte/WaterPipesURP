using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;

public class SpawnSystemUI : MonoBehaviour
{
    private UIReferences uiReferences;

    public static event Action OnPipeSelected;

    private void Start()
    {
        uiReferences = UIReferences.Instance;
        SetupPipeButtons();
    }

    private void SetupPipeButtons()
    {
        foreach (Button btn in uiReferences.GetPipeButtons())
        {
            btn.onClick.AddListener(delegate () { SelectPipe(btn); });
        }
    }

    private void SelectPipe(Button button)
    {
        uiReferences.GetOverlay().SetActive(true);
        OnPipeSelected?.Invoke();
    }

    public void ExitBuildMode()
    {
        uiReferences.GetOverlay().SetActive(false);
    }

    public bool IsInteractingWithUI()
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return true;
        }
        return false;
    }
}

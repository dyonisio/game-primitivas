using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PressButtonAnimation : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private Button _button;
    private float downScale = 0.9f, transitionTime = 0.1f;

    private void Start()
    {
        _button = GetComponent<Button>();
    }

    private void Scale(bool down = false)
    {
        _button.transform.DOScale(down ? downScale : 1f, transitionTime);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Scale(true);
    }
 
    public void OnPointerUp(PointerEventData eventData)
    {
        Scale();
    }
}

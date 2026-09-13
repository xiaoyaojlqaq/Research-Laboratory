using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.EventSystems;

public class LefButton : MonoBehaviour,IPointerDownHandler,IPointerEnterHandler,IPointerExitHandler
{

    RectTransform rectTransform;
    public float scaleFactor;
    [HideInInspector]public bool isSelected;

    // Start is called before the first frame update
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
    }
public void OnPointerDown(PointerEventData eventData)
    {
        isSelected = true;
        rectTransform.DOScale(scaleFactor, 0.2f);
        ButtonSoundManager.Instance?.PlayClick();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        rectTransform.DOScale(scaleFactor, 0.2f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (isSelected) { return; }
        rectTransform.DOScale(1f, 0.2f);
    }
}

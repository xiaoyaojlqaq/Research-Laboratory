using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.EventSystems;

public class MainButtonAnima : MonoBehaviour,IPointerEnterHandler,IPointerExitHandler,IPointerDownHandler,IPointerUpHandler
{
    RectTransform rectTransform;
    public float scaleFactor;

    // Start is called before the first frame update
    void Start()
    {
        rectTransform=GetComponent<RectTransform>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        rectTransform.DOScale(scaleFactor, 0.2f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        rectTransform.DOScale(1f, 0.2f);
    }

        public void OnPointerDown(PointerEventData eventData)
    {
        ButtonSoundManager.Instance?.PlayClick();
    }

public void OnPointerUp(PointerEventData eventData)
    {
        rectTransform.DOScale(1f, 0.2f);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class peerReviewAnima : MonoBehaviour
{
    RectTransform rectTransform;

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        //rectTransform.DOAnchorPosY();
    }
}

using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AllLefButton : MonoBehaviour
{
    int index;
    public Button[] buttons;
    public RectTransform[] rectTransforms;
    // Start is called before the first frame update
    void Start()
    {
        
        buttons[0].onClick.AddListener(() =>
        {
            //LefButtonChoose(0);
        });

        buttons[1].onClick.AddListener(() =>
        {
            //LefButtonChoose(1);
        });

        buttons[2].onClick.AddListener(() =>
        {
            //LefButtonChoose(2);
        });

        buttons[3].onClick.AddListener(() =>
        {
            //LefButtonChoose(3);
        });
    }

    public void LefButtonChoose(int index1)
    {
        rectTransforms[index].DOScale(1, 0.2f);
        rectTransforms[index].GetComponent<LefButton>().isSelected = false;
        index = index1;
        rectTransforms[index].DOScale(1.15f, 0.2f);
    }
}

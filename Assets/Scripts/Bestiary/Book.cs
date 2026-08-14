using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal.Internal;

public class Book : MonoBehaviour
{
    [SerializeField] float pageSpeed = 0.5f;
    [SerializeField] List<Transform> pages;
    int index = -1;
    bool rotate = false;
    [SerializeField] GameObject nextButton;
    [SerializeField] GameObject backButton;
    [SerializeField] private BestiaryDisplay bestiaryDisplay;

    void Start()
    {
        InitialState();

        if (bestiaryDisplay != null) 
        {
            bestiaryDisplay.UpdatePage(0);
        }
    }

    public void InitialState()
    {
        for (int i = 0; i < pages.Count; i++)
        {
            pages[i].transform.rotation=Quaternion.identity;

            GameObject front = pages[i].Find("FrontContent").gameObject;
            GameObject back = pages[i].Find("BackContent").gameObject;

            front.SetActive(true);
            back.SetActive(false);
        }
        pages[0].SetAsLastSibling();
        backButton.SetActive(false);

    }

    public void RotateNext()
    {   
        if (rotate == true) { return; }
        index++;
        bestiaryDisplay.UpdatePage(index);
        float angle = -180;
        NextButtonAction();
        pages[index].SetAsLastSibling();
        StartCoroutine(Rotate(angle, true));
    }

    public void NextButtonAction()
    {
        if (backButton.activeInHierarchy == false)
        {
            backButton.SetActive(true);
        }
        if (index == pages.Count - 1)
        {
            nextButton.SetActive(false);
        }
    }

    public void RotatePrev()
    {
        if (rotate == true) { return; }
        bestiaryDisplay.UpdatePage(index - 1);
        float angle = 0;
        pages[index].SetAsLastSibling();
        BackButtonAction();
        StartCoroutine(Rotate(angle, false));
    }

    public void BackButtonAction()
    {
        if (nextButton.activeInHierarchy == false)
        {
            nextButton.SetActive(true);
        }
        if (index -1 == -1)
        {
            backButton.SetActive(false);
        }
    }

    IEnumerator Rotate(float angle, bool forward)
    {
        float value = 0f;

        GameObject frontContent = pages[index].Find("FrontContent").gameObject;
        GameObject backContent = pages[index].Find("BackContent").gameObject;

        while (true)
        {
            rotate = true;
            Quaternion targetRotation = Quaternion.Euler(0, angle, 0);
            value += Time.unscaledDeltaTime * pageSpeed;
            pages[index].rotation = Quaternion.Slerp(pages[index].rotation, targetRotation, value);
            float angle1 = Quaternion.Angle(pages[index].rotation, targetRotation);

            if (angle1 < 90f)
            {
                if (forward) 
                {
                    // Next page
                    frontContent.SetActive(false);
                    backContent.SetActive(true);
                }
                else 
                {
                    // Previous page
                    frontContent.SetActive(true);
                    backContent.SetActive(false);
                }
            }

            if (angle1 < 0.1f)
            {
                if (forward == false)
                {
                    index--;
                }
                rotate = false;
                break;

            }
            yield return null;
        }
    }
}

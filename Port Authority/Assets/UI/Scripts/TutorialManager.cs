using UnityEngine;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    public Transform pagesParent;
    public Button nextButton;
    public Button prevButton;

    private GameObject[] pages;
    private int currentIndex = 0;

    private void Awake()
    {
        int count = pagesParent.childCount;
        pages = new GameObject[count];
        for (int i = 0; i < count; i++)
        {
            pages[i] = pagesParent.GetChild(i).gameObject;
        }
    }

    private void Start()
    {
        ShowPage();

        nextButton.onClick.AddListener(NextPage);
        prevButton.onClick.AddListener(PrevPage);
    }

    private void ShowPage()
    {
        for (int i = 0; i < pages.Length; i++)
        {
            pages[i].SetActive(i == currentIndex); 
        }

        prevButton.gameObject.SetActive(currentIndex > 0);
        nextButton.gameObject.SetActive(currentIndex < pages.Length - 1);
    }

    public void NextPage()
    {
        if (currentIndex < pages.Length - 1)
        {
            currentIndex++;
            ShowPage();
        }
    }

    public void PrevPage()
    {
        if (currentIndex > 0)
        {
            currentIndex--;
            ShowPage();
        }
    }
}

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class SceneManagerScript : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float fadeDuration = 3f;
    private Coroutine currentFade;
    private bool fadeInStarted = false;
    public TextMeshProUGUI loadingText;

    public static event Action BeforeSceneLoad;


    private void Awake()
    {
        canvasGroup.gameObject.SetActive(true);
    }

    private void Start()
    {
        if (!fadeInStarted)
        {
            fadeInStarted = true;
            StartCoroutine(DelayedFadeIn());
        }
    }
    public void ResetScene()
    {
        StartCoroutine(LoadScene(SceneManager.GetActiveScene().buildIndex));
    }

    public void LoadSceneByIndex(int sceneIndex)
    {
        StartCoroutine(LoadScene(sceneIndex));
    }

    public void LoadMainMenu()
    {
        StartCoroutine(LoadMainMenuCoroutine());
    }

    private IEnumerator DelayedFadeIn()
    {
        yield return null;
        yield return null;
        if (currentFade != null) StopCoroutine(currentFade);
        currentFade = StartCoroutine(FadeCanvasGroup(canvasGroup, 1f, 0f, fadeDuration));
    }

    private IEnumerator LoadScene(int sceneIndex)
    {

        BeforeSceneLoad?.Invoke();

        // Ensure timescale is in a sane state for a new run.
        Time.timeScale = 1f;
        //yield return new WaitForSeconds(1);
        
        if (currentFade != null) StopCoroutine(currentFade);
        currentFade = StartCoroutine(FadeCanvasGroup(canvasGroup, canvasGroup.alpha, 1f, fadeDuration));
        yield return currentFade;
        SceneManager.LoadScene(sceneIndex);

        /*
        loadingText.gameObject.SetActive(true);
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneIndex);
        asyncLoad.allowSceneActivation = false;

        while (asyncLoad.progress < 0.90f)
        {
            yield return null;
        }

        asyncLoad.allowSceneActivation = true;

        yield return null;
        */
        canvasGroup.alpha = 1f;
        //yield return StartCoroutine(FadeCanvasGroup(canvasGroup, 1f, 0f, fadeDuration));
        
    }

    private IEnumerator LoadMainMenuCoroutine()
    {
        if (currentFade != null) StopCoroutine(currentFade);
        currentFade = StartCoroutine(FadeCanvasGroup(canvasGroup, canvasGroup.alpha, 1f, fadeDuration));
        yield return currentFade;
        if (OnlineStatusManager.isOnline)
        {
            SceneManager.LoadScene(4);
        }
        else
        {
            SceneManager.LoadScene(5);
        }
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup cg, float start, float end, float duration)
    {
        float elapsedTime = 0.0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            cg.alpha = Mathf.Lerp(start, end, elapsedTime / duration);
            yield return null;
        }

        cg.alpha = end;
    }
}
using Mirror;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagerScript : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float fadeDuration = 3f;
    private Coroutine currentFade;
    private bool fadeInStarted = false;
    public TextMeshProUGUI loadingText;


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

    private IEnumerator DelayedFadeIn()
    {
        yield return null;
        yield return null;
        if (currentFade != null) StopCoroutine(currentFade);
        currentFade = StartCoroutine(FadeCanvasGroup(canvasGroup, 1f, 0f, fadeDuration));
    }

    private IEnumerator LoadSceneAsync(int sceneIndex)
    {
        if (currentFade != null) StopCoroutine(currentFade);
        currentFade = StartCoroutine(FadeCanvasGroup(canvasGroup, canvasGroup.alpha, 1f, fadeDuration));
        yield return currentFade;

        loadingText.gameObject.SetActive(true);

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneIndex);
        asyncLoad.allowSceneActivation = false;

        while (asyncLoad.progress < 0.90f)
        {
            yield return null;
        }

        asyncLoad.allowSceneActivation = true;

        yield return null;

        canvasGroup.alpha = 1f;
        //yield return StartCoroutine(FadeCanvasGroup(canvasGroup, 1f, 0f, fadeDuration));
    }

    private IEnumerator LoadScene(int sceneIndex)
    {
        // Fade out
        if (currentFade != null) StopCoroutine(currentFade);
        currentFade = StartCoroutine(FadeCanvasGroup(canvasGroup, canvasGroup.alpha, 1f, fadeDuration));
        yield return currentFade;

        // Optionally show loading text
        loadingText.gameObject.SetActive(true);

        // Synchronously load the scene
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneIndex);

        // Make sure canvasGroup is fully visible
        canvasGroup.alpha = 1f;

        yield return null;
    }

    public IEnumerator FadeCanvasGroup(CanvasGroup cg, float start, float end, float duration)
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

    public void FadeIn()
    {
        StartCoroutine(FadeCanvasGroupIn());
    }
    public IEnumerator FadeCanvasGroupIn()
    {

        float elapsedTime = 0.0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = 1f;

        loadingText.gameObject.SetActive(true);
    }

    public void ChangeScene(string sceneName)
    {
        if (NetworkServer.active && NetworkServer.connections.Count > 0)
        {
            NetworkRoomManager.singleton.ServerChangeScene(sceneName);
        }
    }
}
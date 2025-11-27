using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class StatUpgradeCard : MonoBehaviour 
{
    [Header("UI Elements")]
    public TMP_Text upgradeTitleText;
    public TMP_Text currentEffectText;
    public TMP_Text priceText;
    public TMP_Text effectText;
    public Transform notchesContainer;
    public Button currentButton;

    [Header("Prefabs")]
    public GameObject emptyNotchPrefab;
    public GameObject fullNotchPrefab;
    public Button buyButtonPrefab;
    public Button boughtButtonPrefab;

    private int currentLevel;
    private int maxLevel;
    private System.Action onBuyCallback;
    private readonly StatUpgradeCardLogic logic = new StatUpgradeCardLogic();

    public void SetUpgradeInfo(string title, string currentEffect, string price, string effect, int currentLevel, int maxLevel, System.Action onBuy)
    {
        // set upgrade info
        upgradeTitleText.text = title;
        currentEffectText.text = currentEffect;
        priceText.text = price;
        effectText.text = effect;

        this.currentLevel = currentLevel;
        this.maxLevel = maxLevel;
        this.onBuyCallback = onBuy;

        StatUpgradeCardState state = logic.BuildState(currentLevel, maxLevel);

        // reset notches
        foreach (Transform child in notchesContainer)
            Destroy(child.gameObject);

        for (int i = 0; i < state.TotalNotches; i++)
        {
            var prefab = i < state.FilledNotches ? fullNotchPrefab : emptyNotchPrefab;
            GameObject notch = Instantiate(prefab, notchesContainer);

            if (i == state.FilledNotches - 1 && prefab == fullNotchPrefab)
            {
                StartCoroutine(PopIn(notch.transform));
            }
        }

        TMP_Text label = currentButton.GetComponentInChildren<TMP_Text>();
        if (state.UseBoughtVisual && boughtButtonPrefab != null)
        {
            currentButton.image.sprite = boughtButtonPrefab.image.sprite;
            currentButton.image.color = boughtButtonPrefab.image.color;
        }
        else if (!state.UseBoughtVisual && buyButtonPrefab != null)
        {
            currentButton.image.sprite = buyButtonPrefab.image.sprite;
            currentButton.image.color = buyButtonPrefab.image.color;
        }

        currentButton.interactable = state.Interactable;
        if (label != null)
        {
            label.text = state.ButtonLabel;
        }

        currentButton.onClick.RemoveAllListeners();
        if (state.Interactable)
        {
            currentButton.onClick.AddListener(() => onBuyCallback?.Invoke());
        }

    }

    private IEnumerator PopIn(Transform target, float startScale = 0.8f, float duration = 0.1f)
    {
        if (target == null)
        {
            yield break;
        }

        Vector3 originalScale = target.localScale;
        target.localScale = originalScale * startScale;

        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            float factor = Mathf.SmoothStep(startScale, 1f, t / duration);
            target.localScale = originalScale * factor;
            yield return null;
        }

        target.localScale = originalScale;
    }

}

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

        // reset notches
        foreach (Transform child in notchesContainer)
            Destroy(child.gameObject);

        for (int i = 0; i < maxLevel; i++)
        {
            var prefab = i < currentLevel ? fullNotchPrefab : emptyNotchPrefab;
            GameObject notch = Instantiate(prefab, notchesContainer);

            if (i == currentLevel - 1 && prefab == fullNotchPrefab)
            {
                StartCoroutine(PopIn(notch.transform));
            }
        }

        // set button type
        bool isMaxed = currentLevel >= maxLevel;

        TMP_Text label = currentButton.GetComponentInChildren<TMP_Text>();

        if (isMaxed)
        {
            if (boughtButtonPrefab != null)
            {
                currentButton.image.sprite = boughtButtonPrefab.image.sprite;
                currentButton.image.color = boughtButtonPrefab.image.color;
            }

            currentButton.interactable = false;
            if (label != null) label.text = "Max";
            currentButton.onClick.RemoveAllListeners();
        }
        else
        {
            if (buyButtonPrefab != null)
            {
                currentButton.image.sprite = buyButtonPrefab.image.sprite;
                currentButton.image.color = buyButtonPrefab.image.color;
            }

            currentButton.interactable = true;
            if (label != null) label.text = "Buy";
            currentButton.onClick.RemoveAllListeners();
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

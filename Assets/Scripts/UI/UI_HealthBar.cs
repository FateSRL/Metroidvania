using UnityEngine;
using UnityEngine.UI;

public class UI_HealthBar : MonoBehaviour
{
    [SerializeField] private CharacterStats stats;
    [SerializeField] private Image fillImage;

    private bool subscribed;

    private void OnEnable()
    {
        Subscribe();
        UpdateHealthUI();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    private void Start()
    {
        UpdateHealthUI();
    }

    public void Setup(CharacterStats newStats, Image newFillImage)
    {
        Unsubscribe();

        stats = newStats;
        fillImage = newFillImage;

        Subscribe();
        UpdateHealthUI();
    }

    private void Subscribe()
    {
        if (subscribed || stats == null)
            return;

        stats.onHealthChanged += UpdateHealthUI;
        subscribed = true;
    }

    private void Unsubscribe()
    {
        if (!subscribed || stats == null)
            return;

        stats.onHealthChanged -= UpdateHealthUI;
        subscribed = false;
    }

    private void UpdateHealthUI()
    {
        if (stats == null || fillImage == null)
            return;

        int maxHealth = stats.GetMaxHealthValue();
        fillImage.fillAmount = maxHealth > 0 ? (float)stats.currentHealth / maxHealth : 0;
    }
}

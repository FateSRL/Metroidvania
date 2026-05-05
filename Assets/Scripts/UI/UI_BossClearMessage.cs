using UnityEngine;
using UnityEngine.UI;

public class UI_BossClearMessage : MonoBehaviour
{
    [SerializeField] private CharacterStats bossStats;
    [SerializeField] private GameObject messageRoot;
    [SerializeField] private Text messageText;
    [SerializeField] private string clearMessage = "BOSS DEFEATED";

    private bool subscribed;

    private void OnEnable()
    {
        Subscribe();
        Refresh();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    public void Setup(CharacterStats stats, GameObject root, Text text)
    {
        Unsubscribe();

        bossStats = stats;
        messageRoot = root;
        messageText = text;

        Subscribe();
        Refresh();
    }

    private void Subscribe()
    {
        if (subscribed || bossStats == null)
            return;

        bossStats.onHealthChanged += Refresh;
        subscribed = true;
    }

    private void Unsubscribe()
    {
        if (!subscribed || bossStats == null)
            return;

        bossStats.onHealthChanged -= Refresh;
        subscribed = false;
    }

    private void Refresh()
    {
        bool bossDefeated = bossStats != null && bossStats.currentHealth <= 0;

        if (messageText != null)
            messageText.text = clearMessage;

        if (messageRoot != null)
            messageRoot.SetActive(bossDefeated);
    }
}

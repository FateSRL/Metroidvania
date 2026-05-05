using System.Collections;
using UnityEngine;

public class HitImpactFX : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite[] frames;
    [SerializeField] private float frameRate = 24f;
    [SerializeField] private float destroyDelay = .05f;

    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        StartCoroutine(PlayRoutine());
    }

    private IEnumerator PlayRoutine()
    {
        if (spriteRenderer == null || frames == null || frames.Length == 0)
        {
            Destroy(gameObject);
            yield break;
        }

        float frameDelay = 1f / Mathf.Max(1f, frameRate);

        for (int i = 0; i < frames.Length; i++)
        {
            spriteRenderer.sprite = frames[i];
            yield return new WaitForSeconds(frameDelay);
        }

        yield return new WaitForSeconds(destroyDelay);
        Destroy(gameObject);
    }
}

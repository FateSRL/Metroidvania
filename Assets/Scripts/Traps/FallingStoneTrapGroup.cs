using System.Collections;
using UnityEngine;

public class FallingStoneTrapGroup : MonoBehaviour
{
    [SerializeField] private FallingStoneTrap[] stones;
    [SerializeField] private float interval = .18f;
    [SerializeField] private float groupCooldown = .5f;
    [SerializeField] private bool ignoreTriggerWhileSequencing = true;

    private Coroutine sequenceRoutine;
    private float lastTriggerTime = -999f;

    public void TriggerLeftToRight()
    {
        Trigger(true);
    }

    public void TriggerRightToLeft()
    {
        Trigger(false);
    }

    public void Trigger(bool leftToRight)
    {
        if (!isActiveAndEnabled || stones == null || stones.Length == 0)
            return;

        if (ignoreTriggerWhileSequencing && sequenceRoutine != null)
            return;

        if (Time.time < lastTriggerTime + groupCooldown)
            return;

        if (sequenceRoutine != null)
            StopCoroutine(sequenceRoutine);

        sequenceRoutine = StartCoroutine(TriggerSequence(leftToRight));
    }

    private IEnumerator TriggerSequence(bool leftToRight)
    {
        lastTriggerTime = Time.time;

        if (leftToRight)
        {
            for (int i = 0; i < stones.Length; i++)
                yield return TriggerStone(stones[i]);
        }
        else
        {
            for (int i = stones.Length - 1; i >= 0; i--)
                yield return TriggerStone(stones[i]);
        }

        sequenceRoutine = null;
    }

    private IEnumerator TriggerStone(FallingStoneTrap stone)
    {
        if (stone != null)
            stone.Trigger();

        if (interval > 0)
            yield return new WaitForSeconds(interval);
    }

    public void ResetAllStones()
    {
        if (sequenceRoutine != null)
            StopCoroutine(sequenceRoutine);

        sequenceRoutine = null;

        if (stones == null)
            return;

        for (int i = 0; i < stones.Length; i++)
        {
            if (stones[i] != null)
                stones[i].ResetTrap();
        }
    }
}

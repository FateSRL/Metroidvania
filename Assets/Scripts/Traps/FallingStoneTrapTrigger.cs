using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class FallingStoneTrapTrigger : MonoBehaviour
{
    private enum SequenceDirection
    {
        LeftToRight,
        RightToLeft
    }

    [SerializeField] private FallingStoneTrapGroup trapGroup;
    [SerializeField] private SequenceDirection direction;

    private void Reset()
    {
        Collider2D triggerCollider = GetComponent<Collider2D>();
        triggerCollider.isTrigger = true;
        trapGroup = GetComponentInParent<FallingStoneTrapGroup>();
    }

    private void Awake()
    {
        Collider2D triggerCollider = GetComponent<Collider2D>();
        triggerCollider.isTrigger = true;

        if (trapGroup == null)
            trapGroup = GetComponentInParent<FallingStoneTrapGroup>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (trapGroup == null || other.GetComponentInParent<Player>() == null)
            return;

        if (direction == SequenceDirection.LeftToRight)
            trapGroup.TriggerLeftToRight();
        else
            trapGroup.TriggerRightToLeft();
    }
}

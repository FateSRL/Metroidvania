using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class FallingStoneTrapSensor : MonoBehaviour
{
    [SerializeField] private FallingStoneTrap trap;

    private void Reset()
    {
        Collider2D sensorCollider = GetComponent<Collider2D>();
        sensorCollider.isTrigger = true;
        trap = GetComponentInParent<FallingStoneTrap>();
    }

    private void Awake()
    {
        Collider2D sensorCollider = GetComponent<Collider2D>();
        sensorCollider.isTrigger = true;

        if (trap == null)
            trap = GetComponentInParent<FallingStoneTrap>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (trap == null || other.GetComponentInParent<Player>() == null)
            return;

        trap.Trigger();
    }
}

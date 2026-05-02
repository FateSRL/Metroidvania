using UnityEngine;

public class EnemyStats : CharacterStats
{
    [SerializeField] private float destroyDelay = 1f;

    private Enemy enemy;

    protected override void Awake()
    {
        base.Awake();
        enemy = GetComponent<Enemy>();
    }

    protected override void Die()
    {
        base.Die();

        if (enemy != null)
            enemy.Die();

        Destroy(gameObject, destroyDelay);
    }
}

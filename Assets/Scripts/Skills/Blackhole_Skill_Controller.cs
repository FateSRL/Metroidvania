using System.Collections.Generic;
using UnityEngine;

public class Blackhole_Skill_Controller : MonoBehaviour
{
    private float maxSize;
    private float growSpeed;
    private float shrinkSpeed;
    private float blackholeTimer;
    private float damageTimer;
    private float damageInterval;
    private int damageTimesLeft;

    private bool canShrink;
    private readonly List<Enemy> enemies = new List<Enemy>();

    public bool playerCanExitState { get; private set; }

    public void SetupBlackhole(float _maxSize, float _growSpeed, float _shrinkSpeed, float _blackholeDuration, int _damageTimes)
    {
        maxSize = _maxSize;
        growSpeed = _growSpeed;
        shrinkSpeed = _shrinkSpeed;
        blackholeTimer = _blackholeDuration;
        damageTimesLeft = Mathf.Max(0, _damageTimes);
        damageInterval = damageTimesLeft > 0 ? _blackholeDuration / damageTimesLeft : _blackholeDuration;
        damageTimer = 0;
    }

    private void Update()
    {
        if (!canShrink)
        {
            blackholeTimer -= Time.deltaTime;
            damageTimer -= Time.deltaTime;

            transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one * maxSize, growSpeed * Time.deltaTime);

            if (damageTimesLeft > 0 && damageTimer <= 0 && DamageEnemies())
            {
                damageTimesLeft--;
                damageTimer = damageInterval;
            }

            bool damageSequenceFinished = damageTimesLeft <= 0 || enemies.Count == 0;

            if (blackholeTimer <= 0 && damageSequenceFinished)
                FinishBlackhole();
        }
        else
        {
            transform.localScale = Vector3.Lerp(transform.localScale, Vector3.zero, shrinkSpeed * Time.deltaTime);

            if (transform.localScale.x <= .05f)
                Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Enemy enemy = collision.GetComponentInParent<Enemy>();

        if (enemy == null || enemies.Contains(enemy))
            return;

        enemies.Add(enemy);
        enemy.FreezeTime(true);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        Enemy enemy = collision.GetComponentInParent<Enemy>();

        if (enemy == null)
            return;

        enemies.Remove(enemy);
        enemy.FreezeTime(false);
    }

    private void OnDestroy()
    {
        ReleaseEnemies();
    }

    private bool DamageEnemies()
    {
        bool damagedEnemy = false;

        for (int i = enemies.Count - 1; i >= 0; i--)
        {
            Enemy enemy = enemies[i];

            if (enemy == null)
            {
                enemies.RemoveAt(i);
                continue;
            }

            enemy.TakeBlackholeDamage();
            enemy.FreezeTime(true);
            damagedEnemy = true;
        }

        return damagedEnemy;
    }

    private void FinishBlackhole()
    {
        playerCanExitState = true;
        canShrink = true;
        ReleaseEnemies();
    }

    private void ReleaseEnemies()
    {
        for (int i = enemies.Count - 1; i >= 0; i--)
        {
            if (enemies[i] != null)
                enemies[i].FreezeTime(false);
        }

        enemies.Clear();
    }
}

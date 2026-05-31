using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sword_Skill_Controller : MonoBehaviour
{
    private Animator anim;
    private Rigidbody2D rb;
    private Player player;
    private CircleCollider2D cd;

    private bool canRotate;
    private bool isReturning;
    private bool hasHitEnemy;
    private float returnSpeed = 12;

    [SerializeField] private float stickIntoOffset = .18f;

    private void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody2D>();
        cd = GetComponent<CircleCollider2D>();
    }

    private void Update()
    {
        if (canRotate && rb != null && rb.velocity.sqrMagnitude > .01f)
            transform.right = rb.velocity;

        if (isReturning)
            ReturnToPlayer();
    }

    public void SetupSword(Vector2 _dir, float _gravityScale, Player _player, float _returnSpeed)
    {
        player = _player;
        returnSpeed = _returnSpeed;

        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        if (anim == null)
            anim = GetComponentInChildren<Animator>();

        if (cd == null)
            cd = GetComponent<CircleCollider2D>();

        if (rb == null)
        {
            Debug.LogError("Sword_Skill_Controller: Rigidbody2D is missing.", this);
            return;
        }

        rb.isKinematic = false;
        rb.constraints = RigidbodyConstraints2D.None;
        rb.gravityScale = _gravityScale;
        rb.velocity = _dir;

        if (cd != null)
            cd.enabled = true;

        canRotate = true;
        isReturning = false;
        hasHitEnemy = false;
        transform.parent = null;

        if (_dir.sqrMagnitude > .01f)
            transform.right = _dir;

        SetRotationAnimation(true);
    }

    public void ReturnSword()
    {
        if (player == null || isReturning)
            return;

        transform.parent = null;

        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.gravityScale = 0;
            rb.isKinematic = true;
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
        }

        if (cd != null)
            cd.enabled = false;

        canRotate = false;
        isReturning = true;
        SetRotationAnimation(false);
    }

    private void ReturnToPlayer()
    {
        if (player == null)
            return;

        transform.position = Vector2.MoveTowards(transform.position, player.transform.position, returnSpeed * Time.deltaTime);

        if (Vector2.Distance(transform.position, player.transform.position) < .8f)
            player.CatchTheSword();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isReturning)
            return;

        if (collision.GetComponentInParent<Player>() != null)
            return;

        Enemy enemy = collision.GetComponentInParent<Enemy>();

        if (enemy != null)
        {
            if (!hasHitEnemy)
            {
                hasHitEnemy = true;
                int normalAttackDamage = player != null && player.stats != null ? player.stats.GetDamage() : 1;
                enemy.Damage(transform.position, Mathf.Max(1, normalAttackDamage * 2));
                StickInto(collision, enemy.transform);
            }

            return;
        }

        if (collision.isTrigger)
            return;

        StickInto(collision, collision.transform);
    }

    private void StickInto(Collider2D collision, Transform targetParent)
    {
        if (collision.GetComponentInParent<Player>() != null)
            return;

        canRotate = false;
        SetRotationAnimation(false);
        transform.position += transform.right * stickIntoOffset;

        if (cd != null)
            cd.enabled = false;

        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.gravityScale = 0;
            rb.isKinematic = true;
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
        }

        transform.parent = targetParent != null ? targetParent : collision.transform;
    }

    private void SetRotationAnimation(bool value)
    {
        if (anim != null)
            anim.SetBool("Rotation", value);
    }
}

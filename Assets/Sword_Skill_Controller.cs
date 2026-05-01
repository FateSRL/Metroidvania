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

    [SerializeField] private float stickIntoOffset = .35f;

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

        Enemy enemy = collision.GetComponent<Enemy>();

        if (enemy != null && !hasHitEnemy)
        {
            hasHitEnemy = true;
            enemy.Damage(transform);
        }

        StickInto(collision);
    }

    private void StickInto(Collider2D collision)
    {
        if (collision.GetComponent<Player>() != null)
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

        transform.parent = collision.transform;
    }

    private void SetRotationAnimation(bool value)
    {
        if (anim != null)
            anim.SetBool("Rotation", value);
    }
}

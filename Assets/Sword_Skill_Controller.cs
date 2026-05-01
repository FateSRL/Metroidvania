using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sword_Skill_Controller : MonoBehaviour
{
    private Animator anim;
    private Rigidbody2D rb;
    private Player player;
    private CircleCollider2D cd;
    
    
    private void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        rb = GetComponentInParent<Rigidbody2D>();
        cd = GetComponent<CircleCollider2D>();
        // player = GameObject.Find("Player").GetComponent<Player>();
    }
    
    public void SetupSword(Vector2 _dir, float _gravityScale)
    {
        rb.gravityScale = _gravityScale;
        rb.velocity = _dir;
    }
    
}
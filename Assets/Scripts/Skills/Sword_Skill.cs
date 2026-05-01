using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sword_Skill : Skill
{
    [Header("Skill info")]
    [SerializeField] private GameObject skillPrefab;
    [SerializeField] private Vector2 launchDir;
    [SerializeField] private float swordGravity;

    [Header("Aim dots")]
    [SerializeField] private int numberOfDots;
    [SerializeField] private float spaceBetweenDots;
    [SerializeField] private GameObject dotPrefab;
    [SerializeField] private Transform dotsParent;
    
    private GameObject[] dots;

    private Vector2 finalDir;


    protected override void Update()
    {
        if (Input.GetKeyUp(KeyCode.Mouse1))
        {
            finalDir = new Vector2(AimDirection().normalized.x * launchDir.x, AimDirection().normalized.y * launchDir.y);
        }
    }


    public void CreateSword()
    {
        if (player == null)
            player = PlayerManager.instance.player;

        if (skillPrefab == null)
        {
            Debug.LogError("Sword_Skill: skillPrefab 没有在 Inspector 里赋值", this);
            return;
        }

        GameObject newSword = Instantiate(skillPrefab, player.transform.position, transform.rotation);
        Sword_Skill_Controller newSwordScript = newSword.GetComponent<Sword_Skill_Controller>();

        newSwordScript.SetupSword(finalDir, swordGravity);
    }
    public Vector2 AimDirection()
    {
        Vector2 palyerPosition = player.transform.position;
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = mousePosition - palyerPosition;
        return direction;
    }

    public void DotsActive(bool _isActive)
    {
        for (int i = 0; i < dots.Length; i++)
        {
            dots[i].SetActive(_isActive);
        }
    }    
    private void GererateDots()
    {
        dots = new GameObject[numberOfDots];
        for (int i = 0; i < numberOfDots; i++)
        {
            dots[i] = Instantiate(dotPrefab, player.transform.position,Quaternion.identity,dotsParent);
            dots[i].SetActive(false);
        }
    }
}

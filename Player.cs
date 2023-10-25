using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    private Rigidbody2D rigidbody2D;

    [SerializeField] private int hp = 100;
    [SerializeField] private int maxHp = 100;

    public float current_speed = 10.0f;
    [SerializeField] private float slow_speed = 3.0f;
    [SerializeField] private float normal_speed = 7.0f;
    [SerializeField] private float fast_speed = 14.0f;

    private SpriteRenderer sprite;
    [SerializeField] private float startMovingAnimation = 1f;
    public float takeRadius = 1f;
    public float distanceAttack = 10f;
    private Animator animator;

    [SerializeField] private Texture2D cursorStartTexture;
    private CursorMode cursorMode = CursorMode.Auto;
    [SerializeField] private Vector2 hotSpot = Vector2.zero;

    private Inventory playerInventory;
    [SerializeField] private PlayerHpBar hpBar;

    [SerializeField] private GameObject sword;

    [SerializeField] private float attackRange = 0.5f;
    [SerializeField] Transform attackPoint;
    [SerializeField] private LayerMask mobLayer;
    [SerializeField] private float minAttackDamage = 10f;
    [SerializeField] private float maxAttackDamage;

    [SerializeField] private float minDelayRate = 0.5f;
    private float nextDelayTime = 0f;

    [SerializeField] private ParticleSystem stepsParticle;

    private UnityEngine.Object playerExplosion;
    void Start()
    {
        rigidbody2D = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        Cursor.SetCursor(cursorStartTexture, hotSpot, cursorMode);
        playerInventory = GetComponent<Inventory>();
        hpBar.SetHp(hp, maxHp);
        hpBar.SetHp(hp, maxHp);
        if (maxAttackDamage < minAttackDamage)
        {
            maxAttackDamage = minAttackDamage;
        }
        playerExplosion = Resources.Load("PlayerExplosion");
    }

    void FixedUpdate()
    {
        ChangeSpeed();
        Movement();
        Flip();
        MovementAnimation();
        MouseActions();
    }
    public void addWood(Item item, int amount)
    {
        playerInventory.AddToInventory(item, amount);
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
    void MouseActions()
    {
        if (Time.time < nextDelayTime) return;
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 screenPosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            Vector2 worldPosition = Camera.main.ScreenToWorldPoint(screenPosition);
            if (Time.time >= nextDelayTime)
                {
                    sword.transform.position = Vector3.MoveTowards(transform.position, new Vector3(worldPosition.x, worldPosition.y, transform.position.z), 0.8f);
                    animator.SetTrigger("attack");
                    Collider2D[] hitMobs = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, mobLayer);
                    foreach (Collider2D mob in hitMobs)
                    {
                        Mob mobcomponent = mob.GetComponent<Mob>();
                        if (mobcomponent){
                            mobcomponent.Hit(Random.Range((int)minAttackDamage, (int)maxAttackDamage), transform, 6.5f);
                        }
                    }
                    nextDelayTime = Time.time + minDelayRate;
                }
        }
        else if (Input.GetMouseButtonDown(1))
        {
            Vector2 screenPosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            Vector2 worldPosition = Camera.main.ScreenToWorldPoint(screenPosition);
            playerInventory.useItem(worldPosition);
        }
    }
    public void Hit(int damage = 10, Transform hitTransform = null, float hitPower = 5)
    {
        animator.SetTrigger("hit");
        if (hitTransform != null)
        {
            var heading = transform.position - hitTransform.position;
            var distance = heading.magnitude;
            var direction = heading / distance * (hitPower * 10);
            rigidbody2D.AddForce(direction);
        }
        hp -= damage;
        if (hp < 0)
        {
            hp = 0;
        }
        hpBar.SetHp(hp, maxHp);
        checkHp();
    }
    public void AddHp(int additionalHp)
    {
        hp = hp + additionalHp < maxHp ? hp + additionalHp : maxHp;
        hpBar.SetHp(hp, maxHp);
    }
    public void checkHp()
    {
        if (hp > 0) return;

        animator.SetTrigger("died");
        var player = GetComponent<Player>() as MonoBehaviour;

        GameObject explosionRef = (GameObject)Instantiate(playerExplosion);
        explosionRef.transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z);

        Destroy(player);
    }
    void Flip()
    {
        if (rigidbody2D.velocity.x > 0.07f)
        {
            sprite.flipX = true;
        }
        if (rigidbody2D.velocity.x < -0.07f)
        {
            sprite.flipX = false;
        }
    }
    void MovementAnimation()
    {
        if (Mathf.Abs(rigidbody2D.velocity.x) + Mathf.Abs(rigidbody2D.velocity.y) > startMovingAnimation)
        {
            animator.SetBool("moving", true);
            CreateStepsParticle();
        }
        else
        {
            animator.SetBool("moving", false);
        }
    }
    public void ChangeSpeed()
    {
        transform.localScale = new Vector3(transform.localScale.x, 100, transform.localScale.z);
        GetComponent<SpriteRenderer>().material.color = new Color(1f, 1f, 1f, 1f);
        if (Input.GetKey(KeyCode.LeftShift))
        {
            transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y / 2, transform.localScale.z);
            GetComponent<SpriteRenderer>().material.color = new Color(1f, 1f, 1f, 0.4f);
            current_speed = slow_speed;
        }
        else if (Input.GetAxis("Fire3") == 1)
        {
            current_speed = fast_speed;
        }
        else
        {
            current_speed = normal_speed;
        }
    }
    void Movement()
    {
        float move_y = Input.GetAxis("Vertical");
        float move_x = Input.GetAxis("Horizontal");
        Vector2 move = new Vector2(move_x, move_y).normalized * current_speed;
        
        rigidbody2D.AddForce(move);
    }
    public bool searchInInventory(Item item, int count)
    {
        return playerInventory.SeachForSameItem(item, count);
    }
    public void CreateStepsParticle()
    {
        stepsParticle.Play();
    }
}

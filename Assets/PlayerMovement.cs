using UnityEngine;
using UnityEngine.UI; 
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    [Header("Oyun Mekanikleri")]
    public Transform respawnPoint; 
    private bool isDead; 

    [Header("Ses Efektleri")] 
    public AudioSource sfxSource; 
    public AudioClip jumpSound;
    public AudioClip deathSound;

    [Header("Sinematik Ayarlar")]
    public Image fadeScreen; 
    public float fadeSpeed = 4f; 
    public float waitInBlackTime = 0.2f; 

    [Header("Hareket Ayarlari")]
    public float moveSpeed = 8f;
    public float jumpForce = 15f; 

    [Header("Ziplama Ayarlari")]
    public int maxJumps = 2;       
    private int remainingJumps;    
    [Range(0f, 1f)] public float jumpCutMultiplier = 0.5f; 

    [Header("Dash Ayarlari")]
    public float dashSpeed = 25f; 
    public float dashDuration = 0.15f; 
    public float dashCooldown = 0.7f; 
    private bool canDash = true;    
    private bool isDashing;
    private float dashCooldownTimer; 
    private Coroutine dashCoroutine; 

    [Header("Zemin ve Duvar Algilama")]
    public Transform groundCheck;
    public Vector2 groundCheckSize = new Vector2(0.8f, 0.1f); 
    public Transform wallCheck;
    public Vector2 wallCheckSize = new Vector2(0.2f, 0.8f);
    public LayerMask groundLayer;
    public LayerMask wallLayer; 

    [Header("Duvar Mekanikleri")]
    public float wallSlideSpeed = 2f; 
    public Vector2 wallJumpForce = new Vector2(7f, 15f); 
    
    public bool isGrounded;
    public bool isTouchingWall;
    public bool isWallSliding;
    private float wallJumpCooldown; 
    private float facingDirection = 1f; 
    [HideInInspector] public float originalGravity;

    private Rigidbody2D rb;
    private Animator anim;
    private float moveInput;
    private Vector3 baseScale;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        baseScale = transform.localScale; 
        originalGravity = rb.gravityScale;
        
        // Başlangıçta mevcut yetenek durumuna göre maxJumps ayarla
        maxJumps = YetenekManager.CiftZiplamaAcikMi ? 2 : 1;
        remainingJumps = maxJumps; 
        
        if (fadeScreen != null) 
        {
            Color c = fadeScreen.color;
            c.a = 0f;
            fadeScreen.color = c;
        }
    }

    void Update()
    {
        if (isDead) return; 

        // Meyve toplandığında maxJumps anında güncelle
        int yeniMaxJumps = YetenekManager.CiftZiplamaAcikMi ? 2 : 1;
        if (yeniMaxJumps != maxJumps)
        {
            maxJumps = yeniMaxJumps;
            // Yerdeyse kalan zıplama hakkını da güncelle
            if (isGrounded) remainingJumps = maxJumps;
        }

        if (dashCooldownTimer > 0) dashCooldownTimer -= Time.deltaTime;

        isGrounded = Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0, groundLayer);
        isTouchingWall = Physics2D.OverlapBox(wallCheck.position, wallCheckSize, 0, wallLayer);

        if (!isGrounded && !isTouchingWall && remainingJumps == maxJumps) remainingJumps--;
        
        if (isGrounded && rb.velocity.y <= 0.1f) 
        {
            remainingJumps = maxJumps;
            canDash = true; 
        }
        if (isWallSliding) 
        {
            canDash = true; 
        }

        if (isDashing && Input.GetButtonDown("Jump")) CancelDash();
        if (isDashing) return;

        moveInput = Input.GetAxisRaw("Horizontal");
        if (moveInput > 0.01f) facingDirection = 1f;
        else if (moveInput < -0.01f) facingDirection = -1f;

        if (YetenekManager.DashAcikMi && Input.GetKeyDown(KeyCode.LeftShift) && canDash && dashCooldownTimer <= 0f)
        {
            dashCoroutine = StartCoroutine(PerformDash());
            return; 
        }

        if (YetenekManager.DuvarZiplamaAcikMi && isTouchingWall && !isGrounded && rb.velocity.y < 0 && moveInput != 0)
        {
            isWallSliding = true;
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Clamp(rb.velocity.y, -wallSlideSpeed, float.MaxValue));
        }
        else
        {
            isWallSliding = false;
        }

        wallJumpCooldown -= Time.deltaTime;

        if (Input.GetButtonDown("Jump"))
        {
            if (YetenekManager.DuvarZiplamaAcikMi && isWallSliding) 
            {
                rb.velocity = new Vector2(-facingDirection * wallJumpForce.x, wallJumpForce.y);
                remainingJumps = maxJumps - 1; 
                wallJumpCooldown = 0.2f; 
                transform.localScale = new Vector3(-facingDirection * baseScale.x, baseScale.y, baseScale.z);
                facingDirection = -facingDirection;
                PlayJumpSound(); 
            }
            else if (isGrounded) 
            {
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
                remainingJumps--; 
                PlayJumpSound(); 
            }
            else if (YetenekManager.CiftZiplamaAcikMi && remainingJumps > 0) 
            {
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
                remainingJumps--; 
                anim.SetTrigger("doubleJump"); 
                PlayJumpSound(); 
            }
        }

        if (Input.GetButtonUp("Jump") && rb.velocity.y > 0)
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * jumpCutMultiplier);
        }

        if (!isWallSliding)
        {
            if (moveInput > 0.01f) transform.localScale = baseScale;
            else if (moveInput < -0.01f) transform.localScale = new Vector3(-baseScale.x, baseScale.y, baseScale.z);
        }

        anim.SetFloat("speed", Mathf.Abs(moveInput));
        anim.SetFloat("yVelocity", rb.velocity.y);
        anim.SetBool("isGrounded", isGrounded);
        anim.SetBool("isWallSliding", isWallSliding);
    }

    private void PlayJumpSound()
    {
        if (sfxSource != null && jumpSound != null)
        {
            sfxSource.PlayOneShot(jumpSound);
        }
    }

    private void CancelDash()
    {
        if (dashCoroutine != null) StopCoroutine(dashCoroutine);
        isDashing = false;
        rb.gravityScale = originalGravity; 
    }

    private IEnumerator PerformDash()
    {
        canDash = false; 
        dashCooldownTimer = dashCooldown; 
        isDashing = true;
        
        rb.gravityScale = 0f;
        rb.velocity = new Vector2(facingDirection * dashSpeed, 0f);
        
        anim.SetTrigger("dashTrigger");

        yield return new WaitForSeconds(dashDuration);

        rb.gravityScale = originalGravity;
        isDashing = false;
    }

    void FixedUpdate()
    {
        if (isDashing || isDead) return;
        
        if (wallJumpCooldown <= 0)
        {
            rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Trap"))
        {
            StartCoroutine(DieAndRespawnRoutine());
        }
    }

    /// <summary>
    /// Dışarıdan çağrılabilir — karakteri öldürür ve son checkpoint'e ışınlar.
    /// PauseMenu restart butonu için kullanılır.
    /// </summary>
    public void RestartAtCheckpoint()
    {
        StartCoroutine(DieAndRespawnRoutine());
    }

    private IEnumerator DieAndRespawnRoutine()
    {
        if (isDead) yield break; 
        
        isDead = true;
        CancelDash(); 
        transform.SetParent(null); 
        
        rb.velocity = Vector2.zero; 
        rb.gravityScale = 0f; 
        
        if (sfxSource != null && deathSound != null)
        {
            sfxSource.PlayOneShot(deathSound);
        }
        
        rb.velocity = Vector2.zero; 
        rb.gravityScale = 0f; 
        
        if (sfxSource != null && deathSound != null)
        {
            sfxSource.PlayOneShot(deathSound);
        }

        anim.SetTrigger("hitTrigger");
        
        if (fadeScreen != null)
        {
            while (fadeScreen.color.a < 1f)
            {
                Color c = fadeScreen.color;
                c.a += Time.deltaTime * fadeSpeed;
                fadeScreen.color = c;
                yield return null; 
            }
        }
        else { yield return new WaitForSeconds(0.5f); } 
        
        if (respawnPoint != null) 
        {
            Vector3 newPos = respawnPoint.position;
            newPos.z = transform.position.z;
            transform.position = newPos;
        }

        // ---------------------------------------------------------
        // YENİ EKLENEN KISIM: Ekran siyahken tüm asansörleri sıfırla
        AsansorPlatform[] tumAsansorler = Object.FindObjectsOfType<AsansorPlatform>();
        foreach (AsansorPlatform asansor in tumAsansorler)
        {
            asansor.AsansoruSifirla();
        }
        // ---------------------------------------------------------
        
        yield return new WaitForSeconds(waitInBlackTime);
        
        if (fadeScreen != null)
        {
            while (fadeScreen.color.a > 0f)
            {
                Color c = fadeScreen.color;
                c.a -= Time.deltaTime * fadeSpeed;
                fadeScreen.color = c;
                yield return null;
            }
        }
        
        rb.gravityScale = originalGravity; 
        isDead = false; 
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(groundCheck.position, groundCheckSize);
        }
        if (wallCheck != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(wallCheck.position, wallCheckSize);
        }
    }
}
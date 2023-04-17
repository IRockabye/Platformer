 using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    Rigidbody2D rb;
    public float speed;
    public float jumpHeight;
    public Transform groundCheck;
    bool isGrounded;
    Animator anim;
    int curHp;
    int maxHp = 3;
    bool isHit = false;
    public Main main;
    public bool key = false;
    bool canTp = true;
    public bool inWater = false;
    bool isClimbing = false;
    int coins = 0;
    bool canHit = true;
    public GameObject blueGem, greenGem;
    int gemCount = 0;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        curHp = maxHp;
    }

    // Update is called once per frame
    void Update()
    {
        if (inWater && !isClimbing)
        {
            anim.SetInteger("State", 4);
            isGrounded = true;
            if (Input.GetAxis("Horizontal") != 0)
            {
                Flip();
            }
        }   
        else
        {
            CheckGround();
            if (Input.GetAxis("Horizontal") == 0 && (isGrounded) && !isClimbing)
            {
                anim.SetInteger("State", 1);
            }
            else
            {
                Flip();
                if (isGrounded && !isClimbing)
                    anim.SetInteger("State", 2);
            }
            
        }
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
            rb.AddForce(transform.up * jumpHeight, ForceMode2D.Impulse);
    }

    private void FixedUpdate()
    {
        rb.velocity = new Vector2(Input.GetAxis("Horizontal") * speed, rb.velocity.y);
    }

    void Flip()
    {
        if (Input.GetAxis("Horizontal") > 0)
            transform.localRotation = Quaternion.Euler(0, 0, 0);
        if (Input.GetAxis("Horizontal") < 0)
            transform.localRotation = Quaternion.Euler(0, 180, 0);
    }
    void CheckGround()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(groundCheck.position, 0.2f);
        isGrounded = colliders.Length > 1;
        if (!isGrounded && !isClimbing)
            anim.SetInteger("State", 3);
    }

    public void RecountHp(int deltaHp)
    {
        if (deltaHp < 0 && canHit)
        {
            curHp = curHp + deltaHp;
            StopCoroutine(OnHit());
            /*canHit = false;*/
            isHit = true; 
            StartCoroutine(OnHit());

        }
        else if (curHp > maxHp)
        {
            curHp = curHp + deltaHp;
            curHp = maxHp;
        }
        print(curHp);
        if (curHp <= 0)
        {
            GetComponent<CapsuleCollider2D>().enabled = false;
            Invoke("Lose", 1.5f);

        }
    }

    IEnumerator OnHit()
    {
        if (isHit)
            GetComponent<SpriteRenderer>().color = new Color(1f, GetComponent<SpriteRenderer>().color.g - 0.04f, GetComponent<SpriteRenderer>().color.b - 0.04f);
        else
            GetComponent<SpriteRenderer>().color = new Color(1f, GetComponent<SpriteRenderer>().color.g + 0.04f, GetComponent<SpriteRenderer>().color.b + 0.04f);

        if (GetComponent<SpriteRenderer>().color.g == 1f)
        {
            StopCoroutine(OnHit());
            canHit = true;
        }
        if (GetComponent<SpriteRenderer>().color.g <=0)
            isHit = false;
        yield return new WaitForSeconds(0.02f);
        StartCoroutine(OnHit());
    }

    void Lose()
    {
        main.GetComponent<Main>().Lose();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Key")
        {
            Destroy(collision.gameObject);
            key = true;
        }

        if (collision.gameObject.tag == "Door")
        {
            if (collision.gameObject.GetComponent<Door>().isOpen && canTp)
            {
                collision.gameObject.GetComponent<Door>().Teleport(gameObject);
                canTp = false;
                StartCoroutine(TpWait());
            }
            else if (key)
            {
                collision.gameObject.GetComponent<Door>().Unlock();
            }
        }

        if (collision.gameObject.tag == "Coin")
        {
            Destroy(collision.gameObject);
            coins++;
            print("Кол-во монет " + coins);
        }

        if (collision.gameObject.tag == "Heart")
        {
            Destroy(collision.gameObject);
            RecountHp(1);
        }

        if (collision.gameObject.tag == "Mushroom")
        {
            Destroy(collision.gameObject);
            RecountHp(-1);
        }

        if (collision.gameObject.tag == "BlueGem")
        {
            Destroy(collision.gameObject);
            StartCoroutine(NoHit());
        }

        if (collision.gameObject.tag == "GreenGem")
        {
            Destroy(collision.gameObject);
            StartCoroutine(SpeedBonus());
        }
    }
    

    IEnumerator TpWait()
    {
        yield return new WaitForSeconds(1f);
        canTp = true;
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Ladder")
        {
            isClimbing = true;
            rb.bodyType = RigidbodyType2D.Kinematic;
            if(Input.GetAxis("Vertical") == 0)
            {
                anim.SetInteger("State", 5);
            }
            else
            {
                anim.SetInteger("State", 6);
                transform.Translate(Vector3.up * Input.GetAxis("Vertical") * speed * 0.5f * Time.deltaTime );
            }
            
        }

        if ( collision.gameObject.tag == "Icy")
        {
            if (rb.gravityScale == 1f)
            {
                rb.gravityScale = 7f;
                speed *= 0.25f;
            }
        } 
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Ladder")
        {   
            isClimbing = false;
            rb.bodyType = RigidbodyType2D.Dynamic;
        }

        if (collision.gameObject.tag == "Icy")
        {
            if (rb.gravityScale == 7f)
            {
                rb.gravityScale = 1f;
                speed *= 4f;
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Trampoline")
        {
            StartCoroutine(TrampolineAnim(collision.gameObject.GetComponentInParent<Animator>()));
        }    
        
        if (collision.gameObject.tag == "QuickSand")
        {
            speed *= 0.25f;
            rb.mass *= 100f;
        }
    }
     
    IEnumerator TrampolineAnim(Animator an)
    {
        an.SetBool("isJump", true);
        yield return new WaitForSeconds(0.5f);
        an.SetBool("isJump", false);
    }

    IEnumerator NoHit()
    {
        gemCount++;
        blueGem.SetActive(true);
        CheckGems(blueGem);

        canHit = false;
        blueGem.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 1f);
        yield return new WaitForSeconds(4f);
        StartCoroutine(Invis(blueGem.GetComponent<SpriteRenderer>(), 0.02f));
        yield return new WaitForSeconds(1f);
        canHit = true;

        gemCount--;
        blueGem.SetActive(false);
        CheckGems(greenGem);
    }

    IEnumerator SpeedBonus()
    {
        gemCount++;
        greenGem.SetActive(true);
        CheckGems(greenGem);

        speed = speed * 2;
        greenGem.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 1f);
        yield return new WaitForSeconds(4f);
        StartCoroutine(Invis(greenGem.GetComponent<SpriteRenderer>(), 0.02f));
        yield return new WaitForSeconds(1f); 
        speed = speed / 2;

        gemCount--;
        greenGem.SetActive(false);
        CheckGems(blueGem);

    }

    void CheckGems(GameObject obj)
    {
        if (gemCount == 1)
        {
            obj.transform.localPosition = new Vector3(0f, 0.5f, obj.transform.localPosition.z);
        }
        else if (gemCount == 2)
        {
            blueGem.transform.localPosition = new Vector3(-0.535f, 0.5f, blueGem.transform.localPosition.z);
            greenGem.transform.localPosition = new Vector3(0.535f, 0.5f, greenGem.transform.localPosition.z);
        }
    }

    IEnumerator Invis(SpriteRenderer spr,float time)
    {
        spr.color = new Color(1f, 1f, 1f, spr.color.a - time * 2);
        yield return new WaitForSeconds(time);
        if (spr.color.a > 0)
        {
            StartCoroutine(Invis(spr, time)); 
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "QuickSand")
        {
            speed *= 4f;
            rb.mass *= 0.01f;
        }
    }
}

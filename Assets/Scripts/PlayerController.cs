using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    public float speed;
    public float jumpForce;
    public float projectileTimer;
    public bool movementLocked;
    public ColorCycle cycle;
    public GameObject projectile;

    bool checkJump, checkColor, canShoot;
    Rigidbody2D rb2D;
    int jumpNumber;
    float lastInput;


    // Start is called before the first frame update
    void Start()
    {
        rb2D = GetComponent<Rigidbody2D>();
        canShoot = true;
        checkColor = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!movementLocked)
        {
            Vector2 vel = rb2D.velocity;
            float input = Input.GetAxis("Horizontal");
            float xMovement = input * speed;
            vel.x = xMovement;

            // Trigger jump --> Double jump if cyan
            if (Input.GetKeyDown(KeyCode.Space))
            {
                vel = Jump(vel);
            }

            // If player color is yellow --> increase speed
            if (cycle.GetColor() == "yellow")
            {
                speed = 9f;
            }
            else
            {
                speed = 5f;
            }

            // Trigger color change
            if (Input.GetKey(KeyCode.LeftShift))
            {
                cycle.Rotate();
                if (!checkColor)
                {
                    checkColor = true;
                    Invoke(nameof(CheckColor), 0.5f);
                }
            }

            // Track last input in order to fire projectile in the direction the player last moved
            if (input != 0)
            {
                lastInput = input;
            }

            // trigger firing projectile in the direction the player is moving (default right)
            if (Input.GetKey(KeyCode.RightShift) && cycle.GetColor() == "magenta")
            {
                ShootProjectile(lastInput);
            }

            // Check jump reset
            if (checkJump && jumpNumber > 0)
            {
                CheckJump();
            }

            rb2D.velocity = vel;
        }
    }

    // Jump function that checks conditions and boosts y velocity
    Vector2 Jump(Vector2 velocity)
    {
        if (jumpNumber == 0)
        {
            jumpNumber = 1;
            velocity.y = jumpForce;
            checkJump = false;
        }
        else if (jumpNumber == 1 && cycle.GetColor() == "cyan")
        {
            jumpNumber = 2;
            velocity.y = jumpForce;
            checkJump = false;
        }
        checkJump = false;
        StartCoroutine(JumpFrames());
        return velocity;
    }

    // Wait a few frames before checking to reset jump so that the player does not gain double/triple jump
    IEnumerator JumpFrames()
    {
        yield return new WaitForSeconds(0.05f);
        checkJump = true;
    }

    // Fire raycasts down to see if player is ontop of a platform, in which case reset jumps
    void CheckJump()
    {
        RaycastHit2D hitLeft = Physics2D.Raycast(transform.position + (Vector3.left * 0.5f), Vector2.down, 0.52f);
        RaycastHit2D hitRight = Physics2D.Raycast(transform.position + (Vector3.right * 0.5f), Vector2.down, 0.52f);
        if (hitLeft.collider != null || hitRight.collider != null)
        {
            jumpNumber = 0;
        }
    }

    // When player shifts color, check to see player color matches platform they are on
    void CheckColor()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 0.52f);
        if (hit.collider != null)
        {
            if (!hit.collider.CompareTag(cycle.GetColor()) && !hit.collider.CompareTag("Platform"))
                {
                    PlayerDeath();
                }
        }
        checkColor = false;
    }

    // Shoot projectile by instantiating projectile object to left or right of player
    void ShootProjectile(float inputDirection)
    {
        if (!canShoot)
        {
            return;
        }
        canShoot = false;
        if (inputDirection >= 0)
        {
            Instantiate(projectile, transform.position + Vector3.right, Quaternion.Euler(0, 0, 0));
        }
        else
        {
            Instantiate(projectile, transform.position + Vector3.left, Quaternion.Euler(0, 0, 180));
        }
        Invoke(nameof(ResetShoot), projectileTimer);
    }

    // wait for (projectileTimer) seconds before allowing the player to shoot again
    void ResetShoot()
    {
        canShoot = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("CyanOrb"))
        {
            cycle.PickUpColor("cyan");
            Destroy(other.gameObject);
        }
        else if (other.CompareTag("YellowOrb"))
        {
            cycle.PickUpColor("yellow");
            Destroy(other.gameObject);
        }
        else if (other.CompareTag("MagentaOrb"))
        {
            cycle.PickUpColor("magenta");
            Destroy(other.gameObject);
        }
        else if (other.CompareTag("Boss"))
        {
            PlayerDeath();
        }
        else if (other.CompareTag("AllowMove"))
        {
            movementLocked = false;
        }
        else if (other.CompareTag("StageExit"))
        {
            StageComplete();
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Obstacle")){
            PlayerDeath();
        }
        else if (other.gameObject.CompareTag("Platform"))
        {
            return;
        }
        else if (other.gameObject.tag != cycle.GetColor())
        {
            PlayerDeath();
        }
    }

    void PlayerDeath()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void StageComplete()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

}

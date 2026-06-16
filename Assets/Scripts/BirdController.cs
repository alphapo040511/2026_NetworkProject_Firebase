using UnityEngine;

public class BirdController : MonoBehaviour
{
    [SerializeField] float jumpForce = 5f;

    Rigidbody2D rb;
    bool isDead;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (isDead)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            rb.linearVelocity = Vector2.up * jumpForce;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead)
            return;

        isDead = true;

        GameManager.Instance.GameOver();

        gameObject.SetActive(false);
    }
}
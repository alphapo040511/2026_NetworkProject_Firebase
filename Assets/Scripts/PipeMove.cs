using UnityEngine;

public class PipeMove : MonoBehaviour
{
    [SerializeField] float moveSpeed = 3f;

    private void Update()
    {
        transform.Translate(
            Vector3.left *
            moveSpeed *
            Time.deltaTime);

        if (transform.position.x < -15f)
        {
            Destroy(gameObject);
        }
    }
}
using UnityEngine;

public class ScoreZone : MonoBehaviour
{
    bool scored;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (scored)
            return;

        if (other.CompareTag("Player"))
        {
            scored = true;

            GameManager.Instance.AddScore();
        }
    }
}
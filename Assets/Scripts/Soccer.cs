using UnityEngine;

public class Soccer : MonoBehaviour
{
    private LogicManager logicManager;
    private float moveSpeed = 4.5f;
    private Rigidbody rb;

    private void Start()
    {
        if (logicManager == null)
        {
            logicManager = FindFirstObjectByType<LogicManager>();
        }

        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (rb == null) return;

        Node currentNode = logicManager.GetCurrentNode();
        if (currentNode == null)
            return;

        Vector3 targetPosition = new Vector3(currentNode.position.x, 0, currentNode.position.y);
        Vector3 direction = (targetPosition - transform.position);
        direction.y = 0;

        if (direction.magnitude > 0.05f)
        {
            direction.Normalize();
            rb.linearVelocity = direction * moveSpeed;
        }
        else
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
}
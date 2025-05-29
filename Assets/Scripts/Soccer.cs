using UnityEngine;

public class Soccer : MonoBehaviour
{
    private LogicManager logicManager;
    private float moveSpeed = 3;
    private Rigidbody rb;
    public bool movingToGoal {get; private set; } = false;
private Vector3 goalTarget;

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

        if (logicManager != null && logicManager.isGameOver)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.constraints = RigidbodyConstraints.FreezeAll;
            return;
        }

        if (movingToGoal)
        {
            Vector3 dir = (goalTarget - transform.position);
            dir.y = 0;

            if (dir.magnitude > 0.05f)
            {
                rb.constraints = RigidbodyConstraints.None;
                dir.Normalize();
                rb.linearVelocity = dir * moveSpeed;
            }
            else
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.position = goalTarget;
                rb.constraints = RigidbodyConstraints.FreezeAll;
                movingToGoal = false;

                if (logicManager != null)
                    logicManager.isGameOver = true;
            }
            return;
        }

        Node currentNode = logicManager.GetCurrentNode();
        if (currentNode == null)
            return;

        Vector3 targetPosition = new Vector3(currentNode.position.x, 0, currentNode.position.y);
        Vector3 direction = (targetPosition - transform.position);
        direction.y = 0;

        if (direction.magnitude > 0.05f)
        {
            rb.constraints = RigidbodyConstraints.None;
            direction.Normalize();
            rb.linearVelocity = direction * moveSpeed;
        }
        else
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.position = targetPosition;
            rb.constraints = RigidbodyConstraints.FreezeAll;
        }

    }

    public void MoveToGoal(Vector3 goalPos)
    {
        goalTarget = goalPos;
        movingToGoal = true;
    }
}
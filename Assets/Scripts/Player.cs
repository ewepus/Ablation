using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private float movementSpeed = 10f;
    [SerializeField] private float slideDeceleration = 15f;

    private new Rigidbody2D rigidbody;

    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody2D>();
        rigidbody.linearDamping = slideDeceleration;
    }

    private void FixedUpdate()
    {
        Vector2 inputVector = GameInput.Instance.GetMovementVector().normalized;

        if (inputVector.magnitude > 0.1f)
        {
            rigidbody.linearVelocity = inputVector * movementSpeed;
        }

        //rigidbody.MovePosition(rigidbody.position + inputVector * (movementSpeed * Time.fixedDeltaTime));
    }
}

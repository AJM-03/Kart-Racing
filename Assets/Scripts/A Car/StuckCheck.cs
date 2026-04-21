using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StuckCheck : MonoBehaviour
{
    // The player's rigidbody
    private Rigidbody rb;

    // The player's input
    private Vector2 input;

    // Number of seconds player is stationary
    private float stuckTime = 0f;

    // Number of seconds before player is considered stuck
    private const float stuckThreshold = 1.0f;

    // Adjust this value to suit your needs
    readonly float velocityThreshold = 0.3f;

    private void Awake()
    {
        // Get the player's rigidbody and collider
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        // Get the player's input from the horizontal and vertical axis
        input.x = Input.GetAxis("Fire1");
        input.y = Input.GetAxis("Fire2");

        // If the player is trying to move and their velocity is near zero, they are stuck
        if (rb.velocity.magnitude < velocityThreshold && input != Vector2.zero)
        {
            // Increment the stuck time
            stuckTime += Time.deltaTime;

            if (stuckTime >= stuckThreshold)
            {
                // The player has been stuck for over 1 second, so move the player slightly in a random direction
                rb.MovePosition(rb.position + Random.insideUnitSphere * 0.5f);

                // Reset the stuck time
                stuckTime = 0f;
            }
        }
        else
        {
            // Reset the stuck time if the player is not stuck
            stuckTime = 0f;
        }
    }
}
using UnityEngine;

public class InfiniteBackground : MonoBehaviour
{
    public float scrollSpeed = 5f;
    public float backgroundWidth; // Set this to the width of your sprite

    void Update()
    {
        // Move the background to the left
        transform.Translate(Vector3.left * scrollSpeed * Time.deltaTime);

        // If the background has moved off-screen, snap it back to the other side
        if (transform.position.x <= -backgroundWidth)
        {
            Vector3 resetPos = new Vector3(backgroundWidth, transform.position.y, transform.position.z);
            transform.position += new Vector3(backgroundWidth * 2, 0, 0);
        }
    }
}

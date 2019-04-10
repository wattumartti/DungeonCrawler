using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        // Translation
        Vector2 translation = GetInputTranslationDirection() * Time.deltaTime;

        // Speed up movement when shift key held
        if (Input.GetKey(KeyCode.LeftShift))
        {
            translation *= 10.0f;
        }

        Translate(translation);
    }

    public void Translate(Vector2 translation)
    {
        float newX = this.transform.position.x + translation.x;
        float newY = this.transform.position.y + translation.y;
        this.transform.position = new Vector3(newX, newY, this.transform.position.z);
    }

    Vector3 GetInputTranslationDirection()
    {
        Vector2 direction = new Vector2();
        if (Input.GetKey(KeyCode.W))
        {
            direction += Vector2.up;
        }
        if (Input.GetKey(KeyCode.S))
        {
            direction += Vector2.down;
        }
        if (Input.GetKey(KeyCode.A))
        {
            direction += Vector2.left;
        }
        if (Input.GetKey(KeyCode.D))
        {
            direction += Vector2.right;
        }

        return direction;
    }
}

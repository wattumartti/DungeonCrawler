using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public class PlayerController : MonoBehaviour
{
    public CharacterController characterController = null;

    private static Vector3 _playerPosition = Vector3.zero;

    internal static Vector2 PlayerPosition
    {
        get
        {
            int xPosition = _playerPosition.x >= 0 ? (int)(_playerPosition.x + 0.5f) : (int)(_playerPosition.x - 0.5f);
            int yPosition = _playerPosition.y >= 0 ? (int)(_playerPosition.y + 0.5f) : (int)(_playerPosition.y - 0.5f);
            return new Vector2(xPosition, yPosition);
        }
    }
    internal ReactiveProperty<BaseRoom> currentRoom = new ReactiveProperty<BaseRoom>();

    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;

        this.currentRoom.Subscribe(x => RoomGenerator.CreateConnectedRooms(x));
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

        if (translation != Vector2.zero)
        {
            Translate(translation);
        }

        SetPositionAndRoom();
    }

    private void SetPositionAndRoom()
    {
        _playerPosition = this.transform.position;

        if (!RoomGenerator.roomLocations.ContainsKey(PlayerPosition))
        {
            UnityEngine.Debug.LogError("Player out of bounds?");
            return;
        }

        BaseRoom room = RoomGenerator.roomLocations[PlayerPosition];

        if (room != this.currentRoom.Value)
        {
            this.currentRoom.Value = room;
        }
    }

    public void Translate(Vector2 translation)
    {
        this.characterController?.Move(translation);
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

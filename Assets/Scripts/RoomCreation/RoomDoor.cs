using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomDoor : MonoBehaviour
{
    public Vector2 doorEntrance = Vector2.zero;
    public Vector2 doorExit = Vector2.zero;

    public BaseRoom entryRoom = null;
    public BaseRoom exitRoom = null;
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomDoor : MonoBehaviour
{
    internal List<BaseRoom> connectedRooms = new List<BaseRoom>();
    public Vector2 doorEntrance = Vector2.zero;
    public Vector2 doorExit = Vector2.zero;
}

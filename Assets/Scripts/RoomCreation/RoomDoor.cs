using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomDoor : MonoBehaviour
{
    internal Dictionary<Vector2, BaseRoom> connectedRooms = new Dictionary<Vector2, BaseRoom>();

    internal bool IsConnectingRooms()
    {
        if (connectedRooms.Count < 2)
        {
            return false;
        }

        foreach (KeyValuePair<Vector2, BaseRoom> kvp in this.connectedRooms)
        {
            if (kvp.Value == null)
            {
                return false;
            }
        }

        return true;
    }

    internal Vector2 GetLocation()
    {
        Vector2 location = new Vector2(this.transform.position.x, this.transform.position.y);

        return location;
    }
}

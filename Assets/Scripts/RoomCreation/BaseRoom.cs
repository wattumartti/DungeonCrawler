using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseRoom : MonoBehaviour
{
    internal Guid roomId;
    internal List<Vector2> roomLocations = new List<Vector2>();

    public List<RoomDoor> roomDoors = new List<RoomDoor>();

    private void CreateDoors()
    {

    }

    internal void InitRoom()
    {
        this.roomId = Guid.NewGuid();

        // Add the created room to the manager's list
        RoomGenerator.roomDictionary.Add(this.roomId, this);

        foreach (Vector2 pos in this.roomLocations)
        {
            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj.transform.SetParent(this.transform);
            obj.transform.localPosition = pos;

            if (RoomGenerator.Instance?.firstRoom == this)
            {
                obj.GetComponent<MeshRenderer>().material.color = Color.black;
            }
        }

        CreateDoors();
    }

    internal List<BaseRoom> GetNeighbors()
    {
        List<BaseRoom> neighbors = new List<BaseRoom>();
        foreach (RoomDoor door in this.roomDoors)
        {
            BaseRoom room = door.exitRoom;

            if (!neighbors.Contains(room))
            {
                neighbors.Add(room);
            }
        }

        return neighbors;
    }
}

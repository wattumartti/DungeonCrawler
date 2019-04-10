using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseRoom : MonoBehaviour
{
    internal Guid roomId;
    internal List<Vector2> roomLocations = new List<Vector2>();
    internal List<BaseRoom> neighboringRooms = new List<BaseRoom>();

    internal List<RoomDoor> roomDoors = new List<RoomDoor>();

    /// <summary>
    /// Create new doors for this room
    /// </summary>
    /// <param name="createdFromDoor"></param>
    private void CreateDoors(RoomDoor createdFromDoor)
    {
        if (createdFromDoor == null)
        {
            return;
        }

        List<Vector2> directions = new List<Vector2>
        {
            Vector2.left,
            Vector2.right,
            Vector2.up,
            Vector2.down
        };

        // Find furthest tiles in all directions except current door direction      
        Vector2 excludedDirection = GetDoorEntrance(createdFromDoor) - GetDoorExit(createdFromDoor);

        directions.Remove(excludedDirection);

        for (int i = 0; i < directions.Count; ++i)
        {
            Vector2 direction = directions[i];

            Vector2 doorLocation = FindFurthestTileInDirection(direction);

            if (doorLocation == Vector2.zero)
            {
                continue;
            }

            RoomDoor newDoor = Instantiate(RoomGenerator.Instance?.roomDoorPrefab, this.transform);
            newDoor.connectedRooms.Add(doorLocation, this);
            newDoor.connectedRooms.Add(doorLocation + direction, null);
            this.roomDoors.Add(newDoor);
        }
    }

    internal void InitRoom(RoomDoor createdFromDoor)
    {
        this.roomId = Guid.NewGuid();

        // Add the created room to the manager's list
        RoomGenerator.roomDictionary.Add(this.roomId, this);

        foreach (Vector2 pos in this.roomLocations)
        {
            // Create a primitive cube to display tile location
            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj.transform.SetParent(this.transform);
            obj.transform.localPosition = pos;

            if (RoomGenerator.Instance?.firstRoom == this)
            {
                obj.GetComponent<MeshRenderer>().material.color = Color.black;
            }
        }

        CreateDoors(createdFromDoor);
    }

    /// <summary>
    /// Find the room tile that is the furthest in given direction
    /// </summary>
    /// <param name="direction"></param>
    /// <returns></returns>
    private Vector2 FindFurthestTileInDirection(Vector2 direction)
    {
        float furthestValue = float.MinValue; 
        Vector2 furthestPoint = Vector2.zero;

        for (int i = 0; i < this.roomLocations.Count; ++i)
        {
            Vector2 location = this.roomLocations[i];
            float tempValue = Vector2.Dot(direction, location);

            if (tempValue > furthestValue)
            {
                furthestValue = tempValue;
                furthestPoint = location;
            }
        }
        
        return furthestPoint;
    }

    internal Vector2 GetDoorExit(RoomDoor door)
    {
        Vector2 doorExit = Vector2.zero;

        foreach (KeyValuePair<Vector2, BaseRoom> kvp in door.connectedRooms)
        {
            if (kvp.Value == this)
            {
                continue;
            }

            doorExit = kvp.Key;
        }

        return doorExit;
    }

    internal Vector2 GetDoorEntrance(RoomDoor door)
    {
        Vector2 doorEntrance = Vector2.zero;

        foreach (KeyValuePair<Vector2, BaseRoom> kvp in door.connectedRooms)
        {
            if (kvp.Value != this)
            {
                continue;
            }

            doorEntrance = kvp.Key;
        }

        return doorEntrance;
    }
}

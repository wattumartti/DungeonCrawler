using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseRoom : MonoBehaviour
{
    internal Guid roomId;
    internal List<Vector2> roomLocations = new List<Vector2>();
    internal List<BaseRoom> neighboringRooms = new List<BaseRoom>();

    public List<RoomDoor> roomDoors = new List<RoomDoor>();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="createdFromDoor"></param>
    private void CreateDoors(RoomDoor createdFromDoor)
    {
        if (createdFromDoor == null)
        {
            return;
        }

        // Find furthest tiles in all directions except current door direction      
        Vector2 excludedDirection = createdFromDoor.doorExit - createdFromDoor.doorEntrance;

        for (int i = -1; i <= 1; ++i)
        {
            for (int j = -1; j <= 1; ++j)
            {
                if (i == j || (i != 0 && j != 0))
                {
                    continue;
                }

                Vector2 direction = new Vector2(i, j);

                if (direction == excludedDirection)
                {
                    continue;
                }

                Vector2 doorLocation = FindFurthestTileInDirection(direction);

                if (doorLocation == Vector2.zero)
                {
                    continue;
                }

                RoomDoor newDoor = Instantiate(RoomGenerator.Instance?.roomDoorPrefab, this.transform);
                newDoor.connectedRooms.Add(this);
                newDoor.doorEntrance = doorLocation;
                newDoor.doorExit = doorLocation + direction;
                UnityEngine.Debug.Log(doorLocation + " + " + direction);
                this.roomDoors.Add(newDoor);
            }
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
        float furthestValue = 0;
        Vector2 furthestPoint = Vector2.zero;

        for (int i = 0; i < this.roomLocations.Count; ++i)
        {
            Vector2 location = this.roomLocations[i];
            float tempValue = Vector2.Dot(location, direction) / location.magnitude;

            if (tempValue > furthestValue)
            {
                furthestValue = tempValue;
                furthestPoint = location;
            }
        }

        return furthestPoint;
    }
}

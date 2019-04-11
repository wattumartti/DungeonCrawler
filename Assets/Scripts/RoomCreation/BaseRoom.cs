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
    internal Dictionary<Vector2, GameObject> roomWalls = new Dictionary<Vector2, GameObject>();

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

            Vector2 setLocation = doorLocation + (direction * 0.5f);

            if (RoomGenerator.roomLocations.ContainsKey(doorLocation + direction))
            {
                BaseRoom room = RoomGenerator.roomLocations[doorLocation + direction];
                if (room.roomWalls.ContainsKey(setLocation))
                {
                    GameObject destroyWall = room.roomWalls[setLocation];
                    room.roomWalls.Remove(setLocation);
                    Destroy(destroyWall);
                }
            }

            RoomDoor newDoor = Instantiate(RoomGenerator.Instance?.roomDoorPrefab, this.transform);
            newDoor.transform.position = setLocation;
            newDoor.transform.position += new Vector3(0, 0, -5);
            newDoor.transform.rotation = Quaternion.Euler(0, 0, (direction.y != 0 ? 90 : 0));
            newDoor.connectedRooms.Add(doorLocation, this);
            newDoor.connectedRooms.Add(doorLocation + direction, null);
            this.roomDoors.Add(newDoor);
        }
    }

    private void CreateWalls()
    {
        List<Vector2> doorExits = GetDoorExits();
        foreach (Vector2 tile in this.roomLocations)
        {
            List<Vector2> emptyNeighboringTiles = RoomGenerator.GetOpenNeighboringTiles(tile, this.roomLocations);

            for (int i = 0; i < emptyNeighboringTiles.Count; ++i)
            {
                Vector2 neighboringTile = emptyNeighboringTiles[i];

                if (doorExits.Contains(neighboringTile))
                {
                    continue;
                }

                // Create a wall
                GameObject obj = Instantiate(RoomGenerator.Instance?.roomWallPrefab, this.transform);

                // Rotate and position the wall
                Vector2 direction = neighboringTile - tile;
                obj.transform.rotation = Quaternion.Euler(0, 0, (direction.y != 0 ? 90 : 0));
                obj.transform.position = tile + (direction * 0.5f);
                // Adjust to player position
                obj.transform.position += new Vector3(0, 0, -5);

                this.roomWalls.Add(tile + (direction * 0.5f), obj);
            }
        }
    }

    private List<Vector2> GetDoorExits()
    {
        List<Vector2> exits = new List<Vector2>();
        foreach (RoomDoor door in this.roomDoors)
        {
            Vector2 doorExit = GetDoorExit(door);
            if (doorExit != Vector2.zero)
            {
                exits.Add(doorExit);
            }
        }

        return exits;
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
        CreateWalls();
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

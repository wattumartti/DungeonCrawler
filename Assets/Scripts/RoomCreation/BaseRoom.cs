﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseRoom : MonoBehaviour
{
    internal Guid roomId;
    internal List<Vector2> roomLocations = new List<Vector2>();
    internal List<BaseRoom> neighboringRooms = new List<BaseRoom>();

    internal Dictionary<Vector2, RoomDoor> roomDoors = new Dictionary<Vector2, RoomDoor>();
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

        List<Vector2> possibleDirections = new List<Vector2>
        {
            Vector2.left,
            Vector2.right,
            Vector2.up,
            Vector2.down
        };

        // Find furthest tiles in all directions except current door direction      
        Vector2 excludedDirection = GetDoorExit(createdFromDoor) - GetDoorEntrance(createdFromDoor);

        possibleDirections.Remove(excludedDirection);

        List<Vector2> directions = new List<Vector2>();

        if (RoomGenerator.Instance?.doorAmountPerRoom > 3)
        {
            throw new ArgumentOutOfRangeException("Door count higher than 3 will break things!");
        }

        for (int i = 0; i < RoomGenerator.Instance?.doorAmountPerRoom; ++i)
        {
            int randomNumber = UnityEngine.Random.Range(0, possibleDirections.Count);

            directions.Add(possibleDirections[randomNumber]);
            possibleDirections.RemoveAt(randomNumber);
        }

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
                // Disabled creating new doors to old rooms for now
                continue;
            }

            RoomDoor newDoor = Instantiate(RoomGenerator.Instance?.roomDoorPrefab, this.transform);
            newDoor.transform.position = setLocation;
            newDoor.transform.position += new Vector3(0, 0, -5);
            newDoor.transform.rotation = Quaternion.Euler(0, 0, (direction.y != 0 ? 90 : 0));
            newDoor.connectedRooms.Add(doorLocation, this);
            newDoor.connectedRooms.Add(doorLocation + direction, null);
            this.roomDoors.Add(setLocation, newDoor);
        }
    }

    private void CreateWalls()
    {
        foreach (Vector2 tile in this.roomLocations)
        {
            List<Vector2> neighboringTiles = RoomGenerator.GetNeighboringTilesNotInRoom(tile, this);

            for (int i = 0; i < neighboringTiles.Count; ++i)
            {
                Vector2 neighboringTile = neighboringTiles[i];

                Vector2 direction = neighboringTile - tile;
                Vector2 doorLocation = tile + direction * 0.5f;

                if (this.roomDoors.ContainsKey(doorLocation))
                {
                    continue;
                }

                if (RoomGenerator.roomLocations.ContainsKey(neighboringTile))
                {
                    BaseRoom neighboringRoom = RoomGenerator.roomLocations[neighboringTile];
   
                    if (neighboringRoom.roomDoors.ContainsKey(doorLocation))
                    {
                        RoomDoor neighborDoor = neighboringRoom.roomDoors[doorLocation];
                        this.roomDoors.Add(doorLocation, neighborDoor);

                        continue;
                    }
                }

                // Create a wall
                GameObject obj = Instantiate(RoomGenerator.Instance?.roomWallPrefab, this.transform);

                // Rotate and position the wall
                obj.transform.rotation = Quaternion.Euler(0, 0, (direction.y != 0 ? 90 : 0));
                obj.transform.position = doorLocation;
                // Adjust to player position
                obj.transform.position += new Vector3(0, 0, -5);

                this.roomWalls.Add(doorLocation, obj);
            }
        }
    }

    internal List<Vector2> GetDoorExits()
    {
        List<Vector2> exits = new List<Vector2>();
        foreach (KeyValuePair<Vector2, RoomDoor> kvp in this.roomDoors)
        {
            Vector2 doorExit = GetDoorExit(kvp.Value);
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
    internal Vector2 FindFurthestTileInDirection(Vector2 direction)
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
        foreach (KeyValuePair<Vector2, BaseRoom> kvp in door.connectedRooms)
        {
            if (kvp.Value != this)
            {
                return kvp.Key;
            }
        }

        return Vector2.zero;
    }

    internal Vector2 GetDoorEntrance(RoomDoor door)
    {
        foreach (KeyValuePair<Vector2, BaseRoom> kvp in door.connectedRooms)
        {
            if (kvp.Value == this)
            {
                return kvp.Key;
            }
        }

        return Vector2.zero;
    }

    /// <summary>
    /// Gets the distance of the closest tile to the player
    /// </summary>
    /// <returns></returns>
    internal float GetDistanceToPlayer()
    {
        float shortestDistance = float.MaxValue;
        Vector2 playerPosition = PlayerController.PlayerPosition;

        for (int i = 0; i < this.roomLocations.Count; ++i)
        {
            Vector2 tile = this.roomLocations[i];

            float distance = Vector2.Distance(tile, playerPosition);

            if (distance < shortestDistance)
            {
                shortestDistance = distance;
            }
        }

        return shortestDistance;
    }

    internal bool IsNeighboringRoomToPlayer()
    {
        foreach (KeyValuePair<Vector2, RoomDoor> kvp in this.roomDoors)
        {
            if (kvp.Value.connectedRooms.ContainsValue(PlayerController.Instance.currentRoom.Value))
            {
                return true;
            }
        }

        return false;
    }
}

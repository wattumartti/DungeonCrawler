using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class RoomGenerator : MonoBehaviour
{
    internal static RoomGenerator Instance = null;
    // Full list of rooms by room ID
    internal static Dictionary<Guid, BaseRoom> roomDictionary = new Dictionary<Guid, BaseRoom>();
    internal static Dictionary<Vector2, BaseRoom> roomLocations = new Dictionary<Vector2, BaseRoom>();

    public float additionalTileChance = 0.5f;
    public int doorAmountPerRoom = 2;
    public float roomDespawnDistance = 10;

    public BaseRoom baseRoomPrefab = null;
    public BaseRoom firstRoom = null;
    public RoomDoor roomDoorPrefab = null;
    public GameObject roomWallPrefab = null;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        CreateFirstRoom();
        CreateConnectedRooms(this.firstRoom);
    }

    private void Update()
    {
        // Create a temporary dictionary with all rooms that are eligible for despawning
        // Necessary because despawned rooms should also be removed from roomDictionary
        Dictionary<Guid, BaseRoom> despawnRooms = new Dictionary<Guid, BaseRoom>();
        foreach (KeyValuePair<Guid, BaseRoom> kvp in roomDictionary)
        {
            if (kvp.Value.GetDistanceToPlayer() > this.roomDespawnDistance)
            {
                despawnRooms.Add(kvp.Key, kvp.Value);
            }
        }

        // Enumerate through and despawn the rooms
        foreach (KeyValuePair<Guid, BaseRoom> kvp in despawnRooms)
        {
            HandleRoomDespawning(kvp.Value, kvp.Key);
        }

#if UNITY_EDITOR
        // ONLY FOR TESTING PURPOSES!!
        if (Input.GetKeyDown(KeyCode.K))
        {
            foreach (KeyValuePair<Guid, BaseRoom> kvp in roomDictionary)
            {
                if (kvp.Value == null)
                {
                    continue;
                }

                foreach (KeyValuePair<Vector2, RoomDoor> doorPair in kvp.Value.roomDoors)
                {
                    if (doorPair.Value.IsConnectingRooms())
                    {
                        continue;
                    }

                    CreateConnectedRooms(kvp.Value);
                    return;
                }
            }
        }
        // ONLY FOR TESTING PURPOSES!!
#endif
    }

    /// <summary>
    /// Removes the room from the overall dictionary, sets all door parents and destroys the room object
    /// </summary>
    /// <param name="room">Room to destroy</param>
    /// <param name="roomId">Unique ID of the room</param>
    private void HandleRoomDespawning(BaseRoom room, Guid roomId)
    {
        // Remove the room from overall dictionary
        roomDictionary.Remove(roomId);
        
        Dictionary<Vector2, RoomDoor> roomDoors = room.roomDoors;
        Dictionary<RoomDoor, BaseRoom> neighboringRooms = new Dictionary<RoomDoor, BaseRoom>();

        // Enumerate through the room's doors
        foreach (KeyValuePair<Vector2, RoomDoor> kvp in roomDoors)
        {
            RoomDoor door = kvp.Value;

            Vector2 doorExit = room.GetDoorExit(door);

            // Skip doors that don't have another room connected
            if (!roomLocations.ContainsKey(doorExit))
            {
                continue;
            }

            BaseRoom exitRoom = roomLocations[doorExit];

            neighboringRooms.Add(door, exitRoom);
        }

        // Enumerate through all doors
        foreach (KeyValuePair<RoomDoor, BaseRoom> kvp in neighboringRooms)
        {
            BaseRoom neighboringRoom = kvp.Value;

            if (neighboringRoom == null)
            {
                continue;
            }

            RoomDoor currentDoor = kvp.Key;

            Vector2 setLocation = currentDoor.GetLocation();

            // Change connected door's parent to their other connected room so they don't get destroyed
            currentDoor.transform.SetParent(neighboringRoom.transform);   
        }

        // Loop through all the tile locations for the room to be destroyed
        for (int i = 0; i < room.roomLocations.Count; ++i)
        {
            Vector2 location = room.roomLocations[i];
            if (roomLocations.ContainsKey(location))
            {
                // Remove the tile location from the overall used locations
                roomLocations.Remove(location);
            }
        }

        // Finally destroy the room object
        Destroy(room.gameObject);
    }

    /// <summary>
    /// Creates the starting room for the generator
    /// </summary>
    private void CreateFirstRoom()
    {
        if (this.firstRoom == null)
        {
            return;
        }

        // Create a 3x3 room to start from
        for (int i = -1; i < 2; i++)
        {
            for (int j = -1; j < 2; j++)
            {
                this.firstRoom.roomLocations.Add(new Vector2(i, j));
            }
        }

        // Add the used locations to the overall dictionary
        foreach (Vector2 pos in this.firstRoom.roomLocations)
        {
            roomLocations.Add(pos, this.firstRoom);
        }

        CreateRoomDoors(null, this.firstRoom);

        this.firstRoom.InitRoom(null);
    }

    /// <summary>
    /// Creates doors for the first room
    /// </summary>
    private void CreateRoomDoors(RoomDoor createdFromDoor, BaseRoom room)
    {
        if (room == null)
        {
            return;
        }

        // Ignore restrictions for the first room
        bool ignoreRestrictions = createdFromDoor == null;

        List<Vector2> possibleDirections = new List<Vector2>
        {
            Vector2.left,
            Vector2.right,
            Vector2.up,
            Vector2.down
        };

        List<Vector2> directions = new List<Vector2>();

        if (!ignoreRestrictions)
        {
            // Find furthest tiles in all directions except current door direction      
            Vector2 excludedDirection = room.GetDoorExit(createdFromDoor) - room.GetDoorEntrance(createdFromDoor);

            possibleDirections.Remove(excludedDirection);

            for (int i = 0; i < Instance?.doorAmountPerRoom; ++i)
            {
                int randomNumber = UnityEngine.Random.Range(0, possibleDirections.Count);

                directions.Add(possibleDirections[randomNumber]);
                possibleDirections.RemoveAt(randomNumber);
            }
        }
        else
        {
            directions = possibleDirections;
        }

        for (int i = 0; i < directions.Count; ++i)
        {
            Vector2 direction = directions[i];

            Vector2 doorLocation = room.FindFurthestTileInDirection(direction);

            // Skip if no eligible location was found
            if (doorLocation == Vector2.zero)
            {
                continue;
            }

            Vector2 setLocation = doorLocation + (direction * 0.5f);

            if (roomLocations.ContainsKey(doorLocation + direction))
            {
                // Disabled creating new doors to old rooms for now
                continue;

                //BaseRoom room = RoomGenerator.roomLocations[doorLocation + direction];
                //if (room.roomWalls.ContainsKey(setLocation))
                //{
                //    GameObject destroyWall = room.roomWalls[setLocation];
                //    room.roomWalls.Remove(setLocation);
                //    Destroy(destroyWall);
                //}
            }

            RoomDoor newDoor = Instantiate(Instance?.roomDoorPrefab, room.transform);
            newDoor.transform.position = setLocation;
            newDoor.transform.position += new Vector3(0, 0, -5);
            newDoor.transform.rotation = Quaternion.Euler(0, 0, (direction.y != 0 ? 90 : 0));
            newDoor.connectedRooms.Add(doorLocation, room);
            newDoor.connectedRooms.Add(doorLocation + direction, null);
            room.roomDoors.Add(setLocation, newDoor);
        }
    }

    /// <summary>
    /// Creates adjacent rooms to the given room
    /// </summary>
    /// <param name="room"></param>
    internal static void CreateConnectedRooms(BaseRoom room)
    {
        if (room == null)
        {
            Debug.LogError("Given room is null!");
            return;
        }

        if (Instance == null || Instance.baseRoomPrefab == null)
        {
            Debug.LogError("Instance or room prefab is null!");
            return;
        }

        // Enumerate through the room's doors
        foreach (KeyValuePair<Vector2, RoomDoor> kvp in room.roomDoors)
        {
            Vector2 doorExit = room.GetDoorExit(kvp.Value);
            // If there's already a room on the other side of the door, skip and add that room to the door's info
            if (roomLocations.ContainsKey(doorExit) && doorExit != Vector2.zero)
            {
                BaseRoom exitRoom = roomLocations[doorExit];

                kvp.Value.connectedRooms[doorExit] = exitRoom;

                if (!exitRoom.roomDoors.ContainsKey(kvp.Key))
                {
                    exitRoom.roomDoors.Add(kvp.Key, kvp.Value);
                }

                continue;
            }

            CreateRoom(kvp.Value, room);
        }
    }

    /// <summary>
    /// Creates a single room that should be connected to the given door
    /// </summary>
    /// <param name="door"></param>
    private static void CreateRoom(RoomDoor door, BaseRoom connectedRoom)
    {
        if (door == null || connectedRoom == null)
        {
            Debug.LogError("Given door or room is null!");
            return;
        }

        Vector2 doorExit = connectedRoom.GetDoorExit(door);
        // Get the first tile locations to start the recursive operation
        List<Vector2> firstTileLocations = GetOpenNeighboringTiles(doorExit);

        List<Vector2> roomTiles = new List<Vector2>
        {
            doorExit
        };

        UnityEngine.Random.InitState((int)DateTime.UtcNow.Ticks);

        // Loop through the first locations and start creating tiles
        foreach (Vector2 pos in firstTileLocations)
        {
            float cumulativeChance = Instance.additionalTileChance;

            roomTiles = RandomRoomLayoutRecursive(roomTiles, cumulativeChance, pos);
        }

        // When all room tiles have been found, instantiate the object
        BaseRoom newRoom = Instantiate(Instance?.baseRoomPrefab);

        newRoom.roomLocations = roomTiles;
        // Initialize the room after instantiation
        newRoom.InitRoom(door);

        door.connectedRooms[doorExit] = newRoom;

        PlaceRoom(newRoom);
    }

    /// <summary>
    /// Generates a random tile layout for the room using a cumulative probability
    /// </summary>
    /// <param name="tileList"></param>
    /// <param name="cumulativeChance"></param>
    /// <param name="possibleLocation"></param>
    /// <returns></returns>
    private static List<Vector2> RandomRoomLayoutRecursive(List<Vector2> tileList, float cumulativeChance, Vector2 possibleLocation)
    {
        float randomValue = UnityEngine.Random.Range(0f, 1f);

        if (randomValue >= cumulativeChance)
        {
            return tileList;
        }

        if (!tileList.Contains(possibleLocation))
        {
            tileList.Add(possibleLocation);
        }

        cumulativeChance *= cumulativeChance;

        List<Vector2> nextOpenTiles = GetOpenNeighboringTiles(possibleLocation, tileList);

        foreach (Vector2 pos in nextOpenTiles)
        {
            tileList = RandomRoomLayoutRecursive(tileList, cumulativeChance, pos);
        }

        return tileList;
    }

    /// <summary>
    /// Gets all neighboring tiles that are empty. Can be given a list of tiles to ignore
    /// </summary>
    /// <param name="center"></param>
    /// <param name="excludedTiles"></param>
    /// <returns></returns>
    internal static List<Vector2> GetOpenNeighboringTiles(Vector2 center, List<Vector2> excludedTiles = null)
    {
        List<Vector2> tileLocations = new List<Vector2>
        {
            center + Vector2.up,
            center + Vector2.down,
            center + Vector2.left,
            center + Vector2.right
        };

        for (int i = tileLocations.Count - 1; i >= 0; i--)
        {
            Vector2 pos = tileLocations[i];
            if (roomLocations.ContainsKey(pos) || (excludedTiles != null && excludedTiles.Contains(pos)))
            {
                tileLocations.RemoveAt(i);
            }
        }

        return tileLocations;
    }

    internal static List<Vector2> GetNeighboringTiles(Vector2 center)
    {
        List<Vector2> tileLocations = new List<Vector2>
        {
            center + Vector2.up,
            center + Vector2.down,
            center + Vector2.left,
            center + Vector2.right
        };

        return tileLocations;
    }

    internal static RoomDoor GetDoorWithLocation(Vector2 location, BaseRoom room)
    {
        foreach (KeyValuePair<Vector2, RoomDoor> kvp in room.roomDoors)
        {
            if (kvp.Value.connectedRooms.ContainsKey(location))
            {
                return kvp.Value;
            }
        }

        return null;
    }

    /// <summary>
    /// Adds the room to the managers list
    /// </summary>
    /// <param name="room"></param>
    private static void PlaceRoom(BaseRoom room)
    {
        if (room == null)
        {
            UnityEngine.Debug.LogError("Given room is null!");
            return;
        }

        for (int i = 0; i < room.roomLocations.Count; ++i)
        {
            roomLocations.Add(room.roomLocations[i], room);
        }
    }
}

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

    public BaseRoom baseRoomPrefab = null;
    public BaseRoom firstRoom = null;

    internal BaseRoom currentRoom = null;

    [SerializeField] private Vector2 _minRoomSize = Vector2.zero;
    [SerializeField] private Vector2 _maxRoomSize = Vector2.one;

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
        if (Input.GetKeyDown(KeyCode.H))
        {
            BaseRoom room = roomDictionary.First(x => x.Value != null && x.Value != this.firstRoom && !x.Value.isActiveAndEnabled).Value;

            room.gameObject.SetActive(true);
            room.transform.position = Vector3.zero;
        }
    }

    private void CreateFirstRoom()
    {
        if (this.firstRoom == null)
        {
            return;
        }

        foreach (Vector2 pos in this.firstRoom.roomLocations)
        {
            roomLocations.Add(pos, this.firstRoom);
        }

        this.firstRoom.InitRoom();
    }

    internal static void CreateConnectedRooms(BaseRoom room)
    {
        if (room == null)
        {
            UnityEngine.Debug.LogError("Given room is null!");
            return;
        }

        if (Instance == null || Instance.baseRoomPrefab == null)
        {
            UnityEngine.Debug.LogError("Instance or room prefab is null!");
            return;
        }

        foreach (RoomDoor door in room.roomDoors)
        {
            if (roomLocations.ContainsKey(door.doorExit))
            {
                BaseRoom exitRoom = roomLocations[door.doorExit];

                for (int i = 0; i < exitRoom.roomDoors.Count; ++i)
                {
                    RoomDoor possibleConnection = exitRoom.roomDoors[i];
                    if (possibleConnection.doorEntrance == door.doorExit)
                    {
                        door.exitRoom = roomLocations[door.doorExit];
                        break;
                    }
                }

                continue;
            }

            CreateRoom(door);
        }
    }

    private static void CreateRoom(RoomDoor door)
    {
        if (door == null)
        {
            UnityEngine.Debug.LogError("Given door is null!");
            return;
        }

        List<Vector2> firstTileLocations = GetOpenNeighboringTiles(door.doorExit);

        List<Vector2> roomTiles = new List<Vector2>();
        roomTiles.Add(door.doorExit);

        UnityEngine.Random.InitState((int)DateTime.UtcNow.Ticks);

        foreach (Vector2 pos in firstTileLocations)
        {
            float cumulativeChance = Instance.additionalTileChance;

            roomTiles = RandomRoomLayoutRecursive(roomTiles, cumulativeChance, pos);
        }

        BaseRoom newRoom = Instantiate(Instance?.baseRoomPrefab);
        newRoom.gameObject.SetActive(false);
        newRoom.transform.position = new Vector3(-10000, -10000);

        newRoom.roomLocations = roomTiles;
        newRoom.InitRoom();

        PlaceRoom(newRoom);
    }

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

    private static List<Vector2> GetOpenNeighboringTiles(Vector2 center, List<Vector2> excludedTiles = null)
    {
        List<Vector2> tileLocations = new List<Vector2>();

        tileLocations.Add(center + Vector2.up);
        tileLocations.Add(center + Vector2.down);
        tileLocations.Add(center + Vector2.left);
        tileLocations.Add(center + Vector2.right);

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

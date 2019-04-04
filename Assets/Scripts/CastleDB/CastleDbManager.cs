using CastleDBTypes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CastleDbManager : MonoBehaviour
{
    public TextAsset CastleDBAsset;

    internal static CastleDB CastleDB = null;

    private void Awake()
    {
        if (CastleDB == null)
        {
            CastleDB = new CastleDB(CastleDBAsset);
        }
    }
}

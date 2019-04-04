
using UnityEngine;
using CastleDBImporter;
using System.Collections.Generic;
using System;

namespace CastleDBTypes
{
    public class CastleDB
    {
        static CastleDBParser parsedDB;
        public EnemiesType Enemies;

        public CastleDB(TextAsset castleDBAsset)
        {
            parsedDB = new CastleDBParser(castleDBAsset);
            Enemies = new EnemiesType();
        }
        public class EnemiesType 
 {public Enemies TestEnemy { get { return Get(CastleDBTypes.Enemies.RowValues.TestEnemy); } } 
public Enemies Enemy2 { get { return Get(CastleDBTypes.Enemies.RowValues.Enemy2); } } 
public Enemies Enemy3 { get { return Get(CastleDBTypes.Enemies.RowValues.Enemy3); } } 
private Enemies Get(CastleDBTypes.Enemies.RowValues line) { return new Enemies(parsedDB.Root, line); }

                public Enemies[] GetAll() 
                {
                    var values = (CastleDBTypes.Enemies.RowValues[])Enum.GetValues(typeof(CastleDBTypes.Enemies.RowValues));
                    Enemies[] returnList = new Enemies[values.Length];
                    for (int i = 0; i < values.Length; i++)
                    {
                        returnList[i] = Get(values[i]);
                    }
                    return returnList;
                }
 } //END OF Enemies 

    }
}
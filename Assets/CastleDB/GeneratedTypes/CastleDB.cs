
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
public WeaponsType Weapons;

        public CastleDB(TextAsset castleDBAsset)
        {
            parsedDB = new CastleDBParser(castleDBAsset);
            Enemies = new EnemiesType();Weapons = new WeaponsType();
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
public class WeaponsType 
 {public Weapons PISTOL { get { return Get(CastleDBTypes.Weapons.RowValues.PISTOL); } } 
public Weapons SHOTGUN { get { return Get(CastleDBTypes.Weapons.RowValues.SHOTGUN); } } 
public Weapons Get(CastleDBTypes.Weapons.RowValues line) { return new Weapons(parsedDB.Root, line); }

                public Weapons[] GetAll() 
                {
                    var values = (CastleDBTypes.Weapons.RowValues[])Enum.GetValues(typeof(CastleDBTypes.Weapons.RowValues));
                    Weapons[] returnList = new Weapons[values.Length];
                    for (int i = 0; i < values.Length; i++)
                    {
                        returnList[i] = Get(values[i]);
                    }
                    return returnList;
                }
 } //END OF Weapons 

    }
}
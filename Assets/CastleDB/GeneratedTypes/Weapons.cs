
using UnityEngine;
using System;
using System.Collections.Generic;
using SimpleJSON;
using CastleDBImporter;
namespace CastleDBTypes
{ 
    public class Weapons
    {
        public string id;
public int damage;
public int clip_size;
public float spread;
public int bullet_count;
public string UID;

        public enum RowValues { 
PISTOL, 
SHOTGUN
 } 
        public Weapons (CastleDBParser.RootNode root, RowValues line) 
        {
            SimpleJSON.JSONNode node = root.GetSheetWithName("Weapons").Rows[(int)line];
id = node["id"];
damage = node["damage"].AsInt;
clip_size = node["clip_size"].AsInt;
spread = node["spread"].AsFloat;
bullet_count = node["bullet_count"].AsInt;
UID = node["UID"];

        }  
        
public static Weapons.RowValues GetRowValue(string name)
{
    var values = (RowValues[])Enum.GetValues(typeof(RowValues));
    for (int i = 0; i < values.Length; i++)
    {
        if(values[i].ToString() == name)
        {
            return values[i];
        }
    }
    return values[0];
}
    }
}
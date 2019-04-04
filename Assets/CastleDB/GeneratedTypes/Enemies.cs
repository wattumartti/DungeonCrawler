
using UnityEngine;
using System;
using System.Collections.Generic;
using SimpleJSON;
using CastleDBImporter;
namespace CastleDBTypes
{ 
    public class Enemies
    {
        public string id;
public string EnemyName;
public int Health;

        public enum RowValues { 
TestEnemy, 
Enemy2, 
Enemy3
 } 
        public Enemies (CastleDBParser.RootNode root, RowValues line) 
        {
            SimpleJSON.JSONNode node = root.GetSheetWithName("Enemies").Rows[(int)line];
id = node["id"];
EnemyName = node["EnemyName"];
Health = node["Health"].AsInt;

        }  
        
public static Enemies.RowValues GetRowValue(string name)
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
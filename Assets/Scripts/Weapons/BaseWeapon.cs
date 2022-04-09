using SimpleJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseWeapon : MonoBehaviour
{
    public enum WeaponType
    {
        NONE = 0,
        PISTOL = 1,
        SHOTGUN = 2,

        LAST
    }

    public WeaponType type = WeaponType.PISTOL;

    private CastleDBTypes.Weapons _weaponData = null;
    internal CastleDBTypes.Weapons WeaponData
    {
        get
        {
            return this._weaponData != null ? this._weaponData : GetWeaponData();
        }
        set
        {
            this._weaponData = value;
        }
    }

    internal int currentClipAmmo = 0;

    internal CastleDBTypes.Weapons GetWeaponData()
    {
        // Get weapon data with type
        if (CastleDbManager.CastleDB == null)
        {
            Debug.LogError("CastleDbManager.CastleDB is null!");
            return null;
        }

        CastleDBTypes.Weapons weaponData = CastleDbManager.CastleDB.Weapons.Get((CastleDBTypes.Weapons.RowValues)Enum.Parse(typeof(CastleDBTypes.Weapons.RowValues), this.type.ToString()));

        return weaponData;
    }

    internal void InitWeapon()
    {
        this.currentClipAmmo = this.WeaponData.clip_size;
    }

    internal void Shoot()
    {
        if (this.currentClipAmmo <= 0)
        {
            return;
        }

        SpawnBullets();
    }

    internal void SpawnBullets()
    {
        if (this.WeaponData.bullet_count <= 0)
        {
            return;
        }
    }
}

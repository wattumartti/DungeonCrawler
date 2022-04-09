using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipmentManager : MonoBehaviour
{
    public BaseWeapon.WeaponType currentWeaponType = BaseWeapon.WeaponType.NONE;

    internal BaseWeapon currentWeapon = null;

    internal BaseWeapon InstantiateWeaponPrefab(BaseWeapon.WeaponType weaponType)
    {
        if (this.currentWeapon != null)
        {
            Destroy(this.currentWeapon.gameObject);
        }

        if (weaponType == BaseWeapon.WeaponType.NONE)
        {
            return null;
        }

        Object loadedPrefab = Resources.Load("Weapons/" + weaponType.ToString());

        GameObject loadedWeapon = Instantiate(loadedPrefab, this.transform) as GameObject;
        loadedWeapon.name = weaponType.ToString();

        return loadedWeapon.GetComponent<BaseWeapon>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            int equipWeapon = (int)this.currentWeaponType + 1;

            if (equipWeapon >= (int)BaseWeapon.WeaponType.LAST)
            {
                equipWeapon = 0;
            }

            EquipWeapon((BaseWeapon.WeaponType)equipWeapon);
        }
    }

    internal void EquipWeapon(BaseWeapon.WeaponType weaponType)
    {
        BaseWeapon weapon = InstantiateWeaponPrefab(weaponType);

        if (weapon == null)
        {
            Debug.LogError("Invalid weapon type or prefab!");
        }

        this.currentWeaponType = weapon != null ? weapon.type : BaseWeapon.WeaponType.NONE;
        this.currentWeapon = weapon;
    }
}

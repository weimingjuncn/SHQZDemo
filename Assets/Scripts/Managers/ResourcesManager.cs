using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SHQZ
{
    public class ResourcesManager : MonoBehaviour
    {
        Dictionary<string, int> spell_ids = new Dictionary<string, int>();
        Dictionary<string, int> weapon_ids = new Dictionary<string, int>();
        public static ResourcesManager singleton;

        void Awake()
        {
            singleton = this;
            LoadWeaponIds();
            LoadSpellIds();
        }

        void LoadSpellIds()
        {
            SpellItemScriptableObject obj = Resources.Load("SHQZ.SpellItemScriptableObject") as SpellItemScriptableObject;
            if (obj == null)
            {
                Debug.Log("SHQZ.SpellItemScriptableObject couldnt be loaded!");
                return;
            }

            for (int i = 0; i < obj.spell_items.Count; i++)
            {
                if (spell_ids.ContainsKey(obj.spell_items[i].itemName))
                {
                    Debug.Log(obj.spell_items[i].itemName + " item is a duplicate");
                }
                else
                {
                    spell_ids.Add(obj.spell_items[i].itemName, i);
                }
            }
        }

        void LoadWeaponIds()
        {
            WeaponScriptableObject obj = Resources.Load("SHQZ.WeaponScriptableObject") as WeaponScriptableObject;

            if (obj == null)
            {
                Debug.Log("SHQZ.WeaponScriptableObject couldnt be loaded!");
                return;
            }

            for (int i = 0; i < obj.weapons_all.Count; i++)
            {
                if (weapon_ids.ContainsKey(obj.weapons_all[i].itemName))
                {
                    Debug.Log(obj.weapons_all[i].itemName + " item is a duplicate");
                }
                else
                {
                    weapon_ids.Add(obj.weapons_all[i].itemName, i);
                }
            }
        }

        int GetWeaponIdFromString(string id)
        {
            int index = -1;
            if (weapon_ids.TryGetValue(id, out index))
            {
                return index;
            }

            return -1;
        }

        public Weapon GetWeapon(string id)
        {
            WeaponScriptableObject obj = Resources.Load("SHQZ.WeaponScriptableObject") as WeaponScriptableObject;

            if (obj == null)
            {
                Debug.Log("SHQZ.WeaponScriptableObject cant be loaded!");
                return null;
            }

            int index = GetWeaponIdFromString(id);

            if (index == -1)
                return null;

            return obj.weapons_all[index];
        }

        int GetSpellIdFromString(string id)
        {
            int index = -1;
            if (spell_ids.TryGetValue(id, out index))
            {
                return index;
            }

            return index;
        }

        public Spell GetSpell(string id)
        {
            SpellItemScriptableObject obj = Resources.Load("SHQZ.SpellItemScriptableObject") as SpellItemScriptableObject;
            if (obj == null)
            {
                Debug.Log("SHQZ.SpellItemScriptableObject cant be loaded!");
                return null;
            }

            int index = GetSpellIdFromString(id);
            if (index == -1)
                return null;

            return obj.spell_items[index];
        }
    }
}

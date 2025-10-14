using UnityEngine;

public enum ItemCategory { Tool, Weapon, Resource, Consumable }
public enum ToolType { None, Axe, Pickaxe, Hoe, WateringCan }
public enum WeaponType { None, Sword }

[CreateAssetMenu(menuName="Items/Item")]
public class ItemSO : ScriptableObject
{
    public string id;
    public Sprite icon;
    public ItemCategory category;
    public ToolType toolType;
    public WeaponType weaponType;
    public float range = 0.9f; // tầm với phía trước
    public int power = 1;      // sát thương/sức mạnh
}

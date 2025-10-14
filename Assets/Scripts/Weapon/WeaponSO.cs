// WeaponSO.cs
using UnityEngine;

public enum WeaponKind { Sword, Bow }

[CreateAssetMenu(menuName="Items/Weapon")]
public class WeaponSO : ScriptableObject {
    public WeaponKind kind = WeaponKind.Sword;
    public int damage = 1;
    public float range = 1.0f;        // Sword: tâm chém; Bow: khoảng spawn đầu mũi tên
    public float hitRadius = 0.35f;   // chỉ dùng cho Sword
    public float cooldown = 0.25f;
    public GameObject projectilePrefab;  // dùng cho Bow
    public float projectileSpeed = 8f;   // dùng cho Bow
}

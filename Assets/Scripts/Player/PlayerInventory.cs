using UnityEngine;

[System.Serializable]
public struct ItemStack { public ItemSO item; public int count; }

public class PlayerInventory : MonoBehaviour
{
    public ItemStack[] hotbar = new ItemStack[8];
    public int selected = 0;

    public ItemSO CurrentItem => 
        (selected >= 0 && selected < hotbar.Length) ? hotbar[selected].item : null;

    public void SelectSlot(int i) { selected = Mathf.Clamp(i, 0, hotbar.Length - 1); }
}

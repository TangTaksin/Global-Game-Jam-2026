using System.Runtime.CompilerServices;
using UnityEngine;

[CreateAssetMenu(fileName = "MaskData", menuName = "ScriptableObjects/MaskData")]
public class MaskData : ScriptableObject
{
    public Sprite inventory_sprite;
    public Sprite[] player_sprite;
    public Sprite world_sprite;
}

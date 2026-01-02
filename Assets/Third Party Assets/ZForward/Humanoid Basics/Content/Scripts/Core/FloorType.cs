using UnityEngine;

namespace Humanoid_Basics.Core
{
    public class FloorType: MonoBehaviour
    {
        public enum Types {Default, Dirt, Grass, Wood, Stone, Metal, Snow, Water};
        
        public Types type;
    }
}

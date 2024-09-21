namespace CountryGame
{
    using UnityEngine;

    public class TroopMover : MonoBehaviour
    {
        public static TroopMover Instance;
        
        private void Awake()
        {
            Instance = this;
        }
    }
}
using Steamworks;

namespace CountryGame.Multiplayer
{
    using UnityEngine;

    public class GameLobbyManager : MonoBehaviour
    {
        public static GameLobbyManager Instance;
        
        private void Awake()
        {
            Instance = this;
            
        }
    }
}
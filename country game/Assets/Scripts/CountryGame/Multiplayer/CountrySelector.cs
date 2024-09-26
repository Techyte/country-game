using Steamworks;

namespace CountryGame.Multiplayer
{
    using System.Collections.Generic;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    public class CountrySelector : MonoBehaviour
    {
        public static CountrySelector Instance;
        
        [SerializeField] private Transform titleCard;
        [SerializeField] private float titleSpeed;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private Image flagImage;
        [SerializeField] private Transform titleStartPos, titleEndPos;
        [SerializeField] private Transform countriesParent;
        [SerializeField] private TextMeshProUGUI countryPrefab;
        [SerializeField] private Button selectButton;

        private bool _countrySelected;

        public Nation currentNation;

        private void Awake()
        {
            Instance = this;
            titleCard.position = titleStartPos.position;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ResetSelected();
            }
            
            if (_countrySelected)
            {
                titleCard.position = Vector3.Lerp(titleCard.position, titleEndPos.position, titleSpeed * Time.deltaTime);
            }
            else
            {
                titleCard.position = Vector3.Lerp(titleCard.position, titleStartPos.position, titleSpeed * Time.deltaTime);
            }
        }

        public void SomeoneTookCurrentNation()
        {
            selectButton.interactable = false;
        }

        private List<TextMeshProUGUI> currentCountries = new List<TextMeshProUGUI>();
        public void Clicked(Nation nationSelected)
        {
            currentNation = nationSelected;
            
            _countrySelected = true;
            titleCard.position = titleStartPos.position;
            titleText.text = nationSelected.Name;

            flagImage.sprite = nationSelected.flag;

            bool currentNationAvailble = true;

            for (int i = 0; i < SteamMatchmaking.GetNumLobbyMembers(LobbyManager.Instance.lobbyId); i++)
            {
                string memberNation = SteamMatchmaking.GetLobbyMemberData(LobbyManager.Instance.lobbyId,
                    SteamMatchmaking.GetLobbyMemberByIndex(LobbyManager.Instance.lobbyId, i), "nation");

                Debug.Log(memberNation);
                
                if (memberNation == nationSelected.Name)
                {
                    currentNationAvailble = false;
                }
            }
            
            selectButton.interactable = currentNationAvailble;

            foreach (var country in currentCountries)
            {
                Destroy(country.gameObject);
            }
            
            currentCountries.Clear();

            foreach (var country in nationSelected.Countries)
            {
                TextMeshProUGUI countryName = Instantiate(countryPrefab, countriesParent);

                countryName.text = country.countryName;
                
                currentCountries.Add(countryName);
            }
        }

        public void ResetSelected()
        {
            currentNation = null;
            _countrySelected = false;
        }

        public void SelectNation()
        {
            SteamMatchmaking.SetLobbyMemberData(LobbyManager.Instance.lobbyId, "nation",
                currentNation.Name);
            selectButton.interactable = false;
        }
    }
}
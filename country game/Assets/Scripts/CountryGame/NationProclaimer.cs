using System.Collections.Generic;
using System.Linq;
using Riptide;
using TMPro;
using UnityEngine.UI;

namespace CountryGame
{
    using UnityEngine;

    public class NationProclaimer : MonoBehaviour
    {
        public static NationProclaimer Instance;

        [SerializeField] private GameObject proclaimScreen;
        [SerializeField] private Transform lineHolder;
        [SerializeField] private GameObject linePrefab;
        [SerializeField] private TMP_InputField nameField;
        [SerializeField] private TMP_InputField searchField;
        [SerializeField] private Image flagDisplay;
        [SerializeField] private FlexibleColorPicker picker;
        [SerializeField] private Image colourImage;

        List<Sprite> flags = new List<Sprite>();
        List<Sprite> newFlags = new List<Sprite>();

        private void Awake()
        {
            Instance = this;
            flags = Resources.LoadAll<Sprite>("Flags").ToList();


            // create all the lines
            for (int i = 0; i < flags.Count; i++)
            {
                int lineId = Mathf.FloorToInt(i / 3f);

                if (lines.Count == lineId)
                {
                    // we are the first in the line
                    GameObject line = Instantiate(linePrefab, lineHolder);
                    lines.Add(line);
                }
            }

            for (int i = 0; i < flags.Count; i++)
            {
                Sprite flag = flags[i];

                int lineId = Mathf.FloorToInt(i / 3f);

                var index = i % 3;

                GameObject ourLine = lines[lineId];
                Button button = ourLine.GetComponentsInChildren<Button>()[index];
                button.GetComponent<Image>().sprite = flag;
                button.GetComponent<Image>().color = Color.white;

                int flagId = i;

                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => { ClickedFlag(flagId); });
            }

            newFlags = flags;
        }

        private void Start()
        {
            proclaimScreen.SetActive(false);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ResetSelected();
            }

            colourImage.color = picker.color;
        }

        public void ToggleColourScreen()
        {
            picker.gameObject.SetActive(!picker.gameObject.activeSelf);
        }

        private List<GameObject> lines = new List<GameObject>();

        public void ResetSelected()
        {
            proclaimScreen.SetActive(false);
        }

        private void DisplayScreen()
        {
            nameField.text = PlayerNationManager.PlayerNation.Name;
            flagDisplay.sprite = PlayerNationManager.PlayerNation.flag;
            picker.color = PlayerNationManager.PlayerNation.Color;
        }

        public void ClickedFlag(int index)
        {
            flagDisplay.sprite = newFlags[index];
        }

        public void OpenProclaimScreen()
        {
            proclaimScreen.SetActive(true);
            DisplayScreen();
        }

        public void CloseProclaimScreen()
        {
            ResetSelected();
        }

        public void ProclaimNewNation()
        {
            if (NationManager.Instance.GetNationByName(nameField.text) != null ||
                NationManager.Instance.GetNationByFlag(flagDisplay.sprite) != null)
            {
                return;
            }
            
            Message message = Message.Create(MessageSendMode.Reliable, GameMessageId.ProclaimNewNation);
            message.Add(PlayerNationManager.PlayerNation.Name);
            message.Add(nameField.text);
            message.AddString(flagDisplay.sprite.name);
            message.AddFloat(picker.color.r);
            message.AddFloat(picker.color.g);
            message.AddFloat(picker.color.b);
            message.AddFloat(picker.color.a);

            NetworkManager.Instance.Client.Send(message);

            ResetSelected();
        }

        public void SearchUpdated(string search)
        {
            UpdateSorting(searchField.text);
        }

        private void UpdateSorting(string search)
        {
            newFlags = new List<Sprite>();

            for (int i = 0; i < flags.Count; i++)
            {
                if (flags[i].name.ToLower().Replace("_", " ").Contains(search.ToLower()))
                {
                    newFlags.Add(flags[i]);
                }
            }
            
            for (int i = 0; i < flags.Count; i++)
            {
                int lineId = Mathf.FloorToInt(i / 3f);

                var index = i % 3;

                GameObject ourLine = lines[lineId];
                Button button = ourLine.GetComponentsInChildren<Button>()[index];
                button.GetComponent<Image>().sprite = null;
                button.GetComponent<Image>().color = new Color(0, 0, 0, 0);

                button.onClick.RemoveAllListeners();
            }
            
            for (int i = 0; i < newFlags.Count; i++)
            {
                Sprite flag = newFlags[i];

                int lineId = Mathf.FloorToInt(i / 3f);

                var index = i % 3;

                GameObject ourLine = lines[lineId];
                Button button = ourLine.GetComponentsInChildren<Button>()[index];
                button.GetComponent<Image>().sprite = flag;
                button.GetComponent<Image>().color = Color.white;

                int flagId = i;

                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => { ClickedFlag(flagId); });
            }
        }
}
}
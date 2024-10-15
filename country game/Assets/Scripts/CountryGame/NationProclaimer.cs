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
        [SerializeField] private Image flagDisplay;
        [SerializeField] private FlexibleColorPicker picker;
        [SerializeField] private Image colourImage;
        
        List<Sprite> flags = new List<Sprite>();

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

                if (lines.Count == lineId)
                {
                    // we are the first in the line
                    GameObject line = Instantiate(linePrefab, lineHolder);
                    lines.Add(line);
                }

                var index = i % 3;

                if (i==1)
                {
                    index = 2;
                }

                GameObject ourLine = lines[lineId];
                Button button = ourLine.GetComponentsInChildren<Button>()[index];
                button.GetComponent<Image>().sprite = flag;

                int flagId = i;
                
                button.onClick.AddListener(() =>
                {
                    ClickedFlag(flagId);
                });
            }
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
            Debug.Log($"Clicked on {flags[index].name}");
            
            flagDisplay.sprite = flags[index];
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
    }
}
using System;
using System.Collections;
using System.Collections.Generic;

namespace CountryGame
{
    using UnityEngine;

    public class ComputerAgreementCreator : MonoBehaviour
    {
        public static ComputerAgreementCreator Instance;

        private void Awake()
        {
            Instance = this;
        }

        // the time it will take ai controller nations to decide weather to accept your agreement
        [SerializeField] private float decisionTime = 1f;
        [SerializeField] private Notification notificationPrefab;
        [SerializeField] private Transform notificationParent;

        public void PlayerAskedToJoinAgreement(Nation targetNation, Agreement requestedAgreement, bool preexisting)
        {
            StartCoroutine(CompleteAgreement(targetNation, requestedAgreement, preexisting));
        }

        private IEnumerator CompleteAgreement(Nation targetNation, Agreement requestedAgreement, bool preexisting)
        {
            // TODO: check if the player is currently fighting the nation, if so, immediately reject their agreement

            float requiredPower = 30;

            if (requestedAgreement.nonAgression)
            {
                requiredPower = 20;
            }

            if (requestedAgreement.militaryAccess)
            {
                requiredPower += 10;
            }

            if (requestedAgreement.autoJoinWar)
            {
                requiredPower += 20;
            }

            switch (requestedAgreement.influence)
            {
                case 1: // slightly influenced
                    requiredPower += 5;
                    break;
                case 2: // influenced
                    requiredPower += 10;
                    break;
                case 3: // completly influenced
                    requiredPower += 30;
                    break;
            }

            if (PlayerNationManager.PlayerNation.Border(targetNation))
            {
                requiredPower -= 15;
            }
            
            float distance = PlayerNationManager.PlayerNation.DistanceTo(targetNation);

            switch (distance)
            {
                case < 1.5f:
                    // close
                    requiredPower *= 0.75f;
                    break;
                case < 2.5f:
                    requiredPower *= 1f;
                    break;
                case >= 2.5f:
                    requiredPower *= 2f;
                    break;
            }

            List<Agreement> existingSharedAgreements = new List<Agreement>();

            foreach (var agreement in targetNation.agreements)
            {
                if (PlayerNationManager.PlayerNation.agreements.Contains(agreement))
                {
                    existingSharedAgreements.Add(agreement);
                }
            }

            for (int i = 0; i < existingSharedAgreements.Count; i++)
            {
                requiredPower *= 0.8f;
            }

            int highestInfluence = 0;

            foreach (var agreement in existingSharedAgreements)
            {
                if (agreement.influence > highestInfluence)
                {
                    highestInfluence = agreement.influence;
                }
            }

            if (highestInfluence > 0)
            {
                requiredPower *= highestInfluence / 3f;
            }
            
            yield return new WaitForSeconds(decisionTime);

            Debug.Log($"Cost: {requiredPower}");
            if (requiredPower <= PlayerNationManager.Instance.diplomaticPower)
            {
                NationManager.Instance.NewAgreement(requestedAgreement);
                
                // agree
                if (!preexisting)
                {
                    NationManager.Instance.NationJoinAgreement(PlayerNationManager.PlayerNation, requestedAgreement);
                }
                NationManager.Instance.NationJoinAgreement(targetNation, requestedAgreement);
                PlayerNationManager.Instance.diplomaticPower -= 5;
                
                Debug.Log($"{targetNation.Name} accepted the {requestedAgreement.Name} agreement");

                Notification notification = Instantiate(notificationPrefab, notificationParent);
                notification.Init($"{targetNation.Name} Agrees!", $"Today, {targetNation.Name} agreed to {PlayerNationManager.PlayerNation.Name} request and signed on to the {requestedAgreement.Name} agreement", () => {CountrySelector.Instance.OpenAgreementScreen(requestedAgreement);}, 5);
            }
            else
            {
                // disagree
                PlayerNationManager.Instance.diplomaticPower -= 10;
                
                Debug.Log($"{targetNation.Name} rejected the {requestedAgreement.Name} agreement");
                
                Notification notification = Instantiate(notificationPrefab, notificationParent);
                notification.Init($"{targetNation.Name} Rejects!", $"Today, {targetNation.Name} rejected {PlayerNationManager.PlayerNation.Name} {requestedAgreement.Name} agreement, deciding to forge its own path", () => {CountrySelector.Instance.Clicked(targetNation);}, 5);
            }
        }
    }
}
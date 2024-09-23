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
        public int startingAgreementPowerRequirement = 5;

        public void PlayerAskedToJoinAgreement(Nation targetNation, Agreement requestedAgreement, bool preexisting)
        {
            StartCoroutine(CompleteAgreement(targetNation, requestedAgreement, preexisting));
        }

        private IEnumerator CompleteAgreement(Nation targetNation, Agreement requestedAgreement, bool preexisting)
        {
            // TODO: check if the player is currently fighting the nation, if so, immediately reject their agreement

            float requiredPower = GetAgreementCost(requestedAgreement, targetNation);
            
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
                PlayerNationManager.Instance.diplomaticPower -= (int)(requiredPower / 2);
                
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

        public static float GetAgreementCost(Agreement agreement, Nation nationSigning)
        {
            float requiredPower = Instance.startingAgreementPowerRequirement;

            if (agreement.nonAgression)
            {
                requiredPower += 10;
            }

            if (agreement.militaryAccess)
            {
                requiredPower += 10;
            }

            if (agreement.autoJoinWar)
            {
                requiredPower += 20;
            }

            switch (agreement.influence)
            {
                case 1: // slightly influenced
                    requiredPower += 5;
                    break;
                case 2: // influenced
                    requiredPower += 10;
                    break;
                case 3: // completly influenced
                    requiredPower += 20;
                    break;
            }

            if (agreement.AgreementLeader.Border(nationSigning))
            {
                requiredPower -= 15;
            }
            
            float distance = agreement.AgreementLeader.DistanceTo(nationSigning);

            switch (distance)
            {
                case < 2f:
                    // close
                    Debug.Log("Close");
                    requiredPower *= 0.75f;
                    break;
                case < 3.3f:
                    Debug.Log("Medium");
                    requiredPower *= 1f;
                    break;
                case >= 2.5f:
                    Debug.Log("Far");
                    requiredPower *= 2f;
                    break;
            }

            List<Agreement> existingSharedAgreements = new List<Agreement>();

            foreach (var agr in nationSigning.agreements)
            {
                if (agreement.AgreementLeader.agreements.Contains(agr))
                {
                    existingSharedAgreements.Add(agr);
                }
            }

            for (int i = 0; i < existingSharedAgreements.Count; i++)
            {
                requiredPower *= 0.8f;
            }

            int highestInfluence = 0;

            foreach (var agr in existingSharedAgreements)
            {
                if (agr.influence > highestInfluence)
                {
                    highestInfluence = agr.influence;
                }
            }

            if (highestInfluence > 0)
            {
                requiredPower *= highestInfluence / 3f;
            }
            
            requiredPower = Mathf.Max(requiredPower, 10);

            return requiredPower;
        }
    }
}
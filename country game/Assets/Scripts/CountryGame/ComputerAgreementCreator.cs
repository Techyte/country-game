using System;
using System.Collections;
using System.Collections.Generic;
using Riptide;

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
        public int startingAgreementPowerRequirement = 5;

        public void PlayerAskedToJoinAgreement(Nation requestingNation, Nation targetNation, Agreement requestedAgreement, bool preexisting)
        {
            if (NetworkManager.Instance.Host)
            {
                StartCoroutine(CompleteAgreement(requestingNation, targetNation, requestedAgreement, preexisting));
            }
            else
            {
                Message message = Message.Create(MessageSendMode.Reliable, GameMessageId.RequestNewAgreement);
                message.AddString(requestingNation.Name);
                message.AddString(targetNation.Name);
                message.AddAgreement(requestedAgreement);
                message.AddBool(preexisting);

                NetworkManager.Instance.Client.Send(message);
            }
        }

        private IEnumerator CompleteAgreement(Nation requestingAgreement, Nation targetNation, Agreement requestedAgreement, bool preexisting)
        {
            // TODO: check if the player is currently fighting the nation, if so, immediately reject their agreement

            float requiredPower = GetAgreementCost(requestedAgreement, targetNation);
            
            yield return new WaitForSeconds(decisionTime);

            Debug.Log($"Cost: {requiredPower}");
            if (requiredPower <= requestingAgreement.DiplomaticPower)
            {
                Message acceptedMessage = Message.Create(MessageSendMode.Reliable, GameMessageId.AgreementSigned);
                acceptedMessage.AddString(requestingAgreement.Name);
                acceptedMessage.AddString(targetNation.Name);
                acceptedMessage.AddAgreement(requestedAgreement);
                acceptedMessage.AddBool(preexisting);
                acceptedMessage.AddFloat(requiredPower);
                
                NetworkManager.Instance.Server.SendToAll(acceptedMessage);
            }
            else
            {
                Message rejectedMessage = Message.Create(MessageSendMode.Reliable, GameMessageId.AgreementRejected);
                rejectedMessage.AddString(requestingAgreement.Name);
                rejectedMessage.AddString(targetNation.Name);
                rejectedMessage.AddAgreement(requestedAgreement);
                rejectedMessage.AddFloat(requiredPower);
                
                NetworkManager.Instance.Server.SendToAll(rejectedMessage);
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
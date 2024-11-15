using System.Collections.Generic;

namespace CountryGame
{
    using UnityEngine;

    public class Agreement
    {
        public string Name;
        public Nation AgreementLeader;
        public List<Nation> Nations = new List<Nation>();
        public int NationCount => Nations.Count;
        public Color Color;

        public bool nonAgression;
        public bool militaryAccess;
        public bool autoJoinWar;
        public int influence;

        public int turnCreated = 0;

        public void NationJointed(Nation nationThatJoined)
        {
            Nations.Add(nationThatJoined);

            foreach (var nation in Nations)
            {
                nation.UpdateInfluenceColour();
                nation.UpdateTroopDisplays();
            }
        }

        public int Age()
        {
            return TurnManager.Instance.currentTurn - turnCreated;
        }

        public void NationLeft(Nation nationThatLeft)
        {
            if (Nations.Contains(nationThatLeft))
            {
                Nations.Remove(nationThatLeft);
            }
        }

        public void SetAgreementLeader(Nation newAgreementLeader)
        {
            AgreementLeader = newAgreementLeader;

            if (Nations.Contains(newAgreementLeader))
            {
                Nations.Remove(newAgreementLeader);
                Nations.Insert(0, newAgreementLeader);
            }
            else
            {
                Nations.Insert(0, newAgreementLeader);
            }
        }
    }
}
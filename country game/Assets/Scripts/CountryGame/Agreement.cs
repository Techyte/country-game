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

        public bool nonAgression;
        public bool militaryAccess;
        public bool autoJoinWar;
        public int influence;

        public void NationJointed(Nation nationThatJoined)
        {
            Nations.Add(nationThatJoined);
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
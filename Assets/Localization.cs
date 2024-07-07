using UnityEngine;
using System.Collections;
using System;

public class Localization : MonoBehaviour
{

    [Serializable]
    public class LineSet
    {
        [SerializeField]
        private string readyUp;

        [SerializeField]
        private string waitingForOtherPlayers;

        [SerializeField]
        private string plugIn;

        [SerializeField]
        private string target;

        [SerializeField]
        private string missileWarning;

        [SerializeField]
        private string evasion;

        [SerializeField]
        private string acquiring;

        [SerializeField]
        private string locked;

        [SerializeField]
        private string byeBye;

        [SerializeField]
        private string closeOne;

        [SerializeField]
        private string wellPlayed;

        [SerializeField]
        private string goodGame;

        public string CloseOne { get { return closeOne; } }

        public string WellPlayed { get { return wellPlayed; } }

        public string GoodGame { get { return goodGame; } }

        public string Acquiring { get { return acquiring; } }

        public string Locked { get { return locked; } }

        public string ByeBye
        {
            get
            {
                return byeBye;
            }
        }

        public string MissileWarning
        {
            get
            {
                return missileWarning;
            }
        }

        public string Evasion
        {
            get
            {
                return evasion;
            }
        }

        public string Target
        {
            get
            {
                return target;
            }
        }

        public string PlugIn
        {
            get
            {
                return plugIn;
            }
        }

        public string WaitingForOtherPlayers
        {
            get
            {
                return waitingForOtherPlayers;
            }
        }

        public string ReadyUp
        {
            get
            {
                return readyUp;
            }
        }
    }


    [SerializeField]
    private LineSet[] languages;

    public LineSet Lang
    {
        get
        {
            return languages[LangIndex];
        }
    }

    private int LangIndex
    {
        get
        {
            if (Application.systemLanguage == SystemLanguage.French || Game.i.FrogForced)
            {
                return 0;
            }
            else
            {
                return 1;
            }
        }
    }
}

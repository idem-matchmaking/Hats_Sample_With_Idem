using Hats.Game;
using TMPro;
using UnityEngine;

namespace Hats.Game.UI
{
    public class TurnCounterBehaviour : GameEventHandler
    {
        [Header("UI References")]
        public TextMeshProUGUI TurnText;

        // Start is called before the first frame update
        void Start()
        {
            FindGameProcessor();
        }

        // Update is called once per frame
        void Update()
        {
            TurnText.text = GameProcessor.EventProcessor?.CurrentTurn.ToString();
        }
    }
}
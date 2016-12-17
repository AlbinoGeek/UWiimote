using System.Collections.Generic;
using UnityEngine;

namespace Mewzor
{
    public class GameManager : MonoBehaviour
    {
        public enum GameState
        {
            Waiting,
            Playing,
            Ending
        }

        public GameState State;

        public float countdown = -1;

        private List<Tank> players;

        private GUIStyle style;

        #region Unity
        private void Awake()
        {
            this.style = new GUIStyle();
            this.style.alignment = TextAnchor.MiddleCenter;

            this.players = new List<Tank>();
            this.CheckForPlayer(1);
            //this.CheckForPlayer(2);
            //this.CheckForPlayer(3);
            //this.CheckForPlayer(4);
        }

        private void OnGUI()
        {
            // If we're pre-game
            if (this.State == GameState.Waiting)
            {
                // Skip if no countdown show
                if (this.countdown == -1)
                {
                    return;
                }

                // and our countdown is done
                if (this.countdown < 0f)
                {
                    this.State = GameState.Playing;
                    this.countdown = -1;
                }
                // or we're counting down to zero
                else if (this.countdown < 1f)
                {
                    this.style.fontSize = 200;
                    this.style.fontStyle = FontStyle.Bold;
                    this.style.normal.textColor = Color.green + Color.yellow;

                    GUI.Label(new Rect(Screen.width / 2f, Screen.height / 2f, 0, 0), "GO", style);
                }
                // or we're counting down
                else
                {
                    this.style.fontSize = 180 / (int)this.countdown;
                    this.style.fontStyle = FontStyle.BoldAndItalic;
                    this.style.normal.textColor = Color.yellow + Color.red;
                    
                    GUI.Label(new Rect(Screen.width / 2f, Screen.height / 2f, 0, 0), (int)this.countdown + "", style);
                }
            }
        }

        private void Update()
        {
            // If we're pre-game
            if (this.State == GameState.Waiting)
            {
                // If all players are ready , start countdown
                for (int i = 0; i < this.players.Count; i++)
                {
                    if (!this.players[i].PlayerReady)
                    {
                        this.countdown = -1;
                        return;
                    }
                }

                if (this.countdown == -1)
                {
                    this.countdown = 4;
                }
                else
                {
                    this.countdown -= 1f * Time.deltaTime;
                }
                return;
            }
        }
        #endregion

        private void CheckForPlayer(int id)
        {
            GameObject go = GameObject.Find("Player " + id);
            if (go)
            {
                this.players.Add(go.GetComponent<Tank>());
            }
        }
    }
}

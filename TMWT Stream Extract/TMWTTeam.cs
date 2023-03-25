using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMWT_Stream_Extract
{
    public class TMWTTeam
    {
        public TMWTTeam(string teamName, string player1, string player2)
        {
            TeamName = teamName;
            Players = new List<string>() { player1, player2 };
        }
        public bool TeamContainsPlayers(List<string> players)
        {
            if (string.IsNullOrEmpty(players[0]) || string.IsNullOrEmpty(players[1]))
            {
                return false;
            }
            return Players.Contains(players[0]) && Players.Contains(players[1]);
        }
        public bool TeamContains2OfPlayers(List<string> players)
        {
            //players will have 4 elements
            //only 2 need to match
            var matchCount = 0;
            foreach(var player in players)
            {
                if (string.IsNullOrEmpty(player))
                {
                    continue;
                }
                if (Players.Contains(player))
                {
                    matchCount++;
                }
            }
            if(matchCount == 2)
            {
                return true;
            }
            return false;
        }
        public string TeamName { get; set; }
        public List<string> Players { get; set; }
    }
}

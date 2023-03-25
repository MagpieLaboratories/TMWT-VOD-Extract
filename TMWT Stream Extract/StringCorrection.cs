using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMWT_Stream_Extract
{
    public class StringCorrection
    {
        public static string AdjustForDigitsOnly(string input)
        {
            //Replace O with 0, replace i, I, l, L, |, etc with 1
            input = input.Replace("()", "0");
            input = input.Replace('o', '0');
            input = input.Replace('O', '0');
            input = input.Replace('q', '0');
            input = input.Replace('Q', '0');
            input = input.Replace('i', '1');
            input = input.Replace('I', '1');
            input = input.Replace('l', '1');
            input = input.Replace('L', '1');
            input = input.Replace('|', '1');
            input = input.Replace('!', '1');
            input = input.Replace('s', '5');
            input = input.Replace('S', '5');
            input = input.Replace('z', '2');
            input = input.Replace('Z', '2');
            input = input.Replace('b', '8');
            input = input.Replace('B', '8');
            input = input.Replace('g', '9');
            input = input.Replace('G', '9');
            input = input.Replace('a', '4');
            input = input.Replace('A', '4');
            input = input.Replace('t', '7');
            input = input.Replace('T', '7');

            //remove all non-numbers from string and return this value
            return new string(input.Where(c => char.IsDigit(c)).ToArray());
        }

        public static string AdjustCPTime(string input, bool isFirst = false)
        {
            
            input = input.Replace("()", "0");
            input = input.Replace('o', '0');
            input = input.Replace('O', '0');
            input = input.Replace('q', '0');
            input = input.Replace('Q', '0');
            input = input.Replace('i', '1');
            input = input.Replace('I', '1');
            input = input.Replace('l', '1');
            input = input.Replace('L', '1');
            input = input.Replace('|', '1');
            input = input.Replace('!', '1');
            input = input.Replace('s', '5');
            input = input.Replace('S', '5');
            input = input.Replace('z', '2');
            input = input.Replace('Z', '2');
            input = input.Replace('b', '8');
            input = input.Replace('B', '8');
            input = input.Replace('g', '9');
            input = input.Replace('G', '9');
            input = input.Replace('a', '4');
            input = input.Replace('A', '4');
            input = input.Replace('t', '7');
            input = input.Replace('T', '7');

            //remove all characters from the string which are not: 0-9 or . or :

            if(input.Length < 3)
            {
                return string.Empty;
            }

            if (isFirst)
            {
                input = new string(input.Where(c => char.IsDigit(c)).ToArray());

                //must contain a : and a .
                if (!input.Contains(":"))
                {
                    //add a : at the 3rd position
                    input=input.Insert(2, ":");
                }
                if (!input.Contains("."))
                {
                    //insert into the 3rd to last position
                    input=input.Insert(5, ".");
                }
            }
            else
            {
                //if the first input isn't a "+", delete it
                if(input[0] != '+')
                {
                    input = input.Substring(1);
                }
                input = new string(input.Where(c => char.IsDigit(c)).ToArray());
                if(input.Length == 5)
                {
                    input = input.Insert(2, ".");
                }
                else if(input.Length == 4)
                {
                    input = input.Insert(1, ".");
                }
            }

            return input;
        }

        public static string ClosestStringMatch(string input, List<string> Options, int allowedError)
        {
            //synonyms or partial reads
            if (input.ToLower().Contains("bs+") || input.Contains("competition") || input == "BS" || input == "BS+")
            {
                return "BS+ Competition";
            }
            if(input.ToLower().Contains("orks") || input.ToLower().Contains("numel"))
            {
                return "Orks GP Numelops";
            }
            if(input.ToLower().Contains("homyno") || input.ToLower().Contains("tsun"))
            {
                return "Homyno Tsun";
            }
            if (input.ToLower().Contains("schweineaim") || input.ToLower().Contains("racing"))
            {
                return "Schweineaim Racing";
            }
            if (input.ToLower().Contains("alternate") || input.ToLower().Contains("attax"))
            {
                return "Alternate Attax";
            }
            if (input.ToLower().Contains("BDS") || input.ToLower().Contains("Team"))
            {
                return "Team BDS";
            }
            if (input.ToLower().Contains("Gamers") || input.ToLower().Contains("First"))
            {
                return "Gamers First";
            }
            if (input.ToLower().Contains("Into") || input.ToLower().Contains("Breach"))
            {
                return "Into the Breach";
            }

            //find the closest match
            var closestMatch = Options
                .Select(x => new { TeamName = x, Distance = LevenshteinDistance.Compute(input, x, true) })
                .OrderBy(x => x.Distance)
                .First();


            if (closestMatch != null && closestMatch.Distance < allowedError+1)
            {
                return closestMatch.TeamName;
            }
            return input;
        }

        public static List<string> GetPossibleMaps()
        {
            return new List<string>()
            {
                "Aeropipes", "Reps", "Agility Dash", "Flip Of Faith", "Back N Forth",
                "Freestyle", "Gyroscope", "Parkour", "Slippyslides", "Slowdown"
            };
        }
        
        public static string ClosestMapName(string inputString)
        {
            if (string.IsNullOrEmpty(inputString))
            {
                return "";
            }

            var possibleMaps = GetPossibleMaps();

            return ClosestStringMatch(inputString, possibleMaps, 7);
        }

        public static List<string> GetPossibleTeamNames(bool? GrandLeague)
        {
            if(GrandLeague == true)
            {
                return new List<string>()
                {
                    "Into the Breach", "Team BDS", "Solary", "Alliance", "BIG", "Gamers First", "Karmine Corp", "Sinners"
                };
                
            }
            if(GrandLeague == false)
            {
                return new List<string>()
                {
                    "Exalty", "BS+ Competition", "Izi Dream", "Orks GP Numelops", "Schweineaim Racing", "Sprout", "Alternate Attax", "Homyno Tsun"
                };
            }
            return new List<string>()
            {
                "Into the Breach", "Team BDS", "Solary", "Alliance", "BIG", "Gamers First", "Karmine Corp", "Sinners",
                "Exalty", "BS+ Competition", "Izi Dream", "Orks GP Numelops", "Schweineaim Racing", "Sprout", "Alternate Attax", "Homyno Tsun"
            };
        }
        
        public static string ClosestTeamName(string inputString, bool? grandLeague)
        {
            if (string.IsNullOrEmpty(inputString))
            {
                return "";
            }

            var possibleTeamNames = GetPossibleTeamNames(grandLeague);

            return ClosestStringMatch(inputString, possibleTeamNames, 8);
        }

        public static List<string> GetPossiblePlayerNames(bool? GrandLeague)
        {
            if (GrandLeague == true)
            {

                return new List<string>()
                {
                "Mudda", "Soulja", "Affi", "Aurel", "Massa", "Granady", "Gwen", "Binks", "Mime", "eLconn", 
                    "Bren", "Otaaaq", "Kappa", "tween",
                "CarlJr", "Pac"
                };
            }
            if (GrandLeague == false)
            {

                return new List<string>()
                {
                "Wosile", "Skandear", "Snow", "Glast", "Link", "MiQuatro", "Cocow", "Worker", 
                    "Complex", "Panda", "Ratchet", "Barbos", "Scrapie",
                "Dexter", "Feed", "ener"
                };
            }

            return new List<string>()
            {
                "Wosile", "Skandear", "Snow", "Glast", "Link", "MiQuatro", "Cocow", "Worker", "Complex", "Panda", "Ratchet", "Barbos", "Scrapie",
                "Dexter", "Feed", "ener",
                "Mudda", "Soulja", "Affi", "Aurel", "Massa", "Granady", "Gwen", "Binks", "Mime", "eLconn", "Bren", "Otaaaq", "Kappa", "tween",
                "CarlJr", "Pac"
            };
        }

        public static string ClosestPlayerName(string inputString, bool? GrandLeague = null)
        {
            if (string.IsNullOrEmpty(inputString))
            {
                return "";
            }

            var possiblePlayerNames = GetPossiblePlayerNames(GrandLeague);

            return ClosestStringMatch(inputString, possiblePlayerNames, 4);
        }

        public static List<TMWTTeam> GetTMWTTeams(bool? GrandLeague)
        {
            
            if(GrandLeague == true)
            {
                var teamListGL = new List<TMWTTeam>();
                teamListGL.Add(new TMWTTeam("Into the Breach", "Mime", "eLconn"));
                teamListGL.Add(new TMWTTeam("Team BDS", "Affi", "Aurel"));
                teamListGL.Add(new TMWTTeam("Solary", "CarlJr", "Pac"));
                teamListGL.Add(new TMWTTeam("Alliance", "Soulja", "Mudda"));
                teamListGL.Add(new TMWTTeam("BIG", "Massa", "Granady"));
                teamListGL.Add(new TMWTTeam("Gamers First", "Gwen", "Binks"));
                teamListGL.Add(new TMWTTeam("Karmine Corp", "Bren", "Otaaaq"));
                teamListGL.Add(new TMWTTeam("Sinners", "Kappa", "tween"));
                
                return teamListGL;

            }
            if(GrandLeague == false)
            {

                var teamListCL = new List<TMWTTeam>();
                teamListCL.Add(new TMWTTeam("Exalty", "Link", "MiQuatro"));
                teamListCL.Add(new TMWTTeam("BS+ Competition", "Snow", "Glast"));
                teamListCL.Add(new TMWTTeam("Izi Dream", "Cocow", "Worker"));
                teamListCL.Add(new TMWTTeam("Orks GP Numelops", "Complex", "Panda"));
                teamListCL.Add(new TMWTTeam("Schweineaim Racing", "Ratchet", "Barbos"));
                teamListCL.Add(new TMWTTeam("Sprout", "Scrapie", "Dexter"));
                teamListCL.Add(new TMWTTeam("Homyno Tsun", "Feed", "ener"));
                teamListCL.Add(new TMWTTeam("Alternate Attax", "Wosile", "Skandear"));
                return teamListCL;
            }

            var teamList = new List<TMWTTeam>();
            teamList.Add(new TMWTTeam("Into the Breach", "Mime", "eLconn"));
            teamList.Add(new TMWTTeam("Team BDS", "Affi", "Aurel"));
            teamList.Add(new TMWTTeam("Solary", "CarlJr", "Pac"));
            teamList.Add(new TMWTTeam("Alliance", "Soulja", "Mudda"));
            teamList.Add(new TMWTTeam("BIG", "Massa", "Granady"));
            teamList.Add(new TMWTTeam("Gamers First", "Gwen", "Binks"));
            teamList.Add(new TMWTTeam("Karmine Corp", "Bren", "Otaaaq"));
            teamList.Add(new TMWTTeam("Sinners", "Kappa", "tween"));
            teamList.Add(new TMWTTeam("Exalty", "Link", "MiQuatro"));
            teamList.Add(new TMWTTeam("BS+ Competition", "Snow", "Glast"));
            teamList.Add(new TMWTTeam("Izi Dream", "Cocow", "Worker"));
            teamList.Add(new TMWTTeam("Orks GP Numelops", "Complex", "Panda"));
            teamList.Add(new TMWTTeam("Schweineaim Racing", "Ratchet", "Barbos"));
            teamList.Add(new TMWTTeam("Sprout", "Scrapie", "Dexter"));
            teamList.Add(new TMWTTeam("Homyno Tsun", "Feed", "ener"));
            teamList.Add(new TMWTTeam("Alternate Attax", "Wosile", "Skandear"));
            return teamList;
        }
    }
}

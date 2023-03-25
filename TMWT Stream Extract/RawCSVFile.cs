using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration.Attributes;

namespace TMWT_Stream_Extract
{
    public class RawCSVFile
    {
        //  rows = "Team1, Team2, Team1MatchScore, Team2MatchScore, Team1MapScore, Team2MapScore, MapName, mapNo, RoundNo, CPNo, P1Name, P2Name, P3Name, P4Name, P1Time, P2Time, P3Time, P4Time, P1Raw, P2Raw, P3Raw, P4Raw ";
        
        [Name("Team1")]
        public string Team1 { get; set; }
        [Name("Team2")]
        public string Team2 { get; set; }
        [Name("Team1MatchScore")]
        public int Team1MatchScore { get; set; }
        [Name("Team2MatchScore")]
        public int Team2MatchScore { get; set; }
        [Name("Team1MapScore")]
        public int Team1MapScore { get; set; }
        [Name("Team2MapScore")]
        public int Team2MapScore { get; set; }
        [Name("MapName")]
        public string MapName { get; set; }
        [Name("mapNo")]
        public int mapNo { get; set; }
        [Name("RoundNo")]
        public int RoundNo { get; set; }
        [Name("CPNo")]
        public int CPNo { get; set; }
        [Name("P1Name")]
        public string P1Name { get; set; }
        [Name("P2Name")]
        public string P2Name { get; set; }
        [Name("P3Name")]
        public string P3Name { get; set; }
        [Name("P4Name")]
        public string P4Name { get; set; }
        [Name("P1Time")]
        public double P1Time { get; set; }
        [Name("P2Time")]
        public double P2Time { get; set; }
        [Name("P3Time")]
        public double P3Time { get; set; }
        [Name("P4Time")]
        public double P4Time { get; set; }
        [Name("P1Raw")]
        public string P1Raw { get; set; }
        [Name("P2Raw")]
        public string P2Raw { get; set; }
        [Name("P3Raw")]
        public string P3Raw { get; set; }
        [Name("P4Raw")]
        public string P4Raw { get; set; }

    }
}

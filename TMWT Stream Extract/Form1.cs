using CsvHelper;
using CsvHelper.Configuration;
using IronOcr;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;

namespace TMWT_Stream_Extract
{
    public partial class TMWTExtract : Form
    {
        public TMWTExtract()
        {
            InitializeComponent();
            ExtractionTextBox.Text = generateRandomString();
        }
        private BackgroundWorker bgWorker = new BackgroundWorker();
        
        private bool runExtract = false;
        private int ExtractDelay = 1000;
        private string baseFolder = "";
        private string specificScreenshotFolder = "";
        private string specificDataFolder = "";
        private string baseFileName = "screenshot";
        private string CSVExportFile = "";
        private int baseSSID = 1;
        private string lastCSVEntry = "";
        private List<bool> SSHash = new List<bool>() { true, false };

        bool? useGrandLeague = null;

        //checks against important for round management
        private int prevRound = 0;
        private int prevTrack = 0;
        private TimeSpan prevCPTime = new TimeSpan(0);
        private int CPNoGuess = 0;

        private void Run_Click(object sender, EventArgs e)
        {
            CheckFoldersValid();
            CheckTixBox();
            runExtract = !runExtract;

            if (runExtract)
            {
                if (!bgWorker.IsBusy)
                {
                    // Start the background worker
                    StatusTextBox.Text = "Running extract";
                    bgWorker.WorkerSupportsCancellation = true;
                    bgWorker.DoWork += new DoWorkEventHandler(bgWorker_DoWork);
                    bgWorker.RunWorkerAsync();

                }
            }
        }
        private void bgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            Extraction(e);
        }
        private void bgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled) { StatusTextBox.Text = "Cancelled"; }
            else if (e.Error != null) StatusTextBox.Text = e.Error.Message;
            // Update the UI here
        }

        private void CheckTixBox()
        {

            useGrandLeague = null;
            //is grandleague in indeterminate
            if (grandLeague.CheckState == CheckState.Indeterminate)
            {
                useGrandLeague = null;
            }
            else if (grandLeague.CheckState == CheckState.Checked)
            {
                useGrandLeague = true;
            }
            else if (grandLeague.CheckState == CheckState.Unchecked)
            {
                useGrandLeague = false;
            }
        }

        private void CheckFoldersValid()
        {
            baseFolder = FolderTextBox.Text;
            //append a \\ if not exists
            if (!baseFolder.EndsWith("\\"))
            {
                baseFolder += "\\";
            }
            //check if folder exists
            if (!Directory.Exists(baseFolder))
            {
                Directory.CreateDirectory(baseFolder);
            }
            var SSFolder = baseFolder + "Screenshots\\";
            if (!Directory.Exists(SSFolder))
            {
                Directory.CreateDirectory(SSFolder);
            }
            specificScreenshotFolder = SSFolder + "" + ExtractionTextBox.Text + "\\";
            if (!Directory.Exists(specificScreenshotFolder))
            {
                Directory.CreateDirectory(specificScreenshotFolder);
            }

            var parsedDataFolder = baseFolder + "ParsedData\\";
            if (!Directory.Exists(parsedDataFolder))
            {
                Directory.CreateDirectory(parsedDataFolder);
            }
            specificDataFolder  = parsedDataFolder + "" + ExtractionTextBox.Text + "\\";
            if (!Directory.Exists(specificDataFolder))
            {
                Directory.CreateDirectory(specificDataFolder);
            }
        }

        private void Extraction(DoWorkEventArgs e)
        {
            var ocrEngine = new OCREngine();

            CSVCheck();
            
            while (runExtract)
            {
                //check for cancel
                if (bgWorker.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }
                var stopwatch = Stopwatch.StartNew();

                var bmap = Screenshot.TakeScreenshot();

                var OCRResult = ocrEngine.GetTextFromBitMap(bmap);

                //Extract everything we can
                if (OCRResult != null)
                {
                    try
                    { 
                        ReadMatchData(OCRResult, bmap);
                    }
                    catch (Exception ex)
                    {
                        //we can continue - likely is no match detected
                    }
                }



                //wait until running again
                //sleep the remaining time, minus time executing
                var remainingWaitTime = ExtractDelay - stopwatch.Elapsed.TotalMilliseconds;

                if (remainingWaitTime > 0)
                {
                    System.Threading.Thread.Sleep(Convert.ToInt32(remainingWaitTime));
                }
            }
        }

        private void ReadMatchData(OcrResult ocrResult, Bitmap bmap)
        {
            //following is taken using Left screen @ Full screen 
            //pixspy.com

            //original calibration done with a 1850 width, 480 image. 

            //map name approx location: X 1214 Y 179 width ~~ 150
            //shortestmap name = reps? reduce to 3 
            //original: 1850, 480 -> taken as percentage
            //var mapNamestring = OCREngine.GetAllEnclosedTextInOCR(ocrResult, 24, 139, 400, 44);
            var mapNamestring = OCREngine.GetAllEnclosedTextInOCR(ocrResult, bmap, 24, 139, 400, 44);
            var mapName = StringCorrection.ClosestMapName(mapNamestring);
            //MapTextBox.Text = mapName;
            MapTextBox.Invoke((MethodInvoker)(() => MapTextBox.Text = mapName));

            if (string.IsNullOrEmpty(mapName))
            {
                StatusTextBox.Invoke((MethodInvoker)(() => StatusTextBox.Text = "couldn't identify match"));
                return; //probably safest way of checking for valid match without outputting nonsense
            }
            StatusTextBox.Invoke((MethodInvoker)(() => StatusTextBox.Text = "Extracting match"));

            //map (track) number in match: X 120 Y 191 width ~6
            var mapNumberString = OCREngine.GetclosestTextInOCR(ocrResult, bmap, 119, 191, 6, 15, 1);
            //round number (in map): X 236 Y 181 width ~16
            var roundNumberString = OCREngine.GetclosestTextInOCR(ocrResult, bmap, 240, 191, 6, 14, 1);

            var mapNumber = StringCorrection.AdjustForDigitsOnly(mapNumberString);
            var roundNumber = StringCorrection.AdjustForDigitsOnly(roundNumberString);

            //team names approx location: X 816, Y 60, Width ~~1104
            //team 1 name between 910 and ~1136 ish
            var team1Namestr = OCREngine.GetAllEnclosedTextInOCR(ocrResult, bmap, 801, 39, 350, 122);
            //team 2 name between ~1430 and ~1750
            var team2Namestr = OCREngine.GetAllEnclosedTextInOCR(ocrResult, bmap, 1410, 39, 350, 122);
            //correction if possible
            var team1Name = StringCorrection.ClosestTeamName(team1Namestr, useGrandLeague);
            var team2Name = StringCorrection.ClosestTeamName(team2Namestr, useGrandLeague);

            Team1TextBox.Invoke((MethodInvoker)(() => Team1TextBox.Text = team1Name));
            Team2TextBox.Invoke((MethodInvoker)(() => Team2TextBox.Text = team2Name));


            CurrentMatchTextBox.Invoke((MethodInvoker)(() => CurrentMatchTextBox.Text = team1Name +" v "+team2Name));


            //team 2 logo (too far right): X 1771
            //team 1 score position X 1152 Y 74 width ~90
            var team1MapScoreString = OCREngine.GetclosestTextInOCR(ocrResult, bmap, 1185, 70, 60, 60, 1);
            //team 2 score position X 1291 Y 73 width ~100
            var team2MapScoreString = OCREngine.GetclosestTextInOCR(ocrResult, bmap, 1310, 70, 60, 60, 1);
            //correction
            var team1MapScoreAdjusted = StringCorrection.AdjustForDigitsOnly(team1MapScoreString);
            var team2MapScoreAdjusted = StringCorrection.AdjustForDigitsOnly(team2MapScoreString);

            int team1MapScoreAdj = 0;
            int team2MapScoreAdj = 0;
            try
            {
                //only take digits from team1MapScoreAdjusted
                team1MapScoreAdjusted = new string(team1MapScoreAdjusted.Where(c => char.IsDigit(c)).ToArray());
                //check length = max 2
                if(team1MapScoreAdjusted.Length > 2)
                {
                    team1MapScoreAdjusted = team1MapScoreAdjusted.Substring(0, 2);
                }
                team1MapScoreAdj = Convert.ToInt32(team1MapScoreAdjusted);
            }catch {}
            //same team 2
            try
            {
                team2MapScoreAdjusted = new string(team2MapScoreAdjusted.Where(c => char.IsDigit(c)).ToArray());
                //check length = max 2
                if (team2MapScoreAdjusted.Length > 2)
                {
                    team2MapScoreAdjusted = team2MapScoreAdjusted.Substring(0, 2);
                }
                team2MapScoreAdj = Convert.ToInt32(team2MapScoreAdjusted);
            } catch { }

            MapScoreTextBox.Invoke((MethodInvoker)(() => MapScoreTextBox.Text = team1MapScoreAdj + " - " + team2MapScoreAdj));

            //LIVE RANKING:
            //Position 1 player name: X 104, Y 316 , Width ~50-100
            var pos1NameRaw = OCREngine.GetclosestTextInOCR(ocrResult, bmap, 100, 316, 200, 20, 3, true);
            //time (CP/Fin): X 416, Y 315
            var pos1TimeRaw = OCREngine.GetclosestTextInOCR(ocrResult, bmap, 416, 315, 100, 20, 6, true);
            //position 2 name: X 66, Y 356, Width ~50-150
            var pos2NameRaw = OCREngine.GetclosestTextInOCR(ocrResult, bmap, 100, 356, 200, 20, 3, true);
            //time (Delta to first): X 451, Y 350, Width ~100
            var pos2TimeRaw = OCREngine.GetclosestTextInOCR(ocrResult, bmap, 452, 353, 100, 20, 4, true);
            //position 3 name: X 104, Y 403, Width ~100
            var pos3NameRaw = OCREngine.GetclosestTextInOCR(ocrResult, bmap, 100, 403, 200, 20, 3, true);
            //time (delta to first): X 457, Y 403, Width 86
            var pos3TimeRaw = OCREngine.GetclosestTextInOCR(ocrResult, bmap, 452, 403, 86, 20, 4, true);
            //position 4 name: X 105, Y 430, Width ~85
            var pos4NameRaw = OCREngine.GetclosestTextInOCR(ocrResult, bmap, 100, 446, 200, 20, 3, true);
            //time (delta): X: 457, Y 447, width 90, height 20
            var pos4TimeRaw = OCREngine.GetclosestTextInOCR(ocrResult, bmap, 452, 447, 90, 20, 4, true);


            if (string.IsNullOrEmpty(pos4TimeRaw))
            {
                return;
            }
            if (string.IsNullOrEmpty(pos1TimeRaw))
            {
                return;
            }

            //name correction if possible
            var pos1Name = StringCorrection.ClosestPlayerName(pos1NameRaw, useGrandLeague);
            var pos2Name = StringCorrection.ClosestPlayerName(pos2NameRaw, useGrandLeague);
            var pos3Name = StringCorrection.ClosestPlayerName(pos3NameRaw, useGrandLeague);
            var pos4Name = StringCorrection.ClosestPlayerName(pos4NameRaw, useGrandLeague);

            if (string.IsNullOrEmpty(pos1Name)) { pos1Name = pos1NameRaw; }
            if (string.IsNullOrEmpty(pos2Name)) { pos2Name = pos2NameRaw; }
            if (string.IsNullOrEmpty(pos3Name)) { pos3Name = pos3NameRaw; }
            if (string.IsNullOrEmpty(pos4Name)) { pos4Name = pos4NameRaw; }

            var pos1Time = StringCorrection.AdjustCPTime(pos1TimeRaw, true);
            var pos2Time = StringCorrection.AdjustCPTime(pos2TimeRaw);
            var pos3Time = StringCorrection.AdjustCPTime(pos3TimeRaw);
            var pos4Time = StringCorrection.AdjustCPTime(pos4TimeRaw);

            //pos1 should really be a timespan..
            if (string.IsNullOrEmpty(pos1Time))
            {
                pos1Time = "00:00:00";
            }
            var pos1TimeSpan = TimeSpan.Parse("00:"+pos1Time); //most concerning thing here would be missing the decimal probably....
            //others are adjusted to this...
            TimeSpan pos2TimeSpan = new TimeSpan(0);
            try { pos2TimeSpan= pos1TimeSpan.Add(TimeSpan.FromSeconds(Convert.ToDouble(pos2Time))); } catch { }
            TimeSpan pos3TimeSpan = new TimeSpan(0);
            try { pos3TimeSpan = pos1TimeSpan.Add(TimeSpan.FromSeconds(Convert.ToDouble(pos3Time))); }catch { }
            TimeSpan pos4TimeSpan = new TimeSpan(0);
            try { pos4TimeSpan = pos1TimeSpan.Add(TimeSpan.FromSeconds(Convert.ToDouble(pos4Time))); } catch { }

            if(pos1TimeSpan.TotalSeconds > 130)
            {
                pos1TimeSpan = new TimeSpan(0);
            }
            if (pos2TimeSpan.TotalSeconds > 130)
            {
                pos2TimeSpan = new TimeSpan(0);
            }
            if (pos3TimeSpan.TotalSeconds > 130)
            {
                pos3TimeSpan = new TimeSpan(0);
            }
            if (pos4TimeSpan.TotalSeconds > 130)
            {
                pos4TimeSpan = new TimeSpan(0);
            }

            var positionsSummary = $@"Track: ({mapNumber}) - round {roundNumber}
{pos1Name} {pos1Time}
{pos2Name} {pos2Time}
{pos3Name} {pos3Time}
{pos4Name} {pos4Time}";
            //CurrentRoundTextBox.Text = positionsSummary;
            CurrentRoundTextBox.Invoke((MethodInvoker)(() => CurrentRoundTextBox.Text = positionsSummary));


            var team1MapScore = 0;
            var team2MapScore = 0;

            //now to get MatchScore, need to check pixel color at specific places
            //var bmap = new Bitmap(SSFileName);
            //read pixel 970, 180 in screenshot. if light color -> map 3 is won, so team 1 has 3 map points
            //if dark color -> map 3 is not won, so team 1 might 2 map points
            var team1Map3ScoreIndicatorPixel = bmap.GetPixel(970, 180);
            if(team1Map3ScoreIndicatorPixel.R > 50)
            {
                team1MapScore = 3;
            }
            else
            {
                var team1Map2ScoreIndicatorPixel = bmap.GetPixel(850, 176);
                if (team1Map2ScoreIndicatorPixel.R > 50)
                {
                    team1MapScore = 2;
                }
                else
                {
                    var team1Map1ScoreIndicatorPixel = bmap.GetPixel(745, 175);
                    if (team1Map1ScoreIndicatorPixel.R > 50)
                    {
                        team1MapScore = 1;
                    }
                }
            }
            //same for team 2
            var team2Map3ScoreIndicatorPixel = bmap.GetPixel(1570, 180);
            if (team2Map3ScoreIndicatorPixel.R > 60)
            {
                team2MapScore = 3;
            }
            else
            {
                var team2Map2ScoreIndicatorPixel = bmap.GetPixel(1710, 176);
                if (team2Map2ScoreIndicatorPixel.R > 60)
                {
                    team2MapScore = 2;
                }
                else
                {
                    var team2Map1ScoreIndicatorPixel = bmap.GetPixel(1801, 175);
                    if (team2Map1ScoreIndicatorPixel.R > 60)
                    {
                        team2MapScore = 1;
                    }
                }
            }
            

            //MapScoreTextBox.Text = team1MapScore + " " + team2MapScore;
            MapScoreTextBox.Invoke((MethodInvoker)(() => MapScoreTextBox.Text = team1MapScore + " " + team2MapScore));


            List<string> Errors = new List<string>();

            //adjustmentLogic:
            //if prevCPTimeSpan is different, increment CP Guess
            if(prevCPTime != pos1TimeSpan)
            {
                if(pos1TimeSpan == new TimeSpan(0))
                {
                    CPNoGuess = 0;
                }
                else
                {
                    CPNoGuess++;
                }
            }
            prevCPTime = pos1TimeSpan;
            int roundNumberConv = 0;
            try { roundNumberConv = Convert.ToInt32(roundNumber); } catch { }
            int trackNumberCon = 0;
            try { Convert.ToInt32(mapNumber); } catch { }
            //quick sanity check
            if (team2MapScore + team1MapScore + 1 != trackNumberCon)
            {
                //Errors.Add("Map score does not match track number");
            }
            if (prevRound != roundNumberConv)
            {
                //CPNoGuess = 0;
            }
            prevRound = roundNumberConv;
            if (prevTrack != trackNumberCon) //does this even matter?
            {
                
            }
            prevTrack = trackNumberCon;

            //save results into csv
            // "Team1, Team2, Team1MatchScore, Team2MatchScore, Team1MapScore, Team2MapScore," +
            //            " MapName, mapNo, RoundNo, CPNo, P1Name, P2Name, P3Name, P4Name, P1Time, P2Time, P3Time, P4Time, Error(s) ";
            var newRow = $"{team1Name.Replace(',',' ')}, {team2Name.Replace(',', ' ')}, {team1MapScore}, {team2MapScore} , {team1MapScoreAdj}, {team2MapScoreAdj}, " +
                $"{mapName}, {prevTrack}, {prevRound}, {CPNoGuess}, " +
                $"{pos1Name.Replace(',', ' ')}, {pos2Name.Replace(',', ' ')}, {pos3Name.Replace(',', ' ')}, {pos4Name.Replace(',', ' ')}, " +
                $"{pos1TimeSpan.TotalSeconds}, {pos2TimeSpan.TotalSeconds}, {pos3TimeSpan.TotalSeconds}, {pos4TimeSpan.TotalSeconds}, " +
                $"{pos1TimeRaw.Replace(',', '.')}, {pos2TimeRaw.Replace(',', '.')}, {pos3TimeRaw.Replace(',', '.')}, {pos4TimeRaw.Replace(',', '.')}";

            newRow = newRow.Replace("\"", "").Replace("\'", "");
            if (newRow != lastCSVEntry)
            {
                File.AppendAllText(CSVExportFile, newRow + Environment.NewLine);
            }
            lastCSVEntry = newRow;
        }

        private string GetStringShotFileName()
        {
            return $"{specificScreenshotFolder}{baseFileName}{baseSSID.ToString()}.Tiff";
        }

        private string generateRandomString()
        {
            var random = new Random();
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var result = new string(
                Enumerable.Repeat(chars, 10)
                          .Select(s => s[random.Next(s.Length)])
                          .ToArray());
            return result;
        }

        private void CSVCheck()
        {

            //create CSV to write into incase doesn't exist
            CSVExportFile = specificDataFolder + "" + baseFileName + "_raw.csv";
            if (!File.Exists(CSVExportFile))
            {
                //write the header
                var headerRow = "Team1, Team2, Team1MatchScore, Team2MatchScore, Team1MapScore, Team2MapScore," +
                    " MapName, mapNo, RoundNo, CPNo, P1Name, P2Name, P3Name, P4Name, P1Time, P2Time, P3Time, P4Time, P1Raw, P2Raw, P3Raw, P4Raw ";
                File.WriteAllText(CSVExportFile, headerRow + Environment.NewLine);
            }
            
        }

        private void label11_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            //post processing - dig into parent folder for any CSV files
            CheckFoldersValid();
            CheckTixBox();

            //var mapConfigFile = baseFolder + "ParsedData\\" + "MapInfo.json";
            //var mapConfigString = File.ReadAllText(mapConfigFile);
            string mapConfigString = "";
            
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "TMWT_Stream_Extract.MapInfo.json";

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    mapConfigString = reader.ReadToEnd();
                }
            }

            var mapConfig = JsonConvert.DeserializeObject<mapJson>(mapConfigString);
            var mapNames = mapConfig.maps.Select(a => a.mapName).ToList();

            //specific data folder is the parent folder - take all CSV files inside here
            var allCSVFiles = Directory.GetFiles(specificDataFolder, "*.csv");
            foreach(var CSVFile in allCSVFiles)
            {
                try
                {
                    if (!CSVFile.Contains("_processed"))
                    {
                        var skip = false;
                        foreach(var mapName in mapNames)
                        {
                            if (CSVFile.Contains(mapName))
                            {
                                skip = true;
                                break;
                            }
                        }
                        if (skip)
                        {
                            continue;
                        }
                        StatusTextBox.Text = "Running processing...";
                        RunPostProcessing(CSVFile, mapConfig);
                    }
                }
                catch (Exception ex)
                {
                    StatusTextBox.Text = "Error:" + ex.ToString();
                    ErrorTextBox.Text = ex.Message + "\n" + ex.StackTrace;
                }
            }
        }

        private void RunPostProcessing(string CSVFile, mapJson mapConfig)
        {
            //read CSVRawData
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                IgnoreBlankLines = true,
                Delimiter = ",",
                TrimOptions = TrimOptions.Trim,
                MissingFieldFound = null,

            };
            var CSVData = new List<RawCSVFile>();
            using (var reader = new StreamReader(CSVFile))
            using (var csv = new CsvReader(reader, config))
            {
                csv.Read();
                var header = csv.ReadHeader();
                CSVData = csv.GetRecords<RawCSVFile>().ToList();
            }
            CleanData_PlayerNames(CSVData);
            CleanDataSweep1_TeamAndMapNames(CSVData);
            CleanData_PlayerNames(CSVData);
            CleanCPData(CSVData);
            CleanData_TeamForcedSweep(CSVData);
            CleanData_PlayerNames(CSVData);
            CleanCPCheckpointCount(CSVData, mapConfig);
            CleanCPTimes(CSVData);
            CleanMapRounds(CSVData);
            LastPlayerTeamCheck(CSVData);
            //now ready to read CP Times

            var FileName = Path.GetFileNameWithoutExtension(CSVFile);
            //lets save by map
            foreach (var map in mapConfig.maps)
            {
                WriteMapDataCSV(CSVData, map, FileName);
            }

            //save CSVData to a new file
            var processedCSVDataFile = specificDataFolder + FileName + "_processed.csv";

            using (var writer = new StreamWriter(processedCSVDataFile))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(CSVData);
            }
        }

        private void WriteMapDataCSV(List<RawCSVFile> CSVData, mapInstanceJson map, string FileName)
        {
            //go through the CSVData looking for this map... do not take rows where the mapName doesn't match EITHER 10 rows above or 10 rows below (likely mistakes..)
            var mapData = new List<RawCSVFile>();
            
            var processedCSVDataFile = specificDataFolder + FileName + "_" + map.mapName + ".csv";

            var CSVHeaderRow = "Team, Player,";
            for (var i = 0; i < map.CPs.Count; i++) 
            {
                if (i == map.CPs.Count - 1)
                {
                    CSVHeaderRow += "Finish, ";
                }
                else
                {
                    CSVHeaderRow += $"CP{i+1}, ";
                }
            }
            CSVHeaderRow += $"TimeMinusIdentity, IdentityTime(CP {map.Identity-1}), Round Number, Position(in round)";
            File.WriteAllText(processedCSVDataFile, CSVHeaderRow + Environment.NewLine);

            //write example line using CP guides
            var exampleLine = $"Example, Times, ";
            for (var i = 0; i < map.CPs.Count; i++)
            {
                exampleLine += map.CPs[i] + ", ";
            }
            exampleLine += ", , , ";
            File.AppendAllText(processedCSVDataFile, exampleLine + Environment.NewLine);

            for (var i = 0; i < CSVData.Count; i++)
            {
                var row = CSVData[i];
                if (row.MapName == map.mapName)
                {
                    //check 10 rows above and below
                    //if it fails both, its discounted.
                    var belowCheck = true;
                    var aboveCheck = true;
                    
                    if (i > 10)
                    {
                        var mismatchCount = 0;
                        for (var j = i - 10; j < i; j++)
                        {
                            if (CSVData[j].MapName != map.mapName)
                            {
                                mismatchCount++;
                            }
                        }
                        if (mismatchCount > 6)
                        {
                            aboveCheck = false;
                        }
                    }
                    if (i < CSVData.Count - 10)
                    {
                        var mismatchCount = 0;
                        for (var j = i + 1; j < i + 10; j++)
                        {
                            if (CSVData[j].MapName != map.mapName)
                            {
                                mismatchCount++;
                            }
                        }
                        if (mismatchCount > 6)
                        {
                            belowCheck = false;
                        }
                    }

                    if(aboveCheck == false && belowCheck == false)
                    {
                        continue;
                    }
                    mapData.Add(row);
                }
            }
            
            if(mapData.Count == 0)
            {
                return;
            }


            //get all named players in these mapDatarows...
            var namedPlayers = new List<string>();
            foreach (var row in mapData)
            {
                if (!string.IsNullOrEmpty(row.P1Name))
                {
                    if (!namedPlayers.Contains(row.P1Name))
                    {
                        namedPlayers.Add(row.P1Name);
                    }
                }
                if (!string.IsNullOrEmpty(row.P2Name))
                {
                    if (!namedPlayers.Contains(row.P2Name))
                    {
                        namedPlayers.Add(row.P2Name);
                    }
                }
                if (!string.IsNullOrEmpty(row.P3Name))
                {
                    if (!namedPlayers.Contains(row.P3Name))
                    {
                        namedPlayers.Add(row.P3Name);
                    }
                }
                if (!string.IsNullOrEmpty(row.P4Name))
                {
                    if (!namedPlayers.Contains(row.P4Name))
                    {
                        namedPlayers.Add(row.P4Name);
                    }
                }
            }

            var validTeams = StringCorrection.GetTMWTTeams(useGrandLeague);
            //then cycle through the data looking for named player entries
            foreach (var player in namedPlayers)
            {
                var playerCSVLines = new List<string>();
                var TeamName = "";
                foreach (var team in validTeams)
                {
                    if (team.Players.Contains(player))
                    {
                        TeamName = team.TeamName;
                        break;
                    }
                }

                var curPosition = 0;
                var curRound = 0;
                var curCP = 0;
                var currentLine = $"{TeamName}, {player}, ";

                for (var i = 0; i < mapData.Count; i++)
                {
                    var row = mapData[i];
                    if(row.P1Name == player)
                    {
                        if (curRound != row.RoundNo)
                        {
                            //new round => new line needed. Add current line to playerCSVLines new line
                            //does current line have enough commas? It will need 3 + CP Count - 1..?
                            var commasNeeded = 4 + map.CPs.Count - 1;
                            var commasInLine = currentLine.Count(x => x == ',');
                            if(currentLine == $"{TeamName}, {player}, ")
                            {
                                //dont save
                            }
                            else
                            {
                                if (commasInLine < commasNeeded)
                                {
                                    for (var j = commasInLine; j < commasNeeded; j++)
                                    {
                                        currentLine += ", ";
                                    }
                                }
                                currentLine += ", " + curRound + ", " + curPosition;
                                playerCSVLines.Add(currentLine);
                            }
                            currentLine = $"{TeamName}, {player}, ";
                            curRound = row.RoundNo;
                            curPosition = 0;
                            curCP = 0;
                        }

                        if (curCP != row.CPNo)
                        {
                            //new CP -> 
                            //if new CP is 1 larger than current CP, then just add the time as a new , - otherwise, we'll have to skip a number of CPs proportionate in the commas
                            for (var j = curCP; j < row.CPNo - 1; j++)
                            {
                                currentLine += ", ";
                            }
                            //want to find most reliable time possible for this CP, there could be multiple rows with this Round number and this CP Number.
                            var CPDataThisCP = mapData.Where(x => x.RoundNo == row.RoundNo && x.CPNo == row.CPNo && x.P1Name == player && x.P1Time != 0 ).ToList();
                            if (CPDataThisCP.Count > 0)
                            {
                                var CPDataThisCPTime = CPDataThisCP.GroupBy(x => x.P1Time).OrderByDescending(x => x.Count()).First().Key;

                                currentLine += $"{CPDataThisCPTime}, ";
                            }
                            else
                            {
                                currentLine += $", ";
                            }

                            curPosition = 1;
                            curCP = row.CPNo;
                        }
                    }
                    
                    else if (row.P2Name == player)
                    {
                        if (curRound != row.RoundNo)
                        {
                            //new round => new line needed. Add current line to playerCSVLines new line
                            var commasNeeded = 4 + map.CPs.Count - 1;
                            var commasInLine = currentLine.Count(x => x == ',');
                            if (currentLine == $"{TeamName}, {player}, ")
                            {
                                //dont save
                            }
                            else
                            {
                                if (commasInLine < commasNeeded)
                                {
                                    for (var j = commasInLine; j < commasNeeded; j++)
                                    {
                                        currentLine += ", ";
                                    }
                                }
                                currentLine += ", " + curRound + ", " + curPosition;
                                playerCSVLines.Add(currentLine);
                            }
                            currentLine = $"{TeamName}, {player}, ";
                            curRound = row.RoundNo;
                            curPosition = 0;
                            curCP = 0;
                        }
                        
                        if(curCP != row.CPNo)
                        {
                            //if new CP is 1 larger than current CP, then just add the time as a new , - otherwise, we'll have to skip a number of CPs proportionate in the commas
                            for (var j = curCP; j < row.CPNo - 1; j++)
                            {
                                currentLine += ", ";
                            }
                            //want to find most reliable time possible for this CP, there could be multiple rows with this Round number and this CP Number.
                            var CPDataThisCP = mapData.Where(x => x.RoundNo == row.RoundNo && x.CPNo == row.CPNo && (x.P2Name == player || x.P3Name == player || x.P4Name == player) ).ToList();

                            //final position is what matters... can lose 2nd and drop to 3rd or 4th if crashed leading up to that CP from a previous held better position
                            var finalDataPoint = CPDataThisCP.Last();
                            
                            if(finalDataPoint.P2Name == player)
                            {
                                //they really are p2...
                                //remove all P2Times which are 0
                                CPDataThisCP = CPDataThisCP.Where(x => x.P2Time != 0).ToList(); if (CPDataThisCP.Count > 0)
                                {
                                    var gapToP1 = Math.Abs(CPDataThisCP.Last().P1Time - finalDataPoint.P2Time);

                                    //we'll say if >1 second
                                    if (gapToP1 > 1)
                                    {
                                        var CPDataThisCPTime = CPDataThisCP.Last().P2Time;
                                        currentLine += $"{CPDataThisCPTime}, ";
                                    }
                                    else
                                    {
                                        var CPDataThisCPTime = CPDataThisCP.GroupBy(x => x.P2Time).OrderByDescending(x => x.Count()).First().Key;
                                        currentLine += $"{CPDataThisCPTime}, ";
                                    }
                                }
                                else
                                {
                                    currentLine += $", ";
                                }


                                curPosition = 2;
                            }
                            else if (finalDataPoint.P3Name == player)
                            {
                                //they really are p3...
                                var P3DataThisCP = CPDataThisCP.Where(x => x.P3Name == player && x.P3Time != 0).ToList();
                                if(P3DataThisCP.Count > 0)
                                {
                                    var gapToP1 = Math.Abs(P3DataThisCP.Last().P1Time - finalDataPoint.P3Time);

                                    //we'll say if >1 second
                                    if (gapToP1 > 1)
                                    {
                                        var CPDataThisCPTime = P3DataThisCP.Last().P3Time;
                                        currentLine += $"{CPDataThisCPTime}, ";
                                    }
                                    else
                                    {
                                        var CPDataThisCPTime = P3DataThisCP.GroupBy(x => x.P3Time).OrderByDescending(x => x.Count()).First().Key;
                                        currentLine += $"{CPDataThisCPTime}, ";
                                    }
                                }
                                else
                                {
                                    currentLine += $", ";
                                }

                                curPosition = 3;
                            }
                            else if (finalDataPoint.P4Name == player)
                            {
                                //they really are p4...
                                var P4DataThisCP = CPDataThisCP.Where(x => x.P4Name == player && x.P4Time != 0).ToList();
                                if(P4DataThisCP.Count > 0)
                                {
                                    var gapToP1 = Math.Abs(P4DataThisCP.Last().P1Time - finalDataPoint.P4Time);

                                    //we'll say if >1 second
                                    if (gapToP1 > 1)
                                    {
                                        var CPDataThisCPTime = P4DataThisCP.Last().P4Time;
                                        currentLine += $"{CPDataThisCPTime}, ";
                                    }
                                    else
                                    {
                                        var CPDataThisCPTime = P4DataThisCP.GroupBy(x => x.P4Time).OrderByDescending(x => x.Count()).First().Key;
                                        currentLine += $"{CPDataThisCPTime}, ";
                                    }
                                }
                                else
                                {
                                    currentLine += $", ";
                                }

                                curPosition = 4;
                            }

                            curCP = row.CPNo;
                        }
                    }
                    else if (row.P3Name == player)
                    {
                        if (curRound != row.RoundNo)
                        {
                            //new round => new line needed. Add current line to playerCSVLines new line
                            var commasNeeded = 4 + map.CPs.Count - 1;
                            var commasInLine = currentLine.Count(x => x == ',');
                            if (currentLine == $"{TeamName}, {player}, ")
                            {
                                //dont save
                            }
                            else
                            {
                                if (commasInLine < commasNeeded)
                                {
                                    for (var j = commasInLine; j < commasNeeded; j++)
                                    {
                                        currentLine += ", ";
                                    }
                                }
                                currentLine += ", " + curRound + ", " + curPosition;
                                playerCSVLines.Add(currentLine);
                            }
                            currentLine = $"{TeamName}, {player}, ";
                            curRound = row.RoundNo;
                            curPosition = 0;
                            curCP = 0;
                        }
                        if (curCP != row.CPNo)
                        {
                            //if new CP is 1 larger than current CP, then just add the time as a new , - otherwise, we'll have to skip a number of CPs proportionate in the commas
                            for (var j = curCP; j < row.CPNo - 1; j++)
                            {
                                currentLine += ", ";
                            }
                            //want to find most reliable time possible for this CP, there could be multiple rows with this Round number and this CP Number.
                            var CPDataThisCP = mapData.Where(x => x.RoundNo == row.RoundNo && x.CPNo == row.CPNo && ( x.P3Name == player || x.P4Name == player)).ToList();

                            //final position is what matters... can lose 2nd and drop to 3rd or 4th if crashed leading up to that CP from a previous held better position
                            var finalDataPoint = CPDataThisCP.Last();

                            if (finalDataPoint.P3Name == player)
                            {
                                //they really are p3...
                                CPDataThisCP = CPDataThisCP.Where(x => x.P3Time != 0).ToList();

                                if (CPDataThisCP.Count > 0)
                                {
                                    var gapToP1 = Math.Abs(CPDataThisCP.Last().P1Time - finalDataPoint.P3Time);

                                    //we'll say if >1 second
                                    if (gapToP1 > 1)
                                    {
                                        var CPDataThisCPTime = CPDataThisCP.Last().P3Time;
                                        currentLine += $"{CPDataThisCPTime}, ";
                                    }
                                    else
                                    {
                                        var CPDataThisCPTime = CPDataThisCP.GroupBy(x => x.P3Time).OrderByDescending(x => x.Count()).First().Key;
                                        currentLine += $"{CPDataThisCPTime}, ";
                                    }
                                }
                                else
                                {
                                    currentLine += $", ";
                                }
                                curPosition = 3;
                            }
                            else
                            {
                                //p4...
                                var P4DataThisCP = CPDataThisCP.Where(x => x.P4Name == player && x.P4Time != 0).ToList();
                                
                                if(P4DataThisCP.Count > 0)
                                {
                                    var gapToP1 = Math.Abs(CPDataThisCP.Last().P1Time - finalDataPoint.P4Time);

                                    //we'll say if >1 second
                                    if (gapToP1 > 1)
                                    {
                                        var CPDataThisCPTime = CPDataThisCP.Last().P4Time;
                                        currentLine += $"{CPDataThisCPTime}, ";
                                    }
                                    else
                                    {
                                        var CPDataThisCPTime = CPDataThisCP.GroupBy(x => x.P4Time).OrderByDescending(x => x.Count()).First().Key;
                                        currentLine += $"{CPDataThisCPTime}, ";
                                    }
                                }
                                else
                                {
                                    currentLine += $", ";
                                }
                                curPosition = 4;
                            }

                            curCP = row.CPNo;
                        }
                    }
                    else if (row.P4Name == player)
                    {
                        if (curRound != row.RoundNo)
                        {
                            //new round => new line needed. Add current line to playerCSVLines new line
                            var commasNeeded = 4 + map.CPs.Count - 1;
                            var commasInLine = currentLine.Count(x => x == ',');
                            if (currentLine == $"{TeamName}, {player}, ")
                            {
                                //dont save
                            }
                            else
                            {
                                if (commasInLine < commasNeeded)
                                {
                                    for (var j = commasInLine; j < commasNeeded; j++)
                                    {
                                        currentLine += ", ";
                                    }
                                }
                                currentLine += ", " + curRound + ", " + curPosition;
                                playerCSVLines.Add(currentLine);
                            }
                            currentLine = $"{TeamName}, {player}, ";
                            curRound = row.RoundNo;
                            curPosition = 0;
                            curCP = 0;
                        }
                        if( curCP != row.CPNo)
                        {
                            //if new CP is 1 larger than current CP, then just add the time as a new , - otherwise, we'll have to skip a number of CPs proportionate in the commas
                            for (var j = curCP; j < row.CPNo - 1; j++)
                            {
                                currentLine += ", ";
                            }
                            //want to find most reliable time possible for this CP, there could be multiple rows with this Round number and this CP Number.
                            var CPDataThisCP = mapData.Where(x => x.RoundNo == row.RoundNo && x.CPNo == row.CPNo && x.P4Name == player && x.P4Time != 0).ToList();

                            if (CPDataThisCP.Count > 0)
                            {
                                var finalCPCheckTime = CPDataThisCP.Last().P4Time;

                                var gapToP1 = Math.Abs(CPDataThisCP.Last().P1Time - finalCPCheckTime);

                                //we'll say if >1 second
                                if (gapToP1 > 1)
                                {
                                    var CPDataThisCPTime = CPDataThisCP.Last().P4Time;
                                    currentLine += $"{CPDataThisCPTime}, ";
                                }
                                else
                                {
                                    var CPDataThisCPTime = CPDataThisCP.GroupBy(x => x.P4Time).OrderByDescending(x => x.Count()).First().Key;
                                    currentLine += $"{CPDataThisCPTime}, ";
                                }
                            }
                            else
                            {
                                currentLine += $", ";
                            }
                            
                            curPosition = 4;
                        }
                        curCP = row.CPNo;
                    }
                }


                var fcommasNeeded = 4 + map.CPs.Count - 1;
                var fcommasInLine = currentLine.Count(x => x == ',');
                if (currentLine == $"{TeamName}, {player}, ")
                {
                    //dont save
                }
                else
                {
                    if (fcommasInLine < fcommasNeeded)
                    {
                        for (var j = fcommasInLine; j < fcommasNeeded; j++)
                        {
                            currentLine += ", ";
                        }
                    }
                    currentLine += ", " + curRound + ", " + curPosition;
                    playerCSVLines.Add(currentLine);
                }
                //write results to csv
                File.AppendAllLines(processedCSVDataFile, playerCSVLines);
            }
            
            //now transpose the CSV File => swap rows and columns
            var transposedFilePath = specificDataFolder +"_Transposed_" + FileName + "_" + map.mapName + ".csv";
            //flip rows and columns of processedCSVDataFile, saving to transposedFilePath

            string [][] rows = File.ReadAllLines(processedCSVDataFile)
                              .Select(x => x.Split(','))
                              .ToArray();

            int rowCount = rows.Length;
            int colCount = rows[0].Length;
            string[][] cols = new string[colCount][];

            for (int i = 0; i < colCount; i++)
            {
                cols[i] = new string[rowCount];
                for (int j = 0; j < rowCount; j++)
                {
                    cols[i][j] = rows[j][i];
                }
            }

            File.WriteAllLines(transposedFilePath,
                cols.Select(x => string.Join(",", x)));

        }


        private void LastPlayerTeamCheck(List<RawCSVFile> CSVData)
        {
            var validTeams = StringCorrection.GetTMWTTeams(useGrandLeague);

            //check player names against teams
            for (var i = 0; i < CSVData.Count; i++)
            {
                var row = CSVData[i];
                var players = new List<string>() { row.P1Name, row.P2Name, row.P3Name, row.P4Name };

                var teamOpts = new List<string>();

                foreach(var team in validTeams)
                {
                    if (team.TeamContains2OfPlayers(players))
                    {
                        teamOpts.Add(team.TeamName);
                    }
                }

                if(teamOpts.Count < 2)
                {
                    //continue
                    continue;
                }

                if (!teamOpts.Contains(row.Team1))
                {
                    //team 1 is wrong. fill it with first option.
                    if (row.Team2 == teamOpts[0])
                    {
                        row.Team1 = teamOpts[0];
                    }
                    else
                    {
                        row.Team1 = teamOpts[1];
                    }
                }
                
                if (!teamOpts.Contains(row.Team2))
                {
                    //team 2 is wrong. fill it with first option.
                    if (row.Team1 == teamOpts[0])
                    {
                        row.Team2 = teamOpts[0];
                    }
                    else
                    {
                        row.Team2 = teamOpts[1];
                    }
                }
                //order by string length as usual

                if (!string.IsNullOrEmpty(row.Team1) && !string.IsNullOrEmpty(row.Team2))
                {
                    if (row.Team1 == row.Team2)
                    {
                        row.Team2 = "";
                    }
                    if (row.Team1.Length < row.Team2.Length)
                    {
                        var temp = row.Team1;
                        row.Team1 = row.Team2;
                        row.Team2 = temp;
                    }
                }

            }
        }

        private void CleanMapRounds(List<RawCSVFile> CSVData)
        {
            //looking for signals that a new round has begun on a map
            //most obvious when all CPTimes -> 0 but can also check big changes (down in CPNo, since CPNo should be accurate
            //need to reset roundnumber on new map though
            //also reset on new team
            var previousZeroes = false;
            var prevCPNo = 0;
            var RoundNumber = 1;
            var curMapName = "";
            var curTeam1Name = "";

            for (var i = 0; i < CSVData.Count; i++)
            {
                var row = CSVData[i];


                //change in mapname or team1name and not null => round 1 time.
                if((!string.IsNullOrEmpty(row.MapName) && row.MapName != curMapName) || (!string.IsNullOrEmpty(row.Team1) && row.Team1 != curTeam1Name) )
                {
                    //new round
                    RoundNumber = 1;
                    row.RoundNo = RoundNumber;

                    curMapName = row.MapName;
                    curTeam1Name = row.Team1;
                    prevCPNo = 0;
                    continue;
                }

                if( row.CPNo >= prevCPNo) //continue;
                {
                    row.RoundNo = RoundNumber;
                    prevCPNo = row.CPNo;
                    continue;
                }
                if(row.P1Time < 1  && row.P2Time <1 && row.P3Time <1 && row.P4Time <1)
                {
                    if (!previousZeroes)
                    {
                        RoundNumber++;
                        row.RoundNo = RoundNumber;
                        previousZeroes = true;
                        prevCPNo = 0;
                        continue;
                    }
                }
                previousZeroes = false;

                if(row.CPNo +3 < prevCPNo)
                {
                    //probably a new round - CP went down by more than 3
                    prevCPNo = row.CPNo;
                    RoundNumber++;
                }

                row.RoundNo = RoundNumber;
            }
        }

        private void CleanCPTimes(List<RawCSVFile> CSVData)
        {
            //basic logic here: P1Time < P2Time < P3Time < P4Time, if a row violates this, need to remove time(s)
            //also, if CP Number increments, time (for that player) should increement. If row violates this > need to look at that time

            for (var i = 0; i < CSVData.Count; i++)
            {
                //check each Time
                var row = CSVData[i];

                //set each time to 3 sig fig
                row.P1Time = (double)Convert.ToInt32(row.P1Time * 1000) / 1000;
                row.P2Time = (double)Convert.ToInt32(row.P2Time * 1000) / 1000;
                row.P3Time = (double)Convert.ToInt32(row.P3Time * 1000) / 1000;
                row.P4Time = (double)Convert.ToInt32(row.P4Time * 1000) / 1000;

                //universal check
                if(row.P1Time <= row.P2Time && row.P1Time <= row.P3Time && row.P1Time <= row.P4Time && row.P2Time <= row.P3Time && row.P3Time <= row.P4Time)
                {
                    continue;
                }

                //is P2 sensible?
                if (row.P1Time > row.P2Time && row.P1Time != 0 && row.P2Time != 0)
                {
                    //any issues excluding p2?
                    if(row.P1Time <= row.P3Time && row.P1Time <= row.P4Time && row.P3Time <= row.P4Time)
                    {
                        //then the issue is that P2Time is too low... probably safest to just delete this
                        row.P2Time = 0; //(no point assigning to above/below... when thatlookup can happen later anyway
                    }
                }

                //is P3 sensible
                if(row.P2Time > row.P3Time && row.P2Time != 0 && row.P3Time != 0)
                {
                    //any issues with non-p3 times?
                    if(row.P1Time <= row.P2Time && row.P1Time <= row.P4Time && row.P2Time <= row.P4Time)
                    {
                        //delete P3
                        row.P3Time = 0;
                    }
                }

                //is P4 sensible
                if (row.P4Time < row.P3Time)
                {
                    //any issues with non-p4?
                    if(row.P1Time <= row.P2Time && row.P1Time <= row.P3Time && row.P2Time <= row.P3Time)
                    {
                        row.P4Time = 0;
                    }
                }
            }
        }

        private void CleanCPCheckpointCount(List<RawCSVFile> CSVData, mapJson mapJson)
        {
            //provide an approximate guess of CP # based on P1Time
            for(var i=0;i<CSVData.Count;i++)
            {
                var row = CSVData[i];
                row.mapNo = 1 + row.Team1MatchScore + row.Team2MatchScore;

                if(row.P1Time < 1)
                {
                    row.CPNo = 0;
                    continue;
                }
                //get map
                if (!string.IsNullOrEmpty(row.MapName))
                {
                    //get object in maps array where mapName matches
                    var opts = mapJson.maps.Where(a => a.mapName == row.MapName).ToList();
                    if(opts.Count > 0)
                    {
                        mapInstanceJson mapObject = opts.First();
                        //find closest CPTime to row.P1Time

                        var closestCPIndex = -1;
                        double closestCPMatch = 999999;
                        for(var j=0;j< mapObject.CPs.Count; j++)
                        {
                            var CPTime = mapObject.CPs[j];
                            var distance = Math.Abs(row.P1Time - CPTime);
                            if(distance < closestCPMatch)
                            {
                                closestCPIndex = j;
                                closestCPMatch = distance;
                            }
                        }

                        if(closestCPMatch < 10) //being generous to allow for a disaster round i guess...
                        {
                            row.CPNo = closestCPIndex+1;//index 0 is the start after all
                        }
                    }
                }
                else
                {
                    //check against row above. If same as row above within ~0.3 , its the same CPNo, otherwise increment by 1
                    if (i == 0) { continue; }
                    var rowAbove = CSVData[i - 1];

                    var CPTimeDiff = Math.Abs(rowAbove.P1Time - row.P1Time);
                    if(CPTimeDiff < 1)
                    {
                        row.CPNo = rowAbove.CPNo;
                        continue;
                    }
                    row.CPNo = rowAbove.CPNo + 1;
                    continue;
                }
            }
        }

        private void CleanData_TeamForcedSweep(List<RawCSVFile> CSVData)
        {
            for (var m = 0; m < 3; m++)
            {
                var arraySize = 14 + 8 * m;
                var CountRequiredToMatch = 8 + 5 * m;

                for (var i = 0; i < CSVData.Count; i++)
                {
                    if (i > arraySize && i < CSVData.Count - (arraySize + 1))
                    {
                        var row = CSVData[i];

                        if (!string.IsNullOrEmpty(row.Team1) && !string.IsNullOrEmpty(row.Team2))
                        {
                            if (row.Team1 == row.Team2)
                            {
                                row.Team2 = "";
                            }
                            if (row.Team1.Length < row.Team2.Length)
                            {
                                var temp = row.Team1;
                                row.Team1 = row.Team2;
                                row.Team2 = temp;
                            }
                        }
                        //get 10 rows above
                        var rowsAbove = CSVData.Skip(i - 1 - arraySize).Take(arraySize).ToArray();
                        var rowsBelow = CSVData.Skip(i + 1).Take(arraySize).ToArray();

                        //get most common item in rowAboveTeam1
                        var rowAboveTeam1Group = rowsAbove.Select(a => a.Team1).ToList().GroupBy(v => v);
                        var team1AboveMaxCount = rowAboveTeam1Group.Max(g => g.Count());
                        if (team1AboveMaxCount >= CountRequiredToMatch)
                        {
                            var rowAboveTeam1Candidate = rowAboveTeam1Group.Where(x => x.Count() == team1AboveMaxCount).Select(x => x.Key).ToArray().First();

                            var rowsBelowTeam1Group = rowsBelow.Select(a => a.Team1).ToList().GroupBy(v => v);
                            var team1BelowMaxCount = rowsBelowTeam1Group.Max(g => g.Count());
                            if (team1BelowMaxCount >= CountRequiredToMatch)
                            {
                                var rowBelowTeam1Candidate = rowsBelowTeam1Group.Where(x => x.Count() == team1BelowMaxCount).Select(x => x.Key).ToArray().First();

                                //important check now=> are these candidates the same?
                                if (rowAboveTeam1Candidate == rowBelowTeam1Candidate)
                                {
                                    //then this row should also match - there are at least 10-m/10 string matches above and below that agree with it
                                    row.Team1 = rowAboveTeam1Candidate;
                                }
                            }
                        }

                        //now repeat for Team2
                        var rowAboveTeam2Group = rowsAbove.Select(a => a.Team2).ToList().GroupBy(v => v);
                        var team2AboveMaxCount = rowAboveTeam2Group.Max(g => g.Count());
                        if (team2AboveMaxCount >= CountRequiredToMatch)
                        {
                            var rowAboveTeam2Candidate = rowAboveTeam2Group.Where(x => x.Count() == team2AboveMaxCount).Select(x => x.Key).ToArray().First();

                            var rowsBelowTeam2Group = rowsBelow.Select(a => a.Team2).ToList().GroupBy(v => v);
                            var team2BelowMaxCount = rowsBelowTeam2Group.Max(g => g.Count());
                            if (team2BelowMaxCount >= CountRequiredToMatch)
                            {
                                var rowBelowTeam2Candidate = rowsBelowTeam2Group.Where(x => x.Count() == team2BelowMaxCount).Select(x => x.Key).ToArray().First();

                                //important check now=> are these candidates the same?
                                if (rowAboveTeam2Candidate == rowBelowTeam2Candidate)
                                {
                                    //then this row should also match - there are at least 10-m/10 string matches above and below that agree with it
                                    row.Team2 = rowAboveTeam2Candidate;
                                }
                            }
                        }

                        //now repeat for mapName
                        var rowAbovemapNameGroup = rowsAbove.Select(a => a.MapName).ToList().GroupBy(v => v);
                        var mapNameAboveMaxCount = rowAbovemapNameGroup.Max(g => g.Count());
                        if (mapNameAboveMaxCount >= CountRequiredToMatch)
                        {
                            var rowAbovemapNameCandidate = rowAbovemapNameGroup.Where(x => x.Count() == mapNameAboveMaxCount).Select(x => x.Key).ToArray().First();

                            var rowsBelowmapNameGroup = rowsBelow.Select(a => a.MapName).ToList().GroupBy(v => v);
                            var mapNameBelowMaxCount = rowsBelowmapNameGroup.Max(g => g.Count());
                            if (mapNameBelowMaxCount >= CountRequiredToMatch)
                            {
                                var rowBelowmapNameCandidate = rowsBelowmapNameGroup.Where(x => x.Count() == mapNameBelowMaxCount).Select(x => x.Key).ToArray().First();

                                //important check now=> are these candidates the same?
                                if (rowAbovemapNameCandidate == rowBelowmapNameCandidate)
                                {
                                    //then this row should also match - there are at least 10-m/10 string matches above and below that agree with it
                                    row.MapName = rowAbovemapNameCandidate;
                                }
                            }
                        }
                    }
                }
            }


            //micro details ---
            for (var m = 0; m < 3; m++)
            {
                var arraySize = 3 + 3* m;
                var CountRequiredToMatch = 2 + 2* m;

                for (var i = 0; i < CSVData.Count; i++)
                {
                    if (i > arraySize && i < CSVData.Count - (arraySize + 1))
                    {
                        var row = CSVData[i];

                        if (!string.IsNullOrEmpty(row.Team1) && !string.IsNullOrEmpty(row.Team2))
                        {
                            if(row.Team1 == row.Team2)
                            {
                                row.Team2 = "";
                            }
                            if (row.Team1.Length < row.Team2.Length)
                            {
                                var temp = row.Team1;
                                row.Team1 = row.Team2;
                                row.Team2 = temp;
                            }
                        }
                        //get 10 rows above
                        var rowsAbove = CSVData.Skip(i - 1 - arraySize).Take(arraySize).ToArray();
                        var rowsBelow = CSVData.Skip(i + 1).Take(arraySize).ToArray();

                        //get most common item in rowAboveTeam1
                        var rowAboveTeam1Group = rowsAbove.Select(a => a.Team1).ToList().GroupBy(v => v);
                        var team1AboveMaxCount = rowAboveTeam1Group.Max(g => g.Count());
                        if (team1AboveMaxCount >= CountRequiredToMatch)
                        {
                            var rowAboveTeam1Candidate = rowAboveTeam1Group.Where(x => x.Count() == team1AboveMaxCount).Select(x => x.Key).ToArray().First();

                            var rowsBelowTeam1Group = rowsBelow.Select(a => a.Team1).ToList().GroupBy(v => v);
                            var team1BelowMaxCount = rowsBelowTeam1Group.Max(g => g.Count());
                            if (team1BelowMaxCount >= CountRequiredToMatch)
                            {
                                var rowBelowTeam1Candidate = rowsBelowTeam1Group.Where(x => x.Count() == team1BelowMaxCount).Select(x => x.Key).ToArray().First();

                                //important check now=> are these candidates the same?
                                if (rowAboveTeam1Candidate == rowBelowTeam1Candidate)
                                {
                                    //then this row should also match - there are at least 10-m/10 string matches above and below that agree with it
                                    row.Team1 = rowAboveTeam1Candidate;
                                }
                            }
                        }

                        //now repeat for Team2
                        var rowAboveTeam2Group = rowsAbove.Select(a => a.Team2).ToList().GroupBy(v => v);
                        var team2AboveMaxCount = rowAboveTeam2Group.Max(g => g.Count());
                        if (team2AboveMaxCount >= CountRequiredToMatch)
                        {
                            var rowAboveTeam2Candidate = rowAboveTeam2Group.Where(x => x.Count() == team2AboveMaxCount).Select(x => x.Key).ToArray().First();

                            var rowsBelowTeam2Group = rowsBelow.Select(a => a.Team2).ToList().GroupBy(v => v);
                            var team2BelowMaxCount = rowsBelowTeam2Group.Max(g => g.Count());
                            if (team2BelowMaxCount >= CountRequiredToMatch)
                            {
                                var rowBelowTeam2Candidate = rowsBelowTeam2Group.Where(x => x.Count() == team2BelowMaxCount).Select(x => x.Key).ToArray().First();

                                //important check now=> are these candidates the same?
                                if (rowAboveTeam2Candidate == rowBelowTeam2Candidate)
                                {
                                    //then this row should also match - there are at least 10-m/10 string matches above and below that agree with it
                                    row.Team2 = rowAboveTeam2Candidate;
                                }
                            }
                        }

                        //now repeat for mapName
                        var rowAbovemapNameGroup = rowsAbove.Select(a => a.MapName).ToList().GroupBy(v => v);
                        var mapNameAboveMaxCount = rowAbovemapNameGroup.Max(g => g.Count());
                        if (mapNameAboveMaxCount >= CountRequiredToMatch)
                        {
                            var rowAbovemapNameCandidate = rowAbovemapNameGroup.Where(x => x.Count() == mapNameAboveMaxCount).Select(x => x.Key).ToArray().First();

                            var rowsBelowmapNameGroup = rowsBelow.Select(a => a.MapName).ToList().GroupBy(v => v);
                            var mapNameBelowMaxCount = rowsBelowmapNameGroup.Max(g => g.Count());
                            if (mapNameBelowMaxCount >= CountRequiredToMatch)
                            {
                                var rowBelowmapNameCandidate = rowsBelowmapNameGroup.Where(x => x.Count() == mapNameBelowMaxCount).Select(x => x.Key).ToArray().First();

                                //important check now=> are these candidates the same?
                                if (rowAbovemapNameCandidate == rowBelowmapNameCandidate)
                                {
                                    //then this row should also match - there are at least 10-m/10 string matches above and below that agree with it
                                    row.MapName = rowAbovemapNameCandidate;
                                }
                            }
                        }
                    }
                }
            }

            //another macro sweep
            for (var m = 0; m < 3; m++)
            {
                var arraySize = 14 + 8 * m;
                var CountRequiredToMatch = 8 + 5 * m;

                for (var i = 0; i < CSVData.Count; i++)
                {
                    if (i > arraySize && i < CSVData.Count - (arraySize + 1))
                    {
                        var row = CSVData[i];

                        if (!string.IsNullOrEmpty(row.Team1) && !string.IsNullOrEmpty(row.Team2))
                        {
                            if (row.Team1 == row.Team2)
                            {
                                row.Team2 = "";
                            }
                            if (row.Team1.Length < row.Team2.Length)
                            {
                                var temp = row.Team1;
                                row.Team1 = row.Team2;
                                row.Team2 = temp;
                            }
                        }
                        //get 10 rows above
                        var rowsAbove = CSVData.Skip(i - 1 - arraySize).Take(arraySize).ToArray();
                        var rowsBelow = CSVData.Skip(i + 1).Take(arraySize).ToArray();

                        //get most common item in rowAboveTeam1
                        var rowAboveTeam1Group = rowsAbove.Select(a => a.Team1).ToList().GroupBy(v => v);
                        var team1AboveMaxCount = rowAboveTeam1Group.Max(g => g.Count());
                        if (team1AboveMaxCount >= CountRequiredToMatch)
                        {
                            var rowAboveTeam1Candidate = rowAboveTeam1Group.Where(x => x.Count() == team1AboveMaxCount).Select(x => x.Key).ToArray().First();

                            var rowsBelowTeam1Group = rowsBelow.Select(a => a.Team1).ToList().GroupBy(v => v);
                            var team1BelowMaxCount = rowsBelowTeam1Group.Max(g => g.Count());
                            if (team1BelowMaxCount >= CountRequiredToMatch)
                            {
                                var rowBelowTeam1Candidate = rowsBelowTeam1Group.Where(x => x.Count() == team1BelowMaxCount).Select(x => x.Key).ToArray().First();

                                //important check now=> are these candidates the same?
                                if (rowAboveTeam1Candidate == rowBelowTeam1Candidate)
                                {
                                    //then this row should also match - there are at least 10-m/10 string matches above and below that agree with it
                                    row.Team1 = rowAboveTeam1Candidate;
                                }
                            }
                        }

                        //now repeat for Team2
                        var rowAboveTeam2Group = rowsAbove.Select(a => a.Team2).ToList().GroupBy(v => v);
                        var team2AboveMaxCount = rowAboveTeam2Group.Max(g => g.Count());
                        if (team2AboveMaxCount >= CountRequiredToMatch)
                        {
                            var rowAboveTeam2Candidate = rowAboveTeam2Group.Where(x => x.Count() == team2AboveMaxCount).Select(x => x.Key).ToArray().First();

                            var rowsBelowTeam2Group = rowsBelow.Select(a => a.Team2).ToList().GroupBy(v => v);
                            var team2BelowMaxCount = rowsBelowTeam2Group.Max(g => g.Count());
                            if (team2BelowMaxCount >= CountRequiredToMatch)
                            {
                                var rowBelowTeam2Candidate = rowsBelowTeam2Group.Where(x => x.Count() == team2BelowMaxCount).Select(x => x.Key).ToArray().First();

                                //important check now=> are these candidates the same?
                                if (rowAboveTeam2Candidate == rowBelowTeam2Candidate)
                                {
                                    //then this row should also match - there are at least 10-m/10 string matches above and below that agree with it
                                    row.Team2 = rowAboveTeam2Candidate;
                                }
                            }
                        }

                        //now repeat for mapName
                        var rowAbovemapNameGroup = rowsAbove.Select(a => a.MapName).ToList().GroupBy(v => v);
                        var mapNameAboveMaxCount = rowAbovemapNameGroup.Max(g => g.Count());
                        if (mapNameAboveMaxCount >= CountRequiredToMatch)
                        {
                            var rowAbovemapNameCandidate = rowAbovemapNameGroup.Where(x => x.Count() == mapNameAboveMaxCount).Select(x => x.Key).ToArray().First();

                            var rowsBelowmapNameGroup = rowsBelow.Select(a => a.MapName).ToList().GroupBy(v => v);
                            var mapNameBelowMaxCount = rowsBelowmapNameGroup.Max(g => g.Count());
                            if (mapNameBelowMaxCount >= CountRequiredToMatch)
                            {
                                var rowBelowmapNameCandidate = rowsBelowmapNameGroup.Where(x => x.Count() == mapNameBelowMaxCount).Select(x => x.Key).ToArray().First();

                                //important check now=> are these candidates the same?
                                if (rowAbovemapNameCandidate == rowBelowmapNameCandidate)
                                {
                                    //then this row should also match - there are at least 10-m/10 string matches above and below that agree with it
                                    row.MapName = rowAbovemapNameCandidate;
                                }
                            }
                        }
                    }
                }
            }

        }

        private void CleanCPData(List<RawCSVFile> CSVData)
        {
            //mostly concerned with CPTime diffs from above vs below.
            //if there is a large (40s) difference between row above and row above cf the row, then this is probably an improper parsing
            for(var i=0; i< CSVData.Count; i++)
            {
                if(i > 0 && i < CSVData.Count - 1)
                {
                    var row = CSVData[i];

                    var t1DiffAbove = Math.Abs(row.P1Time - CSVData[i - 1].P1Time);
                    var t1DiffBelow = Math.Abs(row.P1Time - CSVData[i + 1].P1Time);
                    if(t1DiffAbove > 35 && t1DiffBelow > 35)
                    {
                        row.P1Time = 0;
                    }
                    //repaet for p2, p3, p4
                    var t2DiffAbove = Math.Abs(row.P2Time - CSVData[i - 1].P2Time);
                    var t2DiffBelow = Math.Abs(row.P2Time - CSVData[i + 1].P2Time);
                    if (t2DiffAbove > 35 && t2DiffBelow > 35)
                    {
                        row.P2Time = 0;
                    }

                    var t3DiffAbove = Math.Abs(row.P3Time - CSVData[i - 1].P3Time);
                    var t3DiffBelow = Math.Abs(row.P3Time - CSVData[i + 1].P3Time);
                    if (t3DiffAbove > 35 && t3DiffBelow > 35)
                    {
                        row.P3Time = 0;
                    }

                    var t4DiffAbove = Math.Abs(row.P4Time - CSVData[i - 1].P4Time);
                    var t4DiffBelow = Math.Abs(row.P4Time - CSVData[i + 1].P4Time);
                    if (t4DiffAbove > 35 && t4DiffBelow > 35)
                    {
                        row.P4Time = 0;
                    }

                    //also consistency check -> are row above and below the same, but this row different?

                    if (CSVData[i - 1].P1Time == CSVData[i + 1].P1Time && CSVData[i - 1].P1Time != row.P1Time)
                    {
                        row.P1Time = CSVData[i - 1].P1Time;
                    }

                    if (CSVData[i - 1].P2Time == CSVData[i + 1].P2Time && CSVData[i - 1].P2Time != row.P2Time)
                    {
                        row.P2Time = CSVData[i - 1].P2Time;
                    }

                    if (CSVData[i - 1].P3Time == CSVData[i + 1].P3Time && CSVData[i - 1].P3Time != row.P3Time)
                    {
                        row.P3Time = CSVData[i - 1].P3Time;
                    }

                    if (CSVData[i - 1].P4Time == CSVData[i + 1].P4Time && CSVData[i - 1].P4Time != row.P4Time)
                    {
                        row.P4Time = CSVData[i - 1].P4Time;
                    }
                }
            }
        }

        private void CleanData_PlayerNames(List<RawCSVFile> CSVData)
        {
            //make sure player names are valid
            //and if 3/4 are filled in, fill the 4th in for the missing position

            var validTeamNames = StringCorrection.GetPossibleTeamNames(useGrandLeague);
            var validPlayerNames = StringCorrection.GetPossiblePlayerNames(useGrandLeague);
            var validTeams = StringCorrection.GetTMWTTeams(useGrandLeague);

            //generally should rely on 2/3 of teamname, playernames

            foreach(var row in CSVData)
            {
                if (!validPlayerNames.Contains(row.P1Name))
                {
                    row.P1Name = "";
                }
                if (!validPlayerNames.Contains(row.P2Name))
                {
                    row.P2Name = "";
                }
                if (!validPlayerNames.Contains(row.P3Name))
                {
                    row.P3Name = "";
                }
                if (!validPlayerNames.Contains(row.P4Name))
                {
                    row.P4Name = "";
                }
            }

            foreach (var row in CSVData)
            {
                //can skip rows where team1, team2, p1name, p2name, p3name, p4name are not null
                if (!string.IsNullOrEmpty(row.P1Name) && !string.IsNullOrEmpty(row.P2Name) && !string.IsNullOrEmpty(row.P3Name) && !string.IsNullOrEmpty(row.P4Name) &&
                    !string.IsNullOrEmpty(row.Team1) && !string.IsNullOrEmpty(row.Team2))
                {
                    continue;
                }

                //attempt to guess at teams
                TMWTTeam team1 = null;
                try { team1 = validTeams.Where(a => a.TeamName == row.Team1).First(); } catch { }
                TMWTTeam team2 = null;
                try { team2 = validTeams.Where(a => a.TeamName == row.Team2).First(); }catch { }

                var players = new List<string>() { row.P1Name, row.P2Name, row.P3Name, row.P4Name };

                if(team1 == null && team2 == null)
                {
                    var opts = validTeams.Where(a => a.TeamContains2OfPlayers(players)).ToList();
                    if(opts.Count > 0)
                    {
                        team1 = opts.First();
                    }
                    //set team2 later
                }

                //if one of these is null, can attempt to read from playernames
                if (team1 == null && team2 != null)
                {
                    var playersUsedInCheck = players.Where(a => a != team2.Players[0] && a != team2.Players[1]).ToList();

                    //see if there is a teammatch for remaining players
                    var opts = validTeams.Where(a => a.TeamContainsPlayers(new List<string>() { playersUsedInCheck[0], playersUsedInCheck[1] })).ToList();
                    if(opts.Count > 0)
                    {
                        team1 = opts.First();
                    }
                }

                if(team2 == null && team1 != null)
                {
                    var playersUsedInCheck = players.Where(a => a != team1.Players[0] && a != team1.Players[1]).ToList();

                    //see if there is a teammatch for remaining players
                    var opts = validTeams.Where(a => a.TeamContainsPlayers(new List<string>() { playersUsedInCheck[0], playersUsedInCheck[1] })).ToList();
                    if(opts.Count > 0)
                    {
                        team2 = opts.First();
                    }
                }

                if (string.IsNullOrEmpty(row.Team1) && team1 != null) { row.Team1 = team1.TeamName; }
                if (string.IsNullOrEmpty(row.Team2) && team2 != null) { row.Team2 = team2.TeamName; }

                //if both are not null, can probably continue safely
                //else, we are dealing with a situation where only 1 player is known and there is no teamname.
                if (team1 != null && team2 != null)
                {
                    //how many players are missing?
                    var missingCount = 0;
                    if (string.IsNullOrEmpty(row.P1Name)) { missingCount++; }
                    if (string.IsNullOrEmpty(row.P2Name)) { missingCount++; }
                    if (string.IsNullOrEmpty(row.P3Name)) { missingCount++; }
                    if (string.IsNullOrEmpty(row.P4Name)) { missingCount++; }

                    if(missingCount == 0)
                    {
                        continue; //only a teamname was missing
                    }

                    if(missingCount > 1)
                    {
                        //would be possible to guess using above/below rows but this is too risky, its the most important part of the data after all
                        continue;
                    }
                    var knownNames = new List<string>();
                    knownNames.AddRange(team1.Players);
                    knownNames.AddRange(team2.Players);
                    //else we are looking at filling in the one remaining name
                    if (string.IsNullOrEmpty(row.P1Name))
                    {
                        try
                        {

                            var EnteredNames = new List<string>() { row.P2Name, row.P3Name, row.P4Name };
                            row.P1Name = knownNames.Except(EnteredNames).First();
                        }
                        catch { }
                    }
                    if (string.IsNullOrEmpty(row.P2Name))
                    {
                        try 
                        { 
                            var EnteredNames = new List<string>() { row.P1Name, row.P3Name, row.P4Name };
                            row.P2Name = knownNames.Except(EnteredNames).First();
                        }
                        catch { }
                    }
                    if (string.IsNullOrEmpty(row.P3Name))
                    {
                        try 
                        { 
                                var EnteredNames = new List<string>() { row.P1Name, row.P2Name, row.P4Name };
                                row.P3Name = knownNames.Except(EnteredNames).First();
                        }
                        catch { }
                    }
                    if (string.IsNullOrEmpty(row.P4Name))
                    {
                        try
                        {
                            var EnteredNames = new List<string>() { row.P1Name, row.P2Name, row.P3Name };
                            row.P4Name = knownNames.Except(EnteredNames).First();
                        }
                        catch { }
                    }
                }
            }
        }

        private void CleanDataSweep1_TeamAndMapNames(List<RawCSVFile> CSVData)
        {
            //cleanup names of teams and maps. - use logic that mistakes are unlikely to happen twice
            //Might need to be careful with "Reps" and "BIG"
            
            var validTeamNames = StringCorrection.GetPossibleTeamNames(useGrandLeague);
            var validMapNames = StringCorrection.GetPossibleMaps();
            foreach(var row in CSVData)
            {
                //delete for now. we'll see how badly this plays out
                //might need a better correction method. but this at least means no mistakes are propagated
                if (!validTeamNames.Contains(row.Team1))
                {
                    row.Team1 = "";
                }
                if (!validTeamNames.Contains(row.Team2))
                {
                    row.Team2 = "";
                }
                if (!validMapNames.Contains(row.MapName))
                {
                    row.MapName = "";
                }
            }

            //step 1 - if a teamname is the same as either row above or row below and if not, delete it
            for(var i=0; i< CSVData.Count; i++)
            {
                if( i > 0 && i < CSVData.Count - 1)
                {
                    var row = CSVData[i];
                    //check if row above or row below have team1 set to "BIG", if not, set row.Team1 = ""
                    var rowAbove = CSVData[i - 1];
                    var rowBelow = CSVData[i + 1];

                    if(rowAbove.Team1 != row.Team1 && rowBelow.Team1 != row.Team1)
                    {
                        row.Team1 = "";
                    }
                    if (rowAbove.Team2 != row.Team2 && rowBelow.Team2 != row.Team2)
                    {
                        row.Team2 = "";
                    }
                    if(rowAbove.MapName != row.MapName && rowBelow.MapName != row.MapName)
                    {
                        row.MapName = "";
                    }
                }
            }


            //if row entry for these is blank, but row above and row below are identical, row becomes the same as those rows
            //if row entry for these is blank, and row above and row below are different, row becomes the same as the row below
            //do not do this for "Reps" or "BIG"
            //do this for "Team1" and "Team2" and "MapName"
            for (var i = 0; i < CSVData.Count; i++)
            {
                var row = CSVData[i];

                //reorder TeamNames for consistency
                if(!string.IsNullOrEmpty(row.Team1) && !string.IsNullOrEmpty(row.Team2))
                {
                    if (row.Team1 == row.Team2)
                    {
                        row.Team2 = "";
                    }
                    if (row.Team1.Length < row.Team2.Length)
                    {
                        var temp = row.Team1;
                        row.Team1 = row.Team2;
                        row.Team2 = temp;
                    }
                }

                if (row.Team1 == "")
                {
                    if (i > 0 && i < CSVData.Count - 1)
                    {
                        var j = i - 1;
                        var k = i + 1;
                        //keep going above and below until we hit a row where Team1 is not null
                        while (CSVData[j].Team1 == "" && j > 0)
                        {
                            j--;
                        }
                        while (CSVData[k].Team1 == "" && k < CSVData.Count - 1)
                        {
                            k++;
                        }
                        var rowAbove = CSVData[j];
                        var rowBelow = CSVData[k];
                        if (rowAbove.Team1 == rowBelow.Team1 )
                        {
                            row.Team1 = rowAbove.Team1;
                        }
                    }
                }
                if (row.Team2 == "")
                {
                    if (i > 0 && i < CSVData.Count - 1)
                    {
                        var j = i - 1;
                        var k = i + 1;
                        //keep going above and below until we hit a row where Team1 is not null
                        while (CSVData[j].Team2 == "" && j > 0)
                        {
                            j--;
                        }
                        while (CSVData[k].Team2 == "" && k < CSVData.Count - 1)
                        {
                            k++;
                        }
                        var rowAbove = CSVData[j];
                        var rowBelow = CSVData[k];
                        if (rowAbove.Team2 == rowBelow.Team2)
                        {
                            row.Team2 = rowAbove.Team2;
                        }
                    }
                }
                if (row.MapName == "")
                {
                    if (i > 0 && i < CSVData.Count - 1)
                    {
                        var j = i - 1;
                        var k = i + 1;
                        //keep going above and below until we hit a row where Team1 is not null
                        while (CSVData[j].MapName == "" && j > 0)
                        {
                            j--;
                        }
                        while (CSVData[k].MapName == "" && k < CSVData.Count - 1)
                        {
                            k++;
                        }
                        var rowAbove = CSVData[j];
                        var rowBelow = CSVData[k];
                        if (rowAbove.MapName == rowBelow.MapName )
                        {
                            row.MapName = rowAbove.MapName;
                        }
                    }
                }
            }

            //now look at any mismatches.
            //here is the logic: while we expect mismatches at map and match borders, there should be some consistency. Lets take a look at the rows 10 above and 10 below the row.
            //If both the rows above and below agree that 8/10 entries
            //are the same, then that row should match those
            //this needs to happen a few times, say 6
            for (var m = 0; m < 6; m++)
            {
                var CountRequiredToMatch = 10 - m;

                for (var i = 0; i < CSVData.Count; i++)
                {
                    if (i > 10 && i < CSVData.Count - 11)
                    {
                        var row = CSVData[i];
                        //get 10 rows above
                        var rowsAbove = CSVData.Skip(i - 11).Take(10).ToArray();
                        var rowsBelow = CSVData.Skip(i + 1).Take(10).ToArray();

                        //get most common item in rowAboveTeam1
                        var rowAboveTeam1Group = rowsAbove.Select(a => a.Team1).ToList().GroupBy(v => v);
                        var team1AboveMaxCount = rowAboveTeam1Group.Max(g => g.Count());
                        if (team1AboveMaxCount >= CountRequiredToMatch)
                        {
                            var rowAboveTeam1Candidate = rowAboveTeam1Group.Where(x => x.Count() == team1AboveMaxCount).Select(x => x.Key).ToArray().First();

                            var rowsBelowTeam1Group = rowsBelow.Select(a => a.Team1).ToList().GroupBy(v => v);
                            var team1BelowMaxCount = rowsBelowTeam1Group.Max(g => g.Count());
                            if (team1BelowMaxCount >= CountRequiredToMatch)
                            {
                                var rowBelowTeam1Candidate = rowsBelowTeam1Group.Where(x => x.Count() == team1BelowMaxCount).Select(x => x.Key).ToArray().First();

                                //important check now=> are these candidates the same?
                                if (rowAboveTeam1Candidate == rowBelowTeam1Candidate)
                                {
                                    //then this row should also match - there are at least 10-m/10 string matches above and below that agree with it
                                    row.Team1 = rowAboveTeam1Candidate;
                                }
                            }
                        }

                        //now repeat for Team2
                        var rowAboveTeam2Group = rowsAbove.Select(a => a.Team2).ToList().GroupBy(v => v);
                        var team2AboveMaxCount = rowAboveTeam2Group.Max(g => g.Count());
                        if (team2AboveMaxCount >= CountRequiredToMatch)
                        {
                            var rowAboveTeam2Candidate = rowAboveTeam2Group.Where(x => x.Count() == team2AboveMaxCount).Select(x => x.Key).ToArray().First();

                            var rowsBelowTeam2Group = rowsBelow.Select(a => a.Team2).ToList().GroupBy(v => v);
                            var team2BelowMaxCount = rowsBelowTeam2Group.Max(g => g.Count());
                            if (team2BelowMaxCount >= CountRequiredToMatch)
                            {
                                var rowBelowTeam2Candidate = rowsBelowTeam2Group.Where(x => x.Count() == team2BelowMaxCount).Select(x => x.Key).ToArray().First();

                                //important check now=> are these candidates the same?
                                if (rowAboveTeam2Candidate == rowBelowTeam2Candidate)
                                {
                                    //then this row should also match - there are at least 10-m/10 string matches above and below that agree with it
                                    row.Team2 = rowAboveTeam2Candidate;
                                }
                            }
                        }

                        //now repeat for mapName
                        var rowAbovemapNameGroup = rowsAbove.Select(a => a.MapName).ToList().GroupBy(v => v);
                        var mapNameAboveMaxCount = rowAbovemapNameGroup.Max(g => g.Count());
                        if (mapNameAboveMaxCount >= CountRequiredToMatch)
                        {
                            var rowAbovemapNameCandidate = rowAbovemapNameGroup.Where(x => x.Count() == mapNameAboveMaxCount).Select(x => x.Key).ToArray().First();

                            var rowsBelowmapNameGroup = rowsBelow.Select(a => a.MapName).ToList().GroupBy(v => v);
                            var mapNameBelowMaxCount = rowsBelowmapNameGroup.Max(g => g.Count());
                            if (mapNameBelowMaxCount >= CountRequiredToMatch)
                            {
                                var rowBelowmapNameCandidate = rowsBelowmapNameGroup.Where(x => x.Count() == mapNameBelowMaxCount).Select(x => x.Key).ToArray().First();

                                //important check now=> are these candidates the same?
                                if (rowAbovemapNameCandidate == rowBelowmapNameCandidate)
                                {
                                    //then this row should also match - there are at least 10-m/10 string matches above and below that agree with it
                                    row.MapName = rowAbovemapNameCandidate;
                                }
                            }
                        }


                        if (!string.IsNullOrEmpty(row.Team1) && !string.IsNullOrEmpty(row.Team2))
                        {
                            if (row.Team1 == row.Team2)
                            {
                                row.Team2 = "";
                            }
                            if (row.Team1.Length < row.Team2.Length)
                            {
                                var temp = row.Team1;
                                row.Team1 = row.Team2;
                                row.Team2 = temp;
                            }
                        }
                    }
                }
            }
        }

        private void StatusTextBox_TextChanged(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (bgWorker.IsBusy)
            {
                bgWorker.CancelAsync();
                StatusTextBox.Text = "Requesting extract stop";
            }
        }

        private void label13_Click(object sender, EventArgs e)
        {

        }
    }
}
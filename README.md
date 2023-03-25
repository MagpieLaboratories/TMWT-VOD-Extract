# TMWT-Stream-Extract

WFA that uses screenshots + Tesseract OCR to read data from the overlay and then can later parse data into a more readable format.

Running this will either require a bit of technical knowledge and/or patience.

The code is rough and ready but the extract part should be fairly straightforward to understand. In total I never had the time to properly finish this and am still unsatisfied but its more useful to release this in its current state than let it rot.

A deprecated feature (was taking up too much disk space) was getting the Extract to save every single screenshot taken, incase re-processing the raw input was needed. The code for this is still around. Enabling this (or at least saving a few screenshots directly from code using a breakpoint) should be helpful for testing and improving the app. If you have copious disk space, you might want to just save everything.

# General setup recommendations
If you're going to be running this a bit, I highly recommend editing the default UI settings in code. 

This means opening up the project in Visual studio, going to "Form1.cs[design]", and replacing the text in the relevant textboxes (mainly working folder).

You might also want to make edits to how the output is handled. The output is currently mainly focused at CP-times which I was interested in.

There is a good chance users may want to just copy/paste a large chunk of some methods and build a new codebase around that rather than dealing with the quite specific and rushed code here.


# Specific setup
Particular attention should be drawn to the files StringCorrection.cs and MapInfo.json

As part of improving the raw output these are heavily used. They are both currently manually set to work for Grand League and Challenger league from stage 1.

This means the extract will not run well on any non-stage 1 maps (including stage 1 E maps) and will not detect non-GL and non-CL players.

Improving this to work for a more general use case is mostly just a matter of taking the time to write up the lookup rules and time suggestions for the new maps and names for teams/players.

I would recommend then integrating this into a dropdown option in the WFA so constant code edits aren't required


# Known issues/ limitations/ bugs (a lot of these can probably be fixed with better logic)
- (Biggest one) - this was written to work for the standard season. The post-processing logic only expects to see each team play each map once.
- This is annoying for playoffs and will break if used with no manual intervention. I recommend going directly into the raw output CSV and splitting it into multiple parts, where each part is a different match. This will create multiple map outputs but combing these isn't so bad.
- As mentioned above, performance is very poor if you try to read non-TMGL/TMCL players/teams or any maps that arent in MapInfo.json (including easy versions) since this guides the post-procesisng
- Because of how CP times update, the tool regularly confuses draws during overtakes. **This is particularly important to manually check for later CPs - actual ties are rare**
- Similarly, massive mistakes sometimes take a while to update since the time delta wont change until a CP is crossed again. Fixing this would take some more advanced logic.
- There is no defined logic for detecting a DNF. Trying to read the "flag" proved beyond the basic OCR engine.
- The tool does not reliably trace exact CP numbers, and this can cause confusion over when rounds end sometimes. I do not recommend putting a hard cap or check on total CP count, since this can mean data is failed to capture - it's usually just easier to quickly review later than try to re-run.
- Mis-read of a players name sometimes occurs(usually very obvious an easy to fix since it goes sequentially)
- a slip in the expected CP number/time (usually means shifting all players back down - I did this quickly in google sheets) -> this usually takes 2 basic forms. First is when CP times are missing/ skipped and then shoved right onto the end of the CSV for that row. Generally deleting these empty cells is enough, but if a misread caused the CP time to go up and down again, the misread time might have to be identified and deleted. The other basic form is when a round is assumed over before the actual CP, but then the CPs continue to be read "from the next round" - in which case, these 2+ rows should be combined into one. Again, this should be easy to spot in any kind of editor.
- The "Replay" feature in VOD confuses the extract, as it assumes the replays shown at the end are some weird continuation of the final round. For Wirtual VODs this means the final round CP times after the finish need to just be deleted. It's definitely possible to get OCR to recognise the text "replay" in that part of the stream and cut out this annoying step.
- The map score was surprisingly difficult to get tesseract to read. Since it ultimately is not that important for my use case, I dropped this issue. I expect reading it properly means taking the exact rectangle dimensions for both team scores and then feeding only that into the OCR engine. It might also require a custom grey-scale function. Alternatively (a dumber solution but probably a smarter one for its efficacy) - just check for the 100% white pixel value at certain set points on the screen. There should be pixel combinations which are only ever all white for each number from 0->10 . I recommend this website for that: https://pixspy.com/
- Same applies to Track # and round #. Luckily, Track # is much easier to track through the match score (+1 on total match points for both teams). Round # is tracked by iterating from the next "00:00.00" checkpoint in post-processing.
- The "Match Score" is expecting the match to be a bo7 and likely is confused by a bo5.
- Running constant screenshots, file updates and OCR takes some amount of computing power. **I really don't know what the limits are - but OCR is not "fast"** (typical runtime varies on size of input but you can expect this to take a significant fraction of a second), but if you have a low-power machine, be aware that screenshot intervals might be longer - increasing the chance of the tool missing CP data. It is tempting to add a lot of checks and additional screenshots + OCR engine requests (like the one to check for "Replay" text) but be aware that doing this will cut into processing time. It can of course be counterbalanced by having the tool read the VOD when the VOD is played at a reduced speed (easy to achieve on youtube for example).

A general caution to any attempts at improving CP-read logic to try and improve on the mistakes made by the extract tool: I gave up on this quite quickly as the number of edge cases was very large. Although manually checking the post-processed data is boring, its certainly faster than recording all CPs during a match. You will have to decide how important accuracy is and how much time you're willing to sacrifice either after each match or in trying to improve the logic enough to avoid doing that.


# Reading raw from a TMWT Stream
- Clone code and build
- Run the exe or run from Visual Studio (or similar)
- Input important settings: mostly "Working Folder" which is taken as the base for file dumping, and "Extraction Name" as well as "Grand League"
- Click "Run Extract"
- Fullscreen the VOD (TMWT video) on the main monitor
- You should now see the application reading the data from the stream. The data is automatically appended to a csv in the following path: [WORKING FOLDER]/ParsedData/[EXTRACTION NAME]/[TEMP IMG BASE NAME]_raw.CSV
- For example: C:\Users\ABC\TMWT_Data\ParsedData\Grand League Day 3\Screenshot_raw.csv

Once read, the data is just a collection of parsed reads from the stream and not really "usable" yet in terms of workable information

The CSV has the following headers as raw data-read outputs:
Team1, Team2, Team1MatchScore, Team2MatchScore, Team1MapScore, Team2MapScore, MapName, mapNo, RoundNo, CPNo, P1Name, P2Name, P3Name, P4Name, P1Time, P2Time, P3Time, P4Time, P1Raw, P2Raw, P3Raw, P4Raw 


# Post-Processing
To get more sensible output, post processing will need to be run ("Run post-processing")

This looks for relevant CSV files in the folder Extraction name

For example, if you just ran an extract called "Grand league day 3" it would look in that folder for any raw csv files which it can process.

The output of this step is broken down on a per-map basis so the CSV will vary depending on the settings for that map.

For example (Aeropipes):

Exalty, Link, 9.405, 17.843, 19.455, 28.174, , 40.868, 45.208, 49.848, , 58.385, 65.075, 84.698, , , 1, 3

Notice how a few CPs are missed - this is not unusual

# Using Post-processing data
Really anything can be done at this point. I usually find some level of cleanup is required, the output being CSV should be easy to work in for importing to excel, google sheets or any future resources.

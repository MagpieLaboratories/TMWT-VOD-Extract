namespace TMWT_Stream_Extract
{
    partial class TMWTExtract
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.InitiateExtractButton = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.CurrentMatchTextBox = new System.Windows.Forms.TextBox();
            this.Team1TextBox = new System.Windows.Forms.TextBox();
            this.Team2TextBox = new System.Windows.Forms.TextBox();
            this.MapTextBox = new System.Windows.Forms.TextBox();
            this.MapScoreTextBox = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.MatchScoreTextBox = new System.Windows.Forms.TextBox();
            this.CurrentRoundTextBox = new System.Windows.Forms.TextBox();
            this.ImgNameTextBox = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.ScreenshotIntervalTextBox = new System.Windows.Forms.TextBox();
            this.StatusTextBox = new System.Windows.Forms.TextBox();
            this.StatusLabel = new System.Windows.Forms.Label();
            this.FolderTextBox = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.label12 = new System.Windows.Forms.Label();
            this.ExtractionTextBox = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.grandLeague = new System.Windows.Forms.CheckBox();
            this.button2 = new System.Windows.Forms.Button();
            this.ErrorTextBox = new System.Windows.Forms.TextBox();
            this.Errors = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(180, 21);
            this.label1.TabIndex = 0;
            this.label1.Text = "TMWT Stream Extraction";
            // 
            // InitiateExtractButton
            // 
            this.InitiateExtractButton.Location = new System.Drawing.Point(12, 34);
            this.InitiateExtractButton.Name = "InitiateExtractButton";
            this.InitiateExtractButton.Size = new System.Drawing.Size(173, 58);
            this.InitiateExtractButton.TabIndex = 1;
            this.InitiateExtractButton.Text = "Run Extract";
            this.InitiateExtractButton.UseVisualStyleBackColor = true;
            this.InitiateExtractButton.Click += new System.EventHandler(this.Run_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(19, 182);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(84, 15);
            this.label2.TabIndex = 2;
            this.label2.Text = "Current Match";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(132, 182);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(44, 15);
            this.label3.TabIndex = 3;
            this.label3.Text = "Team 1";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(230, 182);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(44, 15);
            this.label4.TabIndex = 4;
            this.label4.Text = "Team 2";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(319, 182);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(74, 15);
            this.label5.TabIndex = 5;
            this.label5.Text = "Current Map";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(452, 182);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(106, 15);
            this.label6.TabIndex = 6;
            this.label6.Text = "Current Map Score";
            // 
            // CurrentMatchTextBox
            // 
            this.CurrentMatchTextBox.Location = new System.Drawing.Point(19, 200);
            this.CurrentMatchTextBox.Name = "CurrentMatchTextBox";
            this.CurrentMatchTextBox.Size = new System.Drawing.Size(92, 23);
            this.CurrentMatchTextBox.TabIndex = 7;
            this.CurrentMatchTextBox.Text = "--";
            // 
            // Team1TextBox
            // 
            this.Team1TextBox.Location = new System.Drawing.Point(132, 200);
            this.Team1TextBox.Name = "Team1TextBox";
            this.Team1TextBox.Size = new System.Drawing.Size(92, 23);
            this.Team1TextBox.TabIndex = 8;
            this.Team1TextBox.Text = "--";
            // 
            // Team2TextBox
            // 
            this.Team2TextBox.Location = new System.Drawing.Point(230, 200);
            this.Team2TextBox.Name = "Team2TextBox";
            this.Team2TextBox.Size = new System.Drawing.Size(83, 23);
            this.Team2TextBox.TabIndex = 9;
            this.Team2TextBox.Text = "--";
            // 
            // MapTextBox
            // 
            this.MapTextBox.Location = new System.Drawing.Point(319, 200);
            this.MapTextBox.Name = "MapTextBox";
            this.MapTextBox.Size = new System.Drawing.Size(116, 23);
            this.MapTextBox.TabIndex = 10;
            this.MapTextBox.Text = "0";
            // 
            // MapScoreTextBox
            // 
            this.MapScoreTextBox.Location = new System.Drawing.Point(452, 200);
            this.MapScoreTextBox.Name = "MapScoreTextBox";
            this.MapScoreTextBox.Size = new System.Drawing.Size(80, 23);
            this.MapScoreTextBox.TabIndex = 11;
            this.MapScoreTextBox.Text = "0-0";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(606, 182);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(116, 15);
            this.label7.TabIndex = 12;
            this.label7.Text = "Current Match Score";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(16, 255);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(104, 15);
            this.label8.TabIndex = 13;
            this.label8.Text = "Current Round LB:";
            // 
            // MatchScoreTextBox
            // 
            this.MatchScoreTextBox.Location = new System.Drawing.Point(606, 200);
            this.MatchScoreTextBox.Name = "MatchScoreTextBox";
            this.MatchScoreTextBox.Size = new System.Drawing.Size(116, 23);
            this.MatchScoreTextBox.TabIndex = 14;
            this.MatchScoreTextBox.Text = "0-0";
            // 
            // CurrentRoundTextBox
            // 
            this.CurrentRoundTextBox.Location = new System.Drawing.Point(19, 283);
            this.CurrentRoundTextBox.Multiline = true;
            this.CurrentRoundTextBox.Name = "CurrentRoundTextBox";
            this.CurrentRoundTextBox.Size = new System.Drawing.Size(195, 155);
            this.CurrentRoundTextBox.TabIndex = 15;
            // 
            // ImgNameTextBox
            // 
            this.ImgNameTextBox.Location = new System.Drawing.Point(243, 40);
            this.ImgNameTextBox.Name = "ImgNameTextBox";
            this.ImgNameTextBox.Size = new System.Drawing.Size(129, 23);
            this.ImgNameTextBox.TabIndex = 16;
            this.ImgNameTextBox.Text = "ScreenShot";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(243, 14);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(122, 15);
            this.label9.TabIndex = 17;
            this.label9.Text = "Temp Img Base Name";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(399, 15);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(110, 15);
            this.label10.TabIndex = 18;
            this.label10.Text = "Screenshot Interval:";
            // 
            // ScreenshotIntervalTextBox
            // 
            this.ScreenshotIntervalTextBox.Location = new System.Drawing.Point(399, 40);
            this.ScreenshotIntervalTextBox.Name = "ScreenshotIntervalTextBox";
            this.ScreenshotIntervalTextBox.Size = new System.Drawing.Size(104, 23);
            this.ScreenshotIntervalTextBox.TabIndex = 19;
            this.ScreenshotIntervalTextBox.Text = "1";
            // 
            // StatusTextBox
            // 
            this.StatusTextBox.Location = new System.Drawing.Point(12, 145);
            this.StatusTextBox.Multiline = true;
            this.StatusTextBox.Name = "StatusTextBox";
            this.StatusTextBox.ReadOnly = true;
            this.StatusTextBox.Size = new System.Drawing.Size(202, 34);
            this.StatusTextBox.TabIndex = 20;
            this.StatusTextBox.TextChanged += new System.EventHandler(this.StatusTextBox_TextChanged);
            // 
            // StatusLabel
            // 
            this.StatusLabel.AutoSize = true;
            this.StatusLabel.Location = new System.Drawing.Point(19, 127);
            this.StatusLabel.Name = "StatusLabel";
            this.StatusLabel.Size = new System.Drawing.Size(42, 15);
            this.StatusLabel.TabIndex = 21;
            this.StatusLabel.Text = "Status:";
            // 
            // FolderTextBox
            // 
            this.FolderTextBox.Location = new System.Drawing.Point(529, 40);
            this.FolderTextBox.Name = "FolderTextBox";
            this.FolderTextBox.Size = new System.Drawing.Size(184, 23);
            this.FolderTextBox.TabIndex = 22;
            this.FolderTextBox.Text = "E:\\TMWT_Extract";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(529, 15);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(88, 15);
            this.label11.TabIndex = 23;
            this.label11.Text = "Working Folder";
            this.label11.Click += new System.EventHandler(this.label11_Click);
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(243, 83);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(95, 15);
            this.label12.TabIndex = 24;
            this.label12.Text = "Extraction Name";
            // 
            // ExtractionTextBox
            // 
            this.ExtractionTextBox.Location = new System.Drawing.Point(243, 111);
            this.ExtractionTextBox.Name = "ExtractionTextBox";
            this.ExtractionTextBox.Size = new System.Drawing.Size(108, 23);
            this.ExtractionTextBox.TabIndex = 25;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(532, 91);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(186, 60);
            this.button1.TabIndex = 26;
            this.button1.Text = "Run post-processing";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click_1);
            // 
            // grandLeague
            // 
            this.grandLeague.AutoSize = true;
            this.grandLeague.Checked = true;
            this.grandLeague.CheckState = System.Windows.Forms.CheckState.Indeterminate;
            this.grandLeague.Location = new System.Drawing.Point(402, 111);
            this.grandLeague.Name = "grandLeague";
            this.grandLeague.Size = new System.Drawing.Size(99, 19);
            this.grandLeague.TabIndex = 27;
            this.grandLeague.Text = "Grand League";
            this.grandLeague.ThreeState = true;
            this.grandLeague.UseVisualStyleBackColor = true;
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(12, 98);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(176, 26);
            this.button2.TabIndex = 28;
            this.button2.Text = "Stop";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // ErrorTextBox
            // 
            this.ErrorTextBox.Location = new System.Drawing.Point(272, 281);
            this.ErrorTextBox.Multiline = true;
            this.ErrorTextBox.Name = "ErrorTextBox";
            this.ErrorTextBox.Size = new System.Drawing.Size(469, 151);
            this.ErrorTextBox.TabIndex = 29;
            // 
            // Errors
            // 
            this.Errors.AutoSize = true;
            this.Errors.Location = new System.Drawing.Point(272, 250);
            this.Errors.Name = "Errors";
            this.Errors.Size = new System.Drawing.Size(48, 15);
            this.Errors.TabIndex = 30;
            this.Errors.Text = "Error(s):";
            this.Errors.Click += new System.EventHandler(this.label13_Click);
            // 
            // TMWTExtract
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.Errors);
            this.Controls.Add(this.ErrorTextBox);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.grandLeague);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.ExtractionTextBox);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.FolderTextBox);
            this.Controls.Add(this.StatusLabel);
            this.Controls.Add(this.StatusTextBox);
            this.Controls.Add(this.ScreenshotIntervalTextBox);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.ImgNameTextBox);
            this.Controls.Add(this.CurrentRoundTextBox);
            this.Controls.Add(this.MatchScoreTextBox);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.MapScoreTextBox);
            this.Controls.Add(this.MapTextBox);
            this.Controls.Add(this.Team2TextBox);
            this.Controls.Add(this.Team1TextBox);
            this.Controls.Add(this.CurrentMatchTextBox);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.InitiateExtractButton);
            this.Controls.Add(this.label1);
            this.Name = "TMWTExtract";
            this.Text = "TMWT Extract";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Label label1;
        private Button InitiateExtractButton;
        private Label label2;
        private Label label3;
        private Label label4;
        private Label label5;
        private Label label6;
        private TextBox CurrentMatchTextBox;
        private TextBox Team1TextBox;
        private TextBox Team2TextBox;
        private TextBox MapTextBox;
        private TextBox MapScoreTextBox;
        private Label label7;
        private Label label8;
        private TextBox MatchScoreTextBox;
        private TextBox CurrentRoundTextBox;
        private TextBox ImgNameTextBox;
        private Label label9;
        private Label label10;
        private TextBox ScreenshotIntervalTextBox;
        private TextBox StatusTextBox;
        private Label StatusLabel;
        private TextBox FolderTextBox;
        private Label label11;
        private FolderBrowserDialog folderBrowserDialog1;
        private Label label12;
        private TextBox ExtractionTextBox;
        private Button button1;
        private CheckBox grandLeague;
        private Button button2;
        private TextBox ErrorTextBox;
        private Label Errors;
    }
}
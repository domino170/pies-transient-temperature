namespace PIESTransientTemperature
{
    partial class PIES
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.Run = new System.Windows.Forms.Button();
            this.ReadData = new System.Windows.Forms.Button();
            this.OutputWindow = new System.Windows.Forms.TextBox();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.calculationProgressBar = new System.Windows.Forms.ProgressBar();
            this.calculationTimer = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.label4 = new System.Windows.Forms.Label();
            this.backgroundWorker = new System.ComponentModel.BackgroundWorker();
            this.Stop = new System.Windows.Forms.Button();
            this.shapePictureBox = new System.Windows.Forms.PictureBox();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.ImputDataPage = new System.Windows.Forms.TabPage();
            this.OutputPage = new System.Windows.Forms.TabPage();
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.shapePictureBox)).BeginInit();
            this.tabControl1.SuspendLayout();
            this.ImputDataPage.SuspendLayout();
            this.OutputPage.SuspendLayout();
            this.SuspendLayout();
            // 
            // Run
            // 
            this.Run.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.Run.Location = new System.Drawing.Point(177, 471);
            this.Run.Name = "Run";
            this.Run.Size = new System.Drawing.Size(102, 33);
            this.Run.TabIndex = 0;
            this.Run.Text = "Start";
            this.Run.UseVisualStyleBackColor = true;
            this.Run.Click += new System.EventHandler(this.Run_Click);
            // 
            // ReadData
            // 
            this.ReadData.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.ReadData.Location = new System.Drawing.Point(23, 471);
            this.ReadData.Name = "ReadData";
            this.ReadData.Size = new System.Drawing.Size(118, 33);
            this.ReadData.TabIndex = 1;
            this.ReadData.Text = "Read data";
            this.ReadData.UseVisualStyleBackColor = true;
            this.ReadData.Click += new System.EventHandler(this.WczytajDane_Click);
            // 
            // OutputWindow
            // 
            this.OutputWindow.Location = new System.Drawing.Point(6, 6);
            this.OutputWindow.Multiline = true;
            this.OutputWindow.Name = "OutputWindow";
            this.OutputWindow.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.OutputWindow.Size = new System.Drawing.Size(963, 415);
            this.OutputWindow.TabIndex = 2;
            // 
            // openFileDialog
            // 
            this.openFileDialog.FileName = "Dane wejściowe";
            this.openFileDialog.Filter = "XML files (*.xml)|*.xml";
            this.openFileDialog.RestoreDirectory = true;
            this.openFileDialog.FileOk += new System.ComponentModel.CancelEventHandler(this.openFileDialog_FileOk);
            // 
            // calculationProgressBar
            // 
            this.calculationProgressBar.Location = new System.Drawing.Point(72, 4);
            this.calculationProgressBar.Name = "calculationProgressBar";
            this.calculationProgressBar.Size = new System.Drawing.Size(445, 18);
            this.calculationProgressBar.TabIndex = 5;
            // 
            // calculationTimer
            // 
            this.calculationTimer.AutoSize = true;
            this.calculationTimer.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.calculationTimer.Location = new System.Drawing.Point(523, 5);
            this.calculationTimer.Name = "calculationTimer";
            this.calculationTimer.Size = new System.Drawing.Size(92, 17);
            this.calculationTimer.TabIndex = 6;
            this.calculationTimer.Text = "00:00:00.000";
            // 
            // panel2
            // 
            this.panel2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel2.Controls.Add(this.label4);
            this.panel2.Controls.Add(this.calculationProgressBar);
            this.panel2.Controls.Add(this.calculationTimer);
            this.panel2.Location = new System.Drawing.Point(386, 473);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(620, 31);
            this.panel2.TabIndex = 7;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.label4.Location = new System.Drawing.Point(3, 5);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(63, 16);
            this.label4.TabIndex = 7;
            this.label4.Text = "Progress";
            // 
            // backgroundWorker
            // 
            this.backgroundWorker.WorkerReportsProgress = true;
            this.backgroundWorker.WorkerSupportsCancellation = true;
            this.backgroundWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorker_DoWork);
            this.backgroundWorker.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.backgroundWorker_ProgressChanged);
            this.backgroundWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorker_RunWorkerCompleted);
            // 
            // Stop
            // 
            this.Stop.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.Stop.Location = new System.Drawing.Point(285, 471);
            this.Stop.Name = "Stop";
            this.Stop.Size = new System.Drawing.Size(95, 33);
            this.Stop.TabIndex = 8;
            this.Stop.Text = "Stop";
            this.Stop.UseVisualStyleBackColor = true;
            this.Stop.Click += new System.EventHandler(this.Stop_Click);
            // 
            // shapePictureBox
            // 
            this.shapePictureBox.BackColor = System.Drawing.Color.White;
            this.shapePictureBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.shapePictureBox.Location = new System.Drawing.Point(3, 3);
            this.shapePictureBox.Name = "shapePictureBox";
            this.shapePictureBox.Size = new System.Drawing.Size(420, 420);
            this.shapePictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.shapePictureBox.TabIndex = 10;
            this.shapePictureBox.TabStop = false;
            this.shapePictureBox.WaitOnLoad = true;
            this.shapePictureBox.Paint += new System.Windows.Forms.PaintEventHandler(this.shapePictureBox_Paint);
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.ImputDataPage);
            this.tabControl1.Controls.Add(this.OutputPage);
            this.tabControl1.Location = new System.Drawing.Point(23, 12);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(983, 453);
            this.tabControl1.TabIndex = 11;
            // 
            // ImputDataPage
            // 
            this.ImputDataPage.BackColor = System.Drawing.Color.WhiteSmoke;
            this.ImputDataPage.Controls.Add(this.shapePictureBox);
            this.ImputDataPage.Location = new System.Drawing.Point(4, 22);
            this.ImputDataPage.Name = "ImputDataPage";
            this.ImputDataPage.Padding = new System.Windows.Forms.Padding(3);
            this.ImputDataPage.Size = new System.Drawing.Size(975, 427);
            this.ImputDataPage.TabIndex = 0;
            this.ImputDataPage.Text = "Model";
            // 
            // OutputPage
            // 
            this.OutputPage.BackColor = System.Drawing.Color.WhiteSmoke;
            this.OutputPage.Controls.Add(this.OutputWindow);
            this.OutputPage.Location = new System.Drawing.Point(4, 22);
            this.OutputPage.Name = "OutputPage";
            this.OutputPage.Padding = new System.Windows.Forms.Padding(3);
            this.OutputPage.Size = new System.Drawing.Size(975, 427);
            this.OutputPage.TabIndex = 1;
            this.OutputPage.Text = "Result";
            // 
            // PIES
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(1030, 516);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.Stop);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.ReadData);
            this.Controls.Add(this.Run);
            this.MaximumSize = new System.Drawing.Size(1046, 555);
            this.MinimumSize = new System.Drawing.Size(1046, 555);
            this.Name = "PIES";
            this.Text = "PIES";
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.shapePictureBox)).EndInit();
            this.tabControl1.ResumeLayout(false);
            this.ImputDataPage.ResumeLayout(false);
            this.OutputPage.ResumeLayout(false);
            this.OutputPage.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button Run;
        private System.Windows.Forms.Button ReadData;
        private System.Windows.Forms.TextBox OutputWindow;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private System.Windows.Forms.ProgressBar calculationProgressBar;
        private System.Windows.Forms.Label calculationTimer;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label label4;
        private System.ComponentModel.BackgroundWorker backgroundWorker;
        private System.Windows.Forms.Button Stop;
        private System.Windows.Forms.PictureBox shapePictureBox;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage ImputDataPage;
        private System.Windows.Forms.TabPage OutputPage;
    }
}


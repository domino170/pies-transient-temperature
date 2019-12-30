using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using PIESTransientTemperature.BoundaryConditions;
using PIESTransientTemperature.Objects;
using PIESTransientTemperature.MatrixCalculations;
using PIESTransientTemperature.ShapeFunctions;
using PIESTransientTemperature.ApproximationSeries;
using System.Xml.Linq;
using PIES_TransientTemperature;
using System.Diagnostics;
using PIES_TransientTemperature.Objects;

namespace PIESTransientTemperature
{
    public partial class PIES : Form
    {
        private Problem Problem { get; set; }
        private Stopwatch stopwatch { get; set; }
        private Timer timer { get; set; }
        private Modeling modeling { get; set; }

        public PIES()
        {
            InitializeComponent();
            InitializeTimers();
            Stop.Enabled = false;
        }

        public void Run_Click(object sender, EventArgs e)
        {
            StartTimers();
            OutputWindow.Text = String.Empty;
            Run.Enabled = false;
            ReadData.Enabled = false;
            Stop.Enabled = true;

            if (backgroundWorker.IsBusy != true)
            {
                backgroundWorker.RunWorkerAsync();
            }
        }

        private void Stop_Click(object sender, EventArgs e)
        {
            backgroundWorker.CancelAsync();
            Stop.Enabled = false;
        }

        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            var result = String.Empty;

            if (worker.CancellationPending == true)
            {
                e.Cancel = true;
            }
            else
            {
                var matrixCalculations = new Calculations(Problem);

                for (var iteration = 1; iteration < Problem.IterationProcess.NumberOfIterations + 1; iteration++)
                {
                    Problem.IterationProcess.CalculateCurrentTime(iteration);

                    foreach (var area in this.Problem.Areas)
                    {
                        area.configurationData.iterationProcess = Problem.IterationProcess;
                    }

                    this.Problem.Areas = matrixCalculations.CalculateIteration();

                    //result += matrixCalculations.PrintInitialConditionVector();

                    result += Problem.PrintOutputResults();

                    if (iteration != Problem.IterationProcess.NumberOfIterations)
                    {
                        Problem.CalculateSurfaceNewTemperature();

                        foreach (var area in this.Problem.Areas)
                        {
                            area.CalculateBoundaryTemperature();
                        }
                    }

                    worker.ReportProgress(iteration);

                    if (worker.CancellationPending)
                    {
                        break;
                    }
                }
            }
            e.Result = result;
        }

        private void backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            calculationProgressBar.Value = e.ProgressPercentage;
        }

        private void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            stopwatch.Stop();
            OutputWindow.Text += e.Result;
            Run.Enabled = true;
            Stop.Enabled = false;
            ReadData.Enabled = true;
        }

        private void WczytajDane_Click(object sender, EventArgs e)
        {
            this.openFileDialog.ShowDialog();
        }

        private void openFileDialog_FileOk(object sender, CancelEventArgs e)
        {
            this.OutputWindow.Text = String.Empty;
            try
            {
                var fileName = openFileDialog.FileName.ToString();
                var xml = XDocument.Load(fileName);
                var iterationProcess = XMLInputDataMapper.MapIterationProcess(xml);
                var areas = XMLInputDataMapper.MapAreas(xml, iterationProcess);
                this.Problem = new Problem(areas, iterationProcess);
                this.Problem = XMLInputDataMapper.MapOutputPoints(xml, this.Problem);
                this.modeling = new Modeling(this.Problem, shapePictureBox.Size);
                this.modeling.Draw(this.shapePictureBox.CreateGraphics());

                this.OutputWindow.Text = "**************************************" + Environment.NewLine + "***Data correct***" + Environment.NewLine + "**************************************" + Environment.NewLine;
            }
            catch (Exception exception)
            {
                this.OutputWindow.Text = "Exception:" + Environment.NewLine + exception.Message + Environment.NewLine;
            }
        }

        private void InitializeTimers()
        {
            stopwatch = new Stopwatch();
            timer = new Timer();
            timer.Interval = 100;
            timer.Tick += timer_Tick;
        }

        private void StartTimers()
        {
            timer.Start();
            stopwatch.Start();
            calculationProgressBar.Maximum = this.Problem.IterationProcess.NumberOfIterations;
            calculationProgressBar.Value = 0;
        }

        void timer_Tick(object sender, EventArgs e)
        {
            this.calculationTimer.Text = stopwatch.Elapsed.ToString("hh\\:mm\\:ss\\.fff");
        }

        private void shapePictureBox_Paint(object sender, PaintEventArgs e)
        {
            try
            {
                if (this.Problem != null)
                {
                    this.modeling.Draw(e.Graphics);
                }
            }
            catch (Exception exception)
            {
                this.OutputWindow.Text = "Exception when drawing model:" + Environment.NewLine + exception.Message + Environment.NewLine;
            }
        }
    }
}

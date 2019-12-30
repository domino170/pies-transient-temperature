using PIES_TransientTemperature.BoundaryConditions;
using PIES_TransientTemperature.MatrixCalculations;
using PIES_TransientTemperature.Objects;
using PIESTransientTemperature.BoundaryConditions;
using PIESTransientTemperature.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIESTransientTemperature.MatrixCalculations
{
    public class Calculations
    {
        private Problem Problem;

        public double[,] matrixT { get; set; }

        public double[,] matrixq { get; set; }

        public double[,] matrixM { get; set; }

        public double[,] matrixA { get; set; }

        public double[] vectorF { get; set; }

        public double[] initialCondition { get; set; }

        public double[] heatSource { get; set; }

        public double[] vectorB { get; set; }

        public double[] vectorX { get; set; }

        private int numberOfCollocationPoints { get; set; }

        public Calculations(Problem problem)
        {
            this.Problem = problem;

            numberOfCollocationPoints = this.Problem.IterationProcess.isCombinedProblem ? this.Problem.Areas.Sum(x => x.NumberOfCollocationPoints) : this.Problem.Areas[0].NumberOfCollocationPoints;

            this.matrixA = new double[numberOfCollocationPoints, numberOfCollocationPoints];
            this.matrixM = new double[numberOfCollocationPoints, numberOfCollocationPoints];
            this.matrixq = new double[numberOfCollocationPoints, numberOfCollocationPoints];
            this.matrixT = new double[numberOfCollocationPoints, numberOfCollocationPoints];
            this.vectorB = new double[numberOfCollocationPoints];
            this.vectorX = new double[numberOfCollocationPoints];
            this.vectorF = new double[numberOfCollocationPoints];
            this.initialCondition = new double[numberOfCollocationPoints];
            this.heatSource = new double[numberOfCollocationPoints];
        }

        public List<Area> CalculateIteration()
        {
            if (this.Problem.IterationProcess.isCombinedProblem)
            {
                return this.CalculateIterationForMultipleAreas();
            }
            else
            {
                var area = this.CalculateIterationForSingleArea();
                return new List<Area>() { area };
            }

        }

        public List<Area> CalculateIterationForMultipleAreas()
        {
            foreach (var area in this.Problem.Areas)
            {
                area.calculateVariableBoundaryConditions();
                area.CalculateBoundaryTemperature();
            }
            int j = 0;
            foreach (var area in this.Problem.Areas)
            {
                var areaVector = area.GetKnownBoundaryVector();
                foreach (var value in areaVector)
                {
                    this.vectorF[j++] = value;
                }
            }

            if (Problem.IterationProcess.CurrentIteration == 1 || this.Problem.Areas[0].configurationData.arePropertiesTimeDependent())
            {
                BuildMatrixesForMultipleAreas();
            }

            if (Problem.IterationProcess.CurrentIteration == 1)
            {
                Problem.CalculateCollocationPointsConstants();
                Problem.CalculateSurfaceIntegrationPointsConstants();
            }

            this.SeparateKnownFromUnknownForMultipleAreas();
            this.CalculateKnownVectorForMultipleAreas();

            j = 0;
            foreach (var area in this.Problem.Areas)
            {
                var areaVector = InitialCondition.CalculateBoundaryVector(area);
                foreach (var value in areaVector)
                {
                    this.initialCondition[j++] = value;
                }
            }

            this.AddInitialConditionForMultipleAreas();

            this.SolveEquations();

            this.SetUnknownBoundaryConditionsForMultipleAreas();

            return this.Problem.Areas;
        }

        public Area CalculateIterationForSingleArea()
        {
            this.Problem.Areas[0].calculateVariableBoundaryConditions();
            this.Problem.Areas[0].CalculateBoundaryTemperature();

            this.vectorF = this.Problem.Areas[0].GetKnownBoundaryVector();

            if (this.Problem.IterationProcess.CurrentIteration == 1 || this.Problem.Areas[0].configurationData.arePropertiesTimeDependent())
            {
                this.matrixT = Function_T.CalculateBoundaryMatrix(this.Problem.Areas[0]);
                this.matrixq = Function_q.CalculateBoundaryMatrix(this.Problem.Areas[0]);
            }

            if (Problem.IterationProcess.CurrentIteration == 1)
            {
                Problem.CalculateCollocationPointsConstants();
                Problem.CalculateSurfaceIntegrationPointsConstants();

            }
            this.SeparateKnownFromUnknownForSingleArea();
            this.CalculateKnownVectorForSingleArea();

            //To tradycyjnie
            this.initialCondition = InitialCondition.CalculateBoundaryVector(this.Problem.Areas[0]);
            //To w przypadku przechowywania parametrów
            //this.GetInitialConditionVectorFromCollocationPointsConstants();
            this.AddInitialConditionForSingleArea();

            if (this.Problem.Areas[0].configurationData.addHeatSource)
            {
                if (this.Problem.Areas[0].configurationData.isHeatSourceTimeDependent || this.Problem.IterationProcess.CurrentIteration == 1)
                {
                    this.heatSource = HeatSource.CalculateBoundaryVector(this.Problem.Areas[0]);
                }
                this.AddHeatSource();
            }

            this.SolveEquations();
            this.SetUnknownBoundaryConditionsForSingleArea();

            return this.Problem.Areas[0];
        }

        private void BuildMatrixesForMultipleAreas()
        {
            var matrixT0 = Function_T.CalculateBoundaryMatrix(this.Problem.Areas[0]);
            var matrixq0 = Function_q.CalculateBoundaryMatrix(this.Problem.Areas[0]);
            var matrixT1 = Function_T.CalculateBoundaryMatrix(this.Problem.Areas[1]);
            var matrixq1 = Function_q.CalculateBoundaryMatrix(this.Problem.Areas[1]);

            foreach (var areaR in this.Problem.Areas)
            {
                foreach (var segmentR in areaR.Segments)
                {
                    foreach (var collPointR in segmentR.CollocationPoints)
                    {
                        foreach (var areaI in this.Problem.Areas)
                        {
                            foreach (var segmentI in areaI.Segments)
                            {
                                foreach (var collPointI in segmentI.CollocationPoints)
                                {
                                    var r = this.Problem.Areas.Where(x => x.Index < areaR.Index).Sum(x => x.NumberOfCollocationPoints)
                                        + areaR.Segments.Where(x => x.Index < segmentR.Index).Sum(x => x.CollocationPoints.Count)
                                        + collPointR.Index;
                                    var i = this.Problem.Areas.Where(x => x.Index < areaI.Index).Sum(x => x.NumberOfCollocationPoints)
                                       + areaI.Segments.Where(x => x.Index < segmentI.Index).Sum(x => x.CollocationPoints.Count)
                                       + collPointI.Index;
                                    //var r2 = areaR.Index * areaR.NumberOfCollocationPoints + segmentR.Index * segmentR.CollocationPoints.Count() + collPointR.Index;
                                    //var i2 = areaI.Index * areaI.NumberOfCollocationPoints + segmentI.Index * segmentI.CollocationPoints.Count() + collPointI.Index;

                                    var rx = areaR.Segments.Where(x => x.Index < segmentR.Index).Sum(x => x.CollocationPoints.Count) + collPointR.Index;
                                    var ix = areaI.Segments.Where(x => x.Index < segmentI.Index).Sum(x => x.CollocationPoints.Count) + collPointI.Index;
                                    //var rx2 = segmentR.Index * segmentR.CollocationPoints.Count() + collPointR.Index;
                                    //var ix2 = segmentI.Index * segmentI.CollocationPoints.Count() + collPointI.Index;

                                    this.matrixT[r, i] = 0.0;
                                    this.matrixq[r, i] = 0.0;

                                    if (areaR.Index == 0)
                                    {
                                        if (areaI.Index == 0 && !segmentI.isConnectionBoundary)
                                        {
                                            this.matrixT[r, i] = -matrixT0[rx, ix];
                                            this.matrixq[r, i] = -matrixq0[rx, ix];
                                        }
                                        if (areaI.Index == 0 && segmentI.isConnectionBoundary)
                                        {
                                            var iy = this.Problem.Areas[0].Segments.Where(x => x.Index < this.Problem.Areas[0].Segments.Where(y => y.isConnectionBoundary).First().Index).Sum(x => x.CollocationPoints.Count) + collPointI.Index;
                                            //var iy2 = this.Problem.Areas[0].Segments.Where(x => x.isConnectionBoundary).First().Index * segmentI.CollocationPoints.Count() + collPointI.Index;
                                            this.matrixq[r, i] = -matrixq0[rx, iy];
                                        }
                                        if (areaI.Index == 1 && segmentI.isConnectionBoundary)
                                        {
                                            var iy = this.Problem.Areas[0].Segments.Where(x => x.Index < this.Problem.Areas[0].Segments.Where(y => y.isConnectionBoundary).First().Index).Sum(x => x.CollocationPoints.Count) + collPointI.Index;
                                           // var iy2 = this.Problem.Areas[0].Segments.Where(x => x.isConnectionBoundary).First().Index * segmentI.CollocationPoints.Count() + collPointI.Index;
                                            this.matrixq[r, i] = matrixT0[rx, iy];
                                        }
                                    }
                                    if (areaR.Index == 1)
                                    {
                                        if (areaI.Index == 1 && !segmentI.isConnectionBoundary)
                                        {
                                            this.matrixT[r, i] = matrixT1[rx, ix];
                                        }

                                        if (segmentR.isConnectionBoundary)
                                        {
                                            if (areaI.Index == 0 && segmentI.isConnectionBoundary)
                                            {
                                                var ry = this.Problem.Areas[1].Segments.Where(x => x.Index < this.Problem.Areas[1].Segments.Where(y => y.isConnectionBoundary).First().Index).Sum(x => x.CollocationPoints.Count)
                                                   + segmentR.CollocationPoints.Count() - 1 - collPointR.Index;
                                                var iy = this.Problem.Areas[1].Segments.Where(x => x.Index < this.Problem.Areas[1].Segments.Where(y => y.isConnectionBoundary).First().Index).Sum(x => x.CollocationPoints.Count) + collPointI.Index;
                                                //var ry2 = this.Problem.Areas[1].Segments.Where(x => x.isConnectionBoundary).First().Index * segmentR.CollocationPoints.Count()
                                                //   + segmentR.CollocationPoints.Count() - 1 - collPointR.Index;
                                                // var iy2 = this.Problem.Areas[1].Segments.Where(x => x.isConnectionBoundary).First().Index + collPointI.Index;
                                                this.matrixq[r, i] = matrixq1[ry, iy];
                                            }
                                            if (areaI.Index == 1 && segmentI.isConnectionBoundary)
                                            {
                                                var ry = this.Problem.Areas[1].Segments.Where(x => x.Index < this.Problem.Areas[1].Segments.Where(y => y.isConnectionBoundary).First().Index).Sum(x => x.CollocationPoints.Count)
                                                  + segmentR.CollocationPoints.Count() - 1 - collPointR.Index;
                                                var iy = this.Problem.Areas[1].Segments.Where(x => x.Index < this.Problem.Areas[1].Segments.Where(y => y.isConnectionBoundary).First().Index).Sum(x => x.CollocationPoints.Count) + collPointI.Index;
                                                //var ry2 = this.Problem.Areas[1].Segments.Where(x => x.isConnectionBoundary).First().Index * segmentR.CollocationPoints.Count() + segmentR.CollocationPoints.Count() - 1 - collPointR.Index;
                                                //var iy2 = this.Problem.Areas[1].Segments.Where(x => x.isConnectionBoundary).First().Index + collPointI.Index;
                                                this.matrixq[r, i] = matrixT1[ry, iy];
                                            }
                                            if (areaI.Index == 1 && !segmentI.isConnectionBoundary)
                                            {
                                                this.matrixq[r, i] = matrixq1[rx, ix];
                                            }
                                        }
                                        if (!segmentR.isConnectionBoundary)
                                        {
                                            if (areaI.Index == 0 && segmentI.isConnectionBoundary)
                                            {
                                                var ry = areaR.Segments.Where(x => x.Index < (areaR.Segments.Count() - segmentR.Index)).Sum(x => x.CollocationPoints.Count)
                                                 + segmentR.CollocationPoints.Count() - 1 - collPointR.Index;
                                                var iy = this.Problem.Areas[1].Segments.Where(x => x.Index < this.Problem.Areas[1].Segments.Where(y => y.isConnectionBoundary).First().Index).Sum(x => x.CollocationPoints.Count) + collPointI.Index;
                                                //var ry2 = (areaR.Segments.Count() - segmentR.Index) * segmentR.CollocationPoints.Count() + segmentR.CollocationPoints.Count() - 1 - collPointR.Index;
                                                // var iy2 = this.Problem.Areas[1].Segments.Where(x => x.isConnectionBoundary).First().Index * segmentI.CollocationPoints.Count() + collPointI.Index;
                                                this.matrixq[r, i] = matrixq1[ry, iy];
                                            }
                                            if (areaI.Index == 1 && segmentI.isConnectionBoundary)
                                            {
                                                var ry = areaR.Segments.Where(x => x.Index < (areaR.Segments.Count() - segmentR.Index)).Sum(x => x.CollocationPoints.Count)
                                                + segmentR.CollocationPoints.Count() - 1 - collPointR.Index;
                                                var iy = this.Problem.Areas[1].Segments.Where(x => x.Index < this.Problem.Areas[1].Segments.Where(y => y.isConnectionBoundary).First().Index).Sum(x => x.CollocationPoints.Count) + collPointI.Index;
                                                //var ry2 = (areaR.Segments.Count() - segmentR.Index) * segmentR.CollocationPoints.Count() + segmentR.CollocationPoints.Count() - 1 - collPointR.Index;
                                                //var iy2 = this.Problem.Areas[1].Segments.Where(x => x.isConnectionBoundary).First().Index * segmentI.CollocationPoints.Count() + collPointI.Index;
                                                this.matrixq[r, i] = matrixT1[ry, iy];
                                            }
                                            if (areaI.Index == 1 && !segmentI.isConnectionBoundary)
                                            {
                                                this.matrixq[r, i] = matrixq1[rx, ix];
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public void SeparateKnownFromUnknownForSingleArea()
        {
            for (int k = 0; k < this.Problem.Areas[0].NumberOfCollocationPoints; k++)
            {
                int l = -1;
                for (int i = 0; i < this.Problem.Areas[0].Segments.Count; i++)
                {
                    var segment = this.Problem.Areas[0].Segments.ElementAt(i);
                    if (segment.KnownBoundaryCondition.BoundaryConditionType == BoundaryConditions.BoundaryConditionType.Temperature)
                    {
                        for (int j = 0; j < segment.CollocationPoints.Count; j++)
                        {
                            l++;
                            this.matrixM[k, l] = this.matrixq[k, l];
                            this.matrixA[k, l] = this.matrixT[k, l];
                        }
                    }
                    else
                    {
                        for (int j = 0; j < segment.CollocationPoints.Count; j++)
                        {
                            l++;
                            this.matrixM[k, l] = -this.matrixT[k, l];
                            this.matrixA[k, l] = -this.matrixq[k, l];
                        }
                    }
                }
            }
        }

        public void SeparateKnownFromUnknownForMultipleAreas()
        {
            for (int a = 0; a < this.Problem.Areas.Count(); a++)
            {
                for (int k = 0; k < this.Problem.Areas[a].NumberOfCollocationPoints; k++)
                {
                    for (int a1 = 0; a1 < this.Problem.Areas.Count(); a1++)
                    {
                        for (int i = 0; i < this.Problem.Areas[a1].Segments.Count; i++)
                        {
                            var segment = this.Problem.Areas[a1].Segments.ElementAt(i);

                            if (segment.isConnectionBoundary)
                            {
                                for (int j = 0; j < segment.CollocationPoints.Count; j++)
                                {
                                    var x1 = this.Problem.Areas.Where(x => x.Index < a).Sum(x => x.NumberOfCollocationPoints) + k;
                                    var y1 = this.Problem.Areas.Where(x => x.Index < a1).Sum(x => x.NumberOfCollocationPoints) + this.Problem.Areas[a1].Segments.Where(x => x.Index < i).Sum(x => x.CollocationPoints.Count) + j;

                                    this.matrixM[x1, y1] = -this.matrixT[x1, y1];
                                    this.matrixA[x1, y1] = -this.matrixq[x1, y1];

                                    //this.matrixM[a * this.Problem.Areas[a].NumberOfCollocationPoints + k, a1 * this.Problem.Areas[a1].NumberOfCollocationPoints + i * segment.CollocationPoints.Count + j] = -this.matrixT[a * this.Problem.Areas[a].NumberOfCollocationPoints + k, a1 * this.Problem.Areas[a1].NumberOfCollocationPoints + i * segment.CollocationPoints.Count + j];
                                    //this.matrixA[a * this.Problem.Areas[a].NumberOfCollocationPoints + k, a1 * this.Problem.Areas[a1].NumberOfCollocationPoints + i * segment.CollocationPoints.Count + j] = -this.matrixq[a * this.Problem.Areas[a].NumberOfCollocationPoints + k, a1 * this.Problem.Areas[a1].NumberOfCollocationPoints + i * segment.CollocationPoints.Count + j];
                                }
                            }
                            else
                            {

                                if (segment.KnownBoundaryCondition.BoundaryConditionType == BoundaryConditions.BoundaryConditionType.Temperature)
                                {
                                    for (int j = 0; j < segment.CollocationPoints.Count; j++)
                                    {
                                        var x1 = this.Problem.Areas.Where(x => x.Index < a).Sum(x => x.NumberOfCollocationPoints) + k;
                                        var y1 = this.Problem.Areas.Where(x => x.Index < a1).Sum(x => x.NumberOfCollocationPoints) + this.Problem.Areas[a1].Segments.Where(x => x.Index < i).Sum(x => x.CollocationPoints.Count) + j;

                                        this.matrixM[x1, y1] = this.matrixq[x1, y1];
                                        this.matrixA[x1, y1] = this.matrixT[x1, y1];

                                        //this.matrixM[a * this.Problem.Areas[a].NumberOfCollocationPoints + k, a1 * this.Problem.Areas[a1].NumberOfCollocationPoints + i * segment.CollocationPoints.Count + j] = this.matrixq[a * this.Problem.Areas[a].NumberOfCollocationPoints + k, a1 * this.Problem.Areas[a1].NumberOfCollocationPoints + i * segment.CollocationPoints.Count + j];
                                        //this.matrixA[a * this.Problem.Areas[a].NumberOfCollocationPoints + k, a1 * this.Problem.Areas[a1].NumberOfCollocationPoints + i * segment.CollocationPoints.Count + j] = this.matrixT[a * this.Problem.Areas[a].NumberOfCollocationPoints + k, a1 * this.Problem.Areas[a1].NumberOfCollocationPoints + i * segment.CollocationPoints.Count + j];
                                    }
                                }
                                else
                                {
                                    for (int j = 0; j < segment.CollocationPoints.Count; j++)
                                    {
                                        var x1 = this.Problem.Areas.Where(x => x.Index < a).Sum(x => x.NumberOfCollocationPoints) + k;
                                        var y1 = this.Problem.Areas.Where(x => x.Index < a1).Sum(x => x.NumberOfCollocationPoints) + this.Problem.Areas[a1].Segments.Where(x => x.Index < i).Sum(x => x.CollocationPoints.Count) + j;

                                        this.matrixM[x1, y1] = -this.matrixT[x1, y1];
                                        this.matrixA[x1, y1] = -this.matrixq[x1, y1];

                                        //this.matrixM[a * this.Problem.Areas[a].NumberOfCollocationPoints + k, a1 * this.Problem.Areas[a1].NumberOfCollocationPoints + i * segment.CollocationPoints.Count + j] = -this.matrixT[a * this.Problem.Areas[a].NumberOfCollocationPoints + k, a1 * this.Problem.Areas[a1].NumberOfCollocationPoints + i * segment.CollocationPoints.Count + j];
                                        //this.matrixA[a * this.Problem.Areas[a].NumberOfCollocationPoints + k, a1 * this.Problem.Areas[a1].NumberOfCollocationPoints + i * segment.CollocationPoints.Count + j] = -this.matrixq[a * this.Problem.Areas[a].NumberOfCollocationPoints + k, a1 * this.Problem.Areas[a1].NumberOfCollocationPoints + i * segment.CollocationPoints.Count + j];
                                    }
                                }
                            }
                        }
                    }
                }
            }

        }

        public void CalculateKnownVectorForSingleArea()
        {
            double s;
            for (int i = 0; i < numberOfCollocationPoints; i++)
            {
                s = 0;
                for (int k = 0; k < numberOfCollocationPoints; k++)
                {
                    s = s + matrixM[i, k] * vectorF[k];
                }
                vectorB[i] = s;
            }
        }

        public void CalculateKnownVectorForMultipleAreas()
        {
            double s;

            for (int i = 0; i < numberOfCollocationPoints; i++)
            {
                s = 0;

                for (int k = 0; k < numberOfCollocationPoints; k++)
                {
                    s = s + matrixM[i, k] * vectorF[k];
                }
                vectorB[i] = s;
            }


        }

        public void GetInitialConditionVectorFromCollocationPointsConstants()
        {
            foreach (var area in this.Problem.Areas)
            {
                foreach (var segment in area.Segments)
                {
                    Parallel.ForEach(segment.CollocationPoints, (collPoint) =>
                    {
                        collPoint.CollocationPointInitialConditionConstantValue = this.Problem.GetInitialConditionValueFromCollocationPointsConstants(collPoint);
                    });
                }

                int i = 0;
                foreach (var segment in this.Problem.Areas[0].Segments)
                {
                    foreach (var collPoint in segment.CollocationPoints)
                    {
                        this.initialCondition[i] = collPoint.CollocationPointInitialConditionConstantValue;
                        i++;
                    }
                }
            }
        }

        public void AddInitialConditionForSingleArea()
        {
            for (int i = 0; i < numberOfCollocationPoints; i++)
            {
                vectorB[i] = vectorB[i] + initialCondition[i];
            }
        }

        public void AddInitialConditionForMultipleAreas()
        {
            //Powinno być z +
            for (int i = 0; i < numberOfCollocationPoints; i++)
            {
                if (i > Problem.Areas[0].NumberOfCollocationPoints - 1)
                {
                    vectorB[i] = vectorB[i] + initialCondition[i];
                }
                else
                {
                    vectorB[i] = vectorB[i] - initialCondition[i];
                }
            }
        }

        public void AddHeatSource()
        {
            for (int i = 0; i < numberOfCollocationPoints; i++)
            {
                vectorB[i] = vectorB[i] + heatSource[i];
            }
        }

        public void SolveEquations()
        {
            this.vectorX = GaussElimination.Calculate(matrixA, vectorB, numberOfCollocationPoints);
        }

        public void SetUnknownBoundaryConditionsForSingleArea()
        {
            int j = 0;
            foreach (var area in this.Problem.Areas)
            {
                foreach (var segment in area.Segments)
                {
                    var vector = new double[segment.CollocationPoints.Count];

                    for (int i = 0; i < segment.CollocationPoints.Count; i++)
                    {
                        vector[i] = vectorX[j++];
                    }

                    var unknownBoundaryConditionType = BoundaryConditionType.Temperature;

                    if (segment.KnownBoundaryCondition.BoundaryConditionType == BoundaryConditionType.Temperature)
                    {
                        unknownBoundaryConditionType = BoundaryConditionType.HeatFlux;
                    }

                    segment.UnknownBoundaryCondition = new BoundaryCondition(unknownBoundaryConditionType, segment.CollocationPoints.Count, vector, BoundaryConditionValueType.Value);
                }
            }
        }

        public void SetUnknownBoundaryConditionsForMultipleAreas()
        {
            int j = 0;
            foreach (var area in this.Problem.Areas)
            {
                foreach (var segment in area.Segments)
                {
                    var vector = new double[segment.CollocationPoints.Count];
                    var vectorq = new double[segment.CollocationPoints.Count];
                    var vectorT = new double[segment.CollocationPoints.Count];
                    if (segment.isConnectionBoundary && area.Index == 1)
                    {
                        for (int i = 0; i < segment.CollocationPoints.Count; i++)
                        {
                            vectorq[i] = -vectorX[j];
                            vectorT[i] = vectorX[j - segment.CollocationPoints.Count];
                            j++;
                        }
                        segment.UnknownBoundaryCondition = new BoundaryCondition(BoundaryConditionType.Temperature, segment.CollocationPoints.Count, vectorT, BoundaryConditionValueType.Value);
                        segment.KnownBoundaryCondition = new BoundaryCondition(BoundaryConditionType.HeatFlux, segment.CollocationPoints.Count, vectorq, BoundaryConditionValueType.Value);
                    }
                    else if (segment.isConnectionBoundary && area.Index == 0)
                    {
                        for (int i = 0; i < segment.CollocationPoints.Count; i++)
                        {
                            vectorq[i] = vectorX[j + segment.CollocationPoints.Count];
                            vectorT[i] = vectorX[j];
                            j++;
                        }
                        segment.UnknownBoundaryCondition = new BoundaryCondition(BoundaryConditionType.Temperature, segment.CollocationPoints.Count, vectorT, BoundaryConditionValueType.Value);
                        segment.KnownBoundaryCondition = new BoundaryCondition(BoundaryConditionType.HeatFlux, segment.CollocationPoints.Count, vectorq, BoundaryConditionValueType.Value);
                    }
                    else
                    {
                        for (int i = 0; i < segment.CollocationPoints.Count; i++)
                        {
                            vector[i] = vectorX[j++];
                        }

                        var unknownBoundaryConditionType = BoundaryConditionType.Temperature;

                        if (segment.KnownBoundaryCondition.BoundaryConditionType == BoundaryConditionType.Temperature)
                        {
                            unknownBoundaryConditionType = BoundaryConditionType.HeatFlux;
                        }

                        segment.UnknownBoundaryCondition = new BoundaryCondition(unknownBoundaryConditionType, segment.CollocationPoints.Count, vector, BoundaryConditionValueType.Value);
                    }
                }
            }
        }
        public string PrintMatrixT()
        {
            var stringBuilder = new StringBuilder();

            for (int i = 0; i < numberOfCollocationPoints; i++)
            {
                for (int j = 0; j < numberOfCollocationPoints; j++)
                {
                    stringBuilder.Append(matrixT[i, j] + "\t");
                }
                stringBuilder.Append(Environment.NewLine);
            }
            return stringBuilder.ToString();
        }

        public string PrintMatrixq()
        {
            var stringBuilder = new StringBuilder();

            for (int i = 0; i < numberOfCollocationPoints; i++)
            {
                for (int j = 0; j < numberOfCollocationPoints; j++)
                {
                    stringBuilder.Append(matrixq[i, j] + "\t");
                }
                stringBuilder.Append(Environment.NewLine);
            }
            return stringBuilder.ToString();
        }

        public string PrintInitialConditionVector()
        {
            var stringBuilder = new StringBuilder();

            for (int j = 0; j < numberOfCollocationPoints; j++)
            {
                stringBuilder.Append(initialCondition[j] + Environment.NewLine);
            }

            return stringBuilder.ToString();
        }
    }
}

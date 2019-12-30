using PIES_TransientTemperature.BoundaryConditions;
using PIES_TransientTemperature.Objects;
using PIES_TransientTemperature.Oputput;
using PIESTransientTemperature;
using PIESTransientTemperature.ApproximationSeries;
using PIESTransientTemperature.BoundaryConditions;
using PIESTransientTemperature.Objects;
using PIESTransientTemperature.ShapeFunctions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace PIES_TransientTemperature
{
    public static class XMLInputDataMapper
    {
        private static int singularIntegrationPointsNumberMultiplier = 10;
        private static string configurationDataElementName = "ConfigurationData";
        private static string areasElementName = "Areas";
        // private static string areaElementName = "Area";
        private static string materialPropertiesElementName = "MaterialProperties";
        private static string diffusionCoefficientElementName = "DiffusionCoefficient";
        private static string thermalConductivityElementName = "ThermalConductivity";
        // private static string temperatureDependentAttributeName = "temperatureDependent";
        //private static string timeDependentAttributeName = "timeDependent";
        private static string timeStepElementName = "TimeStep";
        private static string numberOfIterationsElementName = "NumberOfIterations";
        private static string boundarySegmentsElementName = "BoundarySegments";
        private static string segmentElementName = "Segment";
        private static string connectionBoundaryAttributeName = "connectionBoundary";
        private static string boundaryShapeCurveElementName = "BoundaryShapeCurve";
        private static string boundaryShapeTypeAttributeName = "type";
        private static string collocationPointsElementName = "CollocationPoints";
        private static string numberOfCollocationPointsAttributeName = "number";
        private static string collocationPointsPlacementAttributeName = "placement";
        private static string collocationPointsDistanceAttributeName = "distanceFromEdge";
        private static string numberOfIntegrationPointsAttributeName = "numberOfIntegrationPoints";
        private static string distanceFromSingularPointAttributeName = "distanceFromSingularPoint";
        private static string BoundaryConditionElementName = "BoundaryCondition";
        private static string BoundaryConditionTypeAttributeName = "type";
        //private static string valueAttributeName = "value";
        private static string surfacesElementName = "Surfaces";
        private static string surfaceElementName = "Surface";
        private static string numberOfAreaIntegrationPointsXAttributeName = "numberOfIntegrationPointsInDirectionX";
        private static string numberOfAreaIntegrationPointsYAttributeName = "numberOfIntegrationPointsInDirectionY";
        private static string subractSurfaceAttributeName = "subractSurface";
        private static string surfaceShapeElementName = "SurfaceShape";
        private static string surfaceShapeTypeAttributeName = "type";
        private static string initialConditionElementName = "InitialCondition";
        private static string heatSourceFunctionElementName = "HeatSourceFunction";
        private static string outputElementName = "Oputput";
        private static string boundaryOutputElementName = "Boundary";
        private static string domainOutputElementName = "Domain";
        private static string areaIndexAttributeName = "areaIndex";
        private static string segmentIndexAttributeName = "segmentIndex";
        private static string parametricPositionAttributeName = "parametricPosition";
        private static string boundaryConditionAttributeName = "boundaryCondition";
        private static string timeOutputElementName = "WriteResultsIn";
        private static string timeAttributeName = "time";
        private static string timeIntervalAttributeName = "timeInterval";
        private static string valueBoundaryConditionElementName = "Value";
        private static string robinBoundaryConditionElementName = "CovectionRadiation";
        private static string timeIntervalBoundaryConditionElementName = "TimeIntervalValue";
        private static string heatTransferCoefficientAttributeName = "heatTransferCoefficient";
        private static string outherBoundaryDiameterAttributeName = "outherBoundaryDiameter";
        private static string fluidTemperatureAttributeName = "fluidTemperature";
        private static string fluidThermalConductivityAttributeName = "fluidThermalConductivity";
        private static string fluidViscosityAttributeName = "fluidViscosity";
        private static string fluidDensityAttributeName = "fluidDensity";
        private static string fluidSpecificHeatAttributeName = "fluidSpecificHeat";
        private static string emissivityCoefficientAttributeName = "emissivityCoefficient";
        private static string startTimeAttributeName = "startTime";
        private static string endTimeAttributeName = "endTime";


        public static IterationProcess MapIterationProcess(XDocument xml)
        {
            var cd = xml.Root.Descendants(configurationDataElementName).First();

            var timeStep = double.Parse(cd.Element(timeStepElementName).Value.Replace(".", ","));
            var numberOfIterations = int.Parse(cd.Element(numberOfIterationsElementName).Value);
            var areas = xml.Root.Descendants(areasElementName);

            if (areas.Elements().Count() > 2 || areas.Elements().Count() < 1)
            {
                throw new ArgumentException("Wrong number of Areas. Must be one area for regular problem and two areas for combined problem");
            }

            var isCombinedProblem = areas.Elements().Count() == 2;

            return new IterationProcess(timeStep, numberOfIterations, isCombinedProblem);
        }

        public static ConfigurationData MapMaterialProperties(XElement xml, IterationProcess iterationProcess)
        {
            var cd = xml.Elements(materialPropertiesElementName).First();

            var diffusionCoefficientXElement = cd.Element(diffusionCoefficientElementName);
            var diffusionCoefficient = diffusionCoefficientXElement.Value.Replace(".", ",");
            var timeDependentDiffusionCoefficient = diffusionCoefficient.Contains('T');//diffusionCoefficientXElement.Attribute(temperatureDependentAttributeName) != null ? bool.Parse(diffusionCoefficientXElement.Attribute(temperatureDependentAttributeName).Value) : false;

            var thermalConductivityXElement = cd.Element(thermalConductivityElementName);
            var thermalConductivity = thermalConductivityXElement.Value.Replace(".", ",");
            var timeDependentThermalConductivity = thermalConductivity.Contains('T');//thermalConductivityXElement.Attribute(temperatureDependentAttributeName) != null ? bool.Parse(thermalConductivityXElement.Attribute(temperatureDependentAttributeName).Value) : false;

            return new ConfigurationData(diffusionCoefficient, timeDependentDiffusionCoefficient, thermalConductivity, timeDependentThermalConductivity, iterationProcess);
        }

        public static List<Area> MapAreas(XDocument xml, IterationProcess iterationProcess)
        {
            var areas = xml.Root.Descendants(areasElementName);
            var AreaList = new List<Area>();
            int Index = 0;
            foreach (var area in areas.Elements())
            {
                var configurationData = MapMaterialProperties(area, iterationProcess);
                AreaList.Add(MapArea(Index++, area, configurationData));
            }

            return AreaList;

        }
        public static Area MapArea(int Index, XElement xml, ConfigurationData configurationData)
        {
            var area = new Area(Index, configurationData);

            area = MapSurfaces(xml, area);

            area = MapSegments(xml, area);

            return area;
        }

        private static Area MapSegments(XElement xml, Area area)
        {
            int segmentIndex = 0;

            foreach (var Xsegment in xml.Element(boundarySegmentsElementName).Elements(segmentElementName))
            {
                var numberOfIntegrationPoints = int.Parse(Xsegment.Attribute(numberOfIntegrationPointsAttributeName).Value);

                var connectionBoundary = false;
                var connectionBoundaryXAttribute = Xsegment.Attribute(connectionBoundaryAttributeName);
                if (connectionBoundaryXAttribute != null)
                {
                    connectionBoundary = bool.Parse(connectionBoundaryXAttribute.Value);
                }

                var numberOfCollocationPoints = int.Parse(Xsegment.Elements(collocationPointsElementName).First().Attribute(numberOfCollocationPointsAttributeName).Value);
                
                var distanceFromSingularPointAttribute = Xsegment.Attribute(distanceFromSingularPointAttributeName);
                double distanceFromSingularPoint = distanceFromSingularPointAttribute != null ? double.Parse(distanceFromSingularPointAttribute.Value.Replace(".", ",")) : 0.0;

                var collocationPlacementType = MapCollocationPlacementType(Xsegment);

                double collocationPointsRealDistanceFromEdge = 0.0;
                if (collocationPlacementType == CollocationPlacementType.CloserOnEdges)
                {
                    collocationPointsRealDistanceFromEdge = double.Parse(Xsegment.Elements(collocationPointsElementName).First().Attribute(collocationPointsDistanceAttributeName).Value.Replace(".", ","));
                }

                var boundaryshape = MapBoundaryShape(Xsegment);
                var boundaryCondition = MapBoundaryCondition(Xsegment, numberOfCollocationPoints);

                //TODO: Zastanowic się nad poprawą tego
                var numberOfBoundaryIntegrationPointsForDomainIntegral = (int)Math.Sqrt(area.Surfaces.Select(x => x.InitialConditionSurfaceIntegrationPoints.Count).Average()) * 10;

                area.Segments.Add(new Segment(
                    segmentIndex,
                    boundaryshape,
                    numberOfCollocationPoints,
                    collocationPlacementType,
                    collocationPointsRealDistanceFromEdge,
                    numberOfIntegrationPoints,
                    numberOfIntegrationPoints * singularIntegrationPointsNumberMultiplier,
                    numberOfBoundaryIntegrationPointsForDomainIntegral,
                    distanceFromSingularPoint,
                    boundaryCondition,
                    connectionBoundary));

                segmentIndex++;
            }

            return area;
        }

        private static BoundaryCondition MapBoundaryCondition(XElement Xsegment, int numberOfCollocationPoints)
        {
            var XboundaryCondition = Xsegment.Element(BoundaryConditionElementName);
            BoundaryConditionType boundaryConditionType;
            BoundaryConditionValueType boundaryConditionValueType;
            TimeIntervalBoundaryConditionHelper timeIntervalValueBoundaryConditionHelper = null;
            RobinBoundaryConditionHelper robinBoundaryConditionHelper = null;

            if (XboundaryCondition.Attribute(BoundaryConditionTypeAttributeName).Value == "q")
            {
                boundaryConditionType = BoundaryConditionType.HeatFlux;
            }
            else
            {
                boundaryConditionType = BoundaryConditionType.Temperature;
            }

            var boundaryConditionExpression = String.Empty;
            var valueBoundaryConditionXElement = XboundaryCondition.Elements(valueBoundaryConditionElementName);
            var timeIntervalBoundaryConditionXElement = XboundaryCondition.Elements(timeIntervalBoundaryConditionElementName);
            var robinBoundaryConditionXElement = XboundaryCondition.Elements(robinBoundaryConditionElementName);

            if (robinBoundaryConditionXElement.Count() > 0)
            {
                robinBoundaryConditionHelper = MapRobinBoundaryCondition(robinBoundaryConditionXElement.First());
                boundaryConditionValueType = BoundaryConditionValueType.ConvectionRadiation;
            }
            else if (timeIntervalBoundaryConditionXElement.Count() > 0)
            {
                timeIntervalValueBoundaryConditionHelper = MapTimeIntervalValueBoundaryCondition(timeIntervalBoundaryConditionXElement);
                boundaryConditionValueType = BoundaryConditionValueType.TimeIntervalValue;
            }
            else if (valueBoundaryConditionXElement.Count() > 0)
            {
                boundaryConditionExpression = valueBoundaryConditionXElement.First().Value.Replace(".", ",");
                boundaryConditionValueType = BoundaryConditionValueType.Value;
            }
            else
            {
                throw new AggregateException("Wrong boundary condition");
            }

            var boundaryCondition = new BoundaryCondition(boundaryConditionType, numberOfCollocationPoints, boundaryConditionExpression, boundaryConditionValueType, timeIntervalValueBoundaryConditionHelper, robinBoundaryConditionHelper);

            return boundaryCondition;
        }

        private static RobinBoundaryConditionHelper MapRobinBoundaryCondition(XElement XboundaryConditionValue)
        {
            var fluidTemperature = double.Parse(XboundaryConditionValue.Attribute(fluidTemperatureAttributeName).Value.Replace(".", ","));

            if (XboundaryConditionValue.Attribute(heatTransferCoefficientAttributeName) != null)
            {
                var heatTransferCoefficient = double.Parse(XboundaryConditionValue.Attribute(heatTransferCoefficientAttributeName).Value.Replace(".", ","));
                return new RobinBoundaryConditionHelper(fluidTemperature, heatTransferCoefficient);
            }
            else
            {
                var outherBoundaryDiameter = double.Parse(XboundaryConditionValue.Attribute(outherBoundaryDiameterAttributeName).Value.Replace(".", ","));
                var fluidThermalConductivity = double.Parse(XboundaryConditionValue.Attribute(fluidThermalConductivityAttributeName).Value.Replace(".", ","));
                var fluidViscosity = double.Parse(XboundaryConditionValue.Attribute(fluidViscosityAttributeName).Value.Replace(".", ","));
                var fluidDensity = double.Parse(XboundaryConditionValue.Attribute(fluidDensityAttributeName).Value.Replace(".", ","));
                var fluidSpecificHeat = double.Parse(XboundaryConditionValue.Attribute(fluidSpecificHeatAttributeName).Value.Replace(".", ","));
                var emissivityCoefficient = double.Parse(XboundaryConditionValue.Attribute(emissivityCoefficientAttributeName).Value.Replace(".", ","));

                return new RobinBoundaryConditionHelper(outherBoundaryDiameter, fluidTemperature, fluidThermalConductivity, fluidViscosity, fluidDensity, fluidSpecificHeat, emissivityCoefficient);
            }
        }

        private static TimeIntervalBoundaryConditionHelper MapTimeIntervalValueBoundaryCondition(IEnumerable<XElement> XboundaryConditionList)
        {
            var boundaryConditionIntervalList = new List<TimeIntervalBoundaryCondition>();
            foreach (var XboundaryCondition in XboundaryConditionList)
            {
                var startTime = double.Parse(XboundaryCondition.Attribute(startTimeAttributeName).Value.Replace(".", ","));
                var endTime = double.Parse(XboundaryCondition.Attribute(endTimeAttributeName).Value.Replace(".", ","));
                var boundaryConditionFunction = XboundaryCondition.Value.Replace(".", ",");
                boundaryConditionIntervalList.Add(new TimeIntervalBoundaryCondition(startTime, endTime, boundaryConditionFunction));
            }
            return new TimeIntervalBoundaryConditionHelper(boundaryConditionIntervalList);
        }

        private static IBoundaryShape MapBoundaryShape(XElement Xsegment)
        {
            var XboundaryShapeCurve = Xsegment.Element(boundaryShapeCurveElementName);
            IBoundaryShape boundaryshape = null;
            var boundaryShapeType = int.Parse(XboundaryShapeCurve.Attribute(boundaryShapeTypeAttributeName).Value);

            if (boundaryShapeType == 1)
            {
                boundaryshape = new LinearCurve(
                   new RealPoint(double.Parse(XboundaryShapeCurve.Element("P0").Attribute("x").Value.Replace(".", ",")), double.Parse(XboundaryShapeCurve.Element("P0").Attribute("y").Value.Replace(".", ","))),
                   new RealPoint(double.Parse(XboundaryShapeCurve.Element("P1").Attribute("x").Value.Replace(".", ",")), double.Parse(XboundaryShapeCurve.Element("P1").Attribute("y").Value.Replace(".", ",")))
                  );
            }
            else if (boundaryShapeType == 3)
            {
                boundaryshape = new BezieCurve(
                   new RealPoint(double.Parse(XboundaryShapeCurve.Element("P0").Attribute("x").Value.Replace(".", ",")), double.Parse(XboundaryShapeCurve.Element("P0").Attribute("y").Value.Replace(".", ","))),
                   new RealPoint(double.Parse(XboundaryShapeCurve.Element("P3").Attribute("x").Value.Replace(".", ",")), double.Parse(XboundaryShapeCurve.Element("P3").Attribute("y").Value.Replace(".", ","))),
                   new RealPoint(double.Parse(XboundaryShapeCurve.Element("P1").Attribute("x").Value.Replace(".", ",")), double.Parse(XboundaryShapeCurve.Element("P1").Attribute("y").Value.Replace(".", ","))),
                   new RealPoint(double.Parse(XboundaryShapeCurve.Element("P2").Attribute("x").Value.Replace(".", ",")), double.Parse(XboundaryShapeCurve.Element("P2").Attribute("y").Value.Replace(".", ","))));
            }
            return boundaryshape;
        }

        private static CollocationPlacementType MapCollocationPlacementType(XElement Xsegment)
        {
            var XcollocationPlacementType = int.Parse(Xsegment.Elements(collocationPointsElementName).First().Attribute(collocationPointsPlacementAttributeName).Value);
            var collocationPlacementType = CollocationPlacementType.Equal;
            if (XcollocationPlacementType == 0)
            {
                collocationPlacementType = CollocationPlacementType.Czebyshew;
            }
            else if (XcollocationPlacementType == 1)
            {
                collocationPlacementType = CollocationPlacementType.Equal;
            }
            else if (XcollocationPlacementType == 2)
            {
                collocationPlacementType = CollocationPlacementType.CloserOnEdges;
            }
            return collocationPlacementType;
        }

        private static Area MapSurfaces(XElement xml, Area area)
        {
            int index = 0;
            foreach (var Xsurface in xml.Element(surfacesElementName).Elements(surfaceElementName))
            {
                var numberOfInitialConditionIntegrationPointsX = int.Parse(Xsurface.Element(initialConditionElementName).Attribute(numberOfAreaIntegrationPointsXAttributeName).Value);
                var numberOfInitialConditionIntegrationPointsY = int.Parse(Xsurface.Element(initialConditionElementName).Attribute(numberOfAreaIntegrationPointsYAttributeName).Value);
                var initialCondition = double.Parse(Xsurface.Element(initialConditionElementName).Value.Replace(".", ","));

                var subractSurface = Xsurface.Attribute(subractSurfaceAttributeName) != null ? bool.Parse(Xsurface.Attribute(subractSurfaceAttributeName).Value) : false;

                var XheatSourceFunction = Xsurface.Element(heatSourceFunctionElementName);
                var numberOfHeatSourceIntegrationPointsX = 0;
                var numberOfHeatSourceIntegrationPointsY = 0;
                var heatSourceFunction = string.Empty;

                if (XheatSourceFunction != null)
                {
                    area.configurationData.addHeatSource = true;
                    numberOfHeatSourceIntegrationPointsX = int.Parse(XheatSourceFunction.Attribute(numberOfAreaIntegrationPointsXAttributeName).Value);
                    numberOfHeatSourceIntegrationPointsY = int.Parse(XheatSourceFunction.Attribute(numberOfAreaIntegrationPointsYAttributeName).Value);
                    heatSourceFunction = XheatSourceFunction.Value.Replace(".", ",");
                    area.configurationData.isHeatSourceTimeDependent = heatSourceFunction.Contains("t");
                }

                var surfaceShape = MapSurfaceShape(Xsurface);

                var surface = new Surface(index++, area.configurationData, surfaceShape, numberOfInitialConditionIntegrationPointsX, numberOfInitialConditionIntegrationPointsY, initialCondition, heatSourceFunction, numberOfHeatSourceIntegrationPointsX, numberOfHeatSourceIntegrationPointsY, subractSurface);
                area.Surfaces.Add(surface);
            }

            return area;
        }

        private static BezieSurface MapSurfaceShape(XElement Xsurface)
        {
            var Xshape = Xsurface.Element(surfaceShapeElementName);
            var surfaceType = int.Parse(Xshape.Attribute(surfaceShapeTypeAttributeName).Value);
            var surfaceDegree = surfaceType + 1;
            var points = new RealPoint[surfaceDegree, surfaceDegree];

            var Xpoints = Xshape.Elements().Where(x => x.Name.ToString().StartsWith("P"));
            for (int direction1 = 0; direction1 < surfaceDegree; direction1++)
            {
                for (int direction2 = 0; direction2 < surfaceDegree; direction2++)
                {
                    var Xpoint = Xpoints.ElementAt(direction1 * surfaceDegree + direction2);
                    points[direction1, direction2] = new RealPoint(double.Parse(Xpoint.Attribute("x").Value.Replace(".", ",")), double.Parse(Xpoint.Attribute("y").Value.Replace(".", ",")));
                }
            }
            var surfaceShape = new BezieSurface(points, surfaceDegree);
            return surfaceShape;
        }

        public static Problem MapOutputPoints(XDocument xml, Problem problem)
        {
            problem.boundaryOutputPoints = MapBoundaryOutputPoints(xml);

            problem.domainOutputPoints = MapDomainOutputPoints(xml);

            problem.timeOutputPoints = MapTimeOutputPoints(xml, problem.IterationProcess);

            return problem;
        }

        private static List<BoundaryOutputPoint> MapBoundaryOutputPoints(XDocument xml)
        {
            var XboundaryOutput = xml.Root.Element(outputElementName).Element(boundaryOutputElementName);
            var boundaryAreaPoints = new List<BoundaryOutputPoint>();
            foreach (var Xpoint in XboundaryOutput.Elements())
            {
                var areaIndex = int.Parse(Xpoint.Attribute(areaIndexAttributeName).Value);
                var segmentIndex = int.Parse(Xpoint.Attribute(segmentIndexAttributeName).Value);
                var parametricPosition = double.Parse(Xpoint.Attribute(parametricPositionAttributeName).Value.Replace(".", ","));
                var boundaryCondition = Xpoint.Attribute(boundaryConditionAttributeName).Value;
                var boundaryOutputPoint = new BoundaryOutputPoint()
                {
                    areaIndex = areaIndex,
                    segmentIndex = segmentIndex,
                    parametricPosition = parametricPosition,
                    boundaryConditionType = boundaryCondition == "T" ? BoundaryConditionType.Temperature : BoundaryConditionType.HeatFlux
                };
                boundaryAreaPoints.Add(boundaryOutputPoint);
            }

            return boundaryAreaPoints;
        }

        private static List<RealPoint> MapDomainOutputPoints(XDocument xml)
        {
            var XdomainOutput = xml.Root.Element(outputElementName).Element(domainOutputElementName);
            var domainOutputPoints = new List<RealPoint>();

            foreach (var Xpoint in XdomainOutput.Elements())
            {
                domainOutputPoints.Add(new RealPoint(double.Parse(Xpoint.Attribute("x").Value.Replace(".", ",")), double.Parse(Xpoint.Attribute("y").Value.Replace(".", ","))));
            }

            return domainOutputPoints;
        }

        private static List<double> MapTimeOutputPoints(XDocument xml, IterationProcess iterationProcess)
        {
            var XtimeOutput = xml.Root.Element(outputElementName).Element(timeOutputElementName);
            var timeOutputPoints = new List<double>();
            var XtimeIntervalAttribute = XtimeOutput.Attribute(timeIntervalAttributeName);
            if (XtimeIntervalAttribute != null)
            {
                var interval = double.Parse(XtimeIntervalAttribute.Value.Replace(".", ","));
                for (decimal i = Convert.ToDecimal(interval); i <= Decimal.Multiply(Convert.ToDecimal(iterationProcess.TimeStep), Convert.ToDecimal(iterationProcess.NumberOfIterations)); i += Convert.ToDecimal(interval))
                {
                    timeOutputPoints.Add(Convert.ToDouble(i));
                }
            }
            else
            {
                foreach (var Xtime in XtimeOutput.Attribute(timeAttributeName).Value.Split('|'))
                {
                    timeOutputPoints.Add(double.Parse(Xtime.Replace(".", ",")));
                }
            }

            return timeOutputPoints;
        }
    }
}

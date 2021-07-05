using Assets.src;
using System.Collections.Generic;
using System.Linq;

namespace ConcaveHull
{
    public class Hull
    {
        public List<Node> unused = new List<Node>();
        public List<Line> edges = new List<Line>();
        public List<Line> concaveEdges = new List<Line>();

        private List<Node> initialNodes = new List<Node>();

        public Hull(List<Node> dots)
        {
            this.initialNodes.Clear();
            this.initialNodes.AddRange(dots);
        }

        public void Calculate(float concavity, int scaleFactor)
        {
            this.SetConvexHull(this.initialNodes);
            this.SetConcaveHull(concavity, scaleFactor);
        }

        public List<Line> GetHull(List<Node> nodes)
        {
            List<Node> convexH = new List<Node>();
            List<Line> exitLines = new List<Line>();

            convexH.AddRange(GrahamScan.convexHull(nodes));
            for (int i = 0; i < convexH.Count - 1; i++)
            {
                exitLines.Add(new Line(convexH[i], convexH[i + 1]));
            }

            exitLines.Add(new Line(convexH[0], convexH[convexH.Count - 1]));
            return exitLines;
        }

        public void SetConvexHull(List<Node> nodes)
        {
            var exitLines = this.GetHull(nodes);

            unused.AddRange(nodes);
            edges.AddRange(exitLines);

            foreach (Line line in edges)
            {
                foreach (Node node in line.nodes)
                {
                    unused.RemoveAll(a => a.id == node.id);
                }
            }
        }

        public List<Line> SetConcaveHull(float concavity, int scaleFactor)
        {
            /* Run setConvHull before! 
             * Concavity is a value used to restrict the concave angles 
             * It can go from -1 (no concavity) to 1 (extreme concavity) 
             * Avoid concavity == 1 if you don't want 0º angles
             * */
            bool hullWasOptimized;
            concaveEdges.AddRange(edges);

            do
            {
                hullWasOptimized = false;
                for (int linePositionInHull = 0; linePositionInHull < concaveEdges.Count && !hullWasOptimized; linePositionInHull++)
                {
                    Line line = concaveEdges[linePositionInHull];
                    List<Node> nearbyPoints = HullFunctions.GetNearbyPoints(line, unused, scaleFactor);
                    List<Line> dividedLine = HullFunctions.GetDividedLine(line, nearbyPoints, concaveEdges, concavity);

                    if (dividedLine.Count > 0)
                    {
                        hullWasOptimized = true;
                        unused.Remove(unused.Where(n => n.id == dividedLine[0].nodes[1].id).FirstOrDefault()); // Middlepoint no longer free
                        concaveEdges.AddRange(dividedLine);
                        concaveEdges.RemoveAt(linePositionInHull); // Divided line no longer exists
                    }
                }

                concaveEdges = concaveEdges.OrderByDescending(a => Line.getLength(a.nodes[0], a.nodes[1])).ToList();

            } while (hullWasOptimized);

            return concaveEdges;
        }
    }
}
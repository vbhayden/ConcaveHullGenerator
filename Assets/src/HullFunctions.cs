﻿using ConcaveHull;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ConcaveHull
{
    public static class HullFunctions
    {
        public static List<Line> GetDividedLine(Line line, List<Node> nearbyPoints, List<Line> concave_hull, double concavity)
        {
            // returns two lines if a valid middlePoint is found
            // returns empty list if the line can't be divided
            List<Line> dividedLine = new List<Line>();
            List<Node> okMiddlePoints = new List<Node>();
            foreach (Node middlePoint in nearbyPoints)
            {
                double _cos = GetCosine(line.nodes[0], line.nodes[1], middlePoint);
                if (_cos < concavity)
                {
                    Line newLineA = new Line(line.nodes[0], middlePoint);
                    Line newLineB = new Line(middlePoint, line.nodes[1]);
                    if (!LineCollidesWithHull(newLineA, concave_hull) && !LineCollidesWithHull(newLineB, concave_hull))
                    {
                        middlePoint.cos = _cos;
                        okMiddlePoints.Add(middlePoint);
                    }
                }
            }
            if (okMiddlePoints.Count > 0)
            {
                // We want the middlepoint to be the one with the widest angle (smallest cosine)
                okMiddlePoints = okMiddlePoints.OrderBy(p => p.cos).ToList();
                dividedLine.Add(new Line(line.nodes[0], okMiddlePoints[0]));
                dividedLine.Add(new Line(okMiddlePoints[0], line.nodes[1]));
            }
            return dividedLine;
        }

        public static bool LineCollidesWithHull(Line line, List<Line> concave_hull)
        {
            foreach (Line hullLine in concave_hull)
            {
                // We don't want to check a collision with this point that forms the hull AND the line
                if (line.nodes[0].id != hullLine.nodes[0].id && line.nodes[0].id != hullLine.nodes[1].id
                    && line.nodes[1].id != hullLine.nodes[0].id && line.nodes[1].id != hullLine.nodes[1].id)
                {
                    // Avoid line interesections with the rest of the hull
                    if (LineIntersectionFunctions.CheckIntersection(line.nodes[0], line.nodes[1], hullLine.nodes[0], hullLine.nodes[1]))
                        return true;
                }
            }
            return false;
        }

        private static double GetCosine(Node A, Node B, Node O)
        {
            /* Law of cosines */
            double aPow2 = Math.Pow(A.x - O.x, 2) + Math.Pow(A.y - O.y, 2);
            double bPow2 = Math.Pow(B.x - O.x, 2) + Math.Pow(B.y - O.y, 2);
            double cPow2 = Math.Pow(A.x - B.x, 2) + Math.Pow(A.y - B.y, 2);
            double cos = (aPow2 + bPow2 - cPow2) / (2 * Math.Sqrt(aPow2 * bPow2));
            return Math.Round(cos, 4);
        }

        public static List<Node> GetNearbyPoints(Line line, List<Node> nodeList, int scaleFactor)
        {
            /* The bigger the scaleFactor the more points it will return
             * Inspired by this precious algorithm:
             * http://www.it.uu.se/edu/course/homepage/projektTDB/ht13/project10/Project-10-report.pdf
             * Be carefull: if it's too small it will return very little points (or non!), 
             * if it's too big it will add points that will not be used and will consume time
             * */
            List<Node> nearbyPoints = new List<Node>();
            double[] boundary;
            int tries = 0;
            double node_x_rel_pos;
            double node_y_rel_pos;

            while (tries < 2 && nearbyPoints.Count == 0)
            {
                boundary = GetBoundary(line, scaleFactor);
                
                for (int k=0; k<nodeList.Count; k++)
                {
                    var node = nodeList[k];

                    var isFirstPoint = node.x == line.nodes[0].x && node.y == line.nodes[0].y;
                    var isSecondPoint = node.x == line.nodes[1].x && node.y == line.nodes[1].y;

                    var withinLine = isFirstPoint || isSecondPoint;
                    if (!withinLine)
                    {
                        node_x_rel_pos = Math.Floor(node.x / scaleFactor);
                        node_y_rel_pos = Math.Floor(node.y / scaleFactor);

                        var withinX = node_x_rel_pos >= boundary[0] && node_x_rel_pos <= boundary[2];
                        var withinY = node_y_rel_pos >= boundary[1] && node_y_rel_pos <= boundary[3];

                        //Inside the boundary
                        if (withinX && withinY)
                            nearbyPoints.Add(node);
                    }
                }
                //if no points are found we increase the area
                scaleFactor = scaleFactor * 4 / 3;
                tries++;
            }
            return nearbyPoints;
        }

        private static double[] boundaryCache = new double[4];

        private static double[] GetBoundary(Line line, float scaleFactor)
        {
            /* Giving a scaleFactor it returns an area around the line 
             * where we will search for nearby points 
             * */
            Node aNode = line.nodes[0];
            Node bNode = line.nodes[1];
            
            double min_x_position = Math.Floor(Math.Min(aNode.x, bNode.x) / scaleFactor);
            double min_y_position = Math.Floor(Math.Min(aNode.y, bNode.y) / scaleFactor);
            double max_x_position = Math.Floor(Math.Max(aNode.x, bNode.x) / scaleFactor);
            double max_y_position = Math.Floor(Math.Max(aNode.y, bNode.y) / scaleFactor);

            boundaryCache[0] = min_x_position;
            boundaryCache[1] = min_y_position;
            boundaryCache[2] = max_x_position;
            boundaryCache[3] = max_y_position;

            return boundaryCache;
        }
    }
}


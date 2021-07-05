using UnityEngine;
using System.Collections.Generic;

namespace ConcaveHull
{
    public class Demo : MonoBehaviour
    {
        List<Node> dots = new List<Node>(); //Used only for the demo

        public string seed;
        [Min(0.001f)] public int scaleFactor = 1;
        [Min(3)] public int initialDotCount = 100;
        [Range(-5, 5)] public float concavity = 1;

        private Hull hull;

        void Update()
        {
            RandomizeDots(initialDotCount); //Used only for the demo
            
            this.hull = new Hull(dots); 
            this.hull.Calculate(this.concavity, this.scaleFactor);
        }

        public void RandomizeDots(int initialDotCount)
        {
            this.dots.Clear();

            // This method is only used for the demo!
            System.Random pseudorandom = new System.Random(seed.GetHashCode());
            for (int x = 0; x < initialDotCount; x++)
            {
                dots.Add(new Node(pseudorandom.Next(0, 100), pseudorandom.Next(0, 100), x));
            }

            //Delete nodes that share same position
            for (int pivot_position = 0; pivot_position < dots.Count; pivot_position++)
            {
                for (int position = 0; position < dots.Count; position++)
                {
                    if (dots[pivot_position].x == dots[position].x && dots[pivot_position].y == dots[position].y
                        && pivot_position != position)
                    {
                        dots.RemoveAt(position);
                        position--;
                    }
                }
            }
        }

        // Unity demo visualization
        void OnDrawGizmos()
        {
            if (this.hull == null)
            {
                this.hull = new Hull(dots); 
                this.hull.Calculate(this.concavity, this.scaleFactor);
            }

            var previousColor = Gizmos.color;

            // Convex hull
            Gizmos.color = Color.yellow;
            for (int i = 0; i < this.hull.edges.Count; i++)
            {
                Vector2 left = new Vector2((float)this.hull.edges[i].nodes[0].x, (float)this.hull.edges[i].nodes[0].y);
                Vector2 right = new Vector2((float)this.hull.edges[i].nodes[1].x, (float)this.hull.edges[i].nodes[1].y);
                Gizmos.DrawLine(left, right);
            }

            // Concave hull
            Gizmos.color = Color.green;
            for (int i = 0; i < this.hull.concaveEdges.Count; i++)
            {
                Vector2 left = new Vector2((float)this.hull.concaveEdges[i].nodes[0].x, (float)this.hull.concaveEdges[i].nodes[0].y);
                Vector2 right = new Vector2((float)this.hull.concaveEdges[i].nodes[1].x, (float)this.hull.concaveEdges[i].nodes[1].y);
                Gizmos.DrawLine(left, right);
            }

            // Dots
            Gizmos.color = Color.white;
            for (int i = 0; i < dots.Count; i++)
            {
                Gizmos.DrawSphere(new Vector3((float)dots[i].x, (float)dots[i].y, 0), 0.5f);
            }

            Gizmos.color = previousColor;
        }
    }

}

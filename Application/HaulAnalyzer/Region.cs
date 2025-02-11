using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaulAnalyzer
{
    internal class Region
    {
        public List<PointD> Vertices = new List<PointD>();

        /// <summary>
        /// Clones the region
        /// </summary>
        /// <returns>Clone of the region</returns>
        public Region Clone
            (
            )
        {
            Region NewRegion = new Region();
            foreach (PointD V in Vertices)
            {
                NewRegion.Vertices.Add(new PointD(V.x, V.y));
            }

            return NewRegion;
        }
    }
}

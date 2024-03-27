using DS.GraphUtils.Entities;
using QuickGraph;
using Rhino.Geometry;

namespace DS.RevitCmd.EnergyTest
{
    public class EnergyVertex(int id, EnergySpace tag) :
        TaggedVertex<EnergySpace>(id, tag)
    {
    }
}

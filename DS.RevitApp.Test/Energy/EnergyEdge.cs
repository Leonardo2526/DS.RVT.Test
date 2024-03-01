using DS.GraphUtils.Entities;
using QuickGraph;
using Rhino.Geometry;

namespace DS.RevitApp.Test.Energy
{
    public class EnergyVertex(int id, EnergySpace tag) :
        TaggedVertex<EnergySpace>(id, tag)
    {
    }
}

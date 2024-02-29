using DS.GraphUtils.Entities;
using QuickGraph;

namespace DS.RevitApp.Test.Energy
{
    public class EnergyVertex(int id, EnergySpace tag) :
        TaggedVertex<EnergySpace>(id, tag)
    {
    }
}

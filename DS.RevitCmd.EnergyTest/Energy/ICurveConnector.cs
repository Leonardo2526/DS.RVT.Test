using Autodesk.Revit.DB;

namespace DS.RevitCmd.EnergyTest
{
    /// <summary>
    /// The interface that represents object to connect <see cref="Curve"/>s.
    /// </summary>
    internal interface ICurveConnector
    {
        /// <summary>
        /// Intersection point of connection.
        /// </summary>
        XYZ ConnectionPoint { get; }

        /// <summary>
        /// Try to connect <paramref name="sourceCurve"/> to <paramref name="targedCurve"/>.
        /// </summary>
        /// <param name="sourceCurve"></param>
        /// <param name="targedCurve"></param>
        /// <returns>
        /// Connected <see cref="Curve"/> or <see langword="null"/>.
        /// </returns>
        Curve TryConnect(Curve sourceCurve, Curve targedCurve);
    }
}
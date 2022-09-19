using Autodesk.Revit.DB;
using DS.RevitApp.Test.TransformTest;
using DS.RevitLib.Utils.Collisions.Checkers;
using DS.RevitLib.Utils.Collisions.Models;
using OLMP.RevitLib.MEPAC.Collisons.Resolvers.MEPBypass.ElementsTransfer.Resolvers;
using System.Collections.Generic;
using System.Linq;

namespace OLMP.RevitLib.MEPAC.Collisons.Resolvers.MEPBypass.ElementsTransfer
{
    internal class SolidCollisionClient
    {
        private readonly List<SolidElemCollision> _collisions;
        private readonly List<ICollisionChecker> _collisionCheckers;
        private readonly TargetLineModel _targetModel;
        private XYZ _currentPoint;
        private readonly double _minCurveLength;


        public SolidCollisionClient(List<SolidElemCollision> collisions, List<ICollisionChecker> collisionChecker, 
            TargetLineModel targetModel, double minCurveLength)
        {
            _collisions = collisions;
            _collisionCheckers = collisionChecker;
            _targetModel = targetModel;
            _currentPoint = targetModel.StartPlacementPoint;
            _minCurveLength = minCurveLength;
        }


        public void Resolve()
        {
            var collision = _collisions.First();

            var aclr = new AroundCenterLineRotateResolver(collision, _collisionCheckers);
            var clr = new RotateCenterLineResolver(collision, _collisionCheckers);

            aclr.SetSuccessor(clr); // if not resolved, rotate center line at 180 degeres.
            clr.SetSuccessor(aclr); // if not resolved, rotate around center line.
            aclr.Resolve();

            if (!aclr.IsResolved)
            {
                Solid totalIntersectionSolid = GetIntersectionSolid(_collisions);
                var mr = new MoveResolver(collision, _collisionCheckers, _currentPoint, _targetModel, totalIntersectionSolid, _minCurveLength);
                aclr = new AroundCenterLineRotateResolver(collision, _collisionCheckers);
                clr = new RotateCenterLineResolver(collision, _collisionCheckers);

                var currentCollisions = new List<SolidElemCollision>();
                currentCollisions.AddRange(_collisions);
                while (!mr.IsResolved)
                {
                    totalIntersectionSolid = GetIntersectionSolid(currentCollisions);
                    if (totalIntersectionSolid is null)
                    {
                        return;
                    }
                    mr = new MoveResolver(collision, _collisionCheckers, _currentPoint, _targetModel, totalIntersectionSolid, _minCurveLength);
                    aclr = new AroundCenterLineRotateResolver(collision, _collisionCheckers);
                    clr = new RotateCenterLineResolver(collision, _collisionCheckers);

                    mr.SetSuccessor(aclr);
                    aclr.SetSuccessor(clr); // if not resolved, rotate center line at 180 degeres.
                    clr.SetSuccessor(aclr); // if not resolved, rotate around center line.
                    mr.Resolve();
                    _currentPoint = mr.MovePoint;
                    if (_currentPoint is null)
                    {
                        return;
                    }

                    if (mr.UnresolvedCollisions is not null && mr.UnresolvedCollisions.Any())
                    {
                        collision = (SolidElemCollision)mr.UnresolvedCollisions.First();
                        currentCollisions = mr.UnresolvedCollisions.Cast<SolidElemCollision>().ToList();
                    }
                }
            }
        }

        private Solid GetIntersectionSolid(List<SolidElemCollision> collisions)
        {
            var solid = collisions.Select(obj => obj.GetIntersection()).ToList();
            return DS.RevitLib.Utils.Solids.SolidUtils.UniteSolids(solid);
        }
    }
}

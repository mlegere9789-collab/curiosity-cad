using System;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace Curiosity.Plugin.Execution
{
    /// <summary>
    /// Geometry math for constraint-style intents (angle/parallel/perpendicular). Kept separate from
    /// CommandExecutor so it's unit-testable independent of a live AutoCAD Transaction/Database.
    /// </summary>
    public static class GeometrySolver
    {
        /// <summary>
        /// Rotates/repositions <paramref name="target"/> so it intersects <paramref name="reference"/>
        /// at the given angle (degrees), pivoting about the target's existing intersection point with
        /// the reference line (or its nearest endpoint if the lines don't currently intersect).
        /// </summary>
        public static void RotateToAngleFromReference(Line target, Line reference, double degrees)
        {
            var referenceDirection = (reference.EndPoint - reference.StartPoint).GetNormal();
            var pivot = GetOrProjectIntersectionPoint(target, reference);

            var radians = degrees * Math.PI / 180.0;
            var rotatedDirection = referenceDirection.RotateBy(radians, Vector3d.ZAxis);

            var currentLength = target.Length;
            target.StartPoint = pivot;
            target.EndPoint = pivot + rotatedDirection * currentLength;
        }

        private static Point3d GetOrProjectIntersectionPoint(Line target, Line reference)
        {
            var points = new Point3dCollection();
            target.IntersectWith(reference, Intersect.ExtendBoth, points, IntPtr.Zero, IntPtr.Zero);

            if (points.Count > 0)
                return points[0];

            // Lines don't intersect even when extended (parallel case) — fall back to the target's
            // own start point as the pivot. Revisit once tested against real classmate drawings.
            return target.StartPoint;
        }
    }
}

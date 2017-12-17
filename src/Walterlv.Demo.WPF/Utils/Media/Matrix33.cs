using System.Windows;

namespace Walterlv.Demo.Media
{
    /// <summary>
    /// Pepresents a Matrix that contains 9 numbers which uses more numbers than WPF Matrix class.
    /// This takes advantages to perspective transformation.
    /// [ M11 M12 M13 ]
    /// [ M21 M22 M23 ]
    /// [ M31 M32 M33 ]
    /// </summary>
    public struct Matrix33
    {
        public Matrix33(
            double m11, double m12, double m13,
            double m21, double m22, double m23,
            double m31, double m32, double m33)
        {
            M11 = m11;
            M12 = m12;
            M13 = m13;
            M21 = m21;
            M22 = m22;
            M23 = m23;
            M31 = m31;
            M32 = m32;
            M33 = m33;
        }

        public double M11 { get; private set; }
        public double M12 { get; private set; }
        public double M13 { get; private set; }
        public double M21 { get; private set; }
        public double M22 { get; private set; }
        public double M23 { get; private set; }
        public double M31 { get; private set; }
        public double M32 { get; private set; }
        public double M33 { get; private set; }

        public void Adjoint()
        {
            var result = new Matrix33(
                M22 * M33 - M23 * M32, M23 * M31 - M21 * M33, M21 * M32 - M22 * M31,
                M13 * M32 - M12 * M33, M11 * M33 - M13 * M31, M12 * M31 - M11 * M32,
                M12 * M23 - M13 * M22, M13 * M21 - M11 * M23, M11 * M22 - M12 * M21);
            AssignFrom(result);
        }

        public void AssignFrom(Matrix33 matrix)
        {
            M11 = matrix.M11;
            M12 = matrix.M12;
            M13 = matrix.M13;
            M21 = matrix.M21;
            M22 = matrix.M22;
            M23 = matrix.M23;
            M31 = matrix.M31;
            M32 = matrix.M32;
            M33 = matrix.M33;
        }

        public static Matrix33 SquareToQuadrilateral(Point p0, Point p1, Point p2, Point p3)
        {
            var dx3 = p0.X - p1.X + p2.X - p3.X;
            var dy3 = p0.Y - p1.Y + p2.Y - p3.Y;
            if (dx3 is 0.0 && dy3 is 0.0)
            {
                // 这就是普通的仿射变换 Matrix。
                var result = new Matrix33(
                    p1.X - p0.X, p2.X - p1.X, p0.X,
                    p1.Y - p0.Y, p2.Y - p1.Y, p0.Y,
                    0.0f, 0.0f, 1.0f);
                return result;
            }
            else
            {
                // 这是透视变换 Matrix。
                var dx1 = p1.X - p2.X;
                var dx2 = p3.X - p2.X;
                var dy1 = p1.Y - p2.Y;
                var dy2 = p3.Y - p2.Y;
                var denominator = dx1 * dy2 - dx2 * dy1;
                var a13 = (dx3 * dy2 - dx2 * dy3) / denominator;
                var a23 = (dx1 * dy3 - dx3 * dy1) / denominator;
                var result = new Matrix33(
                    p1.X - p0.X + a13 * p1.X, p3.X - p0.X + a23 * p3.X, p0.X,
                    p1.Y - p0.Y + a13 * p1.Y, p3.Y - p0.Y + a23 * p3.Y, p0.Y,
                    a13, a23, 1.0f);
                return result;
            }
        }

        public static Matrix33 QuadrilateralToSquare(Point p0, Point p1, Point p2, Point p3)
        {
            var result = SquareToQuadrilateral(p0, p1, p2, p3);
            result.Adjoint();
            return result;
        }
    }
}

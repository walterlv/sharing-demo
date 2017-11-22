using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace Walterlv.Demo.Media
{
    /// <summary>
    /// 包含变换矩阵的相关计算。
    /// </summary>
    public class TransformMatrix
    {
        /// <summary>
        /// 为 <see cref="MatrixToGroup"/> 方法提供变换中心的指定方法。
        /// 可以从 <see cref="TransformMatrix"/> 中获取到几个预设的变换中心指定方法。
        /// </summary>
        /// <param name="scalingFactor">先进行缩放后进行旋转时，旋转中心的计算可能需要考虑前面缩放后的坐标。此参数可以得知缩放比。</param>
        /// <returns>绝对坐标的缩放中心和旋转中心。</returns>
        public delegate (Point ScalingCenter, Point RotationCenter) CenterSpecification(Vector scalingFactor);

        /// <summary>
        /// 对于给定的变换矩阵 <paramref name="matrix"/>，求出一个可以模拟此变换矩阵的缩放、旋转、平移变换组。
        /// <para>默认情况下变换中心都是元素的左上角点 (0, 0)，也可以通过指定参数 <paramref name="specifyCenter"/> 修改这一行为。</para>
        /// </summary>
        /// <param name="matrix">需要解构的变换矩阵（如果是对 UI 元素进行解构，传入的 Matrix 需要额外考虑 RenderTransformOrigin。）</param>
        /// <param name="specifyCenter">
        /// 如果需要分别为缩放和旋转指定变换中心，则在此参数中通过委托给定。
        /// <para>委托需要返回两个值，第一个是缩放中心（绝对坐标），相对于未进行任何变换时的元素；第二个是旋转中心（绝对坐标），相对于缩放后的元素（缩放比可通过委托参数获取）。</para>
        /// </param>
        /// <returns>按缩放、旋转、平移顺序返回变换参数，可直接应用到对应的 <see cref="Transform"/> 中。</returns>
        public static (Vector Scaling, double Rotation, Vector Translation) MatrixToGroup(
            Matrix matrix, CenterSpecification specifyCenter = null)
        {
            // 生成一个单位矩形（0, 0, 1, 1），计算单位矩形经矩阵变换后形成的带旋转的矩形。
            // 于是，我们将可以通过比较这两个矩形中点的数据来求出一个解。
            var unitPoints = new[] {new Point(0, 0), new Point(1, 0), new Point(1, 1), new Point(0, 1)};
            var transformedPoints = unitPoints.Select(matrix.Transform).ToArray();

            // 测试单位矩形宽高的长度变化量，以求出缩放比（作为参数 specifyCenter 中变换中心的计算参考）。
            var scaling = new Vector(
                (transformedPoints[1] - transformedPoints[0]).Length,
                (transformedPoints[3] - transformedPoints[0]).Length);
            // 测试单位向量的旋转变化量，以求出旋转角度。
            var rotation = Vector.AngleBetween(new Vector(1, 0), transformedPoints[1] - transformedPoints[0]);
            var translation = transformedPoints[0] - unitPoints[0];

            // 如果指定了变换分量的变换中心点。
            if (specifyCenter != null)
            {
                // 那么，就获取指定的变换中心点（缩放中心和旋转中心）。
                var (scalingCenter, rotationCenter) = specifyCenter(scaling);

                // 如果 S 表示所求变换的缩放分量，R 表示所求变换的旋转分量，T 表示所求变换的平移分量；M 表示传入的目标矩阵。
                // 那么，S 将可以通过缩放比和参数指定的缩放中心唯一确定；R 将可以通过旋转角度和参数指定的旋转中心唯一确定。
                // S = scaleMatrix; R = rotateMatrix.
                var scaleMatrix = Matrix.Identity;
                scaleMatrix.ScaleAt(scaling.X, scaling.Y, scalingCenter.X, scalingCenter.Y);
                var rotateMatrix = Matrix.Identity;
                rotateMatrix.RotateAt(rotation, rotationCenter.X, rotationCenter.Y);

                // T 是不确定的，它会受到 S 和 T 的影响；但确定等式 SRT=M，即 T=S^{-1}R^{-1}M。
                // T = translateMatrix; M = matrix.
                scaleMatrix.Invert();
                rotateMatrix.Invert();
                var translateMatrix = Matrix.Multiply(rotateMatrix, scaleMatrix);
                translateMatrix = Matrix.Multiply(translateMatrix, matrix);

                // 用考虑了变换中心的平移量覆盖总的平移分量。
                translation = new Vector(translateMatrix.OffsetX, translateMatrix.OffsetY);
            }

            // 按缩放、旋转、平移来返回变换分量。
            return (scaling, rotation, translation);
        }
    }
}

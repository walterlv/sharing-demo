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
        /// 对于给定的变换矩阵 <paramref name="matrix"/>，求出一个可以模拟此变换矩阵的缩放、旋转、平移变换组。
        /// <para>默认情况下变换中心都是元素的左上角点 (0, 0)，也可以通过指定参数 <paramref name="specifyCenter"/> 修改这一行为。</para>
        /// </summary>
        /// <param name="matrix">需要解构的变换矩阵（如果是对 UI 元素进行解构，传入的 Matrix 需要额外考虑 RenderTransformOrigin。）</param>
        /// <param name="specifyCenter">
        /// 如果需要分别为缩放和旋转指定变换中心，则在此参数中通过委托给定（建议从 <see cref="Centers"/> 中选择预设的委托）。
        /// <para>委托需要返回两个值，第一个是缩放中心（绝对坐标），相对于未进行任何变换时的元素；第二个是旋转中心（绝对坐标），相对于缩放后的元素（缩放比可通过委托参数获取）。</para>
        /// <para>此委托不需要考虑 <see cref="UIElement.RenderTransformOrigin"/>，只关心最终效果。</para>
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

        /// <summary>
        /// 为 <see cref="MatrixToGroup"/> 方法提供变换中心的指定方法。
        /// 可以从 <see cref="TransformMatrix"/> 中获取到几个预设的变换中心指定方法。
        /// </summary>
        /// <param name="scalingFactor">先进行缩放后进行旋转时，旋转中心的计算可能需要考虑前面缩放后的坐标。此参数可以得知缩放比。</param>
        /// <returns>绝对坐标的缩放中心和旋转中心。</returns>
        public delegate (Point ScalingCenter, Point RotationCenter) CenterSpecification(Vector scalingFactor);

        /// <summary>
        /// 包含变换中心点计算的常用计算方法。
        /// </summary>
        public static class Centers
        {
            /// <summary>
            /// 缩放中心是 (0, 0)，旋转中心是 (0.5, 0.5)。<para/>
            /// </summary>
            /// <param name="originalSize">要应用变换组的元素尺寸。</param>
            /// <returns></returns>
            public static CenterSpecification ScaleAtZeroRotateAtCenter(Size originalSize)
            {
                return scalingFactor => (new Point(), new Point(
                    originalSize.Width * scalingFactor.X / 2,
                    originalSize.Height * scalingFactor.Y / 2));
            }
        }

        /// <summary>
        /// 包含生成变换组的常用计算方法。
        /// </summary>
        public static class GroupGenerator
        {
            /// <summary>
            /// 生成一个变换组，并应用到目标元素。此变换组不含缩放（意味着元素需要自己用宽高乘以缩放比来适应变换），
            /// 但会按元素的 <see cref="UIElement.RenderTransformOrigin"/> 为中心进行旋转。
            /// <para>当使用 <see cref="MatrixToGroup"/> 生成变换组数据时，可以将得到的结果作为参数传入。</para>
            /// </summary>
            /// <param name="rotation"><see cref="MatrixToGroup"/> 计算得到的旋转角度。</param>
            /// <param name="translation"><see cref="MatrixToGroup"/> 计算得到的平移向量。</param>
            /// <param name="originalSize">元素的原始宽高（而非为适应变换而计算后的宽高）。</param>
            /// <returns>可以用来设置给元素的变换组。</returns>
            public static TransformGroup NoScaleButRotateAtOrigin(
                double rotation, Vector translation, Size originalSize)
            {
                var group = new TransformGroup();
                group.Children.Add(new RotateTransform {Angle = rotation});
                group.Children.Add(new TranslateTransform {X = translation.X, Y = translation.Y});
                return group;
            }

            /// <summary>
            /// 生成一个变换组，并应用到目标元素。此变换以 (0, 0) 进行缩放，以 (0.5, 0.5) 进行旋转，随后进行平移。
            /// 由于目标元素可能设置了 <see cref="UIElement.RenderTransformOrigin"/> 属性导致以上中心点的计算变得复杂，所以可以将其传入以便抵消它的影响。
            /// <para>当使用 <see cref="MatrixToGroup"/> 生成变换组数据时，可以将得到的结果作为参数传入。</para>
            /// </summary>
            /// <param name="scaling"><see cref="MatrixToGroup"/> 计算得到的缩放比。</param>
            /// <param name="rotation"><see cref="MatrixToGroup"/> 计算得到的旋转角度。</param>
            /// <param name="translation"><see cref="MatrixToGroup"/> 计算得到的平移向量。</param>
            /// <param name="originalSize">元素的原始宽高。</param>
            /// <param name="renderTransformOrigin"></param>
            /// <returns>可以用来设置给元素的变换组。</returns>
            public static TransformGroup ScaleAtZeroRotateAtCenter(
                Vector scaling, double rotation, Vector translation,
                Size originalSize, Point renderTransformOrigin = default(Point))
            {
                var group = new TransformGroup();
                var scaleTransform = new ScaleTransform
                {
                    ScaleX = scaling.X,
                    ScaleY = scaling.Y,
                    CenterX = -originalSize.Width * renderTransformOrigin.X,
                    CenterY = -originalSize.Height * renderTransformOrigin.Y,
                };
                var rotateTransform = new RotateTransform
                {
                    Angle = rotation,
                    CenterX = originalSize.Width * (scaling.X / 2 - renderTransformOrigin.X),
                    CenterY = originalSize.Height * (scaling.Y / 2 - renderTransformOrigin.Y),
                };
                group.Children.Add(scaleTransform);
                group.Children.Add(rotateTransform);
                group.Children.Add(new TranslateTransform {X = translation.X, Y = translation.Y});
                return group;
            }
        }
    }
}

using IxMilia.BCad.Extensions;
using IxMilia.BCad.Helpers;
using Xunit;

namespace IxMilia.BCad.Core.Test
{
    public class MatrixTests
    {
        [Fact]
        public void MatrixIdentityTest()
        {
            Assert.True(Matrix4.Identity.IsIdentity);
            Assert.False(new Matrix4().IsIdentity);
        }

        [Fact]
        public void InvertIdentityMatrixTest()
        {
            var matrix = Matrix4.Identity.Inverse();
            Assert.True(matrix.IsIdentity);
        }

        [Fact]
        public void InvertArbitrartyMatrixTest()
        {
            var matrix = new Matrix4(
                3, 0, 2, -1,
                1, 2, 0, -2,
                4, 0, 6, -3,
                5, 0, 2, 0).Inverse();
            var expected = new Matrix4(
                 0.6, 0,   -0.2, 0,
                -2.5, 0.5,  0.5, 1,
                -1.5, 0,    0.5, 0.5,
                -2.2, 0,    0.4, 1);
            Assert.True(MathHelper.CloseTo(expected, matrix));
        }

        [Fact]
        public void MatrixIdentityMultiplicationTest()
        {
            var matrix = new Matrix4(
                1, 2, 3, 4,
                5, 6, 7, 8,
                9, 10, 11, 12,
                13, 14, 15, 16);
            var result1 = matrix * Matrix4.Identity;
            var result2 = Matrix4.Identity * matrix;
            Assert.True(MathHelper.CloseTo(matrix, result1));
            Assert.True(MathHelper.CloseTo(matrix, result2));
        }

        [Fact]
        public void MultiplyArbitraryMatrixTest()
        {
            var a = new Matrix4(
                9, 7, 5, 3,
                3, 5, 7, 9,
                9, 7, 5, 3,
                1, 3, 5, 7);
            var b = new Matrix4(
                1, 3, 5, 7,
                7, 5, 3, 1,
                1, 3, 5, 7,
                9, 7, 5, 3);
            var result = a * b;
            var expected = new Matrix4(
                90, 98, 106, 114,
                126, 118, 110, 102,
                90, 98, 106, 114,
                90, 82, 74, 66);
            Assert.True(MathHelper.CloseTo(expected, result));
        }

        [Fact]
        public void GenerateScalingMatrixTest()
        {
            var scale = Matrix4.CreateScale(new Vector(1, 2, 3));
            Assert.Equal(1.0, scale.M11);
            Assert.Equal(0.0, scale.M12);
            Assert.Equal(0.0, scale.M13);
            Assert.Equal(0.0, scale.M14);
            Assert.Equal(0.0, scale.M21);
            Assert.Equal(2.0, scale.M22);
            Assert.Equal(0.0, scale.M23);
            Assert.Equal(0.0, scale.M24);
            Assert.Equal(0.0, scale.M31);
            Assert.Equal(0.0, scale.M32);
            Assert.Equal(3.0, scale.M33);
            Assert.Equal(0.0, scale.M34);
            Assert.Equal(0.0, scale.M41);
            Assert.Equal(0.0, scale.M42);
            Assert.Equal(0.0, scale.M43);
            Assert.Equal(1.0, scale.M44);
        }

        [Fact]
        public void SimpleScalingTest()
        {
            var vector = new Vector(1, 2, 3);
            var scale = new Vector(5, 5, 5);
            var scaling = Matrix4.CreateScale(scale);
            var result = scaling * vector;
            var expected = new Vector(5, 10, 15);
            Assert.True(expected.CloseTo(result));
        }

        [Fact]
        public void CreateTranslatingMatrixTest()
        {
            var translate = Matrix4.CreateTranslate(new Vector(1, 2, 3));
            Assert.Equal(1.0, translate.M11);
            Assert.Equal(0.0, translate.M12);
            Assert.Equal(0.0, translate.M13);
            Assert.Equal(1.0, translate.M14);
            Assert.Equal(0.0, translate.M21);
            Assert.Equal(1.0, translate.M22);
            Assert.Equal(0.0, translate.M23);
            Assert.Equal(2.0, translate.M24);
            Assert.Equal(0.0, translate.M31);
            Assert.Equal(0.0, translate.M32);
            Assert.Equal(1.0, translate.M33);
            Assert.Equal(3.0, translate.M34);
            Assert.Equal(0.0, translate.M41);
            Assert.Equal(0.0, translate.M42);
            Assert.Equal(0.0, translate.M43);
            Assert.Equal(1.0, translate.M44);
        }

        [Fact]
        public void SimpleTranslateTest()
        {
            var vector = new Vector(1, 2, 3);
            var translate = new Vector(5, 5, 5);
            var translating = Matrix4.CreateTranslate(translate);
            var result = translating * vector;
            var expected = new Vector(6, 7, 8);
            Assert.True(expected.CloseTo(result));
        }

        [Fact]
        public void ScaleThenTranslateTest()
        {
            var vector = new Vector(1, 1, 1);
            var scaling = Matrix4.CreateScale(new Vector(2, 2, 2));
            var translating = Matrix4.CreateTranslate(new Vector(10, 10, 10));
            var transformation = translating * scaling;
            var result = transformation * vector;
            var expected = new Vector(12, 12, 12);
            Assert.True(expected.CloseTo(result));
        }
    }
}

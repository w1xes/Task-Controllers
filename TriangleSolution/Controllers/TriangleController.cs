using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Triangles.Models;

namespace Triangles.Controllers
{
    [Route("triangle")]
    public class TriangleController : Controller
    {
        private const double EqualityTolerancePercent = 0.00001d;

        [HttpGet("Info")]
        public string Info(double side1, double side2, double side3)
        {
            return Info(new Triangle(side1, side2, side3));
        }

        public string Info(Triangle triangle)
        {
            ValidateTriangleForContext(triangle);

            var sides = SortSides(triangle);
            var perimeter = Perimeter(triangle);
            var area = ParseArea(triangle);

            return string.Format(
                "Triangle:{0}({1}, {2}, {3}){0}Reduced:{0}({4:F2}, {5:F2}, {6:F2}){0}{0}Area = {7:F2}{0}Perimeter = {8}",
                Environment.NewLine,
                sides[0],
                sides[1],
                sides[2],
                sides[0] / perimeter,
                sides[1] / perimeter,
                sides[2] / perimeter,
                area,
                perimeter);
        }

        [HttpGet("Area")]
        public string Area(double side1, double side2, double side3)
        {
            return Area(new Triangle(side1, side2, side3));
        }

        public string Area(Triangle triangle)
        {
            ValidateTriangleForContext(triangle);
            return ParseArea(triangle).ToString("F4");
        }

        [HttpGet("Perimeter")]
        public double Perimeter(double side1, double side2, double side3)
        {
            return Perimeter(new Triangle(side1, side2, side3));
        }

        public double Perimeter(Triangle triangle)
        {
            ValidateTriangleForContext(triangle);
            return triangle.Side1 + triangle.Side2 + triangle.Side3;
        }

        [HttpGet("IsRightAngled")]
        public bool IsRightAngled(double side1, double side2, double side3)
        {
            return IsRightAngled(new Triangle(side1, side2, side3));
        }

        public bool IsRightAngled(Triangle triangle)
        {
            ValidateTriangleForContext(triangle);
            var sides = SortSides(triangle);
            return AreAlmostEqual(sides[0] * sides[0] + sides[1] * sides[1], sides[2] * sides[2]);
        }

        [HttpGet("IsEquilateral")]
        public bool IsEquilateral(double side1, double side2, double side3)
        {
            return IsEquilateral(new Triangle(side1, side2, side3));
        }

        public bool IsEquilateral(Triangle triangle)
        {
            ValidateTriangleForContext(triangle);
            var sides = SortSides(triangle);
            return AreAlmostEqual(sides[0], sides[1]) && AreAlmostEqual(sides[1], sides[2]) && AreAlmostEqual(sides[0], sides[2]);
        }

        [HttpGet("IsIsosceles")]
        public bool IsIsosceles(double side1, double side2, double side3)
        {
            return IsIsosceles(new Triangle(side1, side2, side3));
        }

        public bool IsIsosceles(Triangle triangle)
        {
            ValidateTriangleForContext(triangle);
            var sides = SortSides(triangle);
            return AreAlmostEqual(sides[0], sides[1]) || AreAlmostEqual(sides[1], sides[2]) || AreAlmostEqual(sides[0], sides[2]);
        }

        [HttpGet("AreCongruent")]
        public bool AreCongruent(Triangle tr1, Triangle tr2)
        {
            ValidateTriangleForContext(tr1);
            ValidateTriangleForContext(tr2);

            var first = SortSides(tr1);
            var second = SortSides(tr2);

            return AreAlmostEqual(first[0], second[0])
                && AreAlmostEqual(first[1], second[1])
                && AreAlmostEqual(first[2], second[2]);
        }

        [HttpGet("AreSimilar")]
        public bool AreSimilar(Triangle tr1, Triangle tr2)
        {
            ValidateTriangleForContext(tr1);
            ValidateTriangleForContext(tr2);

            var first = SortSides(tr1);
            var second = SortSides(tr2);

            var firstRatios = new[] { first[0] / first[2], first[1] / first[2], 1d };
            var secondRatios = new[] { second[0] / second[2], second[1] / second[2], 1d };

            return AreAlmostEqual(firstRatios[0], secondRatios[0])
                && AreAlmostEqual(firstRatios[1], secondRatios[1]);
        }

        [HttpGet("GreatestByPerimeter")]
        [HttpGet("GreatesByPerimeter")]
        public string InfoGreatestPerimeter(Triangle[] tr)
        {
            if (tr == null || tr.Length == 0)
            {
                throw new ArgumentException("Triangle array cannot be empty.");
            }

            var triangle = tr.OrderByDescending(Perimeter).First();
            return Info(triangle);
        }

        [HttpGet("GreatestByArea")]
        public string InfoGreatestArea(Triangle[] tr)
        {
            if (tr == null || tr.Length == 0)
            {
                throw new ArgumentException("Triangle array cannot be empty.");
            }

            var triangle = tr.OrderByDescending(ParseArea).First();
            return Info(triangle);
        }

        [HttpGet("PairwiseNonSimilar")]
        public string NumbersPairwiseNotSimilar(Triangle[] tr)
        {
            if (tr == null || tr.Length == 0)
            {
                return string.Empty;
            }

            var pairs = new System.Collections.Generic.List<string>();

            for (var i = 0; i < tr.Length; i++)
            {
                for (var j = i + 1; j < tr.Length; j++)
                {
                    if (!AreSimilar(tr[i], tr[j]))
                    {
                        pairs.Add($"({i + 1}, {j + 1})");
                    }
                }
            }

            return string.Join(Environment.NewLine, pairs);
        }

        private void ValidateTriangleForContext(Triangle triangle)
        {
            if (ControllerContext != null && ControllerContext.HttpContext != null)
            {
                ValidateRequestTriangle(triangle);
                return;
            }

            ValidateTriangle(triangle);
        }

        private static void ValidateTriangle(Triangle triangle)
        {
            if (triangle == null)
            {
                throw new ArgumentNullException(nameof(triangle));
            }

            if (SortSides(triangle).Any(side => side <= 0))
            {
                throw new ArgumentException("Each side must be positive.");
            }
        }

        private static void ValidateRequestTriangle(Triangle triangle)
        {
            ValidateTriangle(triangle);

            var sides = SortSides(triangle);
            if (sides[2] >= sides[0] + sides[1])
            {
                throw new ArgumentException("The triangle is invalid.");
            }
        }

        private static double[] SortSides(Triangle triangle)
        {
            return new[] { triangle.Side1, triangle.Side2, triangle.Side3 }.OrderBy(side => side).ToArray();
        }

        private static bool AreAlmostEqual(double first, double second)
        {
            var tolerance = Math.Abs(first) * EqualityTolerancePercent;
            if (tolerance < 1e-8)
            {
                tolerance = 1e-8;
            }

            return Math.Abs(first - second) <= tolerance;
        }

        private static double ParseArea(Triangle triangle)
        {
            ValidateTriangle(triangle);

            var sides = SortSides(triangle);
            var semiPerimeter = (sides[0] + sides[1] + sides[2]) / 2d;
            return Math.Sqrt(semiPerimeter * (semiPerimeter - sides[0]) * (semiPerimeter - sides[1]) * (semiPerimeter - sides[2]));
        }
    }
}

// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.IO;
using System.Linq;
using IxMilia.BCad.Entities;
using IxMilia.BCad.FileHandlers.Extensions;
using IxMilia.Dxf;
using IxMilia.Dxf.Entities;
using Xunit;

namespace IxMilia.BCad.FileHandlers.Test
{
    public class DxfFileHandlerTests : FileHandlerTestsBase
    {
        public override IFileHandler FileHandler => new DxfFileHandler();

        protected override Entity RoundTripEntity(Entity entity)
        {
            // shortcut to avoid file writing/reading
            return entity.ToDxfEntity(new Layer("layer")).ToEntity();
        }

        private DxfFile WriteToFile(Drawing drawing)
        {
            using (var stream = WriteToStream(drawing))
            {
                return DxfFile.Load(stream);
            }
        }

        [Fact]
        public void RoundTripColorTest()
        {
            VerifyRoundTrip(new Line(Point.Origin, Point.Origin, color: null));
            VerifyRoundTrip(new Line(Point.Origin, Point.Origin, CadColor.Red));
            VerifyRoundTrip(new Line(Point.Origin, Point.Origin, CadColor.FromArgb(255, 1, 2, 5)));
        }

        [Fact]
        public void RoundTripArcTest()
        {
            VerifyRoundTrip(new Arc(new Point(1.0, 2.0, 3.0), 4.0, 5.0, 6.0, Vector.YAxis, CadColor.Yellow, thickness: 1.2345));
        }

        [Fact]
        public void RoundTripCircleTest()
        {
            VerifyRoundTrip(new Circle(new Point(1.0, 2.0, 3.0), 4.0, Vector.XAxis, CadColor.Red, thickness: 1.2345));
        }

        [Fact]
        public void RoundTripLineTest()
        {
            VerifyRoundTrip(new Line(new Point(1.0, 2.0, 3.0), new Point(4.0, 5.0, 6.0), CadColor.Green, thickness: 1.2345));
        }

        [Fact]
        public void RoundTripPolylineTest()
        {
            // 90 degree arc from (0,0) to (1,1) representing the fourth quadrant
            VerifyRoundTrip(new Polyline(new[]
            {
                new Vertex(Point.Origin),
                new Vertex(new Point(1.0, 1.0, 0.0), 90.0, VertexDirection.CounterClockwise)
            }));

            // 90 degree arc from (0,0) to (1,1) representing the second quadrant
            VerifyRoundTrip(new Polyline(new[]
            {
                new Vertex(Point.Origin),
                new Vertex(new Point(1.0, 1.0, 0.0), 90.0, VertexDirection.Clockwise)
            }));
        }

        [Fact]
        public void RoundTripLayerTest()
        {
            // don't try to round-trip a null layer color; DXF always resets it to white
            VerifyRoundTrip(new Layer("name", color: CadColor.White));
            VerifyRoundTrip(new Layer("name", color: CadColor.Red));
            VerifyRoundTrip(new Layer("name", color: CadColor.White, isVisible: false));
        }

        [Fact]
        public void VerifyDefaultDxfVersionInDirectWriteTest()
        {
            var file = WriteToFile(new Drawing());
            Assert.Equal(DxfAcadVersion.R12, file.Header.Version);
        }

        [Fact]
        public void VerifyNonDefaultVersionIsPreservedInDirectWriteTest()
        {
            var file = new DxfFile();
            file.Header.Version = DxfAcadVersion.R2007;
            using (var ms = new MemoryStream())
            {
                file.Save(ms);
                ms.Seek(0, SeekOrigin.Begin);
                var drawing = ReadFromStream(ms);
                var result = WriteToFile(drawing);

                // file read from disk has preserved version
                Assert.Equal(DxfAcadVersion.R2007, result.Header.Version);
            }
        }

        [Fact]
        public async void VerifyDefaultDxfVersionInWriteTest()
        {
            using (var ms = new MemoryStream())
            {
                var drawing = new Drawing();

                // write file with defaults
                Assert.True(await Workspace.ReaderWriterService.TryWriteDrawing("filename.dxf", drawing, ViewPort.CreateDefaultViewPort(), ms, preserveSettings: false));

                // verify that the written default is correct
                ms.Seek(0, SeekOrigin.Begin);
                var file = DxfFile.Load(ms);
                Assert.Equal(DxfAcadVersion.R12, file.Header.Version);
            }
        }

        [Fact]
        public async void VerifyNonDefaultVersionIsPreservedInWriteTest()
        {
            using (var ms1 = new MemoryStream())
            {
                var drawing = new Drawing();

                // write file as R13
                using (new DxfFileSettingsProvider(new DxfFileSettings() { FileVersion = DxfFileVersion.R13 }))
                {
                    Assert.True(await Workspace.ReaderWriterService.TryWriteDrawing("filename.dxf", drawing, ViewPort.CreateDefaultViewPort(), ms1, preserveSettings: false));
                }

                // verify that it was written as such
                ms1.Seek(0, SeekOrigin.Begin);
                var file = DxfFile.Load(ms1);
                Assert.Equal(DxfAcadVersion.R13, file.Header.Version);

                using (var ms2 = new MemoryStream())
                {
                    // write it again
                    Assert.True(await Workspace.ReaderWriterService.TryWriteDrawing("filename.dxf", drawing, ViewPort.CreateDefaultViewPort(), ms2, preserveSettings: true));

                    // verify again that it was written correctly without specifying drawing settings
                    ms2.Seek(0, SeekOrigin.Begin);
                    file = DxfFile.Load(ms2);
                    Assert.Equal(DxfAcadVersion.R13, file.Header.Version);
                }
            }
        }

        [Fact]
        public void ReadLwPolylineTest()
        {
            var lwpoly = new DxfLwPolyline(new[]
            {
                new DxfLwPolylineVertex() { X = 1.0, Y = 2.0 },
                new DxfLwPolylineVertex() { X = 3.0, Y = 4.0 }
            })
            {
                Elevation = 12.0
            };
            var poly = (Polyline)lwpoly.ToEntity();
            Assert.Equal(2, poly.Vertices.Count());
            Assert.Equal(new Point(1.0, 2.0, 12.0), poly.Vertices.First().Location);
            Assert.Equal(new Point(3.0, 4.0, 12.0), poly.Vertices.Last().Location);
        }

        [Fact]
        public void ArcWithNegativeNormalTest()
        {
            // gleaned from https://github.com/ixmilia/dxf/issues/105
            // A 90 degree arc from 270-360 degrees with a negative Z normal is displayed by both AutoCAD and Teigha in Q3.
            var dxfArc = new DxfArc(DxfPoint.Origin, 1.0, 270.0, 360.0)
            {
                Normal = new DxfVector(0.0, 0.0, -1.0)
            };
            var arc = (Arc)dxfArc.ToEntity();
            Assert.True(arc.MidPoint.X < 0.0);
            Assert.True(arc.MidPoint.Y < 0.0);
            AssertClose(Math.Abs(arc.MidPoint.X), Math.Abs(arc.MidPoint.Y));
        }
    }
}

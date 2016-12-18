using Catrobat.Paint.WindowsPhone.Command;
using System;
using System.Collections.Generic;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace Catrobat.Paint.WindowsPhone.Tool
{
    //TODO: performance critical... doing some optimizations

    class EraserTool : ToolBase
    {
        private Path _path;
        private PathGeometry _pathGeometry;
        private PathFigureCollection _pathFigureCollection;
        private PathFigure _pathFigure;
        private PathSegmentCollection _pathSegmentCollection;
        private Point _lastPoint;
        private bool _lastPointSet;
        private List<Point> _points;
        private PixelData.PixelData _pixelDataEraser;
        private PixelData.PixelData _pixelData;

        public EraserTool()
        {
            ToolType = ToolType.Eraser;
            _lastPointSet = false;
            _points = new List<Point>();
            _pixelData = new PixelData.PixelData();
            _pixelDataEraser = new PixelData.PixelData();
            InitPathInstances();
            InitPathStrokeSettings();
        }

        private void InitPathInstances()
        {
            _path = new Path();
            _pathGeometry = new PathGeometry();
            _pathFigureCollection = new PathFigureCollection();
            _pathFigure = new PathFigure();
            _pathSegmentCollection = new PathSegmentCollection();
        }

        private void InitPathStrokeSettings()
        {
            _path.StrokeLineJoin = PenLineJoin.Round;
            _path.Stroke = new SolidColorBrush(Color.FromArgb(0xff, 0xff, 0xff, 0xff));
            _path.StrokeThickness = PocketPaintApplication.GetInstance().PaintData.thicknessSelected;
            _path.StrokeStartLineCap = PocketPaintApplication.GetInstance().PaintData.penLineCapSelected;
            _path.StrokeEndLineCap = PocketPaintApplication.GetInstance().PaintData.penLineCapSelected;
        }

        public override async void HandleDown(object arg)
        {
            if (!(arg is Point))
            {
                return;
            }
            _lastPointSet = false;
            _points = new List<Point>();
            var coordinate = (Point)arg;
            await _pixelData.preparePaintingAreaCanvasPixel();

            InitPathInstances();
            InitPathStrokeSettings();

            _pathFigure.StartPoint = coordinate;
            _pathFigure.Segments = _pathSegmentCollection;
            _pathFigureCollection.Add(_pathFigure);
            _pathGeometry.Figures = _pathFigureCollection;
            _lastPoint = coordinate;
            _path.Data = _pathGeometry;

            PocketPaintApplication.GetInstance().PaintingAreaView.AddElementToEraserCanvas(_path);

            var rectangleGeometry = new RectangleGeometry
            {
                Rect = new Rect(0, 0, PocketPaintApplication.GetInstance().PaintingAreaCanvas.ActualWidth,
                PocketPaintApplication.GetInstance().PaintingAreaCanvas.ActualHeight)
            };
            _path.Clip = rectangleGeometry;
            _path.InvalidateArrange();
            _path.InvalidateMeasure();
        }

        public override void HandleMove(object arg)
        {
            if (!(arg is Point))
            {
                return;
            }

            var coordinate = (Point)arg;

            if (!_lastPointSet)
            {
                _lastPoint = coordinate;
                _lastPointSet = true;
                return;
            }
            if (_lastPointSet && !_lastPoint.Equals(coordinate))
            {
                var qbs = new QuadraticBezierSegment
                {
                    Point1 = _lastPoint,
                    Point2 = coordinate
                };
                _pathSegmentCollection.Add(qbs);

                PocketPaintApplication.GetInstance().PaintingAreaLayoutRoot.InvalidateMeasure();
                _lastPointSet = false;
            }
        }

        public override async void HandleUp(object arg)
        {
            if (!(arg is Point))
            {
                return;
            }

            var coordinate = (Point)arg;

            if (_lastPoint.Equals(coordinate))
            {
                var qbs = new QuadraticBezierSegment
                {
                    Point1 = _lastPoint,
                    Point2 = coordinate
                };

                _pathSegmentCollection.Add(qbs);

                PocketPaintApplication.GetInstance().PaintingAreaLayoutRoot.InvalidateMeasure();
                _path.InvalidateArrange();
            }
           
            int returnValue = await _pixelDataEraser.PreparePaintingAreaCanvasForEraser();
            if (returnValue == 1)
                throw new Exception("Preparing pixeldataeraser failed!");
            _points = _pixelDataEraser.GetWhitePixels();
            _pixelData.SetPixel(_points, "0_0_0_0");
            PocketPaintApplication.GetInstance().EraserCanvas.Children.Clear();
            if(await _pixelData.PixelBufferToBitmap())
                CommandManager.GetInstance().CommitCommand(new EraserCommand(_points));
        }
        public override async void Draw(object obj)
        {
            await _pixelData.preparePaintingAreaCanvasPixel();
            _pixelDataEraser = new PixelData.PixelData();
            await _pixelDataEraser.PreparePaintingAreaCanvasForEraser();
            List<Point> pointsToSet = ((List<Point>)obj);
            _pixelData.SetPixel(pointsToSet, "0_0_0_0");
            PocketPaintApplication.GetInstance().EraserCanvas.Children.Clear();
            var image = await _pixelData.BufferToImage();
            PocketPaintApplication.GetInstance().PaintingAreaCanvas.Children.Add(image);
        }

        public override void ResetDrawingSpace()
        {
            throw new NotImplementedException();
        }

        public override void ResetUsedElements()
        {
            PocketPaintApplication.GetInstance().EraserCanvas.Visibility = Visibility.Collapsed;
        }
    }
}

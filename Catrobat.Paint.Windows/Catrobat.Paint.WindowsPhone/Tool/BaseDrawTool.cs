using Catrobat.Paint.WindowsPhone.Command;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace Catrobat.Paint.WindowsPhone.Tool
{
    class BaseDrawTool : ToolBase
    {
        public Path _path;
        private PathGeometry _path_geometry;
        private PathFigureCollection _path_figure_collection;
        private PathFigure _path_figure;
        public  PathSegmentCollection _path_segment_collection;
        private Point _last_point;
        private bool _last_point_set;
        private PixelData.PixelData _pixel_data_eraser;
        private PixelData.PixelData _pixel_data;
        public bool _is_transparence_color_selected = false;

        public BaseDrawTool()
        {
            
        }

        private Path newPathWithSpecificSettings()
        {
            Path path = new Path();
            path.StrokeLineJoin = PenLineJoin.Round;
            path.Stroke = PocketPaintApplication.GetInstance().PaintData.colorSelected;
            path.StrokeThickness = PocketPaintApplication.GetInstance().PaintData.thicknessSelected;
            path.StrokeStartLineCap = PocketPaintApplication.GetInstance().PaintData.penLineCapSelected;
            path.StrokeEndLineCap = PocketPaintApplication.GetInstance().PaintData.penLineCapSelected;
            return path;
        }

        public override void HandleDown(object arg)
        {
            if (!(arg is Point))
            {
                return;
            }
            if(PocketPaintApplication.GetInstance().Bitmap == null)
            {
                PocketPaintApplication.GetInstance().SaveAsWriteableBitmapToRam();
            }

            _path = newPathWithSpecificSettings();
            _is_transparence_color_selected = PocketPaintApplication.GetInstance().PaintData.colorSelected.Color.A == 0x00 ? true : false;
            if (_is_transparence_color_selected)
            {
                _path.Stroke = new SolidColorBrush(Colors.White);
            }

            var coordinate = (Point)arg;
            _last_point = coordinate;

            _path_figure = new PathFigure();
            _path_figure.StartPoint = coordinate;

            _path_segment_collection = new PathSegmentCollection();
            _path_figure.Segments = _path_segment_collection;

            _path_figure_collection = new PathFigureCollection();
            _path_figure_collection.Add(_path_figure);

            _path_geometry = new PathGeometry();
            _path_geometry.Figures = _path_figure_collection;

            if (_is_transparence_color_selected)
            {
                PocketPaintApplication.GetInstance().EraserCanvas.Children.Add(_path);
            }
            else
            {
                PocketPaintApplication.GetInstance().PaintingAreaView.addElementToPaintingAreCanvas(_path);
            }

            _path.Data = _path_geometry;
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
            if (!_last_point_set)
            {
                _last_point = coordinate;
                _last_point_set = true;
                return;
            }

            if (_last_point_set && !_last_point.Equals(coordinate))
            {
                var qbs = new QuadraticBezierSegment
                {
                    Point1 = _last_point,
                    Point2 = coordinate
                };
                _path_segment_collection.Add(qbs);
                _last_point_set = false;
            }
        }

        async public override void HandleUp(object arg)
        {
            if (!(arg is Point))
            {
                return;
            }

            var coordinate = (Point)arg;
            if (_last_point.Equals(coordinate))
            {
                var qbs = new QuadraticBezierSegment
                {
                    Point1 = _last_point,
                    Point2 = coordinate
                };
                _path_segment_collection.Add(qbs);
                _path.InvalidateArrange();
            }

            if (_is_transparence_color_selected)
            {
                _pixel_data_eraser = new PixelData.PixelData();
                _pixel_data = new PixelData.PixelData();

                await _pixel_data_eraser.preparePaintingAreaCanvasForEraser();
                await _pixel_data.preparePaintingAreaCanvasPixel();
                
                var points = _pixel_data_eraser.GetWhitePixels();
                _pixel_data.SetPixel(points, "0_0_0_0");
                
                await _pixel_data.PixelBufferToBitmap();
            }
            PocketPaintApplication.GetInstance().EraserCanvas.Children.Clear();
        }

        async public override void Draw(object o)
        {
            PocketPaintApplication.GetInstance().EraserCanvas.Children.Add((Path)o);
            _pixel_data_eraser = new PixelData.PixelData();
            _pixel_data = new PixelData.PixelData();

            await _pixel_data_eraser.preparePaintingAreaCanvasForEraser();
            await _pixel_data.preparePaintingAreaCanvasPixel();

            var points = _pixel_data_eraser.GetWhitePixels();
            _pixel_data.SetPixel(points, "0_0_0_0");

            await _pixel_data.PixelBufferToBitmap();
            PocketPaintApplication.GetInstance().EraserCanvas.Children.Clear();
        }

        public override void ResetDrawingSpace()
        {
        }

        public override void ResetUsedElements()
        {
        }
    }
}

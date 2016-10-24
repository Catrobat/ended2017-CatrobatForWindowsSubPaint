using Catrobat.Paint.WindowsPhone.Tool;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Shapes;

namespace Catrobat.Paint.WindowsPhone.Command
{
    class LineCommand : CommandBase
    {
        private Path _path;
        private Canvas _painting_area_canvas;

        public LineCommand(Path path)
        {
            ToolType = ToolType.Line;
            _path = path;
            _painting_area_canvas = PocketPaintApplication.GetInstance().PaintingAreaCanvas;
        }

        public override bool ReDo()
        {
            if (!_painting_area_canvas.Children.Contains(_path))
            {
                _painting_area_canvas.Children.Add(_path);
            }
            return true;
        }

        public override bool UnDo()
        {
            if (_painting_area_canvas.Children.Contains(_path))
            {
                _painting_area_canvas.Children.Remove(_path);
                return true;
            }
            return false;
        }
    }
}

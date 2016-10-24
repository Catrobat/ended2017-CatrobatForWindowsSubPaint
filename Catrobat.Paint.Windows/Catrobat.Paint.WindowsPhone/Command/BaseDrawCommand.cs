using Catrobat.Paint.WindowsPhone.Tool;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace Catrobat.Paint.WindowsPhone.Command
{
    class BaseDrawCommand : CommandBase
    {
        private Path _path;
        private bool _is_transparence_color = false;
        private Canvas painting_area_canvas;

        public BaseDrawCommand(Path path, bool is_transparence_color)
        {
            ToolType = ToolType.Brush;
            _path = path;
            _is_transparence_color = is_transparence_color;
            painting_area_canvas = PocketPaintApplication.GetInstance().PaintingAreaCanvas;
        }

        public override bool ReDo()
        {
            if (!_is_transparence_color)
            {
                if (!painting_area_canvas.Children.Contains(_path))
                {
                    painting_area_canvas.Children.Add(_path);
                    return true;
                }
            }
            else
            {
                PocketPaintApplication.GetInstance().ToolCurrent.Draw(_path);
                return true;
            }
            return false;
        }

        public override bool UnDo()
        {
            if (painting_area_canvas.Children.Contains(_path))
            {
                painting_area_canvas.Children.Remove(_path);
                return true;                
            }
            return false;
        }
    }
}

using Catrobat.Paint.WindowsPhone.Command;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace Catrobat.Paint.WindowsPhone.Tool
{
    class BrushTool : ToolBase
    {
        BaseDrawTool _base_draw_tool;

        public BrushTool()
        {
            ToolType = ToolType.Brush;
            _base_draw_tool = new BaseDrawTool();
        }

        public override void HandleDown(object arg)
        {
            if (!(arg is Point))
            {
                return;
            }
            _base_draw_tool.HandleDown(arg);
        }

        public override void HandleMove(object arg)
        {
            if (!(arg is Point))
            {
                return;
            }
            _base_draw_tool.HandleMove(arg);
        }

        public override void HandleUp(object arg)
        {
            if (!(arg is Point))
            {
                return;
            }
            _base_draw_tool.HandleUp(arg);
            CommandManager.GetInstance().CommitCommand(new BrushCommand(_base_draw_tool._path, _base_draw_tool._is_transparence_color_selected));
        }

        public override void Draw(object o)
        {
            _base_draw_tool.Draw(o);
        }

        public override void ResetDrawingSpace()
        {
        }

        public override void ResetUsedElements()
        {
        }
    }
}

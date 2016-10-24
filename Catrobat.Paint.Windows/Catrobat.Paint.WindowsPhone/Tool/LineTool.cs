using Catrobat.Paint.WindowsPhone.Command;
using System;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace Catrobat.Paint.WindowsPhone.Tool
{
    class LineTool : ToolBase
    {
        private LineSegment _line_segment;
        private BaseDrawTool _base_draw_tool;

        public LineTool()
        {
            ToolType = ToolType.Line;
            _base_draw_tool = new BaseDrawTool();
        }

        public override void HandleDown(object arg)
        {
            if (!(arg is Point))
            {
                return;
            }
            
            _line_segment = new LineSegment();
            _base_draw_tool.HandleDown(arg);
        }

        public override void HandleMove(object arg)
        {
            if (!(arg is Point))
            {
                return;
            }

            var coordinate = (Point)arg;
            _base_draw_tool._path_segment_collection.Remove(_line_segment);
            _line_segment.Point = coordinate;
            _base_draw_tool._path_segment_collection.Add(_line_segment);
        }

        public override void HandleUp(object arg)
        {
            _base_draw_tool.HandleUp(arg);
           CommandManager.GetInstance().CommitCommand(new BaseDrawCommand(_base_draw_tool._path, _base_draw_tool._is_transparence_color_selected));            
        }

        public override void Draw(object o)
        {
            _base_draw_tool.Draw(o);
        }

        public override void ResetDrawingSpace()
        {
            throw new NotImplementedException();
        }
        public override void ResetUsedElements()
        {
        }
    }
}

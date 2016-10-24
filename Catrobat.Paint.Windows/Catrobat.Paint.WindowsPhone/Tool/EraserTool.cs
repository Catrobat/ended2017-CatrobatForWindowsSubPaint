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
    class EraserTool : ToolBase
    {
        private SolidColorBrush current_color;
        private BaseDrawTool _base_draw_tool;

        public EraserTool()
        {
            ToolType = ToolType.Eraser;
            _base_draw_tool = new BaseDrawTool();
        }

        public override void HandleDown(object arg)
        {
            if (!(arg is Point))
            {
                return;
            }
            current_color = PocketPaintApplication.GetInstance().PaintData.colorSelected;
            Color color = new Color();
            color.A = 0x00;
            PocketPaintApplication.GetInstance().PaintData.colorSelected = new SolidColorBrush(color);
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
            PocketPaintApplication.GetInstance().PaintData.colorSelected = current_color;
            CommandManager.GetInstance().CommitCommand(new BaseDrawCommand(_base_draw_tool._path, _base_draw_tool._is_transparence_color_selected));
        }
        public override void Draw(object obj)
        {
            _base_draw_tool.Draw(obj);
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

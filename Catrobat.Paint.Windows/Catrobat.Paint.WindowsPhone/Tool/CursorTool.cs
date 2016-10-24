using Catrobat.Paint.WindowsPhone.Command;
using Catrobat.Paint.WindowsPhone.Data;
using System;
using System.Collections.Generic;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
// TODO: using Catrobat.Paint.Phone.Command;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace Catrobat.Paint.WindowsPhone.Tool
{
    class CursorTool : ToolBase
    {
        private TransformGroup _transforms;
        private double height = 0.0;
        private double width = 0.0;
        private BaseDrawTool _base_draw_tool;

        public CursorTool()
        {
            ToolType = ToolType.Cursor;
            _base_draw_tool = new BaseDrawTool();
            if (PocketPaintApplication.GetInstance().GridCursor.RenderTransform != null)
            {
                _transforms = PocketPaintApplication.GetInstance().GridCursor.RenderTransform as TransformGroup;
            }
            if (_transforms == null)
            {
                PocketPaintApplication.GetInstance().GridCursor.RenderTransform = _transforms = new TransformGroup();
            }
        }

        public override void HandleDown(object arg)
        {
            PocketPaintApplication.GetInstance().EraserCanvas.Visibility = Visibility.Collapsed;
            setHeightWidth();
            var coordinate = new Point(width + _transforms.Value.OffsetX, height + _transforms.Value.OffsetY);
            _base_draw_tool.HandleDown(coordinate);
        }

        public override void HandleMove(object arg)
        {
           var coordinate = new Point(width + _transforms.Value.OffsetX, height + _transforms.Value.OffsetY);
            if (arg.GetType() == typeof(Point))
            {
                if (PocketPaintApplication.GetInstance().cursorControl.isDrawingActivated())
                {
                    setHeightWidth();
                    
                    _base_draw_tool.HandleMove(coordinate);
                }
            }
            else if (arg.GetType() == typeof(TranslateTransform))
            {
                var move = (TranslateTransform)arg;
                _transforms.Children.Add(move);
            }

            if (PocketPaintApplication.GetInstance() != null)
            {
                AppBarButton appBarButtonReset = PocketPaintApplication.GetInstance().PaintingAreaView.getAppBarResetButton();
                if (appBarButtonReset != null)
                {
                    if (!appBarButtonReset.IsEnabled)
                    {
                        appBarButtonReset.IsEnabled = true;
                    }
                }
            }
        }

        public override void HandleUp(object arg)
        {
            _base_draw_tool.HandleUp(arg);
            if (PocketPaintApplication.GetInstance().cursorControl.isDrawingActivated())
            {
                CommandManager.GetInstance().CommitCommand(new BaseDrawCommand(_base_draw_tool._path, _base_draw_tool._is_transparence_color_selected));
            }
        }

        public void app_btnResetCursor_Click(object sender, RoutedEventArgs e)
        {
            ((AppBarButton)sender).IsEnabled = false;
            PocketPaintApplication.GetInstance().GridCursor.RenderTransform = _transforms = new TransformGroup();
        }

        public override void Draw(object o)
        {
            _base_draw_tool.Draw(o);
        }

        public override void ResetDrawingSpace()
        {
            throw new NotImplementedException();
        }

        public void setHeightWidth()
        {
            height = PocketPaintApplication.GetInstance().GridWorkingSpace.ActualHeight / 2.0;
            width = PocketPaintApplication.GetInstance().GridWorkingSpace.ActualWidth / 2.0;
        }

        public override void ResetUsedElements()
        {
            PocketPaintApplication.GetInstance().GridCursor.RenderTransform = _transforms = new TransformGroup();
            PocketPaintApplication.GetInstance().cursorControl.setCursorLook(false);
        }
    }
}

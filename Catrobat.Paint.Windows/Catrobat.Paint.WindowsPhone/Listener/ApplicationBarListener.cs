using Catrobat.Paint.WindowsPhone.Command;
using Catrobat.Paint.WindowsPhone.Tool;
// TODO: using Catrobat.Paint.Phone.Command;
using Catrobat.Paint.WindowsPhone.View;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Catrobat.Paint.WindowsPhone.Listener
{
    public class ApplicationBarListener
    {
        public void BtnUndo_Click(object sender, RoutedEventArgs e)
        {
            CommandManager.GetInstance().UnDo();

            // TODO: Maybe it would be better to swap out the following code to the paintingarea-file.
            if(PocketPaintApplication.GetInstance().PaintingAreaCanvas.Children.Count == 0)
            {
                // TODO: Write a method in the paintingareaview and do the following lines in this method.
                PocketPaintApplication.GetInstance().PaintingAreaView.ChangeEnabledOfASecondaryAppbarButton("appBarButtonClearWorkingSpace", false);
                PocketPaintApplication.GetInstance().PaintingAreaView.ChangeEnabledOfASecondaryAppbarButton("appbarButtonSave", false);
            }

            if(PocketPaintApplication.GetInstance().PaintingAreaView.IsASelectionControlSelected())
            {
                PocketPaintApplication.GetInstance().PaintingAreaView.ChangeBackgroundColorAndOpacityOfPaintingAreaCanvas(Colors.Black, 0.5);
            }
            PocketPaintApplication.GetInstance().PaintingAreaView.CheckAndUpdateStampAppBarButtons();
        }

        public void BtnRedo_Click(object sender, RoutedEventArgs e)
        {
            CommandManager.GetInstance().ReDo();
            // TODO: Maybe it would be better to swap out the following code to the paintingarea-file.
            if (PocketPaintApplication.GetInstance().PaintingAreaCanvas.Children.Count > 0)
            {
                PocketPaintApplication.GetInstance().PaintingAreaView.ChangeEnabledOfASecondaryAppbarButton("appBarButtonClearWorkingSpace", true);
                PocketPaintApplication.GetInstance().PaintingAreaView.ChangeEnabledOfASecondaryAppbarButton("appbarButtonSave", true);
            }
            PocketPaintApplication.GetInstance().PaintingAreaView.CheckAndUpdateStampAppBarButtons();
        }

        public void BtnColor_Click(object sender, RoutedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;

            if (rootFrame != null)
            {
                PocketPaintApplication.GetInstance().is_border_color = false;
                rootFrame.Navigate(typeof(ViewColorPicker));
            }
        }

        public void BtnMoveScreenEllipse_OnClick(object sender, RoutedEventArgs e)
        {
            ToolType currentTooltype = PocketPaintApplication.GetInstance().ToolCurrent.GetToolType();
            if (currentTooltype != ToolType.Move)
            {
                //PocketPaintApplication.GetInstance().PaintingAreaView.setVisibilityGrdSliderThickness(Visibility.Collapsed);
                PocketPaintApplication.GetInstance().SwitchTool(ToolType.Move);
                PocketPaintApplication.GetInstance().PaintingAreaView.ChangeVisibilityOfSelectionsControls(Visibility.Collapsed);
                PocketPaintApplication.GetInstance().PaintingAreaView.ChangeBackgroundColorAndOpacityOfPaintingAreaCanvas(Colors.Transparent, 1.0);
            }
            else if (currentTooltype == ToolType.Move)
            {
                if (PocketPaintApplication.GetInstance().ToolWhileMoveTool == null)
                    return;

                ToolType newSelectedTooltype = PocketPaintApplication.GetInstance().ToolWhileMoveTool.GetToolType();
                PocketPaintApplication.GetInstance().SwitchTool(newSelectedTooltype);
                PocketPaintApplication.GetInstance().PaintingAreaView.ChangeVisibilityOfActiveSelectionControl(Visibility.Visible);
                if (newSelectedTooltype == ToolType.Ellipse || newSelectedTooltype == ToolType.ImportPng || newSelectedTooltype == ToolType.Rect)
                {
                    PocketPaintApplication.GetInstance().PaintingAreaView.ChangeBackgroundColorAndOpacityOfPaintingAreaCanvas(Colors.Black, 0.5);
                }
                PocketPaintApplication.GetInstance().PaintingAreaView.ResetActiveSelectionControl();
            }
        }

        public void BtnTools_OnClick(object sender, RoutedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;
            
            if(rootFrame != null)
            {
                rootFrame.Navigate(typeof(ViewToolPicker));
            }
        }
    }
}

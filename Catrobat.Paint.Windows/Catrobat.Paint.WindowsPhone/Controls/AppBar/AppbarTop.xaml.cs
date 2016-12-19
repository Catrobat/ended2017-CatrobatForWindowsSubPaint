using Catrobat.Paint.WindowsPhone.Tool;
using System;
using System.Linq;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Shapes;

// Die Elementvorlage "Leere Seite" ist unter http://go.microsoft.com/fwlink/?LinkID=390556 dokumentiert.

namespace Catrobat.Paint.WindowsPhone.Controls.AppBar
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet werden kann oder auf die innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class AppbarTop
    {
        public AppbarTop()
        {
            InitializeComponent();
            SetAppBarTopLayout();
        }
        private void SetAppBarTopLayout()
        {
            double multiplicator = PocketPaintApplication.GetInstance().size_width_multiplication;
            GrdLayoutRoot.Width *= multiplicator;
            GrdLayoutRoot.Height *= multiplicator;

            foreach (var element in GrdLayoutRoot.Children.Where(obj => obj is Button || obj is Ellipse || obj is Rectangle).Cast<FrameworkElement>())
            {
                element.Height *= multiplicator;
                element.Width *= multiplicator;

                element.Margin = new Thickness(
                    element.Margin.Left * multiplicator,
                    element.Margin.Top * multiplicator,
                    element.Margin.Right * multiplicator,
                    element.Margin.Bottom * multiplicator);
            }

            ImgTransparence.Width *= multiplicator;
            ImgTransparence.Height *= multiplicator;
            ImgTransparence.Margin = new Thickness(
                                        ImgTransparence.Margin.Left * multiplicator,
                                        ImgTransparence.Margin.Top * multiplicator,
                                        ImgTransparence.Margin.Right * multiplicator,
                                        ImgTransparence.Margin.Bottom * multiplicator);

            GrdBtnSelectedColor.Height *= multiplicator;
            GrdBtnSelectedColor.Width *= multiplicator;

            BtnSelectedColor.Background = PocketPaintApplication.GetInstance().PaintData.colorSelected;
            PocketPaintApplication.GetInstance().PaintData.colorChanged += ColorChangedHere;
            PocketPaintApplication.GetInstance().PaintData.toolCurrentChanged += ToolChangedHere;
            GrdLayoutRoot.Background = new SolidColorBrush(Color.FromArgb(255, 25, 165, 184));
            PocketPaintApplication.GetInstance().AppbarTop = this;
             

            BtnUndo.Click += PocketPaintApplication.GetInstance().ApplicationBarListener.BtnUndo_Click;
            BtnRedo.Click += PocketPaintApplication.GetInstance().ApplicationBarListener.BtnRedo_Click;
            BtnTools.Click += PocketPaintApplication.GetInstance().ApplicationBarListener.BtnTools_OnClick;
            BtnSelectedColor.Click += PocketPaintApplication.GetInstance().ApplicationBarListener.BtnColor_Click;
            //btnMoveScreen.Click += PocketPaintApplication.GetInstance().ApplicationBarListener.BtnMoveScreen_OnClick;
            BtnToolSelection.Click += PocketPaintApplication.GetInstance().ApplicationBarListener.BtnMoveScreenEllipse_OnClick;
            BtnUndo.IsEnabled = false;
            BtnRedo.IsEnabled = false;
        }

        private void ColorChangedHere(SolidColorBrush color)
        {
            if (color != null && color.Color != Colors.Transparent)
            {
                RecSelectedColor.Fill = color;
            }
            else
            {
                RecSelectedColor.Fill = new SolidColorBrush(Colors.Transparent);
            }
        }

        public void ToolChangedHere(ToolBase tool)
        {
            ImageBrush imgFront = new ImageBrush();
            ImageBrush imgBehind = new ImageBrush
            {
                ImageSource = new BitmapImage(
                    new Uri("ms-resource:/Files/Assets/ToolMenu/icon_menu_move.png", UriKind.Absolute))
            };

            Visibility currentStateOfGridThicknessControl = PocketPaintApplication.GetInstance().GrdThicknessControlState;
            PocketPaintApplication.GetInstance().PaintingAreaView.GrdThicknessControlVisibility = Visibility.Collapsed;

            if(tool.GetToolType() == ToolType.Eraser && PocketPaintApplication.GetInstance().isBrushEraser)
            {
                tool = new BrushTool();
            }
            else
            {
                if (PocketPaintApplication.GetInstance().isToolPickerUsed)
                {
                    PocketPaintApplication.GetInstance().isBrushEraser = false;
                }
            }

            switch (tool.GetToolType())
            {
                case ToolType.Brush:
                    imgFront.ImageSource = new BitmapImage(GetToolImageUri(ToolType.Brush));
                    PocketPaintApplication.GetInstance().PaintingAreaView.GrdThicknessControlVisibility
                        = currentStateOfGridThicknessControl;
                    break;
                case ToolType.Crop:
                    imgFront.ImageSource = new BitmapImage(GetToolImageUri(ToolType.Crop));
                    PocketPaintApplication.GetInstance().PaintingAreaView.GrdThicknessControlVisibility
                        = currentStateOfGridThicknessControl;
                    break;
                case ToolType.Cursor:
                    imgFront.ImageSource = new BitmapImage(GetToolImageUri(ToolType.Cursor));
                    break;
                case ToolType.Ellipse:
                    imgFront.ImageSource = new BitmapImage(GetToolImageUri(ToolType.Ellipse));
                    break;
                case ToolType.Eraser:
                    imgFront.ImageSource = new BitmapImage(GetToolImageUri(ToolType.Eraser));
                    break;
                case ToolType.Fill:
                    imgFront.ImageSource = new BitmapImage(GetToolImageUri(ToolType.Fill));
                    PocketPaintApplication.GetInstance().PaintingAreaView.GrdThicknessControlVisibility
                        = currentStateOfGridThicknessControl;
                    break;
                case ToolType.Flip:
                    imgFront.ImageSource = new BitmapImage(GetToolImageUri(ToolType.Flip));
                    break;
                case ToolType.ImportPng:
                    imgFront.ImageSource = new BitmapImage(GetToolImageUri(ToolType.ImportPng));
                    PocketPaintApplication.GetInstance().PaintingAreaView.GrdThicknessControlVisibility
                        = currentStateOfGridThicknessControl;
                    break;
                case ToolType.Line:
                    imgFront.ImageSource = new BitmapImage(GetToolImageUri(ToolType.Line));
                    PocketPaintApplication.GetInstance().PaintingAreaView.GrdThicknessControlVisibility
                        = currentStateOfGridThicknessControl;
                    break;
                case ToolType.Move:
                    imgFront.ImageSource = new BitmapImage(GetToolImageUri(ToolType.Move));
                    imgBehind.ImageSource = new BitmapImage(
                     GetToolImageUri(PocketPaintApplication.GetInstance().ToolWhileMoveTool.GetToolType()));
                    break;
                case ToolType.Pipette:
                    imgFront.ImageSource = new BitmapImage(GetToolImageUri(ToolType.Pipette));
                    break;
                case ToolType.Rect:
                    imgFront.ImageSource = new BitmapImage(GetToolImageUri(ToolType.Rect));
                    break;
                case ToolType.Rotate:
                    imgFront.ImageSource = new BitmapImage(GetToolImageUri(ToolType.Rotate));
                    break;
                case ToolType.Stamp:
                    imgFront.ImageSource = new BitmapImage(GetToolImageUri(ToolType.Stamp));
                    break;
                case ToolType.Zoom:
                    imgFront.ImageSource = new BitmapImage(GetToolImageUri(ToolType.Zoom));
                    break;
                // TODO: BtnMoveScreen.ImageSource = null;
                    // TODO: BtnMoveScreen.Background = null;
            }

            ellipseTool_behind.Opacity = 0.1;
            ellipseTool_behind.Fill = imgBehind;
            ellipseTool_front.Fill = imgFront;
        }

        public void BtnSelectedColorVisible(bool enable)
        {
            BtnSelectedColor.IsEnabled = enable;
        }

        private Uri GetToolImageUri(ToolType tooltype)
        { 
            switch (tooltype)
            {
                case ToolType.Brush:
                    return new Uri("ms-resource:/Files/Assets/ToolMenu/icon_menu_brush.png", UriKind.Absolute);
                case ToolType.Crop:
                    return new Uri("ms-resource:/Files/Assets/ToolMenu/icon_menu_crop.png", UriKind.Absolute);
                case ToolType.Cursor:
                    return new Uri("ms-resource:/Files/Assets/ToolMenu/icon_menu_cursor.png", UriKind.Absolute);
                case ToolType.Fill:
                    return new Uri("ms-resource:/Files/Assets/ToolMenu/icon_menu_bucket.png", UriKind.Absolute);
                case ToolType.Ellipse:
                    return new Uri("ms-resource:/Files/Assets/ToolMenu/icon_menu_ellipse.png", UriKind.Absolute);
                case ToolType.Eraser:
                    return new Uri("ms-resource:/Files/Assets/ToolMenu/icon_menu_eraser.png", UriKind.Absolute);
                case ToolType.Flip:
                    return new Uri("ms-resource:/Files/Assets/ToolMenu/icon_menu_flip_horizontal.png", UriKind.Absolute);
                case ToolType.ImportPng:
                    return new Uri("ms-resource:/Files/Assets/ToolMenu/icon_menu_import_image.png", UriKind.Absolute);
                case ToolType.Line:
                    return new Uri("ms-resource:/Files/Assets/ToolMenu/icon_menu_straight_line.png", UriKind.Absolute);
                case ToolType.Move:
                    return new Uri("ms-resource:/Files/Assets/ToolMenu/icon_menu_move.png", UriKind.Absolute);
                case ToolType.Pipette:
                    return new Uri("ms-resource:/Files/Assets/ToolMenu/icon_menu_pipette.png", UriKind.Absolute);
                case ToolType.Rect:
                    return new Uri("ms-resource:/Files/Assets/ToolMenu/icon_menu_rectangle.png", UriKind.Absolute);
                case ToolType.Stamp:
                    return new Uri("ms-resource:/Files/Assets/ToolMenu/icon_menu_stamp.png", UriKind.Absolute);
                case ToolType.Rotate:
                    return new Uri("ms-resource:/Files/Assets/ToolMenu/icon_menu_rotate_left.png", UriKind.Absolute);
                case ToolType.Zoom:
                    return new Uri("ms-resource:/Files/Assets/ToolMenu/icon_menu_zoom.png", UriKind.Absolute);
                default:
                    return null;
            }
        }

        public bool BtnRedoEnable
        {
            get { return BtnRedo.IsEnabled; }
            set { BtnRedo.IsEnabled = value; }
        }

        public bool BtnUndoEnable
        {
            get { return BtnUndo.IsEnabled; }
            set { BtnUndo.IsEnabled = value; }
        }
    }
}

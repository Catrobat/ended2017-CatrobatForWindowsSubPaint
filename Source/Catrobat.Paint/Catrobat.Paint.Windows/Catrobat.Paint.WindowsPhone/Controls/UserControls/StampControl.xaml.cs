using Catrobat.Paint.WindowsPhone.Tool;
using System;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

// Die Elementvorlage "Benutzersteuerelement" ist unter http://go.microsoft.com/fwlink/?LinkId=234236 dokumentiert.

namespace Catrobat.Paint.WindowsPhone.Controls.UserControls
{
    public sealed partial class StampControl
    {
        public RectangleShapeBaseControl RectangleShapeBase { get; private set; }
        public Rectangle RectangleToDraw { get; private set; }
        public Image image { get; private set; }
        public Grid GridMain { get; private set; }

        TransformGroup _transformGridMain;

        public double OriginalHeightStampedImage { get; private set; }
        public double OriginalWidthStampedImage { get; private set; }

        public bool FoundLeftPixel { get; private set; }

        double _limitLeft;
        double _limitRight;
        double _limitBottom;
        double _limitTop;

        double _offsetMargin;
        double _heightStampControl;
        double _widthStampControl;

        double _scaleValueWorkingSpace;

        PixelData.PixelData _pixelData = new PixelData.PixelData();

        //public Point LeftTopNullPointStampSelection { get; set; }

        double _heightOfRectangle;
        double _widthOfRectangle;

        public StampControl()
        {
            this.InitializeComponent();

            RectangleShapeBase = RectangleShapeBaseControl;


            GridMain = (Grid)RectangleShapeBaseControl.FindName("AreaToDrawGrid");
            
            image = (Image)RectangleShapeBaseControl.FindName("imgStampedImage");
            RectangleToDraw = (Rectangle)RectangleShapeBaseControl.FindName("AreaToDrawStamp");
            RectangleToDraw.Visibility = Visibility.Visible;

            _transformGridMain = new TransformGroup();
            GridMain.RenderTransform = _transformGridMain;
            PocketPaintApplication.GetInstance().StampControl = this;
            SetIsModifiedRectangleMovement = false;
            //LeftTopNullPointStampSelection = new Point(0.0, 0.0);

            _offsetMargin = 5.0;
            _heightStampControl = 0.0;
            _widthStampControl = 0.0;
            _scaleValueWorkingSpace = 0.0;
            _heightOfRectangle = GridMain.Height;
            _widthOfRectangle = GridMain.Width;
        }

        private Point GetExtremeLeftAndTopCoordinate(double initLeft, double initTop,
                                                     ref int xCoordinateOfExtremeTop)
        {
            Point extremePoint = new Point(initLeft, initTop);
            FoundLeftPixel = false;

            double paintingAreaCanvasHeight = PocketPaintApplication.GetInstance().PaintingAreaCanvas.Height;
            double paintingAreaCanvasWidth = PocketPaintApplication.GetInstance().PaintingAreaCanvas.Width;

            // left pixel
            for (int indexWidth = 0; indexWidth < (int)paintingAreaCanvasWidth; indexWidth++)
                for (int indexHeight = 0; indexHeight < (int)paintingAreaCanvasHeight; indexHeight++)
                {
                    if (_pixelData.getPixelAlphaFromCanvas(indexWidth, indexHeight) != 0x00)
                    {
                        extremePoint.X = indexWidth;
                        FoundLeftPixel = true;

                        // found extreme point --> set break conditions
                        indexWidth = (int)paintingAreaCanvasWidth;
                        indexHeight = (int)paintingAreaCanvasHeight;
                    }
                }
            // top pixel
            if (FoundLeftPixel)
                for (int indexHeight = 0; indexHeight < (int)paintingAreaCanvasHeight; indexHeight++)
                    for (int indexWidth = (int)paintingAreaCanvasWidth - 1; indexWidth >= (int)extremePoint.X; indexWidth--)
                    {
                        if (_pixelData.getPixelAlphaFromCanvas(indexWidth, indexHeight) != 0x00)
                        {
                            extremePoint.Y = indexHeight;
                            xCoordinateOfExtremeTop = indexWidth;

                            // found extreme point --> set break conditions
                            indexHeight = (int)paintingAreaCanvasHeight;
                            indexWidth = 0;
                        }
                    }
            System.Diagnostics.Debug.WriteLine("extremelefttop: " + extremePoint);
            return extremePoint;
        }

        private Point GetExtremeRightAndBottomCoordinate(double initRight, double initBottom,
                                                         Point extremeLeftAndTopCoordinate,
                                                         int xCoordinateOfExtremeTop)
        {
            double paintingAreaCanvasHeight = PocketPaintApplication.GetInstance().PaintingAreaCanvas.Height;
            double paintingAreaCanvasWidth = PocketPaintApplication.GetInstance().PaintingAreaCanvas.Width;
            Point extremePoint = new Point(initRight, initBottom);

            if (FoundLeftPixel)
            {
                // right pixel
                int yCoordinateOfExtremeRight = 0;
                for (int indexWidth = (int)paintingAreaCanvasWidth - 1; indexWidth >= xCoordinateOfExtremeTop; indexWidth--)
                    for (int indexHeight = (int)paintingAreaCanvasHeight - 1; indexHeight >= extremeLeftAndTopCoordinate.Y; indexHeight--)
                    {
                        if (_pixelData.getPixelAlphaFromCanvas(indexWidth, indexHeight) != 0x00)
                        {
                            extremePoint.X = indexWidth;
                            yCoordinateOfExtremeRight = indexHeight;

                            // found extreme point --> set break conditions
                            indexWidth = 0;
                            indexHeight = 0;
                        }
                    }
                // bottom pixel
                for (int indexHeight = (int)paintingAreaCanvasHeight - 1; indexHeight >= yCoordinateOfExtremeRight; indexHeight--)
                    for (int indexWidth = (int)extremePoint.X; indexWidth >= (int)extremeLeftAndTopCoordinate.X; indexWidth--)
                    {
                        if (_pixelData.getPixelAlphaFromCanvas(indexWidth, indexHeight) != 0x00)
                        {
                            extremePoint.Y = indexHeight;

                            // found extreme point --> set break conditions
                            indexHeight = 0;
                            indexWidth = 0;
                        }
                    }
            }
            System.Diagnostics.Debug.WriteLine("extremerightbottom: " + extremePoint);
            return extremePoint;
        }

        // TODO: David Refactor the following function.
        private void _calculateAndSetStampControlPositionWithoutRotating(double doubleBorderWidthValue, double scaleValueWorkingSpace, bool isWorkingSpaceFlippedHorizontally, bool isWorkingSpaceFlippedVertically)
        {
            PocketPaintApplication currentPaintApplication = PocketPaintApplication.GetInstance();
            TransformGroup tgPaintingAreaCheckeredGrid = currentPaintApplication.GridWorkingSpace.RenderTransform as TransformGroup;
            if (tgPaintingAreaCheckeredGrid == null)
            {
                return;
            }

            // is needed to move the blue selection to the right position
            TranslateTransform ttfMoveStampControl = new TranslateTransform();

            double heightOfpaintingAreaCheckeredGrid = currentPaintApplication.GridWorkingSpace.Height;
            double widthOfPaintingAreaCheckeredGrid = currentPaintApplication.GridWorkingSpace.Width;
            // Calculate the position from Stamp selection in connection with the working space respectively with the drawing
            // in the working space. In other words the Stamp selection should be adapted on the drawing in the working space.
            Point extremeLeftAndTopCoordinate = new Point(0.0, 0.0);
            Point extremeRightAndBottomCoordinate = new Point(widthOfPaintingAreaCheckeredGrid - 1.0, heightOfpaintingAreaCheckeredGrid - 1.0);

            bool isThereSomethingDrawn = currentPaintApplication.PaintingAreaCanvas.Children.Count != 0;
            if (isThereSomethingDrawn)
            {
                int extremeCoordinateOfTop = 0;
                extremeLeftAndTopCoordinate = GetExtremeLeftAndTopCoordinate(extremeLeftAndTopCoordinate.X, extremeLeftAndTopCoordinate.Y, 
                    ref extremeCoordinateOfTop);
                extremeRightAndBottomCoordinate = GetExtremeRightAndBottomCoordinate(extremeRightAndBottomCoordinate.X, extremeRightAndBottomCoordinate.Y,
                                                                                     extremeLeftAndTopCoordinate, extremeCoordinateOfTop);

                // index starts with zero, so we have to add the value one.              
                _heightStampControl = (extremeRightAndBottomCoordinate.Y - extremeLeftAndTopCoordinate.Y + 1.0 + (image.Margin.Top * 2)) * scaleValueWorkingSpace;
                _widthStampControl = (extremeRightAndBottomCoordinate.X - extremeLeftAndTopCoordinate.X + 1.0 + (image.Margin.Left * 2)) * scaleValueWorkingSpace;

                System.Diagnostics.Debug.WriteLine("values: " + extremeRightAndBottomCoordinate + " " + extremeLeftAndTopCoordinate + " " + scaleValueWorkingSpace);
                System.Diagnostics.Debug.WriteLine("form: " + _heightStampControl + " " + _widthStampControl + " " + doubleBorderWidthValue);

                //if (isWorkingSpaceFlippedHorizontally)
                //{
                //    ttfMoveStampControl.X = tgPaintingAreaCheckeredGrid.Value.OffsetX + ((widthOfPaintingAreaCheckeredGrid - extremeRightAndBottomCoordinate.X) * scaleValueWorkingSpace);
                //}
                //else
                //{
                //    ttfMoveStampControl.X = tgPaintingAreaCheckeredGrid.Value.OffsetX + (extremeLeftAndTopCoordinate.X * scaleValueWorkingSpace);
                //}

                //if (isWorkingSpaceFlippedVertically)
                //{
                //    ttfMoveStampControl.Y = tgPaintingAreaCheckeredGrid.Value.OffsetY + ((heightOfpaintingAreaCheckeredGrid - extremeRightAndBottomCoordinate.Y) * scaleValueWorkingSpace);
                //}
                //else
                //{
                //    ttfMoveStampControl.Y = tgPaintingAreaCheckeredGrid.Value.OffsetY + (extremeLeftAndTopCoordinate.Y * scaleValueWorkingSpace);
                //}

                Grid drawGrid = (Grid)RectangleShapeBaseControl.FindName("AreaToDrawGrid");

                PocketPaintApplication.GetInstance().StampControl.HorizontalAlignment = HorizontalAlignment.Left;
                PocketPaintApplication.GetInstance().StampControl.VerticalAlignment = VerticalAlignment.Top;

                RectangleShapeBaseControl.SetHeightOfControl(_heightStampControl);
                RectangleShapeBaseControl.SetWidthOfControl(_widthStampControl);

                TransformGroup workingSpaceTransformation =  PocketPaintApplication.GetInstance().PaintingAreaView.getGridWorkingSpaceTransformGroup();

                ttfMoveStampControl.X = (extremeLeftAndTopCoordinate.X - drawGrid.Margin.Left - image.Margin.Left) * scaleValueWorkingSpace + workingSpaceTransformation.Value.OffsetX - 20;
                ttfMoveStampControl.Y = (extremeLeftAndTopCoordinate.Y - drawGrid.Margin.Top - image.Margin.Top) * scaleValueWorkingSpace + workingSpaceTransformation.Value.OffsetY - 20;

                RectangleShapeBase.addTransformation(ttfMoveStampControl);
            }
            
        }

        public Point GetLeftTopPointOfStampedSelection()
        {
            uint canvasHeight = (uint)PocketPaintApplication.GetInstance().PaintingAreaCanvas.Height;
            uint canvasWidth = (uint)PocketPaintApplication.GetInstance().PaintingAreaCanvas.Width;

            uint uwidthOfStampSelection = (uint)PocketPaintApplication.GetInstance().StampControl.GetHeightOfRectangleStampSelection();
            uint uheightOfStampSelection = (uint)PocketPaintApplication.GetInstance().StampControl.GetWidthOfRectangleStampSelection();

            Point currentLeftTopCoordinateOfStampSelection = GetXyOffsetBetweenPaintingAreaAndStampControlSelection();
            System.Diagnostics.Debug.WriteLine("cLT: " + currentLeftTopCoordinateOfStampSelection);
            //uint offsetX = (uwidthOfStampSelection + (uint)currentLeftTopCoordinateOfStampSelection.X) > canvasWidth ? canvasWidth - uwidthOfStampSelection : (uint)currentLeftTopCoordinateOfStampSelection.X;
            //uint offsetY = ((uint)uheightOfStampSelection + (uint)currentLeftTopCoordinateOfStampSelection.Y) > canvasHeight ? canvasHeight - uheightOfStampSelection : (uint)currentLeftTopCoordinateOfStampSelection.Y;

            return new Point(Convert.ToDouble(currentLeftTopCoordinateOfStampSelection.X), Convert.ToDouble(currentLeftTopCoordinateOfStampSelection.Y));
        }

        public void ResetCurrentCopiedSelection()
        {
            image.Source = null;
            RectangleShapeBaseControl.ResetRectangleShapeBaseControl();
        }
        // TODO: Refactor the setStampSelection function.
        public async void SetStampSelection()
        {
            PocketPaintApplication currentPaintApplication = PocketPaintApplication.GetInstance();
            if (currentPaintApplication == null)
            {
                return;
            }

            currentPaintApplication.ProgressRing.IsActive = true;
            await _pixelData.preparePaintingAreaCanvasPixel();

            TransformGroup paintingAreaCheckeredGridTransformGroup = currentPaintApplication.GridWorkingSpace.RenderTransform as TransformGroup;
            // Das Selection-Control soll die Zeichnung einschließen und nicht darauf liegen. Daher wird
            // dieser Wert mit 10 verwendet. Anschließend wird dann die Margin Left und Top, um 5 verringert.
            double doubleBorderWidthValue = _offsetMargin * 2.0;

            _transformGridMain.Children.Clear();
            //GridMain.Margin = new Thickness(-5.0, -5.0, 0.0, 0.0);

            bool isWorkingSpaceFlippedHorizontally = paintingAreaCheckeredGridTransformGroup != null && (int)paintingAreaCheckeredGridTransformGroup.Value.M11 == -1;
            bool isWorkingSpaceFlippedVertically = paintingAreaCheckeredGridTransformGroup != null && (int)paintingAreaCheckeredGridTransformGroup.Value.M22 == -1;
            FoundLeftPixel = false;

            if (currentPaintApplication.angularDegreeOfWorkingSpaceRotation == 0)
            {
                if (paintingAreaCheckeredGridTransformGroup != null)
                    _scaleValueWorkingSpace = paintingAreaCheckeredGridTransformGroup.Value.M11;
                _calculateAndSetStampControlPositionWithoutRotating(doubleBorderWidthValue, _scaleValueWorkingSpace, isWorkingSpaceFlippedHorizontally, isWorkingSpaceFlippedVertically);
            }

            currentPaintApplication.StampControl.Visibility = Visibility.Visible;
            currentPaintApplication.ProgressRing.IsActive = false;
        }

        private void _SetLimitsForMovableControlBorder(uint rotatedValue, PocketPaintApplication currentPaintApplication, TransformGroup tgPaintingAreaCheckeredGrid)
        {
            double paintingAreaCheckeredGridHeight = currentPaintApplication.GridWorkingSpace.Height;
            double paintingAreaCheckeredGridWidth = currentPaintApplication.GridWorkingSpace.Width;

            if (rotatedValue == 0)
            {
                _limitLeft = tgPaintingAreaCheckeredGrid.Value.OffsetX - _offsetMargin;
                _limitTop = tgPaintingAreaCheckeredGrid.Value.OffsetY - _offsetMargin;
                // TODO: Explain the following line.
                _limitBottom = _limitTop + (paintingAreaCheckeredGridHeight * _scaleValueWorkingSpace) + _offsetMargin * 2;
                _limitRight = _limitLeft + (paintingAreaCheckeredGridWidth * _scaleValueWorkingSpace) + _offsetMargin * 2;
            }
            else if (rotatedValue == 90)
            {
                _limitTop = tgPaintingAreaCheckeredGrid.Value.OffsetY - _offsetMargin;
                _limitBottom = _limitTop + (paintingAreaCheckeredGridWidth * _scaleValueWorkingSpace) + _offsetMargin * 2;
                _limitRight = tgPaintingAreaCheckeredGrid.Value.OffsetX + _offsetMargin;
                _limitLeft = _limitRight - (paintingAreaCheckeredGridHeight * _scaleValueWorkingSpace) - _offsetMargin * 2;
            }
            else if (rotatedValue == 180)
            {
                _limitRight = tgPaintingAreaCheckeredGrid.Value.OffsetX + _offsetMargin;
                _limitBottom = tgPaintingAreaCheckeredGrid.Value.OffsetY + _offsetMargin;
                _limitTop = _limitBottom - (paintingAreaCheckeredGridHeight * _scaleValueWorkingSpace) - _offsetMargin * 2;
                _limitLeft = _limitRight - (paintingAreaCheckeredGridWidth * _scaleValueWorkingSpace) - _offsetMargin * 2;
            }
            else if (rotatedValue == 270)
            {
                _limitBottom = tgPaintingAreaCheckeredGrid.Value.OffsetY + _offsetMargin;
                _limitTop = _limitBottom - (paintingAreaCheckeredGridWidth * _scaleValueWorkingSpace) - _offsetMargin * 2;
                _limitLeft = tgPaintingAreaCheckeredGrid.Value.OffsetX - _offsetMargin;
                _limitRight = _limitLeft + (paintingAreaCheckeredGridHeight * _scaleValueWorkingSpace) + _offsetMargin * 2;
            }
        }

        private TranslateTransform CreateTranslateTransform(double x, double y)
        {
            TranslateTransform move = new TranslateTransform { X = x, Y = y };

            return move;
        }

        private bool IsStampControlMovable()
        {
            if(HasPaintingAreaCanvasElements() || PocketPaintApplication.GetInstance().PaintingAreaView.isAppBarButtonSelected("appBtnStampCopy"))
            {
                return true;
            }
            return false;
        }

        public void ResetAppBarButtonRectangleSelectionControl(bool activated)
        {
            AppBarButton appBarButtonReset = PocketPaintApplication.GetInstance().PaintingAreaView.getAppBarResetButton();
            if (appBarButtonReset != null)
            {
                appBarButtonReset.IsEnabled = activated;
            }
        }

        public bool SetIsModifiedRectangleMovement { get; set; }

        // TODO: Move the following code in the paintingareaview

        public bool HasPaintingAreaCanvasElements()
        {
            bool result = false;
            if (PocketPaintApplication.GetInstance().PaintingAreaCanvas != null)
            {
                result = PocketPaintApplication.GetInstance().PaintingAreaCanvas.Children.Count > 0;
            }
            return result;
        }

        public double GetHeightOfRectangleStampSelection()
        {
            return _heightOfRectangle / _scaleValueWorkingSpace;
        }

        public double GetWidthOfRectangleStampSelection()
        {
            return _widthOfRectangle / _scaleValueWorkingSpace;
        }

        public Point GetXyOffsetBetweenPaintingAreaAndStampControlSelection()
        {
            TransformGroup tgPaintingAreaCheckeredGrid = PocketPaintApplication.GetInstance().GridWorkingSpace.RenderTransform as TransformGroup;
            _transformGridMain = RectangleShapeBaseControl.GetTransformation();
            PocketPaintApplication currentPaintApplication = PocketPaintApplication.GetInstance();
            if (currentPaintApplication == null || tgPaintingAreaCheckeredGrid == null)
            {
                // TODO: raise Exception
                return new Point(0.0, 0.0);
            }

            Point cornerCoordinates = new Point(_transformGridMain.Value.OffsetX + GridMain.Margin.Left, _transformGridMain.Value.OffsetY + GridMain.Margin.Top);

            System.Diagnostics.Debug.WriteLine("offset " + _transformGridMain.Value.OffsetX + " " + _transformGridMain.Value.OffsetY);
            System.Diagnostics.Debug.WriteLine("cornerCoor: " + cornerCoordinates);

            if (currentPaintApplication.angularDegreeOfWorkingSpaceRotation == 0)
            {
                //double offsetX = ((_transformGridMain.Value.OffsetX + 5.0 + GridMain.Margin.Left) - tgPaintingAreaCheckeredGrid.Value.OffsetX) / 0.75;
                //double offsetY = ((_transformGridMain.Value.OffsetY + 5.0 + GridMain.Margin.Top) - tgPaintingAreaCheckeredGrid.Value.OffsetY) / 0.75;

                double offsetX = (cornerCoordinates.X - tgPaintingAreaCheckeredGrid.Value.OffsetX) / _scaleValueWorkingSpace;
                double offsetY = (cornerCoordinates.Y - tgPaintingAreaCheckeredGrid.Value.OffsetY) / _scaleValueWorkingSpace;

                return new Point(Math.Ceiling(offsetX), Math.Ceiling(offsetY));
            }
            else
            {
                return new Point(0, 0);
            }
            
        }

        public ImageSource GetImageSourceStampedImage()
        {
            return image.Source;
        }

        public void SetOriginalSizeOfStampedImage(double height, double width)
        {
            OriginalHeightStampedImage = height;
            OriginalWidthStampedImage = width;
        }


        public void SetSourceImageStamp(ImageSource imageSource)
        {
            image.Source = imageSource;
        }

        private void rectRectangleStampSelection_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ((StampTool)PocketPaintApplication.GetInstance().ToolCurrent).StampPaste();
        }

        public void setHeightOfControl(double height)
        {
            _heightOfRectangle = height;
        }

        public void setWidthOfControl(double width)
        {
            _widthOfRectangle = width;
        }
    }
}
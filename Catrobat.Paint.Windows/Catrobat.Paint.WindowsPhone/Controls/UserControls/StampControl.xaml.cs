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
        public ImageBrush imgBrush { get; private set; }
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

        bool isWorkingSpaceFlippedVertically;
        bool isWorkingSpaceFlippedHorizontally;

        PixelData.PixelData _pixelData = new PixelData.PixelData();

        double _heightOfRectangle;
        double _widthOfRectangle;

        public StampControl()
        {
            this.InitializeComponent();

            RectangleShapeBase = RectangleShapeBaseControl;


            GridMain = (Grid)RectangleShapeBaseControl.FindName("AreaToDrawGrid");
            
            imgBrush = (ImageBrush)RectangleShapeBaseControl.FindName("imgBrush");
            RectangleToDraw = (Rectangle)RectangleShapeBaseControl.FindName("AreaToDrawStamp");

            //RectangleToDraw.Visibility = Visibility.Visible;

            _transformGridMain = new TransformGroup();
            GridMain.RenderTransform = _transformGridMain;
            PocketPaintApplication.GetInstance().StampControl = this;
            SetIsModifiedRectangleMovement = false;

            isWorkingSpaceFlippedVertically = false;
            isWorkingSpaceFlippedHorizontally = false;

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
                    if (_pixelData.GetPixelAlphaFromCanvas(indexWidth, indexHeight) != 0x00)
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
                        if (_pixelData.GetPixelAlphaFromCanvas(indexWidth, indexHeight) != 0x00)
                        {
                            extremePoint.Y = indexHeight;
                            xCoordinateOfExtremeTop = indexWidth;

                            // found extreme point --> set break conditions
                            indexHeight = (int)paintingAreaCanvasHeight;
                            indexWidth = 0;
                        }
                    }
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
                        if (_pixelData.GetPixelAlphaFromCanvas(indexWidth, indexHeight) != 0x00)
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
                        if (_pixelData.GetPixelAlphaFromCanvas(indexWidth, indexHeight) != 0x00)
                        {
                            extremePoint.Y = indexHeight;

                            // found extreme point --> set break conditions
                            indexHeight = 0;
                            indexWidth = 0;
                        }
                    }
            }
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
                _heightStampControl = (extremeRightAndBottomCoordinate.Y - extremeLeftAndTopCoordinate.Y + 1.0) * scaleValueWorkingSpace;
                _widthStampControl = (extremeRightAndBottomCoordinate.X - extremeLeftAndTopCoordinate.X + 1.0) * scaleValueWorkingSpace;

                Grid drawGrid = (Grid)RectangleShapeBaseControl.FindName("AreaToDrawGrid");

                PocketPaintApplication.GetInstance().StampControl.HorizontalAlignment = HorizontalAlignment.Left;
                PocketPaintApplication.GetInstance().StampControl.VerticalAlignment = VerticalAlignment.Top;

                RectangleShapeBaseControl.SetHeightOfControl(_heightStampControl);
                RectangleShapeBaseControl.SetWidthOfControl(_widthStampControl);

                TransformGroup workingSpaceTransformation =  PocketPaintApplication.GetInstance().PaintingAreaView.GetGridWorkingSpaceTransformGroup();

                ttfMoveStampControl.X = extremeLeftAndTopCoordinate.X * scaleValueWorkingSpace + workingSpaceTransformation.Value.OffsetX - drawGrid.Margin.Left;
                ttfMoveStampControl.Y = extremeLeftAndTopCoordinate.Y * scaleValueWorkingSpace + workingSpaceTransformation.Value.OffsetY - drawGrid.Margin.Top;

                if (isWorkingSpaceFlippedHorizontally)
                {
                    ttfMoveStampControl.Y = tgPaintingAreaCheckeredGrid.Value.OffsetY - drawGrid.Margin.Top - drawGrid.Height +
                        ((currentPaintApplication.PaintingAreaCanvas.Height - extremeLeftAndTopCoordinate.Y) * scaleValueWorkingSpace);
                }

                if (isWorkingSpaceFlippedVertically)
                {
                    ttfMoveStampControl.X = tgPaintingAreaCheckeredGrid.Value.OffsetX - drawGrid.Margin.Left - drawGrid.Width + 
                        ((currentPaintApplication.PaintingAreaCanvas.Width - extremeLeftAndTopCoordinate.X) * scaleValueWorkingSpace);
                }

                RectangleShapeBase.addTransformation(ttfMoveStampControl);
            }
            
        }

        public Point GetLeftTopPointOfStampedSelection()
                {
            uint canvasHeight = (uint)PocketPaintApplication.GetInstance().PaintingAreaCanvas.Height;
            uint canvasWidth = (uint)PocketPaintApplication.GetInstance().PaintingAreaCanvas.Width;

            uint widthOfStampSelection = (uint)PocketPaintApplication.GetInstance().StampControl.GetHeightOfRectangleStampSelection();
            uint heightOfStampSelection = (uint)PocketPaintApplication.GetInstance().StampControl.GetWidthOfRectangleStampSelection();

            Point currentLeftTopCoordinateOfStampSelection = GetXyOffsetBetweenPaintingAreaAndStampControlSelection();
            System.Diagnostics.Debug.WriteLine("cLT: " + currentLeftTopCoordinateOfStampSelection);
         
            return new Point(Convert.ToDouble(currentLeftTopCoordinateOfStampSelection.X), Convert.ToDouble(currentLeftTopCoordinateOfStampSelection.Y));
                }

        public void ResetCurrentCopiedSelection()
                {
            PocketPaintApplication.GetInstance().StampControl.Visibility = Visibility.Collapsed;
            imgBrush.ImageSource = null;
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

            //for the scale
            TransformGroup gridWorkingSpaceTransformGroup = currentPaintApplication.GridWorkingSpace.RenderTransform as TransformGroup;
            TransformGroup paintingAreaCanvasTransformGroup = currentPaintApplication.PaintingAreaCanvas.RenderTransform as TransformGroup;


            // Das Selection-Control soll die Zeichnung einschließen und nicht darauf liegen. Daher wird
            // dieser Wert mit 10 verwendet. Anschließend wird dann die Margin Left und Top, um 5 verringert.
            double doubleBorderWidthValue = _offsetMargin * 2.0;

            _transformGridMain.Children.Clear();

            isWorkingSpaceFlippedVertically = paintingAreaCanvasTransformGroup != null && (int)paintingAreaCanvasTransformGroup.Value.M11 == -1;
            isWorkingSpaceFlippedHorizontally = paintingAreaCanvasTransformGroup != null && (int)paintingAreaCanvasTransformGroup.Value.M22 == -1;

            FoundLeftPixel = false;

            if (currentPaintApplication.angularDegreeOfWorkingSpaceRotation == 0)
            {
                if (gridWorkingSpaceTransformGroup != null)
                    _scaleValueWorkingSpace = gridWorkingSpaceTransformGroup.Value.M11;
                _calculateAndSetStampControlPositionWithoutRotating(doubleBorderWidthValue, _scaleValueWorkingSpace, isWorkingSpaceFlippedHorizontally, isWorkingSpaceFlippedVertically);
            }
            else if (currentPaintApplication.angularDegreeOfWorkingSpaceRotation == 90)
            {
                // Attention: Working space is rotated 90°
                if (gridWorkingSpaceTransformGroup != null)
                    _scaleValueWorkingSpace = gridWorkingSpaceTransformGroup.Value.M12;
                _calculateAndSetStampControlPositionWith90DegreeRotation(doubleBorderWidthValue, _scaleValueWorkingSpace, isWorkingSpaceFlippedHorizontally, isWorkingSpaceFlippedVertically);
        }
            else if (currentPaintApplication.angularDegreeOfWorkingSpaceRotation == 180)
            {
                // Attention: Working space is rotated 180°
                if (gridWorkingSpaceTransformGroup != null)
                    _scaleValueWorkingSpace = Math.Abs(gridWorkingSpaceTransformGroup.Value.M11);
                _calculateAndSetStampControlPositionWith180DegreeRotation(doubleBorderWidthValue, _scaleValueWorkingSpace, isWorkingSpaceFlippedHorizontally, isWorkingSpaceFlippedVertically);
            }
            else if (currentPaintApplication.angularDegreeOfWorkingSpaceRotation == 270)
            {
                if (gridWorkingSpaceTransformGroup != null)
                    _scaleValueWorkingSpace = gridWorkingSpaceTransformGroup.Value.M21;
                // Attention: Working space is rotated 270°
                _calculateAndSetStampControlPositionWith270DegreeRotation(doubleBorderWidthValue, _scaleValueWorkingSpace, isWorkingSpaceFlippedHorizontally, isWorkingSpaceFlippedVertically);
            }

            currentPaintApplication.StampControl.Visibility = Visibility.Visible;
            currentPaintApplication.ProgressRing.IsActive = false;
        }

        private void _calculateAndSetStampControlPositionWith90DegreeRotation(double doubleBorderWidthValue, double scaleValueWorkingSpace, bool isWorkingSpaceFlippedHorizontally, bool isWorkingSpaceFlippedVertically)
        {
            PocketPaintApplication currentPaintApplication = PocketPaintApplication.GetInstance();
            TransformGroup tgPaintingAreaCheckeredGrid = currentPaintApplication.GridWorkingSpace.RenderTransform as TransformGroup;
            if (tgPaintingAreaCheckeredGrid == null)
            {
                return;
            }
            TranslateTransform ttfMoveStampControl = new TranslateTransform();
            double heightPaintingAreaCheckeredGrid = currentPaintApplication.GridWorkingSpace.Height;
            double widthPaintingAreaCheckeredGrid = currentPaintApplication.GridWorkingSpace.Width;

            Point extremeLeftAndTopCoordinate = new Point(0.0, 0.0);
            Point extremeRightAndBottomCoordinate = new Point(0.0, 0.0);

            bool isThereSomethingDrawn = currentPaintApplication.PaintingAreaCanvas.Children.Count != 0;
            if (isThereSomethingDrawn)
            {
                // TODO: David create a function for the following code.
                int xCoordinateOfExtremeTop = 0;
                extremeLeftAndTopCoordinate = GetExtremeLeftAndTopCoordinate(extremeLeftAndTopCoordinate.X, extremeLeftAndTopCoordinate.Y,
                                                                             ref xCoordinateOfExtremeTop);
                extremeRightAndBottomCoordinate = GetExtremeRightAndBottomCoordinate(extremeRightAndBottomCoordinate.X, extremeRightAndBottomCoordinate.Y,
                                                                                     extremeLeftAndTopCoordinate, xCoordinateOfExtremeTop);

                //width and height reversed cause of 90 degree rotation
                _widthStampControl = (extremeRightAndBottomCoordinate.Y - extremeLeftAndTopCoordinate.Y + 1.0) * scaleValueWorkingSpace;
                _heightStampControl = (extremeRightAndBottomCoordinate.X - extremeLeftAndTopCoordinate.X + 1.0) * scaleValueWorkingSpace;

                Grid drawGrid = (Grid)RectangleShapeBaseControl.FindName("AreaToDrawGrid");

                PocketPaintApplication.GetInstance().StampControl.HorizontalAlignment = HorizontalAlignment.Left;
                PocketPaintApplication.GetInstance().StampControl.VerticalAlignment = VerticalAlignment.Top;

                RectangleShapeBaseControl.SetHeightOfControl(_heightStampControl);
                RectangleShapeBaseControl.SetWidthOfControl(_widthStampControl);

                TransformGroup workingSpaceTransformation = PocketPaintApplication.GetInstance().PaintingAreaView.GetGridWorkingSpaceTransformGroup();

                ttfMoveStampControl.X = workingSpaceTransformation.Value.OffsetX - (extremeLeftAndTopCoordinate.Y * scaleValueWorkingSpace) - drawGrid.Margin.Left - _widthStampControl;
                ttfMoveStampControl.Y = workingSpaceTransformation.Value.OffsetY + (extremeLeftAndTopCoordinate.X * scaleValueWorkingSpace) - drawGrid.Margin.Top;

                if (isWorkingSpaceFlippedHorizontally)
                {
                   ttfMoveStampControl.X = tgPaintingAreaCheckeredGrid.Value.OffsetX - drawGrid.Margin.Left -
                        ((currentPaintApplication.PaintingAreaCanvas.Height - extremeLeftAndTopCoordinate.Y) * scaleValueWorkingSpace);
                }

                if (isWorkingSpaceFlippedVertically)
                {
                    ttfMoveStampControl.Y = tgPaintingAreaCheckeredGrid.Value.OffsetY - drawGrid.Margin.Top - drawGrid.Height +
                        ((currentPaintApplication.PaintingAreaCanvas.Width - extremeLeftAndTopCoordinate.X) * scaleValueWorkingSpace);
                }

                RectangleShapeBase.addTransformation(ttfMoveStampControl);
            }
        }

        private void _calculateAndSetStampControlPositionWith180DegreeRotation(double doubleBorderWidthValue, double scaleValueWorkingSpace, bool isWorkingSpaceFlippedHorizontally, bool isWorkingSpaceFlippedVertically)
        {
            PocketPaintApplication currentPaintApplication = PocketPaintApplication.GetInstance();
            TransformGroup tgPaintingAreaCheckeredGrid = currentPaintApplication.GridWorkingSpace.RenderTransform as TransformGroup;
            if (tgPaintingAreaCheckeredGrid == null)
            {
                return;
            }
            TranslateTransform ttfMoveStampControl = new TranslateTransform();
            double heightPaintingAreaCheckeredGrid = currentPaintApplication.GridWorkingSpace.Height;
            double widthPaintingAreaCheckeredGrid = currentPaintApplication.GridWorkingSpace.Width;

            Point extremeLeftAndTopCoordinate = new Point(widthPaintingAreaCheckeredGrid - 1.0, 0.0);
            Point extremeRightAndBottomCoordinate = new Point(0.0, heightPaintingAreaCheckeredGrid - 1.0);

            bool isThereSomethingDrawn = currentPaintApplication.PaintingAreaCanvas.Children.Count != 0;
            if (isThereSomethingDrawn)
            {
                int xCoordinateOfExtremeTop = 0;
                extremeLeftAndTopCoordinate = GetExtremeLeftAndTopCoordinate(extremeLeftAndTopCoordinate.X, extremeLeftAndTopCoordinate.Y,
                                                                             ref xCoordinateOfExtremeTop);
                extremeRightAndBottomCoordinate = GetExtremeRightAndBottomCoordinate(extremeRightAndBottomCoordinate.X, extremeRightAndBottomCoordinate.Y,
                                                                                     extremeLeftAndTopCoordinate, xCoordinateOfExtremeTop);

                _heightStampControl = (extremeRightAndBottomCoordinate.Y - extremeLeftAndTopCoordinate.Y + 1.0) * scaleValueWorkingSpace;
                _widthStampControl = (extremeRightAndBottomCoordinate.X - extremeLeftAndTopCoordinate.X + 1.0) * scaleValueWorkingSpace;

                Grid drawGrid = (Grid)RectangleShapeBaseControl.FindName("AreaToDrawGrid");

                PocketPaintApplication.GetInstance().StampControl.HorizontalAlignment = HorizontalAlignment.Left;
                PocketPaintApplication.GetInstance().StampControl.VerticalAlignment = VerticalAlignment.Top;

                RectangleShapeBaseControl.SetHeightOfControl(_heightStampControl);
                RectangleShapeBaseControl.SetWidthOfControl(_widthStampControl);

                TransformGroup workingSpaceTransformation = PocketPaintApplication.GetInstance().PaintingAreaView.GetGridWorkingSpaceTransformGroup();

                ttfMoveStampControl.X = workingSpaceTransformation.Value.OffsetX - (extremeLeftAndTopCoordinate.X * scaleValueWorkingSpace) - _widthStampControl - drawGrid.Margin.Left;
                ttfMoveStampControl.Y = workingSpaceTransformation.Value.OffsetY - (extremeLeftAndTopCoordinate.Y * scaleValueWorkingSpace) - drawGrid.Margin.Top - _heightStampControl;

                if (isWorkingSpaceFlippedHorizontally)
                {
                    ttfMoveStampControl.Y = tgPaintingAreaCheckeredGrid.Value.OffsetY - drawGrid.Margin.Top -
                        ((currentPaintApplication.PaintingAreaCanvas.Height - extremeLeftAndTopCoordinate.Y) * scaleValueWorkingSpace);
                }

                if (isWorkingSpaceFlippedVertically)
                {
                    ttfMoveStampControl.X = tgPaintingAreaCheckeredGrid.Value.OffsetX - drawGrid.Margin.Left -
                        ((currentPaintApplication.PaintingAreaCanvas.Width - extremeLeftAndTopCoordinate.X) * scaleValueWorkingSpace);
                }

                RectangleShapeBase.addTransformation(ttfMoveStampControl);
            }
        }

        private void _calculateAndSetStampControlPositionWith270DegreeRotation(double doubleBorderWidthValue, double scaleValueWorkingSpace, bool isWorkingSpaceFlippedHorizontally, bool isWorkingSpaceFlippedVertically)
        {
            PocketPaintApplication currentPaintApplication = PocketPaintApplication.GetInstance();
            TransformGroup tgPaintingAreaCheckeredGrid = currentPaintApplication.GridWorkingSpace.RenderTransform as TransformGroup;
            if (tgPaintingAreaCheckeredGrid == null)
            {
                return;
            }
            TranslateTransform ttfMoveStampControl = new TranslateTransform();
            double heightOfPaintingAreaCheckeredGrid = currentPaintApplication.GridWorkingSpace.Height;
            double widthOfPaintingAreaCheckeredGrid = currentPaintApplication.GridWorkingSpace.Width;

            Point extremeLeftAndTopCoordinate = new Point(widthOfPaintingAreaCheckeredGrid - 1.0, heightOfPaintingAreaCheckeredGrid - 1.0);
            Point extremeRightAndBottomCoordinate = new Point(0.0, 0.0);

            bool isThereSomethingDrawn = currentPaintApplication.PaintingAreaCanvas.Children.Count != 0;
            if (isThereSomethingDrawn)
            {
                int xCoordinateOfExtremeTop = 0;
                extremeLeftAndTopCoordinate = GetExtremeLeftAndTopCoordinate(extremeLeftAndTopCoordinate.X, extremeLeftAndTopCoordinate.Y,
                                                                             ref xCoordinateOfExtremeTop);
                extremeRightAndBottomCoordinate = GetExtremeRightAndBottomCoordinate(extremeRightAndBottomCoordinate.X, extremeRightAndBottomCoordinate.Y,
                                                                                     extremeLeftAndTopCoordinate, xCoordinateOfExtremeTop);

                _widthStampControl = (extremeRightAndBottomCoordinate.Y - extremeLeftAndTopCoordinate.Y + 1.0) * scaleValueWorkingSpace;
                _heightStampControl = (extremeRightAndBottomCoordinate.X - extremeLeftAndTopCoordinate.X + 1.0) * scaleValueWorkingSpace;

                Grid drawGrid = (Grid)RectangleShapeBaseControl.FindName("AreaToDrawGrid");

                PocketPaintApplication.GetInstance().StampControl.HorizontalAlignment = HorizontalAlignment.Left;
                PocketPaintApplication.GetInstance().StampControl.VerticalAlignment = VerticalAlignment.Top;

                RectangleShapeBaseControl.SetHeightOfControl(_heightStampControl);
                RectangleShapeBaseControl.SetWidthOfControl(_widthStampControl);

                TransformGroup workingSpaceTransformation = PocketPaintApplication.GetInstance().PaintingAreaView.GetGridWorkingSpaceTransformGroup();
        
                ttfMoveStampControl.X = workingSpaceTransformation.Value.OffsetX + (extremeLeftAndTopCoordinate.Y * scaleValueWorkingSpace) - drawGrid.Margin.Left;
                ttfMoveStampControl.Y = workingSpaceTransformation.Value.OffsetY - (extremeLeftAndTopCoordinate.X * scaleValueWorkingSpace) - drawGrid.Margin.Top - _heightStampControl;

                if (isWorkingSpaceFlippedHorizontally)
            {
                    ttfMoveStampControl.X = tgPaintingAreaCheckeredGrid.Value.OffsetX - drawGrid.Margin.Left - drawGrid.Width+
                         ((currentPaintApplication.PaintingAreaCanvas.Height - extremeLeftAndTopCoordinate.Y) * scaleValueWorkingSpace);
            }

                if (isWorkingSpaceFlippedVertically)
            {
                    ttfMoveStampControl.Y = tgPaintingAreaCheckeredGrid.Value.OffsetY - drawGrid.Margin.Top -
                        ((currentPaintApplication.PaintingAreaCanvas.Width - extremeLeftAndTopCoordinate.X) * scaleValueWorkingSpace);
            }

                RectangleShapeBase.addTransformation(ttfMoveStampControl);
            }
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
            if(HasPaintingAreaCanvasElements() || PocketPaintApplication.GetInstance().PaintingAreaView.IsAppBarButtonSelected("appBtnStampCopy"))
            {
                return true;
            }
            return false;
        }

        public void ResetAppBarButtonRectangleSelectionControl(bool activated)
        {
            AppBarButton appBarButtonReset = PocketPaintApplication.GetInstance().PaintingAreaView.GetAppBarResetButton();
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

            Point cornerCoordinates = new Point(_transformGridMain.Value.OffsetX, _transformGridMain.Value.OffsetY);     

            if (currentPaintApplication.angularDegreeOfWorkingSpaceRotation == 0)
        {
                cornerCoordinates.X += GridMain.Margin.Left;
                cornerCoordinates.Y += GridMain.Margin.Top;

                double offsetX = (cornerCoordinates.X - tgPaintingAreaCheckeredGrid.Value.OffsetX) / _scaleValueWorkingSpace;
                double offsetY = (cornerCoordinates.Y - tgPaintingAreaCheckeredGrid.Value.OffsetY) / _scaleValueWorkingSpace;

                if (isWorkingSpaceFlippedHorizontally)
            {
                    offsetY = PocketPaintApplication.GetInstance().GridWorkingSpace.Height - (offsetY + GridMain.Height / _scaleValueWorkingSpace);
            }

                if (isWorkingSpaceFlippedVertically)
            {
                    offsetX = PocketPaintApplication.GetInstance().GridWorkingSpace.Width - (offsetX + GridMain.Width / _scaleValueWorkingSpace);
            }
                return new Point(Math.Ceiling(offsetX), Math.Ceiling(offsetY));
        }
            else if (currentPaintApplication.angularDegreeOfWorkingSpaceRotation == 90)
            {
                cornerCoordinates.X += (GridMain.Margin.Left + GridMain.Width);
                cornerCoordinates.Y += GridMain.Margin.Top;

                double offsetY = (tgPaintingAreaCheckeredGrid.Value.OffsetX - cornerCoordinates.X) / _scaleValueWorkingSpace;
                double offsetX = (cornerCoordinates.Y - tgPaintingAreaCheckeredGrid.Value.OffsetY) / _scaleValueWorkingSpace;

                if (isWorkingSpaceFlippedHorizontally)
            {
                    offsetY = PocketPaintApplication.GetInstance().GridWorkingSpace.Height - (offsetY + GridMain.Width / _scaleValueWorkingSpace);
            }

                if (isWorkingSpaceFlippedVertically)
            {
                    offsetX = PocketPaintApplication.GetInstance().GridWorkingSpace.Width - (offsetX + GridMain.Height / _scaleValueWorkingSpace);
            }

                return new Point(offsetX, offsetY);
        }
            else if (currentPaintApplication.angularDegreeOfWorkingSpaceRotation == 180)
            {
                cornerCoordinates.X += (GridMain.Margin.Left + GridMain.Width);
                cornerCoordinates.Y += (GridMain.Margin.Top + GridMain.Height);

                double offsetX = (tgPaintingAreaCheckeredGrid.Value.OffsetX - cornerCoordinates.X) / _scaleValueWorkingSpace;
                double offsetY = (tgPaintingAreaCheckeredGrid.Value.OffsetY - cornerCoordinates.Y) / _scaleValueWorkingSpace;

                if (isWorkingSpaceFlippedHorizontally)
        {
                    offsetY = PocketPaintApplication.GetInstance().GridWorkingSpace.Height - (offsetY + GridMain.Height / _scaleValueWorkingSpace);
        }

                if (isWorkingSpaceFlippedVertically)
        {
                    offsetX = PocketPaintApplication.GetInstance().GridWorkingSpace.Width - (offsetX + GridMain.Width / _scaleValueWorkingSpace);
        }

                return new Point(offsetX, offsetY);
        }
            else if (currentPaintApplication.angularDegreeOfWorkingSpaceRotation == 270)
            {
                cornerCoordinates.X += GridMain.Margin.Left;
                cornerCoordinates.Y += (GridMain.Margin.Top + GridMain.Height);

                double offsetX = (tgPaintingAreaCheckeredGrid.Value.OffsetY - cornerCoordinates.Y) / _scaleValueWorkingSpace;
                double offsetY = (cornerCoordinates.X - tgPaintingAreaCheckeredGrid.Value.OffsetX) / _scaleValueWorkingSpace;

                if (isWorkingSpaceFlippedHorizontally)
                {
                    offsetY = PocketPaintApplication.GetInstance().GridWorkingSpace.Height - (offsetY + GridMain.Width / _scaleValueWorkingSpace);
                }

                if (isWorkingSpaceFlippedVertically)
                {
                    offsetX = PocketPaintApplication.GetInstance().GridWorkingSpace.Width - (offsetX + GridMain.Height / _scaleValueWorkingSpace);
                }

                return new Point(offsetX, offsetY);
                }
                else
                {
                return new Point(0, 0);
                }

        }

        public ImageSource GetImageSourceStampedImage()
            {
            return imgBrush.ImageSource;
        }

        public void SetOriginalSizeOfStampedImage(double height, double width)
            {
            OriginalHeightStampedImage = height;
            OriginalWidthStampedImage = width;
        }


        public void SetSourceImageStamp(ImageSource imageSource)
        {
            var degree = PocketPaintApplication.GetInstance().angularDegreeOfWorkingSpaceRotation;
            TransformGroup transformation = new TransformGroup();
            RotateTransform rt = new RotateTransform();
            ScaleTransform flip = new ScaleTransform();

            rt.Angle = degree;
            rt.CenterX = 0.5;
            rt.CenterY = 0.5;

            if (isWorkingSpaceFlippedHorizontally)
        {
                flip.ScaleY = -1;
        }

            if (isWorkingSpaceFlippedVertically)
            {
                flip.ScaleX = -1;
            }

            flip.CenterX = 0.5;
            flip.CenterY = 0.5;

            transformation.Children.Add(flip);
            transformation.Children.Add(rt);

            imgBrush.RelativeTransform = transformation;

            imgBrush.ImageSource = imageSource;
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
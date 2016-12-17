using Catrobat.Paint.WindowsPhone.Command;
using System;
using System.IO;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace Catrobat.Paint.WindowsPhone.Tool
{
    class StampTool : RectangleShapeBaseTool
    {
        public StampTool()
        {
            ToolType = ToolType.Stamp;
             
        }

        public override void HandleDown(object arg)
        {
            
        }

        public override void HandleMove(object arg)
        {
            
        }

        public override void HandleUp(object arg)
        
        {
        }

        public override void Draw(object o)
        {
            
        }

        public async void StampCopy()
        {
            PocketPaintApplication currentPaintApplication = PocketPaintApplication.GetInstance();

            double heightStampControl = currentPaintApplication.StampControl.GetHeightOfRectangleStampSelection();
            double widthStampControl = currentPaintApplication.StampControl.GetWidthOfRectangleStampSelection();

            heightStampControl -= (currentPaintApplication.StampControl.image.Margin.Left * 2);
            widthStampControl -= (currentPaintApplication.StampControl.image.Margin.Top * 2);
            double croppedImageHeight = heightStampControl, croppedImageWidth = widthStampControl;

            Point leftTopPointStampSelection = currentPaintApplication.StampControl.GetLeftTopPointOfStampedSelection();

            //check if stamp control is beside the working space area
            if (leftTopPointStampSelection.X < 0)
            {
                croppedImageWidth = widthStampControl + leftTopPointStampSelection.X;
            }
            if(leftTopPointStampSelection.Y < 0)
            {
                croppedImageHeight = heightStampControl + leftTopPointStampSelection.Y;
            }

            if (currentPaintApplication.angularDegreeOfWorkingSpaceRotation == 90 || currentPaintApplication.angularDegreeOfWorkingSpaceRotation == 180)
            {
                double temp = croppedImageHeight;
                croppedImageHeight = croppedImageWidth;
                croppedImageWidth = temp;
            }

            double xOffsetStampControl = leftTopPointStampSelection.X + currentPaintApplication.StampControl.image.Margin.Left;
            double yOffsetStampControl = leftTopPointStampSelection.Y + currentPaintApplication.StampControl.image.Margin.Top;

            string filename = "stamp" + ".png";
            await currentPaintApplication.StorageIo.WriteBitmapToPngMediaLibrary(filename);
            StorageFile storageFile = await KnownFolders.PicturesLibrary.GetFileAsync(filename);
            InMemoryRandomAccessStream mrAccessStream = new InMemoryRandomAccessStream();

            using (Stream stream = await storageFile.OpenStreamForReadAsync())
            {
                using (var memStream = new MemoryStream())
                {
                    await stream.CopyToAsync(memStream);
                    memStream.Position = 0;

                    BitmapDecoder decoder = await BitmapDecoder.CreateAsync(memStream.AsRandomAccessStream());
                    BitmapEncoder encoder = await BitmapEncoder.CreateForTranscodingAsync(mrAccessStream, decoder);

                    encoder.BitmapTransform.ScaledHeight = (uint)currentPaintApplication.PaintingAreaCanvas.RenderSize.Height;
                    encoder.BitmapTransform.ScaledWidth = (uint)currentPaintApplication.PaintingAreaCanvas.RenderSize.Width;

                    BitmapBounds bounds = new BitmapBounds
                    {
                        Height = (uint) croppedImageHeight - 1,
                        Width = (uint) croppedImageWidth - 1,
                        X = (uint) ((xOffsetStampControl) < 0 ? 0 : xOffsetStampControl),
                        Y = (uint) ((yOffsetStampControl) < 0 ? 0 : yOffsetStampControl)
                    };

                    encoder.BitmapTransform.Bounds = bounds;

                    // write out to the stream
                    try
                    {
                        await encoder.FlushAsync();
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }

                //render the stream to the screen
                WriteableBitmap wbCroppedBitmap = new WriteableBitmap((int)croppedImageWidth, (int)croppedImageHeight);
                wbCroppedBitmap.SetSource(mrAccessStream);

                currentPaintApplication.StampControl.SetSourceImageStamp(wbCroppedBitmap);
            }
        }

        public void StampClear()
        {
            PocketPaintApplication.GetInstance().StampControl.SetSourceImageStamp(null);
        }

        public void StampPaste()
        {
            PocketPaintApplication currentPaintApplication = PocketPaintApplication.GetInstance();

            double heightStampControl = currentPaintApplication.StampControl.GetHeightOfRectangleStampSelection();
            double widthStampControl = currentPaintApplication.StampControl.GetWidthOfRectangleStampSelection();

            heightStampControl -= (currentPaintApplication.StampControl.image.Margin.Left * 2);
            widthStampControl -= (currentPaintApplication.StampControl.image.Margin.Top * 2);

            Point leftTopPointStampSelection = currentPaintApplication.StampControl.GetLeftTopPointOfStampedSelection();
            double xCoordinateOnWorkingSpace =  leftTopPointStampSelection.X + currentPaintApplication.StampControl.image.Margin.Left;
            double yCoordinateOnWorkingSpace = leftTopPointStampSelection.Y + currentPaintApplication.StampControl.image.Margin.Top;

            if (currentPaintApplication.angularDegreeOfWorkingSpaceRotation == 90 || currentPaintApplication.angularDegreeOfWorkingSpaceRotation == 180)
            {
                double temp = heightStampControl;
                heightStampControl = widthStampControl;
                widthStampControl = temp;
            }

            System.Diagnostics.Debug.WriteLine("xCoor: " + xCoordinateOnWorkingSpace + " " + yCoordinateOnWorkingSpace);
            Image stampedImage = new Image
            {
                Source = currentPaintApplication.StampControl.GetImageSourceStampedImage(),
                Height = heightStampControl,
                Width = widthStampControl,
                Stretch = Stretch.Fill
            };

            currentPaintApplication.PaintingAreaView.addElementToPaintingAreCanvas(stampedImage, (int)(xCoordinateOnWorkingSpace), (int)(yCoordinateOnWorkingSpace));
            CommandManager.GetInstance().CommitCommand(new StampCommand((uint)xCoordinateOnWorkingSpace, (uint)yCoordinateOnWorkingSpace, stampedImage));
        }

        public void StampPaste(uint xCoordinateOnWorkingSpace, uint yCoordinateOnWorkingSpace, Image stampedImage)
        {
            Canvas.SetLeft(stampedImage, xCoordinateOnWorkingSpace);
            Canvas.SetTop(stampedImage, yCoordinateOnWorkingSpace);
            PocketPaintApplication.GetInstance().PaintingAreaCanvas.Children.Add(stampedImage);
        }

        public override void ResetDrawingSpace()
        {
            PocketPaintApplication.GetInstance().StampControl.SetStampSelection();
            PocketPaintApplication.GetInstance().StampControl.ResetCurrentCopiedSelection();
            PocketPaintApplication.GetInstance().PaintingAreaView.app_btnStampClear_Click(new object(), new RoutedEventArgs());
        }

        public override void ResetUsedElements()
        {
            PocketPaintApplication.GetInstance().StampControl.SetSourceImageStamp(null);
        }
    }
}

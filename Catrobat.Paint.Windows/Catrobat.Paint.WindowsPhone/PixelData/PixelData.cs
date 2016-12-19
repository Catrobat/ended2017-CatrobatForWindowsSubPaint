using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using System.Runtime.InteropServices.WindowsRuntime;
using System.IO;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Controls;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI;

namespace Catrobat.Paint.WindowsPhone.PixelData
{
    public class PixelData
    {
        private WriteableBitmap Bitmap;
        public String ReturnString;
        private int X;
        private int Y;
        public byte[] pixelsCanvas;
        public byte[] pixelsCanvasEraser;
        public int pixelHeightCanvas;
        public int pixelWidthCanvas;

        #region Contructor
        public PixelData()
        {
            pixelsCanvas = null;
            pixelHeightCanvas = 0;
            pixelWidthCanvas = 0;
            ReturnString = string.Empty;
        }
        #endregion

        #region PipetteTool
        public async Task<SolidColorBrush> GetPixel(WriteableBitmap bitmap, int x, int y)
        {
            this.X = x;
            this.Y = y;
            this.Bitmap = bitmap;
            return await GetPixelColor();
        }

        private async Task<SolidColorBrush> GetPixelColor()
        {
            RenderTargetBitmap retarbi = new RenderTargetBitmap();
            var paintingCanvas = PocketPaintApplication.GetInstance().PaintingAreaCanvas;
            await retarbi.RenderAsync(paintingCanvas,
                                (int)paintingCanvas.ActualWidth,
                                (int)paintingCanvas.ActualHeight);

            Windows.Storage.Streams.IBuffer buffer = await (retarbi.GetPixelsAsync());
            var pixels = WindowsRuntimeBufferExtensions.ToArray(buffer);

            var width = retarbi.PixelWidth;
            var height = retarbi.PixelHeight;

            var bitmap = PocketPaintApplication.GetInstance().Bitmap;
            if (bitmap == null)
                return null;
            double NormfactorX = (double)width / (double)bitmap.PixelWidth;
            double NormfactorY = (double)height / (double)bitmap.PixelHeight;

            double doubleY = ((double)Y) * NormfactorY;
            double doubleX = ((double)X) * NormfactorX;

            int intX = (int)Math.Round(doubleX, 0);
            int intY = (int)Math.Round(doubleY, 0);

            int intTemp = intY * width;
            int intXTemp = intTemp + intX;
            int intValue = intXTemp * 4;

            var a = pixels[intValue + 3];
            var r = pixels[intValue + 2];
            var g = pixels[intValue + 1];
            var B = pixels[intValue];

            return new SolidColorBrush(Windows.UI.Color.FromArgb(a, r, g, B));
        }
        #endregion

        #region FillTool
        public byte[] ConvertArray(byte[] pixles, int width, int height)
        {
            if (pixles != null)
            {
                int pixelWidth = PocketPaintApplication.GetInstance().Bitmap.PixelWidth;
                int pixelHeight = PocketPaintApplication.GetInstance().Bitmap.PixelHeight;

                byte[] PixelsBitmap = new byte[pixelWidth * pixelHeight * 4];

                double NormfactorX = pixelWidth / (double)width;
                double NormfactorY = pixelHeight / (double)height;

                for (int i = 0; i < height; i++)
                {
                    for (int j = 0; j < width; j++)
                    {

                        double doubleY = i * NormfactorY;
                        double doubleX = j * NormfactorX;

                        int intX = (int)Math.Round(doubleX, 0);
                        int intY = (int)Math.Round(doubleY, 0);

                        int intTemp = intY * pixelWidth;
                        int intXTemp = intTemp + intX;
                        int intValue = intXTemp * 4;
                        if (intValue >= PixelsBitmap.Length)
                            break;

                        int intTempCanvas = i * width;
                        int intXTempCanvas = intTempCanvas + j;
                        int intValueCanvas = intXTempCanvas * 4;

                        PixelsBitmap[intValue] = pixles[intValueCanvas];
                        PixelsBitmap[intValue + 1] = pixles[intValueCanvas + 1];
                        PixelsBitmap[intValue + 2] = pixles[intValueCanvas + 2];
                        PixelsBitmap[intValue + 3] = pixles[intValueCanvas + 3];

                    }
                }
                    

                return PixelsBitmap;
            }
            return null;
        }

        public async Task<int> preparePaintingAreaCanvasPixel()
        {
            RenderTargetBitmap retarbi = new RenderTargetBitmap();
            Canvas canvas = PocketPaintApplication.GetInstance().PaintingAreaCanvas;
            await retarbi.RenderAsync(canvas);

            Windows.Storage.Streams.IBuffer buffer = await (retarbi.GetPixelsAsync());
            pixelsCanvas = WindowsRuntimeBufferExtensions.ToArray(buffer);

            this.pixelHeightCanvas = retarbi.PixelHeight;
            this.pixelWidthCanvas = retarbi.PixelWidth;
            return 0;
        }

        public bool FloodFill4(Point coordinate, SolidColorBrush color)
        {
            try
            {
                if (color.Color.A == 0)
                {
                    color = new SolidColorBrush(Color.FromArgb(0x00, 0x00, 0x00, 0x00));
                }

                if (PocketPaintApplication.GetInstance().Bitmap == null)
                {
                    PocketPaintApplication.GetInstance().SaveAsWriteableBitmapToRam();
                }

                string newColor = ColorToString(color);
                coordinate = ConvertCoordinates(coordinate);

                string oldColor = GetPixelFromCanvas(coordinate);
                if (oldColor == newColor)
                    return false;

                Stack<Point> stackWithCoordinates = new Stack<Point>();
                stackWithCoordinates.Push(coordinate);
                while (stackWithCoordinates.Count > 0)
                {
                    var currentCoordinate = stackWithCoordinates.Pop();
                    SetPixel(currentCoordinate, newColor);

                    if (currentCoordinate.X > 1)
                    {
                        if (ComparePixelsColorForFloddFill(new Point(currentCoordinate.X - 1, currentCoordinate.Y), oldColor))
                            stackWithCoordinates.Push(new Point(currentCoordinate.X - 1, currentCoordinate.Y));
                    }

                    if (currentCoordinate.X < pixelWidthCanvas)
                    {
                        if (ComparePixelsColorForFloddFill(new Point(currentCoordinate.X + 1, currentCoordinate.Y), oldColor))
                            stackWithCoordinates.Push(new Point(currentCoordinate.X + 1, currentCoordinate.Y));
                    }

                    if (currentCoordinate.Y > 1)
                    {
                        if (ComparePixelsColorForFloddFill(new Point(currentCoordinate.X, currentCoordinate.Y - 1), oldColor))
                            stackWithCoordinates.Push(new Point(currentCoordinate.X, currentCoordinate.Y - 1));
                    }

                    if (currentCoordinate.Y < pixelHeightCanvas)
                    {
                        if (ComparePixelsColorForFloddFill(new Point(currentCoordinate.X, currentCoordinate.Y + 1), oldColor))
                            stackWithCoordinates.Push(new Point(currentCoordinate.X, currentCoordinate.Y + 1));
                    }
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void SetPixel(Point p, string color)
        {
            try
            {
                var intTemp = ((int)p.Y - 1) * pixelWidthCanvas;
                var intXTemp = intTemp + ((int)p.X - 1);
                var intValue = intXTemp * 4;

                var argb = color.Split('_');

                pixelsCanvas[intValue + 3] = Convert.ToByte(argb[0]);
                pixelsCanvas[intValue + 2] = Convert.ToByte(argb[1]);
                pixelsCanvas[intValue + 1] = Convert.ToByte(argb[2]);
                pixelsCanvas[intValue] = Convert.ToByte(argb[3]);
            }
            catch (Exception)
            {
                // ignored
            }
        }

        public List<Point> GetWhitePixels()
        {
            byte ff = Convert.ToByte(0xff);
            List<Point> results = new List<Point>();

            //TODO: An Philipp: Wofür wird hier die canvas-Variable benötigt?
            if (pixelsCanvasEraser != null && pixelsCanvasEraser.Length > 0)
            {
                for (int x = 0; x < pixelWidthCanvas; x++)
                {
                    for (int y = 0; y < pixelHeightCanvas; y++)
                    {
                        int Temp = y * pixelWidthCanvas;
                        int XTemp = Temp + x;
                        int Value = XTemp * 4;

                        if (pixelsCanvasEraser[Value] == ff &&
                        pixelsCanvasEraser[Value + 1] == ff &&
                        pixelsCanvasEraser[Value + 2] == ff &&
                        pixelsCanvasEraser[Value + 3] == ff)
                        {
                            results.Add(new Point(x, y));
                        }
                    }
                }
            }
            return results;
        }

        public async Task<Image> BufferToImage()
        {
            var wb = new WriteableBitmap(pixelWidthCanvas, pixelHeightCanvas);

            await wb.PixelBuffer.AsStream().WriteAsync(pixelsCanvas, 0, pixelsCanvas.Length);

            PocketPaintApplication.GetInstance().PaintingAreaCanvas.Children.Clear();
            var image = new Image
            {
                Stretch = Stretch.Uniform,
                Source = wb,
                Height = PocketPaintApplication.GetInstance().PaintingAreaCanvas.Height,
                Width = PocketPaintApplication.GetInstance().PaintingAreaCanvas.Width
            };

            return image;
        }

        public async Task<bool> PixelBufferToBitmap()
        {
            try
            {
                var bitmapPixels = ConvertArray(pixelsCanvas, pixelWidthCanvas, pixelHeightCanvas);
                var wbCroppedBitmap = new WriteableBitmap(PocketPaintApplication.GetInstance().Bitmap.PixelWidth, PocketPaintApplication.GetInstance().Bitmap.PixelHeight);
                await wbCroppedBitmap.PixelBuffer.AsStream().WriteAsync(bitmapPixels, 0, bitmapPixels.Length);

                var image = new Image
                {
                    Source = wbCroppedBitmap,
                    Height = wbCroppedBitmap.PixelHeight,
                    Width = wbCroppedBitmap.PixelWidth
                };

                PocketPaintApplication.GetInstance().PaintingAreaCanvas.Children.Clear();
                PocketPaintApplication.GetInstance().PaintingAreaCanvas.Children.Add(image);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private Point ConvertCoordinates(Point oldPoint)
        {
            double NormfactorX = (double)pixelWidthCanvas / PocketPaintApplication.GetInstance().Bitmap.PixelWidth;
            double NormfactorY = (double)pixelHeightCanvas / PocketPaintApplication.GetInstance().Bitmap.PixelHeight;
            double doubleY = (oldPoint.Y) * NormfactorY;
            double doubleX = (oldPoint.X) * NormfactorX;
            return new Point(Math.Round(doubleX, 0), Math.Round(doubleY, 0));
        }

        public string GetPixelFromCanvas(Point p)
        {
            if (pixelsCanvas != null)
            {
                var intTemp = ((int)p.Y - 1) * pixelWidthCanvas;
                var intXTemp = intTemp + ((int)p.X - 1);
                var intValue = intXTemp * 4;

                var a = pixelsCanvas[intValue + 3];
                var r = pixelsCanvas[intValue + 2];
                var g = pixelsCanvas[intValue + 1];
                var b = pixelsCanvas[intValue];

                return RGBToString(a, r, g, b);
            }
            return null;
        }

        public bool ComparePixelsColorForFloddFill(Point p, string oldColor)
        {
            if (pixelsCanvas != null)
            {
                var intTemp = ((int)p.Y - 1) * pixelWidthCanvas;
                var intXTemp = intTemp + ((int)p.X - 1);
                var intValue = intXTemp * 4;

                var aO = oldColor.Split('_')[0];
                var rO = oldColor.Split('_')[1];
                var gO = oldColor.Split('_')[2];
                var bO = oldColor.Split('_')[3];
                var a = pixelsCanvas[intValue + 3];
                var r = pixelsCanvas[intValue + 2];
                var g = pixelsCanvas[intValue + 1];
                var b = pixelsCanvas[intValue];
                if (Math.Abs(Convert.ToInt16(aO) - (int)a) > 70)
                    return false;
                if (Math.Abs(Convert.ToInt16(rO) - (int)r) > 70)
                    return false;
                if (Math.Abs(Convert.ToInt16(gO) - (int)g) > 70)
                    return false;
                if (Math.Abs(Convert.ToInt16(bO) - (int)b) > 70)
                    return false;

                return true;
            }
            return false;
        }

        public string ColorToString(SolidColorBrush color)
        {
            return color.Color.A + "_" + color.Color.R + "_" + color.Color.G + "_" + color.Color.B;
        }

        public string RGBToString(int a, int r, int g, int b)
        {
            return a + "_" + r + "_" + g + "_" + b;
        }

        #endregion

        public async Task<int> PreparePaintingAreaCanvasForEraser()
        {
            RenderTargetBitmap retarbi = new RenderTargetBitmap();
            Canvas eraserCanvas = PocketPaintApplication.GetInstance().EraserCanvas;
            try
            {
                if (eraserCanvas.Visibility == Visibility.Collapsed)
                    eraserCanvas.Visibility = Visibility.Visible;
                await retarbi.RenderAsync(eraserCanvas);

                Windows.Storage.Streams.IBuffer buffer = await (retarbi.GetPixelsAsync());
                pixelsCanvasEraser = WindowsRuntimeBufferExtensions.ToArray(buffer);

                this.pixelHeightCanvas = retarbi.PixelHeight;
                this.pixelWidthCanvas = retarbi.PixelWidth;
            }
            catch 
            { 
                return 1; 
            }
            return 0;
        }

        public Byte GetPixelAlphaFromCanvas(int x, int y)
        {
            double NormfactorX = (double)pixelWidthCanvas / (double)PocketPaintApplication.GetInstance().PaintingAreaCanvas.Width;
            double NormfactorY = (double)pixelHeightCanvas / (double)PocketPaintApplication.GetInstance().PaintingAreaCanvas.Height;

            double doubleY = ((double)y) * NormfactorY;
            double doubleX = ((double)x) * NormfactorX;

            int intX = (int)Math.Round(doubleX, 0);
            int intY = (int)Math.Round(doubleY, 0);

            int intTemp = intY * pixelWidthCanvas;
            int intXTemp = intTemp + intX;
            int intValue = intXTemp * 4;

            return pixelsCanvas[intValue + 3];
        }

        public void SetPixel(List<Point> points, string c)
        {
            foreach (var point in points)
            {
                SetPixel(point, c);
            }
        }
    }
}

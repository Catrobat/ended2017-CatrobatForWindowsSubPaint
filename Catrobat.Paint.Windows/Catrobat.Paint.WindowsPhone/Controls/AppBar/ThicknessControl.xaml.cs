using System;
using System.Linq;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;

// Die Elementvorlage "Leere Seite" ist unter http://go.microsoft.com/fwlink/?LinkID=390556 dokumentiert.

namespace Catrobat.Paint.WindowsPhone.Controls.AppBar
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet werden kann oder auf die innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class ThicknessControl
    {
        Int32 _SliderThicknessTextboxLastValue = 1;
        double _multiplicator = PocketPaintApplication.GetInstance().size_width_multiplication;

        public ThicknessControl()
        {
            InitializeComponent();
            SetLayout();
        }

        private void SetLayout()
        {
            GrdLayoutRoot.Width *= _multiplicator;
            GrdLayoutRoot.Height *= _multiplicator;
            GrdThicknessKeyboard.Width *= _multiplicator;
            GrdThicknessKeyboard.Height *= _multiplicator;

            GrdSliderThickness.Width *= _multiplicator;
            GrdSliderThickness.Height *= _multiplicator;
            GrdSliderThickness.Margin = new Thickness(
                                            GrdSliderThickness.Margin.Left * _multiplicator,
                                            GrdSliderThickness.Margin.Top * _multiplicator,
                                            GrdSliderThickness.Margin.Right * _multiplicator,
                                            GrdSliderThickness.Margin.Bottom * _multiplicator);

            foreach (Object obj in GrdLayoutRoot.Children.Concat(GrdBrushType.Children.Concat(GrdSlider.Children.Concat(GrdThicknessKeyboard.Children))))
            {
                if (obj.GetType() == typeof(Button))
                {
                    Button button = ((Button)obj);
                    button.Height *= _multiplicator;
                    button.Width *= _multiplicator;

                    button.Margin = new Thickness(
                                            button.Margin.Left * _multiplicator,
                                            button.Margin.Top * _multiplicator,
                                            button.Margin.Right * _multiplicator,
                                            button.Margin.Bottom * _multiplicator);

                    button.FontSize *= _multiplicator;

                    var buttonContent = ((Button)obj).Content;
                    if (buttonContent != null && buttonContent is Image)
                    {
                        Image contentImage = (Image)buttonContent;
                        contentImage.Height *= _multiplicator;
                        contentImage.Width *= _multiplicator;

                        contentImage.Margin = new Thickness(
                                                contentImage.Margin.Left * _multiplicator,
                                                contentImage.Margin.Top * _multiplicator,
                                                contentImage.Margin.Right * _multiplicator,
                                                contentImage.Margin.Bottom * _multiplicator);
                    }
                }
                else if (obj is Slider)
                {
                    Slider slider = (Slider)obj;
                    slider.Height *= _multiplicator;
                    slider.Width *= _multiplicator;

                    slider.Margin = new Thickness(
                                            slider.Margin.Left * _multiplicator,
                                            slider.Margin.Top * _multiplicator,
                                            slider.Margin.Right * _multiplicator,
                                            slider.Margin.Bottom * _multiplicator);
                }
            }

            SliderThickness.Value = PocketPaintApplication.GetInstance().PaintData.thicknessSelected;
        }

        public void CheckAndSetPenLineCap(PenLineCap penLineCap)
        {
            SolidColorBrush brushGray = new SolidColorBrush(Colors.Gray);
            SolidColorBrush brushWhite = new SolidColorBrush(Colors.Black);
            if (penLineCap == PenLineCap.Round)
            {
                BtnRoundImage.BorderBrush = brushWhite;
                BtnSquareImage.BorderBrush = brushGray;
                BtnTriangleImage.BorderBrush = brushGray;
            }
            else if (penLineCap == PenLineCap.Square)
            {
                BtnRoundImage.BorderBrush = brushGray;
                BtnSquareImage.BorderBrush = brushWhite;
                BtnTriangleImage.BorderBrush = brushGray;
            }
            else
            {
                BtnRoundImage.BorderBrush = brushGray;
                BtnSquareImage.BorderBrush = brushGray;
                BtnTriangleImage.BorderBrush = brushWhite;
            }
        }

        public void RoundButton_OnClick(object sender, RoutedEventArgs e)
        {
            var penLineCap = PenLineCap.Round;
            PocketPaintApplication.GetInstance().PaintData.penLineCapSelected = penLineCap;
            PocketPaintApplication.GetInstance().cursorControl.changeCursorType(penLineCap);
            CheckAndSetPenLineCap(penLineCap);
        }

        public void SquareButton_OnClick(object sender, RoutedEventArgs e)
        {
            var penLineCap = PenLineCap.Square;
            PocketPaintApplication.GetInstance().PaintData.penLineCapSelected = penLineCap;
            PocketPaintApplication.GetInstance().cursorControl.changeCursorType(penLineCap);
            CheckAndSetPenLineCap(penLineCap);
        }

        public void SetValueBtnBrushThickness(int value)
        {
            BtnBrushThickness.Content = value.ToString();
        }

        public void SetValueSliderThickness(double value)
        {
            SliderThickness.Value = value;
        }

        public void TriangleButton_OnClick(object sender, RoutedEventArgs e)
        {
            var penLineCap = PenLineCap.Triangle;
            PocketPaintApplication.GetInstance().PaintData.penLineCapSelected = penLineCap;
            PocketPaintApplication.GetInstance().cursorControl.changeCursorType(penLineCap);
            CheckAndSetPenLineCap(penLineCap);
        }

        private void SliderThickness_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (SliderThickness != null)
            {
                BtnBrushThickness.Content = Convert.ToInt32(SliderThickness.Value).ToString();
                _SliderThicknessTextboxLastValue = Convert.ToInt32(SliderThickness.Value);
                PocketPaintApplication.GetInstance().PaintData.thicknessSelected = Convert.ToInt32(SliderThickness.Value);
                if (PocketPaintApplication.GetInstance().cursorControl != null)
                {
                    PocketPaintApplication.GetInstance().cursorControl.changeCursorsize();
                }
            }
        }

        private void ButtonNumbers_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null)
            {
                string getClickedButtonNumber = button.Name.Substring(8);
                if (BtnBrushThickness.Content == null || BtnBrushThickness.Content.ToString().Length < 2)
                {
                    BtnBrushThickness.Content += getClickedButtonNumber;
                }
                else if (BtnBrushThickness.Content.ToString().Length == 2)
                {
                    BtnBrushThickness.Content = "";
                    BtnBrushThickness.Content += getClickedButtonNumber;
                }
                CheckIfValueIsInRange();
            }
        }

        private void CheckIfValueIsInRange()
        {
            if (BtnBrushThickness.Content == null)
            {
                btnValue0.IsEnabled = true;
                btnValue1.IsEnabled = true;
                btnValue2.IsEnabled = true;
                btnValue3.IsEnabled = true;
                btnValue4.IsEnabled = true;
                btnValue5.IsEnabled = true;
                btnValue6.IsEnabled = true;
                btnValue7.IsEnabled = true;
                btnValue8.IsEnabled = true;
                btnValue9.IsEnabled = true;
                btnValue0.IsEnabled = false;
            }
            else
            {
                Int32 input = Convert.ToInt32(BtnBrushThickness.Content.ToString());
                if (input > 5 && input < 10)
                {
                    btnValue0.IsEnabled = false;
                    btnValue1.IsEnabled = false;
                    btnValue2.IsEnabled = false;
                    btnValue3.IsEnabled = false;
                    btnValue4.IsEnabled = false;
                    btnValue5.IsEnabled = false;
                    btnValue6.IsEnabled = false;
                    btnValue7.IsEnabled = false;
                    btnValue8.IsEnabled = false;
                    btnValue9.IsEnabled = false;
                }
                else if (input == 5)
                {
                    btnValue0.IsEnabled = true;
                    btnValue1.IsEnabled = false;
                    btnValue2.IsEnabled = false;
                    btnValue3.IsEnabled = false;
                    btnValue4.IsEnabled = false;
                    btnValue5.IsEnabled = false;
                    btnValue6.IsEnabled = false;
                    btnValue7.IsEnabled = false;
                    btnValue8.IsEnabled = false;
                    btnValue9.IsEnabled = false;
                }
                else if (input < 5)
                {
                    btnValue0.IsEnabled = true;
                    btnValue1.IsEnabled = true;
                    btnValue2.IsEnabled = true;
                    btnValue3.IsEnabled = true;
                    btnValue4.IsEnabled = true;
                    btnValue5.IsEnabled = true;
                    btnValue6.IsEnabled = true;
                    btnValue7.IsEnabled = true;
                    btnValue8.IsEnabled = true;
                    btnValue9.IsEnabled = true;
                }
                else
                {
                    btnValue0.IsEnabled = false;
                    btnValue1.IsEnabled = true;
                    btnValue2.IsEnabled = true;
                    btnValue3.IsEnabled = true;
                    btnValue4.IsEnabled = true;
                    btnValue5.IsEnabled = true;
                    btnValue6.IsEnabled = true;
                    btnValue7.IsEnabled = true;
                    btnValue8.IsEnabled = true;
                    btnValue9.IsEnabled = true;
                }

                SliderThickness.Value = Convert.ToDouble(input);
            }
        }

        private void btnDeleteNumbers_Click(object sender, RoutedEventArgs e)
        {
            if (BtnBrushThickness.Content != null && BtnBrushThickness.Content.ToString().Length > 0)
            {
                BtnBrushThickness.Content = BtnBrushThickness.Content.ToString().Remove(BtnBrushThickness.Content.ToString().Length - 1);
            }

            CheckIfValueIsInRange();
        }

        private void btnAccept_Click(object sender, RoutedEventArgs e)
        {
            CheckIfThicknessWasEntered();
            CheckIfValueIsInRange();

            GrdThicknessKeyboard.Visibility = Visibility.Collapsed;
            GrdSliderThickness.Margin = new Thickness(0.0, 0.0, 0.0, 0.0);
        }

        public void CheckIfThicknessWasEntered()
        {
            string sliderThicknessTextBoxValue = string.Empty;
            if (BtnBrushThickness.Content != null)
            {
                sliderThicknessTextBoxValue = BtnBrushThickness.Content.ToString();
            }

            if (!sliderThicknessTextBoxValue.Equals(""))
            {
                var sliderThicknessTextBoxIntValue = Convert.ToInt32(sliderThicknessTextBoxValue);

                if (!(sliderThicknessTextBoxIntValue >= 1 && sliderThicknessTextBoxIntValue <= 50))
                {
                    BtnBrushThickness.Content = _SliderThicknessTextboxLastValue.ToString();
                }
                else
                {
                    _SliderThicknessTextboxLastValue = sliderThicknessTextBoxIntValue;
                    SliderThickness.Value = sliderThicknessTextBoxIntValue;
                }
            }
            else
            {
                BtnBrushThickness.Content = _SliderThicknessTextboxLastValue.ToString();
            }

            BtnBrushThickness.Foreground = new SolidColorBrush(Colors.White);
        }

        private void BtnBrushThickness_Click(object sender, RoutedEventArgs e)
        {
            CheckIfThicknessWasEntered();
            if (GrdThicknessKeyboard.Visibility == Visibility.Collapsed)
            {
                GrdThicknessKeyboard.Visibility = Visibility.Visible;
                GrdSliderThickness.Margin = new Thickness(0.0, 0.0, 0.0, (180.0 * _multiplicator));
            }
            else
            {
                GrdThicknessKeyboard.Visibility = Visibility.Collapsed;
                GrdSliderThickness.Margin = new Thickness(0.0, 0.0, 0.0, 0.0);
            }
        }
        
    }
}

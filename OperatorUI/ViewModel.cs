using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Shapes;

namespace OperatorUI
{
    public sealed class ViewModel : INotifyPropertyChanged, IOperatorLogger
    {
        public ViewModel()
        {
            LogOutputs = new ObservableCollection<LogOutput>();
        }

        private int _counter;

        public int Counter
        {
            get { return _counter; }
            set
            {
                if (value == _counter) return;
                _counter = value;
                OnPropertyChanged();
            }
        }
        
        public ObservableCollection<LogOutput> LogOutputs { get; set; }

        public void AddLogOutput(LogOutput newOutput)
        {
            LogOutputs.Insert(0, newOutput);
            // TODO This never seems to return? :o
        }


        public event PropertyChangedEventHandler PropertyChanged;
        
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private Shape _layoutImage;
        public Shape LayoutImage
        {
            get { return _layoutImage; }
            set
            {
                _layoutImage = value;
                OnPropertyChanged();
            }
        }

        public void InitializeLayoutImage(MainWindow mainWindow)
        {
            double width = mainWindow.LayoutCanvas.Width;
            double height = width;
            double unit = width / 12;

            List<List<Vector>> shapeList = new List<List<Vector>>();

            // Borders
            List<Vector> borders = new List<Vector>();
            borders.Add(new Vector(0, 0));
            borders.Add(new Vector(0, width));
            borders.Add(new Vector(height, width));
            borders.Add(new Vector(height, 0));
            borders.Add(new Vector(0, 0));
            shapeList.Add(borders);

            // Outer lines
            List<Vector> outerShape = new List<Vector>();
            outerShape.Add(new Vector(0, 0));
            outerShape.Add(new Vector(0, 4 * unit));
            outerShape.Add(new Vector(8 * unit, 4 * unit));
            outerShape.Add(new Vector(8 * unit, 12 * unit));
            outerShape.Add(new Vector(12 * unit, 12 * unit));
            shapeList.Add(outerShape);

            // Inner lines
            List<Vector> innerShape = new List<Vector>();
            innerShape.Add(new Vector(2*unit, 0));
            innerShape.Add(new Vector(2*unit, 2*unit));
            innerShape.Add(new Vector(10*unit, 2*unit));
            innerShape.Add(new Vector(10*unit, 10*unit));
            innerShape.Add(new Vector(12*unit, 10*unit));
            shapeList.Add(innerShape);


            foreach (var shape in shapeList)
            {
                for (int i = 0; i < shape.Count - 1; i++)
                {
                    var line = new Line();
                    line.Stroke = Brushes.Black;
                    line.X1 = shape[i].X;
                    line.Y1 = shape[i].Y;
                    line.X2 = shape[i + 1].X;
                    line.Y2 = shape[i + 1].Y;
                    mainWindow.LayoutCanvas.Children.Add(line);
                }
            }

            // Add drinks
            // TODO: somehow link objects to private members so color can be changed
            // Lower
            var ellipse = new Ellipse();
            ellipse.Stroke = Brushes.DarkRed;
            ellipse.StrokeThickness = 3;
            ellipse.Width = 1.5*unit;
            ellipse.Height = 1.5*unit;
            Canvas.SetLeft(ellipse, 0.25*unit);
            Canvas.SetTop(ellipse, 2.25*unit);

            mainWindow.LayoutCanvas.Children.Add(ellipse);

            var ellipse2 = new Ellipse();
            ellipse2.Stroke = Brushes.DarkRed;
            ellipse2.StrokeThickness = 3;
            ellipse2.Width = 1.5 * unit;
            ellipse2.Height = 1.5 * unit;
            Canvas.SetLeft(ellipse2, 5.25 * unit);
            Canvas.SetTop(ellipse2, 2.25 * unit);

            mainWindow.LayoutCanvas.Children.Add(ellipse2);

            var ellipse3 = new Ellipse();
            ellipse3.Stroke = Brushes.DarkRed;
            ellipse3.StrokeThickness = 3;
            ellipse3.Width = 1.5 * unit;
            ellipse3.Height = 1.5 * unit;
            Canvas.SetLeft(ellipse3, 8.25 * unit);
            Canvas.SetTop(ellipse3, 5.25 * unit);

            mainWindow.LayoutCanvas.Children.Add(ellipse3);

            var ellipse4 = new Ellipse();
            ellipse4.Stroke = Brushes.DarkRed;
            ellipse4.StrokeThickness = 3;
            ellipse4.Width = 1.5 * unit;
            ellipse4.Height = 1.5 * unit;
            Canvas.SetLeft(ellipse4, 8.25 * unit);
            Canvas.SetTop(ellipse4, 10.25 * unit);

            mainWindow.LayoutCanvas.Children.Add(ellipse4);

            // Higher
            var ellipse5 = new Ellipse();
            ellipse5.Stroke = Brushes.DarkRed;
            ellipse5.StrokeThickness = 3;
            ellipse5.Width = 1.5 * unit;
            ellipse5.Height = 1.5 * unit;
            Canvas.SetLeft(ellipse5, 4.25 * unit);
            Canvas.SetTop(ellipse5, 0.25 * unit);

            mainWindow.LayoutCanvas.Children.Add(ellipse5);

            var ellipse6 = new Ellipse();
            ellipse6.Stroke = Brushes.DarkRed;
            ellipse6.StrokeThickness = 3;
            ellipse6.Width = 1.5 * unit;
            ellipse6.Height = 1.5 * unit;
            Canvas.SetLeft(ellipse6, 10.25 * unit);
            Canvas.SetTop(ellipse6, 0.25 * unit);

            mainWindow.LayoutCanvas.Children.Add(ellipse6);

            var ellipse7 = new Ellipse();
            ellipse7.Stroke = Brushes.DarkRed;
            ellipse7.StrokeThickness = 3;
            ellipse7.Width = 1.5 * unit;
            ellipse7.Height = 1.5 * unit;
            
            // Tooltip test
            var tooltip = new ToolTip();
            tooltip.Content = "Olen viinaa";
            ellipse.ToolTip = tooltip;
            tooltip.Placement = PlacementMode.Right;
            tooltip.HorizontalOffset = 10;
            tooltip.VerticalOffset = 10;

            Canvas.SetLeft(ellipse7, 10.25 * unit);
            Canvas.SetTop(ellipse7, 6.25 * unit);

            mainWindow.LayoutCanvas.Children.Add(ellipse7);




        }

        private class Vector
        {
            public Vector(double x, double y)
            {
                X = x;
                Y = y;
            }

            public double X;
            public double Y;
        }

        private Ellipse _testEllipse;
    }
}

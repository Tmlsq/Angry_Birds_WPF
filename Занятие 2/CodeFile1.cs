using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Input;

namespace Project3
{
    public class MainWindow : Window
    {
        private Canvas canvas;
        private TextBox angleTextBox;
        private TextBox velocityTextBox;
        private Button calculateButton;

        public MainWindow()
        {
            this.Title = "Траектория тела";
            this.Width = 700;
            this.Height = 700;
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;

            var grid = new Grid();
            this.Content = grid;

            grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition());

            // Панель ввода
            var inputPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(20),
                HorizontalAlignment = HorizontalAlignment.Center
            };

            // Элементы ввода
            angleTextBox = new TextBox { Width = 100, Margin = new Thickness(10), ToolTip = "Угол в градусах" };
            velocityTextBox = new TextBox { Width = 100, Margin = new Thickness(10), ToolTip = "Скорость (м/с)" };

            calculateButton = new Button
            {
                Content = "Рассчитать",
                Margin = new Thickness(10),
                Padding = new Thickness(5)
            };
            calculateButton.Click += CalculateButton_Click;

            // Добавляем элементы
            inputPanel.Children.Add(new Label { Content = "Угол:", VerticalAlignment = VerticalAlignment.Center });
            inputPanel.Children.Add(angleTextBox);
            inputPanel.Children.Add(new Label { Content = "Скорость:", VerticalAlignment = VerticalAlignment.Center });
            inputPanel.Children.Add(velocityTextBox);
            inputPanel.Children.Add(calculateButton);

            // Область для рисования
            canvas = new Canvas
            {
                Background = Brushes.WhiteSmoke,
                Margin = new Thickness(20)
            };

            grid.Children.Add(inputPanel);
            Grid.SetRow(inputPanel, 0);
            grid.Children.Add(canvas);
            Grid.SetRow(canvas, 1);
        }

        private void CalculateButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                double angle = double.Parse(angleTextBox.Text);
                double velocity = double.Parse(velocityTextBox.Text);

                if (velocity <= 0 || angle <= 0 || angle >= 90)
                {
                    MessageBox.Show("Некорректные данные!\nСкорость > 0\nУгол 0-90°", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                canvas.Children.Clear();
                DrawAxes();

                var points = CalculateTrajectory(angle, velocity);
                DrawPoints(points);
            }
            catch (FormatException)
            {
                MessageBox.Show("Введите числовые значения!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private PointCollection CalculateTrajectory(double angleDegrees, double initialVelocity)
        {
            const double g = 9.81;
            double angleRadians = angleDegrees * Math.PI / 180;
            double vx = initialVelocity * Math.Cos(angleRadians);
            double vy = initialVelocity * Math.Sin(angleRadians);

            double t = 0;
            double dt = 0.1;
            var points = new PointCollection();

            while (true)
            {
                double x = vx * t;
                double y = vy * t - 0.5 * g * t * t;

                if (y < 0) break;

                points.Add(new Point(x, y));
                t += dt;
            }

            ScalePoints(points);
            return points;
        }

        private void ScalePoints(PointCollection points)
        {
            if (points.Count == 0) return;

            double maxX = 0, maxY = 0;
            foreach (Point p in points)
            {
                if (p.X > maxX) maxX = p.X;
                if (p.Y > maxY) maxY = p.Y;
            }

            double scaleX = (canvas.ActualWidth - 150) / maxX;
            double scaleY = (canvas.ActualHeight - 150) / maxY;
            double scale = Math.Min(scaleX, scaleY) * 0.8;

            // Смещение для центрирования
            double offsetX = 75;
            double offsetY = canvas.ActualHeight - 75; // Нижний край (ось Y вверх)

            // Масштабируем точки
            for (int i = 0; i < points.Count; i++)
            {
                points[i] = new Point(
                    points[i].X * scale + offsetX,
                    offsetY - points[i].Y * scale);
            }
        }

        private void DrawAxes()
        {
            // Ось X
            var xAxis = new Line
            {
                X1 = 50,
                Y1 = canvas.ActualHeight - 75,
                X2 = canvas.ActualWidth - 50,
                Y2 = canvas.ActualHeight - 75,
                Stroke = Brushes.Black,
                StrokeThickness = 2
            };
            canvas.Children.Add(xAxis);

            // Ось Y
            var yAxis = new Line
            {
                X1 = 75,
                Y1 = canvas.ActualHeight - 50,
                X2 = 75,
                Y2 = 50,
                Stroke = Brushes.Black,
                StrokeThickness = 2
            };
            canvas.Children.Add(yAxis);

            // Подписи осей
            var xLabel = new TextBlock
            {
                Text = "X (м)",
                Margin = new Thickness(canvas.ActualWidth - 100, canvas.ActualHeight - 50, 0, 0),
                FontWeight = FontWeights.Bold
            };
            canvas.Children.Add(xLabel);

            var yLabel = new TextBlock
            {
                Text = "Y (м)",
                Margin = new Thickness(30, 30, 0, 0),
                FontWeight = FontWeights.Bold
            };
            canvas.Children.Add(yLabel);
        }

        private void DrawPoints(PointCollection points)
        {
            for (int i = 0; i < points.Count; i++)
            {
                var ellipse = new Ellipse
                {
                    Width = 6,
                    Height = 6,
                    Fill = Brushes.Red,
                    ToolTip = $"Шаг {i + 1}\nX: {GetOriginalX(points[i]):F1} м\nY: {GetOriginalY(points[i]):F1} м",
                    Margin = new Thickness(points[i].X - 3, points[i].Y - 3, 0, 0)
                };
                canvas.Children.Add(ellipse);
            }
        }

        private double GetOriginalX(Point scaledPoint)
        {
            return (scaledPoint.X - 75) / 0.8;
        }

        private double GetOriginalY(Point scaledPoint)
        {
            return (canvas.ActualHeight - 75 - scaledPoint.Y) / 0.8;
        }

        [STAThread]
        public static void Main()
        {
            Application app = new Application();
            app.Run(new MainWindow());
        }
    }
}
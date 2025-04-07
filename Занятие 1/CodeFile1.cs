using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace TrajectoryCalculator
{
    public class MainWindow : Window
    {
        private TextBox velocityInput;
        private TextBox angleInput;
        private ListView resultsListView;

        public MainWindow()
        {
            // Настройка окна
            Title = "Калькулятор траектории";
            Width = 600;
            Height = 450;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;

            // Создание элементов интерфейса
            var mainGrid = new Grid { Margin = new Thickness(10) };

            // Добавляем строки в Grid
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            // Поле ввода скорости
            var velocityPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 10) };
            velocityPanel.Children.Add(new TextBlock { Text = "Начальная скорость (м/с): ", VerticalAlignment = VerticalAlignment.Center });
            velocityInput = new TextBox { Width = 100, Margin = new Thickness(5, 0, 0, 0) };
            velocityPanel.Children.Add(velocityInput);
            Grid.SetRow(velocityPanel, 0);
            mainGrid.Children.Add(velocityPanel);

            // Поле ввода угла
            var anglePanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 10) };
            anglePanel.Children.Add(new TextBlock { Text = "Угол броска (градусы): ", VerticalAlignment = VerticalAlignment.Center });
            angleInput = new TextBox { Width = 100, Margin = new Thickness(5, 0, 0, 0) };
            anglePanel.Children.Add(angleInput);
            Grid.SetRow(anglePanel, 1);
            mainGrid.Children.Add(anglePanel);

            // Кнопка расчета
            var calculateButton = new Button
            {
                Content = "Рассчитать",
                Width = 100,
                Height = 30,
                Margin = new Thickness(0, 0, 0, 10)
            };
            calculateButton.Click += CalculateButton_Click;
            Grid.SetRow(calculateButton, 2);
            mainGrid.Children.Add(calculateButton);

            // Список результатов
            resultsListView = new ListView();
            var gridView = new GridView();
            gridView.Columns.Add(new GridViewColumn { Header = "Время (сек)", DisplayMemberBinding = new System.Windows.Data.Binding("Time"), Width = 100 });
            gridView.Columns.Add(new GridViewColumn { Header = "X (м)", DisplayMemberBinding = new System.Windows.Data.Binding("X"), Width = 100 });
            gridView.Columns.Add(new GridViewColumn { Header = "Y (м)", DisplayMemberBinding = new System.Windows.Data.Binding("Y"), Width = 100 });
            resultsListView.View = gridView;
            Grid.SetRow(resultsListView, 3);
            mainGrid.Children.Add(resultsListView);

            // Устанавливаем основной Grid как содержимое окна
            Content = mainGrid;
        }

        private void CalculateButton_Click(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(velocityInput.Text, out double v0) &&
                double.TryParse(angleInput.Text, out double alpha) &&
                v0 > 0 && alpha > 0 && alpha < 90)
            {
                Physics physics = new Physics();
                var trajectory = physics.CalculateTrajectory(v0, alpha);

                resultsListView.Items.Clear();
                foreach (var point in trajectory)
                {
                    resultsListView.Items.Add(new
                    {
                        Time = Math.Round(point.time, 2),
                        X = Math.Round(point.x, 2),
                        Y = Math.Round(point.y, 2)
                    });
                }
            }
            else
            {
                MessageBox.Show("Ошибка ввода! Проверьте, что:\n- Скорость > 0\n- Угол от 0 до 90 градусов", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    public class Physics
    {
        private const double g = 9.81;

        public List<(double x, double y, double time)> CalculateTrajectory(double v0, double alpha)
        {
            double radian = alpha * (Math.PI / 180);
            double vx = v0 * Math.Cos(radian);
            double vy = v0 * Math.Sin(radian);
            double totalTime = (2 * vy) / g;

            List<(double x, double y, double time)> trajectory = new List<(double x, double y, double time)>();

            for (double time = 0; time <= totalTime; time += 0.1)
            {
                double x = vx * time;
                double y = vy * time - (0.5 * g * time * time);
                if (y >= 0) // Игнорируем точки ниже земли
                {
                    trajectory.Add((x, y, time));
                }
            }

            return trajectory;
        }
    }

    public class Program
    {
        [STAThread]
        public static void Main()
        {
            Application app = new Application();
            app.Run(new MainWindow());
        }
    }
}
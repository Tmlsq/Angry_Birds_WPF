using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace TrajectoryCalculatorMenuApp
{
    public class MainWindow : Window
    {
        private double? savedVelocity = null;
        private double? savedAngle = null;

        public MainWindow()
        {
            Title = "Меню: Калькулятор траектории";
            Width = 500;
            Height = 500;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;

            var menu = new Menu();

            var fileMenu = new MenuItem { Header = "_Файл" };

            var newItem = new MenuItem { Header = "_Создать" };
            newItem.Click += NotAvailable_Click;
            fileMenu.Items.Add(newItem);

            var openItem = new MenuItem { Header = "_Открыть" };
            openItem.Click += NotAvailable_Click;
            fileMenu.Items.Add(openItem);

            var saveItem = new MenuItem { Header = "_Сохранить" };
            saveItem.Click += NotAvailable_Click;
            fileMenu.Items.Add(saveItem);

            var exitItem = new MenuItem { Header = "_Выход" };
            exitItem.Click += (s, e) => Close();
            fileMenu.Items.Add(new Separator());
            fileMenu.Items.Add(exitItem);

            var actionsMenu = new MenuItem { Header = "_Действия" };

            var inputItem = new MenuItem { Header = "_Ввести данные" };
            inputItem.Click += InputData_Click;
            actionsMenu.Items.Add(inputItem);

            var showCoordsItem = new MenuItem { Header = "_Показать координаты" };
            showCoordsItem.Click += ShowCoords_Click;
            actionsMenu.Items.Add(showCoordsItem);

            var helpMenu = new MenuItem { Header = "_Справка" };
            var aboutItem = new MenuItem { Header = "_О программе" };
            aboutItem.Click += (s, e) =>
            {
                MessageBox.Show("Программа для расчёта координат тела, брошенного под углом.\nАвтор: Шарапов Даниил, ПМ-201", "О программе", MessageBoxButton.OK, MessageBoxImage.Information);
            };
            helpMenu.Items.Add(aboutItem);

            menu.Items.Add(fileMenu);
            menu.Items.Add(actionsMenu);
            menu.Items.Add(helpMenu);

            var panel = new DockPanel();
            DockPanel.SetDock(menu, Dock.Top);
            panel.Children.Add(menu);

            var label = new TextBlock
            {
                Text = "Выберите действие из меню.",
                Margin = new Thickness(10),
                FontSize = 16,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            panel.Children.Add(label);

            Content = panel;
        }

        private void NotAvailable_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Данное действие на данный момент не доступно.", "Недоступно", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void InputData_Click(object sender, RoutedEventArgs e)
        {
            var velocity = PromptDialog("Введите начальную скорость (м/с):");
            var angle = PromptDialog("Введите угол броска (в градусах):");

            if (double.TryParse(velocity, out double v0) && v0 > 0 &&
                double.TryParse(angle, out double alpha) && alpha > 0 && alpha < 90)
            {
                savedVelocity = v0;
                savedAngle = alpha;
                MessageBox.Show($"Скорость: {v0} м/с\nУгол: {alpha}°", "Данные сохранены", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Некорректный ввод. Убедитесь, что:\n- скорость > 0\n- угол от 0 до 90°", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ShowCoords_Click(object sender, RoutedEventArgs e)
        {
            if (savedVelocity.HasValue && savedAngle.HasValue)
            {
                var physics = new Physics();
                var points = physics.CalculateTrajectory(savedVelocity.Value, savedAngle.Value);

                string result = "Время (с)\tX (м)\tY (м)\n";
                foreach (var p in points)
                {
                    result += $"{Math.Round(p.time, 2)}\t{Math.Round(p.x, 2)}\t{Math.Round(p.y, 2)}\n";
                }

                MessageBox.Show(result, "Результаты расчета", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Сначала введите данные!", "Нет данных", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private string PromptDialog(string message)
        {
            var window = new Window
            {
                Title = "Ввод данных",
                Width = 300,
                Height = 150,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                ResizeMode = ResizeMode.NoResize,
                Owner = this
            };

            var inputBox = new TextBox { Margin = new Thickness(10) };
            var okButton = new Button { Content = "ОК", IsDefault = true, Width = 60, Margin = new Thickness(10) };
            okButton.Click += (s, e) => window.DialogResult = true;

            var stack = new StackPanel();
            stack.Children.Add(new TextBlock { Text = message, Margin = new Thickness(10) });
            stack.Children.Add(inputBox);
            stack.Children.Add(okButton);

            window.Content = stack;

            return window.ShowDialog() == true ? inputBox.Text : null;
        }
    }

    public class Physics
    {
        private const double g = 9.81;

        public List<(double x, double y, double time)> CalculateTrajectory(double v0, double alpha)
        {
            double rad = alpha * Math.PI / 180;
            double vx = v0 * Math.Cos(rad);
            double vy = v0 * Math.Sin(rad);
            double totalTime = 2 * vy / g;

            var points = new List<(double x, double y, double time)>();
            for (double t = 0; t <= totalTime; t += 0.5)
            {
                double x = vx * t;
                double y = vy * t - 0.5 * g * t * t;
                if (y >= 0)
                    points.Add((x, y, t));
            }

            return points;
        }
    }

    public class Program
    {
        [STAThread]
        public static void Main()
        {
            var app = new Application();
            app.Run(new MainWindow());
        }
    }
}

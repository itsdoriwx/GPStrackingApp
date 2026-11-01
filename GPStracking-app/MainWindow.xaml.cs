using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GPStracking_app
{
    public partial class MainWindow : Window
    {
        private Vehicle vehicle;
        private GpsTracker tracker;
        private CancellationTokenSource? cts;
        private Warehouse warehouse;
        private Customer customer;
        private LogisticsManager manager;
        private Random rnd = new Random();

        private readonly Color colTextDark = (Color)ColorConverter.ConvertFromString("#0F172A");
        private readonly Color colBlue = (Color)ColorConverter.ConvertFromString("#1D4ED8");
        private readonly Color colGreen = (Color)ColorConverter.ConvertFromString("#16A34A");
        private readonly Color colRed = (Color)ColorConverter.ConvertFromString("#DC2626");

        private (double X, double Y, string City)[] points = new (double, double, string)[7];

        public MainWindow()
        {
            InitializeComponent();

            vehicle = new Truck();
            tracker = new GpsTracker(vehicle);
            warehouse = new Warehouse();
            customer = new Customer();
            manager = new LogisticsManager();

            manager.TripDispatchRequested += OnTripDispatched;
            BindTrackerEvents();

            warehouse.MessageGenerated += msg => LogManager(msg, colTextDark);
            manager.MessageGenerated += msg => LogManager(msg, colTextDark);

            customer.ParcelReceivedByCustomer += msg => LogManager(msg, colTextDark);
            customer.MessageGenerated += msg => LogCustomer(msg, colTextDark);
        }

        private void BindTrackerEvents()  // підписка на трекер
        { 
            if (vehicle is Truck t) tracker.ArrivedAtDestination += t.OnArrived;

            // коли транспорт доїде, виконаються усі чотири по черзі: і менеджер, і склад, і клієнт, і UI

            tracker.ArrivedAtDestination += warehouse.PrepareUnloading;
            tracker.ArrivedAtDestination += customer.NotifyArrival;
            tracker.ArrivedAtDestination += OnArrivedUI;

            tracker.DeviationFromRoute += msg => LogManager(msg, colRed);
            tracker.DeviationFromRoute += _ => manager.OnDeviation(); // менеджер отримує повідомлення про відхилення
            tracker.DeviationFromRoute += _ => warehouse.OnDeviation(); // _ = не використовується, бо методи OnDeviation не приймають параметрів

            tracker.SpeedLimitExceeded += msg => LogManager(msg, colRed);

            tracker.PositionChanged += (x, y) => // оновлення позиції маркера на карті
            {
                Canvas.SetLeft(truckMarker, x - 9);
                Canvas.SetTop(truckMarker, y - 9);
            };
        }

        private void mapCanvas_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateMap();
            ResetUI();
        }

        private void mapCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (mapCanvas.ActualWidth > 0) UpdateMap();
        }

        private void UpdateMap()
        {
            double w;

            if (mapCanvas.ActualWidth > 0)
            {
                w = mapCanvas.ActualWidth;
            }
            else
            {
                w = 800;
            }

            points = new (double, double, string)[]
            {
                (w * 0.10, 145, "Київ"),
                (w * 0.23, 122, "Наближаємося до Полтави.."),
                (w * 0.36, 100, "Полтава"),
                (w * 0.49, 100, "Наближаємося до Лубен.."),
                (w * 0.63, 100, "Лубни"),
                (w * 0.76, 68, "Під'їзд до Харкова"),
                (w * 0.90, 35, "Харків")
            };

            Canvas.SetLeft(pointKyiv, points[0].X - 5); Canvas.SetTop(pointKyiv, points[0].Y - 5);
            Canvas.SetLeft(lblKyiv, points[0].X - 12); Canvas.SetTop(lblKyiv, points[0].Y + 8);

            Canvas.SetLeft(pointPoltava, points[2].X - 5); Canvas.SetTop(pointPoltava, points[2].Y - 5);
            Canvas.SetLeft(lblPoltava, points[2].X - 18); Canvas.SetTop(lblPoltava, points[2].Y + 8);

            Canvas.SetLeft(pointLubny, points[4].X - 5); Canvas.SetTop(pointLubny, points[4].Y - 5);
            Canvas.SetLeft(lblLubny, points[4].X - 12); Canvas.SetTop(lblLubny, points[4].Y + 8);

            Canvas.SetLeft(pointKharkiv, points[6].X - 5); Canvas.SetTop(pointKharkiv, points[6].Y - 5);
            Canvas.SetLeft(lblKharkiv, points[6].X - 15); Canvas.SetTop(lblKharkiv, points[6].Y - 20);

            routeLine.Points.Clear();  // очищення лінії маршруту перед додаванням нових точок
            foreach (var p in points) routeLine.Points.Add(new Point(p.X, p.Y));
             
            if (btnRegister.IsEnabled) // якщо посилка ще не зареєстрована, то маркер ставимо на стартову точку
            {
                Canvas.SetLeft(truckMarker, points[0].X - 9);
                Canvas.SetTop(truckMarker, points[0].Y - 9);
            }
        }

        private void btnRegister_Click(object sender, RoutedEventArgs e)
        {
            cmbTransportType.IsEnabled = false;
            btnRegister.IsEnabled = false;
            btnAllowTrip.IsEnabled = true;

            btnAllowTrip.Background = new SolidColorBrush(colBlue);
            btnAllowTrip.Foreground = Brushes.White;

            if (cmbTransportType.SelectedIndex == 1)
            {
                vehicle = new Train();
            }
            else
            {
                var t = new Truck();
                t.MessageGenerated += msg => LogManager(msg, colTextDark);
                vehicle = t;
            }

            tracker = new GpsTracker(vehicle);
            BindTrackerEvents(); // підписка на трекер після зміни транспортного засобу

            txtStatus.Text = "Зареєстровано";
            txtCustomerAlert.Text = "Посилку зареєстровано у Києві. Очікується виїзд.";
            txtCustomerAlert.HorizontalAlignment = HorizontalAlignment.Left;
            statusAlertBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EFF6FF"));

            manager.RegisterShipment();
            LogCustomer("Посилку зареєстровано в Києві.", colTextDark);
            LogManager($"МЕНЕДЖЕР: Для рейсу обрано {vehicle.Name}.", colTextDark);
        }

        private void btnAllowTrip_Click(object sender, RoutedEventArgs e)
        {
            btnAllowTrip.IsEnabled = false;
            btnAllowTrip.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E2E8F0"));
            btnAllowTrip.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#64748B"));

            manager.DispatchTrip(vehicle.Name);
        }

        private async void OnTripDispatched() // коли менеджер дозволив рейс, починаємо рух
        {
            cts = new CancellationTokenSource();
            var token = cts.Token;

            try
            {
                txtStatus.Text = "Підготовка...";
                vehicle.Speed = 60;

                if (vehicle is IDeliverable d) LogManager($"{vehicle.Name}: {d.Deliver()}", colTextDark);
                if (vehicle is IMonitored m) LogManager($"{vehicle.Name}: {m.GetStatus()}", colTextDark);

                await Task.Delay(1000, token); // пауза перед початком руху

                txtStatus.Text = "В дорозі";
                txtSpeed.Text = "60 км/год";
                txtCustomerAlert.Text = "Посилка виїхала з Києва та прямує до Харкова.";

                LogCustomer("Посилка виїхала з Києва.", colTextDark);
                LogManager($"{vehicle.Name}: Двигун запущено, виїжджаємо на маршрут.", colTextDark);

                await MoveVehicle(token);

                var last = points[points.Length - 1];
                tracker.SetDestination(last.X, last.Y); // встановлюємо кінцеву точку для трекера
                tracker.Update(last.X, last.Y, last.X, last.Y, 0);
            }
            catch (OperationCanceledException) { }
        }

        private async Task MoveVehicle(CancellationToken token)
        {
            int steps = 22;
            bool isDeviated = rnd.Next(0, 100) < 50; // на горб
            bool isSpeeding = rnd.Next(0, 100) < 70;

            double devHeight = rnd.Next(25, 60);
            int devDir;

            if (rnd.Next(0, 2) == 0)
            {
                devDir = 1;
            }
            else // якшо 1
            {
                devDir = -1;
            }

            passedLine.Points.Clear();
            deviationLine.Points.Clear();

            for (int i = 0; i < points.Length - 1; i++)
            {
                var pStart = points[i];
                var pEnd = points[i + 1];

                bool isDevSeg = (i == 2 || i == 3) && isDeviated;
                bool isSpeedSeg = (i == 1 && isSpeeding);

                double speed;
                if (isDevSeg) speed = 30;
                else if (isSpeedSeg) speed = rnd.Next(96, 131);
                else if (pEnd.City.Contains("Наближаємося") || pEnd.City.Contains("Під'їзд")) speed = 90;
                else speed = 50;

                txtSpeed.Text = $"{speed} км/год";

                if (isDevSeg && i == 2) // якщо відхилення на сегменті Полтава-Лубни, то лінія маршруту має бути розірваною
                {
                    routeLine.Points.Clear();
                    for (int k = 0; k <= 2; k++) routeLine.Points.Add(new Point(points[k].X, points[k].Y));
                    for (int k = 4; k < points.Length; k++) routeLine.Points.Add(new Point(points[k].X, points[k].Y));
                }

                for (int step = 0; step <= steps; step++)
                {
                    double t = (double)step / steps;

                    // початок + (кінець − початок) * частка
                    double planX = pStart.X + (pEnd.X - pStart.X) * t;
                    double planY = pStart.Y + (pEnd.Y - pStart.Y) * t;
                    double posX = planX;
                    double posY = planY;

                    if (isDevSeg)
                    {
                        double baseProgress;

                        if (i == 2)
                        {
                            baseProgress = 0.0;
                        }
                        else
                        {
                            baseProgress = 0.5;
                        }

                        double progressInDev = baseProgress + (t * 0.5);
                        posY -= Math.Sin(progressInDev * Math.PI) * devHeight * devDir;
                        // devHeight = макс зсув, devDir = напрямок, Math.Sin = настільки близько до центру горба

                        deviationLine.Points.Add(new Point(posX, posY));
                    }
                    else
                    {
                        passedLine.Points.Add(new Point(posX, posY));
                    }

                    tracker.Update(posX, posY, planX, planY, speed);

                    double progress = (((double)i + t) / (points.Length - 1)) * 100;
                    txtProgress.Text = $"{(int)progress}%";

                    await Task.Delay(35, token);
                }

                string city = pEnd.City;
                LogManager($"GPS: Пройдено пункт -> {city}", colTextDark);

                if (city == "Полтава" || city == "Лубни")
                {
                    LogCustomer($"Пройдено пункт: {city}", colTextDark);
                    txtCustomerAlert.Text = $"Посилка пройшла контроль у м. {city}";
                }
            }
        }

        private void btnPickUp_Click(object sender, RoutedEventArgs e)
        {
            btnPickUp.IsEnabled = false;
            btnPickUp.Content = "Посилку отримано";
            btnPickUp.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E2E8F0"));
            btnPickUp.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#64748B"));

            txtCustomerAlert.Text = "Посилку отримано. Дякуємо!";
            txtCustomerAlert.HorizontalAlignment = HorizontalAlignment.Left;
            txtCustomerAlert.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#15803D"));
            statusAlertBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#DCFCE7"));
            statusAlertBorder.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#86EFAC"));

            customer.PickUpParcel();
        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            ResetUI();
        }

        private void ResetUI()
        {
            cts?.Cancel();
            tracker.Reset();
            UpdateMap();

            logManagerBox.Items.Clear();
            logCustomerBox.Items.Clear();
            passedLine.Points.Clear();
            deviationLine.Points.Clear();

            cmbTransportType.IsEnabled = true;
            btnRegister.IsEnabled = true;
            btnAllowTrip.IsEnabled = false;
            btnPickUp.IsEnabled = false;
            btnPickUp.Content = "Забрати посилку";

            btnAllowTrip.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E2E8F0"));
            btnAllowTrip.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#64748B"));

            btnPickUp.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E2E8F0"));
            btnPickUp.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#64748B"));

            txtSpeed.Text = "0 км/год";
            txtProgress.Text = "0%";
            txtStatus.Text = "Очікування";

            txtCustomerAlert.Text = "Очікування реєстрації посилки...";
            txtCustomerAlert.HorizontalAlignment = HorizontalAlignment.Left;
            txtCustomerAlert.Foreground = new SolidColorBrush(colTextDark);
            statusAlertBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F8FAFC"));
            statusAlertBorder.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E2E8F0"));

            if (points.Length > 0 && points[0].X > 0)
            {
                Canvas.SetLeft(truckMarker, points[0].X - 9);
                Canvas.SetTop(truckMarker, points[0].Y - 9);
            }
        }

        private void OnArrivedUI()
        {
            txtSpeed.Text = "0 км/год";
            txtStatus.Text = "Доставлено";
            txtProgress.Text = "100%";

            txtCustomerAlert.Text = "Посилка прибула у відділення. Заберіть замовлення";
            txtCustomerAlert.HorizontalAlignment = HorizontalAlignment.Left;
            txtCustomerAlert.Foreground = new SolidColorBrush(colGreen);
            statusAlertBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#DCFCE7"));

            btnPickUp.IsEnabled = true;
            btnPickUp.Background = new SolidColorBrush(colGreen);
            btnPickUp.Foreground = Brushes.White;
        }

        private void LogManager(string msg, Color col) // журнал логів для менеджера
        {
            var tb = new TextBlock
            {
                Text = $"[{DateTime.Now:HH:mm:ss}] {msg}",
                Foreground = new SolidColorBrush(col),
                Margin = new Thickness(0, 1, 0, 1)
            };

            logManagerBox.Items.Add(tb);
            logManagerBox.ScrollIntoView(tb);
        }

        private void LogCustomer(string msg, Color col) // журнал логів для клієнта
        {
            var tb = new TextBlock
            {
                Text = $"[{DateTime.Now:HH:mm:ss}] {msg}",
                Foreground = new SolidColorBrush(col),
                Margin = new Thickness(0, 1, 0, 1)
            };
            logCustomerBox.Items.Add(tb);
            logCustomerBox.ScrollIntoView(tb);
        }
    }
}
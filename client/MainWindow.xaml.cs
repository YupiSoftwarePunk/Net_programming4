using client.Serveces;
using client.Services;
using System.Collections.ObjectModel;
using System.IO;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string onlineUsers = "Пользователей онлайн: 0";
        public string UserName { get; }


        private TcpClientService tcpService;
        private UDPClientService udpService;
        private HttpService httpService;
        private WeatherService weatherService;


        private static readonly string[] AvailableNames = new[]
        {
            "Пельмень 1", "Пельмень 2", "Пельмень 3", "Пельмень 4", "Пельмень 5", "Тапок",
            "Тюлень", "405 база", "Крутой чувак", "Вареник 1", "Вареник 2", "Дядя Женя",
            "Лебовски", "Просто чувак", "Найсик", "ИГОООООРЬ!!!", "Проходимец", "Йоу"
        };

        public ObservableCollection<string> Messages { get; } = new();
        public ObservableCollection<string> OnlineUserNames { get; } = new();

        public ObservableCollection<string> Notifications { get; } = new();
        public ICommand SendMessageCommand { get; }
        private string messageInput = "";
        public string MessageInput
        {
            get => messageInput;
            set 
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    messageInput = value;
                });
            }
        }

        public string OnlineUsers
        {
            get => onlineUsers;
            set
            {
                onlineUsers = value;
                Application.Current.Dispatcher.Invoke(() =>
                {
                    OnlineCounter.Text = onlineUsers;
                });
            }
        }

        public MainWindow()
        {
            InitializeComponent();

            DataContext = this;

            Random random = new Random();
            UserName = AvailableNames[random.Next(AvailableNames.Length)];


            tcpService = new TcpClientService("192.168.31.146", 8080, UserName);
            tcpService.MessageReceived += msg =>
            {
                Application.Current.Dispatcher.Invoke(() => Messages.Add(msg));
            };

            tcpService.MessageSent += msg =>
            {
                Application.Current.Dispatcher.Invoke(() => Messages.Add(msg));
            };


            udpService = new UDPClientService("192.168.31.146", 8082, UserName);
            udpService.NotificationReceived += msg =>
            {
                Application.Current.Dispatcher.Invoke(() => Messages.Add(msg));
            };


            udpService.OnlineCountChanged += count =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    OnlineUsers = $"Пользователей онлайн: {count}";
                });
            };


            httpService = new HttpService("http://192.168.31.146:5000/api/");


            weatherService = new WeatherService("8ac6dac1419c86360ea78a30704379b6");

            _ = udpService.ConnectAsync();
        }



        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            tcpService?.Disconnect();
            udpService?.DisconnectAsync();
        }


        public async void SendMessageClick(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(MessageInput))
            {
                string messageToSend = MessageInput;
                await tcpService.SendMessageAsync(messageToSend);

                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageInputBox.Text = "";
                });
            }
        }

        public async void StatsClick(object sender, RoutedEventArgs e)
        {
            var stats = await httpService.GetStatsAsync();

            try
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Messages.Add($"Статистика: Пользователей онлайн = {stats.UsersCount}, " +
                    $"Количество сообщений = {stats.MessagesCount}");
                });
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Messages.Add("Ошибка HTTP: " + ex.Message);
                });
            }
        }



        public async void WeatherClick(object sender, RoutedEventArgs e)
        {
            var weather = await weatherService.GetWeatherAsync("Екатеринбург");
            try
            {
                string msg = $"Погода в {weather.City}: {weather.Temp}°C, {weather.Description}";
                await tcpService.SendMessageAsync(msg);
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Messages.Add("Ошибка получения погоды" + ex.Message);
                });
            }
        }
    }
}
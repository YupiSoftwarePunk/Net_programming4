using client.Serveces;
using System.Collections.ObjectModel;
using System.IO;
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
        private TcpClient client;


        private TcpClientService tcpService;
        private UDPClientService udpService;


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
            set { messageInput = value; }
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

            tcpService = new TcpClientService("192.168.43.159", 8080, UserName);
            tcpService.MessageReceived += msg =>
            {
                Application.Current.Dispatcher.Invoke(() => Messages.Add(msg));
            };

            udpService = new UDPClientService("192.168.43.159", 8081, UserName);
            udpService.NotificationReceived += msg =>
            {
                Application.Current.Dispatcher.Invoke(() => Notifications.Add(msg));
            };
            udpService.OnlineCountChanged += count =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    OnlineUsers = $"Пользователей онлайн: {count}";
                });
            };

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
                MessageInput = "";
                var stream = client.GetStream();
                byte[] data = Encoding.UTF8.GetBytes($"MSG:{UserName}:{messageToSend}");
                await stream.WriteAsync(data, 0, data.Length);
            }
        }
    }
}
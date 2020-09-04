using Newtonsoft.Json;
using System;
using System.Configuration;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace RTTCockpit
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            ServiceBusHandler.MessageReceived += new EventHandler<MessageReceivedEventArgs>(UpdateGuiText);
            RegisterServiceBus();
        }

        private void RegisterServiceBus()
        {
            var queueGateCamera = ConfigurationManager.AppSettings["QueueGateCamera"].ToString();
            var queueCarCamera = ConfigurationManager.AppSettings["QueueCarCamera"].ToString();
            var queueDepartureCamera = ConfigurationManager.AppSettings["QueueDepartureCamera"].ToString();

            queueGateCamera.RegisterQueue();
            queueCarCamera.RegisterQueue();
            queueDepartureCamera.RegisterQueue();
        }
        public void UpdateGuiText(object sender, MessageReceivedEventArgs e)
        {
            Console.WriteLine($"Received message: SequenceNumber:{e.Message.SystemProperties.SequenceNumber} Body:{Encoding.UTF8.GetString(e.Message.Body)}");
            dynamic data = JsonConvert.DeserializeObject(Encoding.UTF8.GetString(e.Message.Body));
            var bitmapImage = GetBitmapImageFromUrl(data["imageUrl"].ToString());

            if (data["name"] != null)
            {
                var securityCourseDone = bool.Parse(data["securityCourseDone"].ToString());

                Dispatcher.Invoke(() => labelName.Content = data["name"]);
                Dispatcher.Invoke(() => labelSecurityCourseDone.Content = securityCourseDone ? "Yes" : "No");
                Dispatcher.Invoke(() => labelSecurityCourseDone.Foreground = securityCourseDone ? new SolidColorBrush(Colors.Green) : new SolidColorBrush(Colors.Red));
                Dispatcher.Invoke(() => textBoxVisits.Text = data["visits"]);
                Dispatcher.Invoke(() => imageVisitor.Source = bitmapImage);
            }
            else if (data["licensePlate"] != null)
            {
                var expirationDate = DateTime.ParseExact(data["periodicVehiceInspectionValidUntil"].ToString(), "yyyyMMdd", CultureInfo.InvariantCulture);

                Dispatcher.Invoke(() => labelLicensePlate.Content = data["licensePlate"]);
                Dispatcher.Invoke(() => labelPeriodicMaintenance.Content = expirationDate.ToString("dd-MM-yyyy"));
                Dispatcher.Invoke(() => labelPeriodicMaintenance.Foreground = expirationDate > DateTime.Now.AddMonths(1) ? new SolidColorBrush(Colors.Green) : new SolidColorBrush(Colors.Red));
                Dispatcher.Invoke(() => labelFuelType.Content = data["fuelType"]);
                Dispatcher.Invoke(() => imageLicenseplate.Source = bitmapImage);
            }
            else if (data["ship"] != null)
            {
                var signatureFound = bool.Parse(data["signatureFound"].ToString());

                Dispatcher.Invoke(() => labelShipVisited.Content = data["ship"]);
                Dispatcher.Invoke(() => labelReason.Content = data["reason"]);
                Dispatcher.Invoke(() => labelDate.Content = data["date"]);
                Dispatcher.Invoke(() => labelSignedBy.Content = data["signedBy"]);
                Dispatcher.Invoke(() => labelShipVisited.Content = data["ship"]);
                Dispatcher.Invoke(() => labelSignatureFound.Content = signatureFound ? "Yes" : "No");
                Dispatcher.Invoke(() => labelSignatureFound.Foreground = signatureFound ? new SolidColorBrush(Colors.Green) : new SolidColorBrush(Colors.Red));
                Dispatcher.Invoke(() => imageWorksheet.Source = bitmapImage);
            }
        }

        public static BitmapImage GetBitmapImageFromUrl(string url)
        {
            WebRequest request = WebRequest.Create(url);
            Stream responseStream = request.GetResponse().GetResponseStream();
            Bitmap bitmap = new Bitmap(responseStream);

            using (var memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Png);
                memory.Position = 0;

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();

                return bitmapImage;
            }
        }
    }
}

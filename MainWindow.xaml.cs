using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace ChucMung20_10
{
    public partial class MainWindow : Window
    {
        // Cấu trúc lưu thông tin: Lời chúc + Đường dẫn ảnh (file trong máy hoặc link ảnh)
        private class PersonProfile
        {
            public string Wish { get; set; } = string.Empty;
            public string ImagePath { get; set; } = string.Empty;
        }

        private readonly Dictionary<string, PersonProfile> _profiles;
        private readonly List<string> _defaultWishes;
        private readonly DispatcherTimer _petalTimer;
        private readonly Random _random = new Random();

        public MainWindow()
        {
            InitializeComponent();

            // 1. Khởi tạo danh sách dữ liệu theo tên (Bạn có thể sửa/thêm tên và ảnh tại đây)
            // Lưu ý: Đặt ảnh vào thư mục "Images" của Project hoặc dùng link ảnh trực tiếp
            _profiles = new Dictionary<string, PersonProfile>(StringComparer.OrdinalIgnoreCase)
            {
                {
                    "mẹ", new PersonProfile
                    {
                        Wish = "Chúc Mẹ yêu ngày 20/10 thật nhiều sức khỏe, luôn nở nụ cười hạnh phúc trên môi. Cảm ơn Mẹ vì tất cả những hy sinh thầm lặng dành cho con!",
                        ImagePath = "Images/me.jpg" // Sẽ tự động dùng ảnh dự phòng nếu chưa chép file me.jpg vào
                    }
                },
                {
                    "linh", new PersonProfile
                    {
                        Wish = "Chúc Linh ngày 20/10 luôn rạng rỡ như ánh nắng ban mai, xinh đẹp, tự tin và gặt hái được thật nhiều thành công trên con đường mình chọn nhé!",
                        ImagePath = "Images/linh.jpg"
                    }
                },
                {
                    "trang", new PersonProfile
                    {
                        Wish = "Nhân ngày 20/10, chúc Trang luôn giữ vững nét dịu dàng, thông minh và tràn đầy năng lượng tích cực. Mọi điều tốt đẹp nhất sẽ đến với bạn!",
                        ImagePath = "Images/trang.jpg"
                    }
                },
                {
                    "hương", new PersonProfile
                    {
                        Wish = "Chúc Hương một ngày lễ 20/10 ngập tràn hoa, quà và những lời yêu thương ngọt ngào nhất. Luôn tỏa hương thơm ngát và rạng ngời như chính tên gọi của mình nhé!",
                        ImagePath = "Images/huong.jpg"
                    }
                }
            };

            // 2. Danh sách câu chúc ngẫu nhiên cho những tên khác được nhập vào
            _defaultWishes = new List<string>
            {
                "Nhân ngày Phụ nữ Việt Nam 20/10, chúc {0} luôn xinh đẹp rạng ngời, ngập tràn niềm vui và luôn được yêu thương trọn vẹn mỗi ngày!",
                "Chúc {0} có một ngày 20/10 thật ý nghĩa, hạnh phúc đong đầy, công việc và học tập đều thuận buồm xuôi gió!",
                "Gửi tới {0} ngàn lời chúc tốt đẹp nhất nhân ngày 20/10. Hãy luôn tự tin, tỏa sáng và mỉm cười thật tươi nhé!"
            };

            // 3. Khởi tạo bộ đếm thời gian tạo hiệu ứng cánh hoa rơi
            _petalTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(450)
            };
            _petalTimer.Tick += SpawnFallingPetal;
        }

        // Xử lý nút XÁC NHẬN
        private void BtnConfirm_Click(object sender, RoutedEventArgs e)
        {
            string fullName = txtFullName.Text.Trim();

            if (string.IsNullOrEmpty(fullName))
            {
                txtError.Visibility = Visibility.Visible;
                txtFullName.Focus();
                return;
            }

            txtError.Visibility = Visibility.Hidden;
            DisplayGreetingFor(fullName);

            // Chạy Animation chuyển sang màn hình lời chúc
            if (FindResource("ShowGreetingAnim") is Storyboard sb)
            {
                sb.Begin();
            }

            _petalTimer.Start();
        }

        // Xử lý nút HỦY BỎ (Xóa trắng để nhập lại)
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            txtFullName.Clear();
            txtError.Visibility = Visibility.Hidden;
            txtFullName.Focus();
        }

        // Nhấn Enter trong ô TextBox cũng tự động Xác nhận
        private void TxtFullName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BtnConfirm_Click(sender, e);
            }
        }

        // Xử lý nút QUAY LẠI
        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            _petalTimer.Stop();
            ParticleCanvas.Children.Clear();

            if (FindResource("BackToInputAnim") is Storyboard sb)
            {
                sb.Begin();
            }

            txtFullName.SelectAll();
            txtFullName.Focus();
        }

        // Logic chọn câu chúc và hình nền theo Họ và Tên
        private void DisplayGreetingFor(string fullName)
        {
            txtRecipientName.Text = $"Thân gửi {fullName} 💖";

            // Tách lấy tên chính (từ cuối cùng trong Họ và Tên) để tra cứu dễ hơn
            // Ví dụ nhập "Nguyễn Thùy Linh" -> tách lấy chữ "Linh"
            string[] nameParts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            string firstName = nameParts.Length > 0 ? nameParts[^1] : fullName;

            PersonProfile? matchedProfile = null;

            // Tìm theo cả họ tên hoặc tên gọi
            if (_profiles.TryGetValue(fullName, out var exactMatch))
                matchedProfile = exactMatch;
            else if (_profiles.TryGetValue(firstName, out var firstNameMatch))
                matchedProfile = firstNameMatch;

            if (matchedProfile != null)
            {
                txtWishMessage.Text = matchedProfile.Wish;
                SetBackgroundImage(matchedProfile.ImagePath, fullName);
            }
            else
            {
                // Tạo câu chúc theo tên người dùng vừa nhập
                int index = Math.Abs(fullName.GetHashCode()) % _defaultWishes.Count;
                txtWishMessage.Text = string.Format(_defaultWishes[index], fullName);
                SetBackgroundImage("Images/default.jpg", fullName);
            }
        }

        // Thiết lập hình nền (Ưu tiên ảnh trong thư mục máy -> nếu không có sẽ tạo nền nghệ thuật)
        private void SetBackgroundImage(string relativePath, string personName)
        {
            try
            {
                string fullPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relativePath);
                if (File.Exists(fullPath))
                {
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(fullPath, UriKind.Absolute);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    bgPersonImage.ImageSource = bitmap;
                    return;
                }
            }
            catch
            {
                // Bỏ qua nếu lỗi đọc file và chuyển sang tạo nền tự động bên dưới
            }

            // NẾU CHƯA CÓ FILE ẢNH TRONG THƯ MỤC: Tự động vẽ một hình nền Gradient + Chữ cái tên người đó
            bgPersonImage.ImageSource = GenerateStylizedAvatarBackground(personName);
        }

        // Hàm tạo hình nền nghệ thuật mang tên người nhận (Đảm bảo chạy đẹp ngay lập tức không cần file ngoài)
        private ImageSource GenerateStylizedAvatarBackground(string name)
        {
            DrawingVisual visual = new DrawingVisual();
            using (DrawingContext dc = visual.RenderOpen())
            {
                // Vẽ nền Gradient hồng tím hiện đại
                LinearGradientBrush grad = new LinearGradientBrush(
                    Color.FromRgb(236, 64, 122),
                    Color.FromRgb(142, 36, 170),
                    new Point(0, 0), new Point(1, 1));
                dc.DrawRectangle(grad, null, new Rect(0, 0, 500, 800));

                // Vẽ vòng tròn phát sáng trang trí
                RadialGradientBrush glow = new RadialGradientBrush(
                    Color.FromArgb(110, 255, 255, 255),
                    Color.FromArgb(0, 255, 255, 255));
                dc.DrawEllipse(glow, null, new Point(250, 260), 180, 180);

                // Vẽ khung Avatar tròn ở giữa trên
                SolidColorBrush circleBrush = new SolidColorBrush(Color.FromArgb(50, 255, 255, 255));
                Pen circlePen = new Pen(new SolidColorBrush(Color.FromArgb(140, 255, 255, 255)), 4);
                dc.DrawEllipse(circleBrush, circlePen, new Point(250, 260), 110, 110);

                // Lấy chữ cái đầu của tên để hiển thị
                string[] parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                string initial = parts.Length > 0 ? parts[^1].Substring(0, 1).ToUpper() : "🌸";

                FormattedText text = new FormattedText(
                    initial,
                    System.Globalization.CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    new Typeface(new FontFamily("Segoe UI"), FontStyles.Normal, FontWeights.Bold, FontStretches.Normal),
                    96,
                    Brushes.White,
                    VisualTreeHelper.GetDpi(this).PixelsPerDip);

                dc.DrawText(text, new Point(250 - text.Width / 2, 260 - text.Height / 2));
            }

            RenderTargetBitmap bmp = new RenderTargetBitmap(500, 800, 96, 96, PixelFormats.Pbgra32);
            bmp.Render(visual);
            return bmp;
        }

        // Hiệu ứng Animation: Tạo các cánh hoa rơi mềm mại trên màn hình chúc mừng
        private void SpawnFallingPetal(object? sender, EventArgs e)
        {
            if (GreetingView.Visibility != Visibility.Visible) return;

            double canvasWidth = ParticleCanvas.ActualWidth > 0 ? ParticleCanvas.ActualWidth : 380;
            double canvasHeight = ParticleCanvas.ActualHeight > 0 ? ParticleCanvas.ActualHeight : 680;

            // Tạo hình dáng cánh hoa bằng Ellipse
            Ellipse petal = new Ellipse
            {
                Width = _random.Next(10, 18),
                Height = _random.Next(6, 12),
                Fill = new SolidColorBrush(Color.FromArgb(
                    (byte)_random.Next(140, 220),
                    255,
                    (byte)_random.Next(160, 210),
                    (byte)_random.Next(190, 230))),
                RenderTransformOrigin = new Point(0.5, 0.5)
            };

            TransformGroup group = new TransformGroup();
            RotateTransform rotate = new RotateTransform(_random.Next(0, 360));
            TranslateTransform translate = new TranslateTransform();
            group.Children.Add(rotate);
            group.Children.Add(translate);
            petal.RenderTransform = group;

            double startX = _random.Next(10, (int)canvasWidth - 10);
            Canvas.SetLeft(petal, startX);
            Canvas.SetTop(petal, -20);
            ParticleCanvas.Children.Add(petal);

            Duration duration = new Duration(TimeSpan.FromSeconds(_random.Next(4, 7)));

            // Rơi từ trên xuống
            DoubleAnimation fallAnim = new DoubleAnimation(-20, canvasHeight + 20, duration);
            // Lắc lư nhẹ sang ngang
            DoubleAnimation swayAnim = new DoubleAnimation(startX, startX + _random.Next(-60, 60), duration);
            // Xoay tròn cánh hoa
            DoubleAnimation spinAnim = new DoubleAnimation(0, 360, duration);

            fallAnim.Completed += (s, args) => ParticleCanvas.Children.Remove(petal);

            petal.BeginAnimation(Canvas.TopProperty, fallAnim);
            petal.BeginAnimation(Canvas.LeftProperty, swayAnim);
            rotate.BeginAnimation(RotateTransform.AngleProperty, spinAnim);
        }
    }
}
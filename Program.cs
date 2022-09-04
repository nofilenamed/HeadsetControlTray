using HeadsetControlTray.Properties;

using System.Diagnostics;

using Timer = System.Windows.Forms.Timer;

namespace HeadsetControlTray
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            Application.SetCompatibleTextRenderingDefault(false);

            using (TrayIcon pi = new TrayIcon())
            {

                pi.Display();
                Application.Run();
            }
        }
    }


    class TrayIcon : IDisposable
    {
        Bitmap m_Bitmap = Resources.headset;

        NotifyIcon m_NotifyIcon;

        public TrayIcon()
        {
            m_NotifyIcon = new NotifyIcon();
        }

        const string k_TrayName = "Headset";

        public void SetIcon(int status)
        {
            int width = m_Bitmap.Width;
            int height = m_Bitmap.Height;

            Debug.WriteLine(status);

            Color color = HandleStatus(status);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color p = m_Bitmap.GetPixel(x, y);
                    if (p.A == 255)
                    {
                        m_Bitmap.SetPixel(x, y, color);
                    }
                }
            }

            IntPtr pIcon = m_Bitmap.GetHicon();
            using (var icon = Icon.FromHandle(pIcon))
            {
                m_NotifyIcon.Icon = icon;
            }
        }

        public Color HandleStatus(int value)
        {
            Color color = Color.White;
            string result = "";
            if (value == -100)
            {
                result = "Offline";
                color = Color.LightGray;
            }
            else if (value == 0)
            {
            }
            else if (value == -1)
            {
                result = "Charging";
                color = Color.DeepSkyBlue;
            }
            else if (value <= 15)
            {
                result = "Crit: " + value + '%';
                color = Color.Red;
            }
            else if (value <= 25)
            {
                result = "Low: " + value + '%';
                color = Color.Yellow;
            }
            else if (value <= 100)
            {
                result = "Good: " + value + '%';
                color = Color.Green;
            }


            m_NotifyIcon.Text = k_TrayName + '\n' + result;
            return color;
        }


        public int Ask(string args, out int status)
        {

            ProcessStartInfo psi = new ProcessStartInfo()
            {
                FileName = AppDomain.CurrentDomain.BaseDirectory + "/headsetcontrol.exe",
                Arguments = args,
                CreateNoWindow = true,
                ErrorDialog = false,

                RedirectStandardError = false,
                RedirectStandardInput = false,
                RedirectStandardOutput = true,

                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Hidden
            };


            Process process = new Process();
            process.StartInfo = psi;
            process.Start();

            string output = process.StandardOutput.ReadToEnd();

            process.WaitForExit();

            int exit = process.ExitCode;

            if (exit == 0)
            {

                if (int.TryParse(output, out status))
                {

                    return exit;

                }
            }

            return status = exit;

        }

        public void Display()
        {
            m_NotifyIcon.MouseClick += new MouseEventHandler(onMouseClick);

            SetIcon(0);

            m_NotifyIcon.Text = k_TrayName;
            m_NotifyIcon.Visible = true;
            m_NotifyIcon.ContextMenuStrip = new ContextMenus().Create();

            Timer timer = new();
            timer.Interval = 5 * 1000;
            timer.Tick += OnTimer;
            timer.Start();

            OnTimer(null, null);
        }

        private void OnTimer(object? sender, EventArgs e)
        {
            var exit = Ask("-bc", out int status);

            SetIcon(status);
        }

        public void Dispose()
        {
            m_NotifyIcon.Dispose();
        }

        void onMouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
            }
        }
    }

    class ContextMenus
    {
        public ContextMenuStrip Create()
        {
            ContextMenuStrip menu = new ContextMenuStrip();

            ToolStripMenuItem item = new ToolStripMenuItem();
            item.Text = "Exit";
            item.Click += new System.EventHandler(Exit_Click);
            menu.Items.Add(item);

            return menu;
        }

        void Exit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
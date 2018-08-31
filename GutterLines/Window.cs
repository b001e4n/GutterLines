using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using Newtonsoft.Json;

namespace GutterLines
{
    public partial class Window : Form
    {
        private MemRead memRead;
        private FileRead fileRead;
        private int lat;
        private int lon;
        private const int gridScale = 4;
        private const int lineOffset = gridScale / 2;
        private const int gridMax = gridScale * 40;
        private MenuItem alertToggle;
        private bool flashAlert;
        private Settings _settings = null;

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool ReleaseCapture();

        private void SaveSettings(Settings settings)
        {   
            File.WriteAllText("settings.ini", JsonConvert.SerializeObject(_settings, Formatting.Indented));
        }
        private Settings LoadSettings()
        {
            return JsonConvert.DeserializeObject<Settings>(File.ReadAllText("settings.ini"));
        }

        [STAThread]
        static void Main()
        {
            Application.Run(new Window());           
        }

        public Window()
        {
            

            try
            {
                if (File.Exists("settings.ini"))
                {
                    _settings = LoadSettings();
                }
                else
                {
                    _settings = new Settings()
                    {
                        CoordsAccessMod = CoordsAccessMods.File,
                        ChatLogConfigs = new ChatLogConfig[] 
                        {
                            new ChatLogConfig()
                            {
                                Name = "Default",
                                Path = @"C:\Games\Ragnarok Online\Chat",
                                FileNamePattern = @"Chat_General*",
                                CoordsPattern = @"[^\:]* \((?<city>[0-9a-zA-Z_]+)\) : (?<lat>\d{1,3})\, (?<lon>\d{1,3})\Z",
                                RemoveFilesAfterRead = true
                            },
                            new ChatLogConfig()
                            {
                                Name = "ruRO",
                                Path = @"C:\Games\Ragnarok Online\Chat",
                                FileNamePattern = @"Chat_Общий чат*",
                                CoordsPattern = @"[^\:]* \((?<city>[0-9a-zA-Z_]+)\) : (?<lat>\d{1,3})\, (?<lon>\d{1,3})\Z",
                                RemoveFilesAfterRead = true
                            }
                        },
                        CurrentChatLogConfigIndex = 1
                    };
                    SaveSettings(_settings);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), "Load Settings Error");
                Application.Exit();
            }

            InitializeComponent();
            if (_settings.CoordsAccessMod == CoordsAccessMods.File) cbChangeMode.Checked = true;
            else cbChangeMode.Checked = false;
            flashAlert = GetAlertToggleBool();
            CreateContextMenu();
            StartPosition = FormStartPosition.Manual;
            SetWindowPos();
            BackColor = Color.Pink;
            TransparencyKey = Color.Pink;
            memRead = new MemRead();
            fileRead = new FileRead();
            memRead.GetProcess();
            var Timer = new Timer()
            {
                Interval = (250)
            };
            Timer.Tick += new EventHandler(UpdateWindow);
            Timer.Start();
        }

        private void CreateContextMenu()
        {
            var notifyIcon = new NotifyIcon(components = new Container())
            {
                Icon = Properties.Resources.icon,
                ContextMenu = new ContextMenu(new[]
                {
                    new MenuItem("Reset Window Position", ResetWindowPos),
                    alertToggle = new MenuItem($"Show When in Gutter (Current:{flashAlert})", ToggleAlert),
                    new MenuItem("-"),
                    new MenuItem("Exit GutterLines", ExitBtn_Click)
                }),
                Text = "GutterLines",
                Visible = true
            };
        }

        private void ToggleAlert(object sender, EventArgs e)
        {
            flashAlert = !flashAlert;
            using (var key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\ByTribe\GutterLines"))
            {
                try
                {
                    key?.SetValue("alertToggle", flashAlert);
                }
                catch {/* If we cant write to the registry do nothing*/}
            }
            alertToggle.Text = $"Show When in Gutter (Current:{flashAlert})";
        }

        private bool GetAlertToggleBool()
        {
            using (var key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\ByTribe\GutterLines"))
            {
                try
                {
                    return Convert.ToBoolean(key?.GetValue("alertToggle"));
                }
                catch { return false; }
            }
        }

        private void UpdateWindow(object sender, EventArgs e)
        {
            GameInfo gi = null;
            if (_settings.CoordsAccessMod == CoordsAccessMods.Memory)
            {
                gi = memRead.GetValues();
            }
            else
            {
                if (_settings.ChatLogConfigs.Length > _settings.CurrentChatLogConfigIndex)
                    gi = fileRead.GetValues(_settings.ChatLogConfigs[_settings.CurrentChatLogConfigIndex]);
            }
             
            if (gi != null)
            {
                LatLonLbl.Text = $"{gi.Name} @ {gi.Lat},{gi.Lon}";
                if (lat != gi.Lat || lon != gi.Lon)
                {
                    lat = gi.Lat;
                    lon = gi.Lon;
                    DrawGutters(gi.Lat, gi.Lon, GutterAlert());
                }
            }
            else
            {
                LatLonLbl.Text = "Unable to find data";
            }
        }

        private Color GutterAlert()
        {
            if (flashAlert)
            {
                //check both lat and lon to see if the player is in a gutter line, if they are make the backgroud color non white
                if (lat % 40 <= 4 || lon % 40 <= 4)
                {
                    return Color.Orange;
                }
            }
            return Color.White;
        }

        private void DrawGutters(int playerX, int playerY, Color backCol)
        {
            Graphics g = gridMap.CreateGraphics();
            g.Clear(backCol);
            for (int i = 4; i >= 0; i--)
            {
                var gutterPosX = GetGutterLinePos(playerX, i) * gridScale;
                var gutterPosY = GetGutterLinePos(playerY, i) * gridScale;
                Pen pen = new Pen(i == 0 ? Brushes.Red : Brushes.Blue, gridScale);
                g.DrawLine(pen, new Point(gutterPosX, 0), new Point(gutterPosX, gridMax));
                //RO's 0,0 is bottom left, Winforms Picture box's 0,0 is top left. So we flip our Y
                g.DrawLine(pen, new Point(0, gridMax - gutterPosY), new Point(gridMax, gridMax - gutterPosY));
            }
            //draw players dot
            g.FillRectangle(Brushes.Black, 20 * gridScale - lineOffset, 20 * gridScale - lineOffset, gridScale, gridScale);
        }

        private int GetGutterLinePos(int playerAxisPos, int mod)
        {
            int Gutter = playerAxisPos + (40 - (playerAxisPos % 40));
            if (Gutter + mod > playerAxisPos + 20)
            {
                Gutter = playerAxisPos - (playerAxisPos % 40);
            }
            return Gutter - playerAxisPos + 20 + mod;
        }



        private void SetWindowPos()
        {
            using (var key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\ByTribe\GutterLines"))
            {
                try
                {
                    Location = new Point((int)key?.GetValue("winX"), (int)key?.GetValue("winY"));
                }
                catch (Exception)
                {
                    Location = new Point(10, 10);
                }
            }
        }

        private void ResetWindowPos(object sender, EventArgs e)
        {
            SaveWindowPos(10, 10);
            SetWindowPos();
        }

        private void SaveWindowPos(int winX, int winY)
        {
            using (var key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\ByTribe\GutterLines"))
            {
                try
                {
                    key?.SetValue("winX", winX);
                    key?.SetValue("winY", winY);
                }
                catch {/* If we cant write to the registry do nothing*/}
            }
        }

        #region form controls
        private void Window_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, 0xA1, 0x2, 0);
            }
        }
        private void ExitBtn_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
        private void NextClientBtn_Click(object sender, EventArgs e)
        {
            if(_settings.CoordsAccessMod == CoordsAccessMods.Memory)
            {
                memRead.GetProcess();
            }
            else
            {
                if (_settings.ChatLogConfigs.Length > 0)
                    _settings.CurrentChatLogConfigIndex = (_settings.CurrentChatLogConfigIndex + 1) % _settings.ChatLogConfigs.Length;
            }
        }
        private void Window_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveWindowPos(Location.X, Location.Y);
        }

        #endregion

        private void cbChangeMode_CheckedChanged(object sender, EventArgs e)
        {
            if((sender as CheckBox).Checked)
            {
                _settings.CoordsAccessMod = CoordsAccessMods.File;
            }
            else
            {
                _settings.CoordsAccessMod = CoordsAccessMods.Memory;
            }
        }
    }
}


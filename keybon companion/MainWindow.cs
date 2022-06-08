using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;

namespace keybon
{

    public partial class MainWindow : Form
    {
        String portName = "COM14";
        public SerialPort _serialPort;
        static int numLayout = 16;
        public ScreenLayout[] Layouts = new ScreenLayout[numLayout];
        public bool[] checkedBox2 = new bool[numLayout];

        int selectedButton = 0;
        int currentLayout = 0;
        String currentApp;
        string[] ports;

        public int CurrentLayout
        {
            get { return currentLayout; }
            set // respond to change of currentLayout
            {
                if (!value.Equals(currentLayout))
                {
                    currentLayout = value;
                    if (currentLayout == 0)
                    {
                        pictureBox01.Visible = false;
                        pictureBox02.Visible = false;
                        pictureBox03.Visible = false;
                        pictureBox04.Visible = false;
                        pictureBox05.Visible = false;
                        pictureBox06.Visible = false;
                        pictureBox07.Visible = false;
                        pictureBox08.Visible = false;
                        pictureBox09.Visible = false;
                        AddBox.Enabled = false;
                        RemoveBox.Enabled = false;
                        hotkeyBox.Enabled = false;
                        listBox1.Enabled = false;
                        checkBox2.Enabled = false;
                    }
                    else
                    {
                        pictureBox01.Visible = true;
                        pictureBox02.Visible = true;
                        pictureBox03.Visible = true;
                        pictureBox04.Visible = true;
                        pictureBox05.Visible = true;
                        pictureBox06.Visible = true;
                        pictureBox07.Visible = true;
                        pictureBox08.Visible = true;
                        pictureBox09.Visible = true;
                        AddBox.Enabled = true;
                        RemoveBox.Enabled = true;
                        hotkeyBox.Enabled = true;
                        listBox1.Enabled = true;
                        checkBox2.Enabled = true;
                    }
                }
                else
                {
                    currentLayout = value;
                }
            }
        }

        public String CurrentApp
        {
            get { return currentApp; }
            set // respond to change of currentApp
            {
                if (!value.Equals(currentApp) && !value.Equals("Keybon Companion"))
                {
                    currentApp = value;
                    int nextLayout = 0;
                    for (int i = 0; i < Layouts.Length; i++)
                    {
                        if (Layouts[i].Apps.Contains(currentApp))
                        {
                            nextLayout = i;
                            break;
                        }
                    }
                    switchToLayout(nextLayout);
                }
            }
        }

        public void addAppSelection(String newApp) // for passing data from AppSelect
        {
            Layouts[CurrentLayout].Apps.Add(newApp);
            listBox1.DataSource = null;
            listBox1.DataSource = Layouts[CurrentLayout].Apps;
        }

        public MainWindow()
        {
            InitializeComponent();
            pictureBox01.AllowDrop = true;
            pictureBox02.AllowDrop = true;
            pictureBox03.AllowDrop = true;
            pictureBox04.AllowDrop = true;
            pictureBox05.AllowDrop = true;
            pictureBox06.AllowDrop = true;
            pictureBox07.AllowDrop = true;
            pictureBox08.AllowDrop = true;
            pictureBox09.AllowDrop = true;


            for (int i = 0; i < Layouts.Length; i++)
            {
                if (i <= numLayout/2)
                {
                    Layouts[i] = new ScreenLayout();
                    Layouts[i].name = $"Layout {i}";
                }
                else 
                {
                    Layouts[i] = new ScreenLayout();
                    Layouts[i].name = $"Extension Layout {i}";
                }
                
            }

            Layouts[0].name = "Default";

            comboBox1.DataSource = Layouts;
            comboBox1.DisplayMember = "Name";
            TSComboBox1.ComboBox.BindingContext = comboBox1.BindingContext;
            TSComboBox1.ComboBox.DataSource = comboBox1.DataSource;
            TSComboBox1.ComboBox.DisplayMember = comboBox1.DisplayMember;

            loadSettings();

            // Serial init
            _serialPort = new SerialPort(portName, 115200, Parity.None, 8, StopBits.One);
            _serialPort.DtrEnable = true;
            ports = SerialPort.GetPortNames();
            comboBox2.DataSource = ports;
            if (ports.Contains(portName))
            {
                comboBox2.SelectedItem = portName;
                try
                {
                    _serialPort.Open();
                }
                catch { }
            }
            _serialPort.DataReceived += portDataReceived;

            Timer timer1 = new Timer { Interval = 250 };
            timer1.Enabled = true;
            timer1.Tick += new System.EventHandler(OnTimerEvent);
        }

        private void portDataReceived(object sender, EventArgs args)
        {

            SerialPort port = sender as SerialPort;

            if (port == null)
            {
                return;
            }
            int keyReceived = _serialPort.ReadChar();
            Console.WriteLine(keyReceived);
            // https://docs.microsoft.com/en-us/dotnet/api/system.windows.forms.sendkeys.send

            bool btn_pressed = false;
            try
            {
                if (keyReceived == '1' & btn_pressed == false & checkBox2.Checked == true)
                {
                    switchToLayout(11);
                    btn_pressed = true;
                }
                else if (keyReceived == '1' & btn_pressed == true & checkBox2.Checked == true)
                {
                    switchToLayout(1);
                    btn_pressed = false;
                }
                else
                {
                    SendKeys.SendWait(Layouts[currentLayout].keyCommand[keyReceived - 49]);
                }

            }
            catch { }

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            notifyIcon1.BalloonTipTitle = "Keybon";
            notifyIcon1.BalloonTipText = "App has been minimized to tray and will be run in background mode";
            notifyIcon1.Text = "Keybon Companion";
            notifyIcon1.ShowBalloonTip(1000);
        }

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll")]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);


        private void OnTimerEvent(object sender, EventArgs e)
        {
            const int nChars = 256;
            StringBuilder Buff = new StringBuilder(nChars);
            IntPtr handle = GetForegroundWindow();
            GetWindowText(handle, Buff, nChars);
            Process[] AllProcess = Process.GetProcesses();

            foreach (Process pro in AllProcess)
            {
                String mainWindowTitle = pro.MainWindowTitle;
                if (mainWindowTitle != "" && Buff.ToString().Equals(mainWindowTitle))
                {
                    // Console.WriteLine(pro.ProcessName.ToString());
                    CurrentApp = pro.ProcessName.ToString();
                    break;
                }
            }

        }

        private void switchToLayout(int layoutNum)
        {
            CurrentLayout = layoutNum;
            pictureBox01.Image = Layouts[currentLayout].oleds[0];
            pictureBox02.Image = Layouts[currentLayout].oleds[1];
            pictureBox03.Image = Layouts[currentLayout].oleds[2];
            pictureBox04.Image = Layouts[currentLayout].oleds[3];
            pictureBox05.Image = Layouts[currentLayout].oleds[4];
            pictureBox06.Image = Layouts[currentLayout].oleds[5];
            pictureBox07.Image = Layouts[currentLayout].oleds[6];
            pictureBox08.Image = Layouts[currentLayout].oleds[7];
            pictureBox09.Image = Layouts[currentLayout].oleds[8];

            if (layoutNum == 0)
            {
                byte[] command = { (Byte)'D' };
                checkBox2.Checked = false;
                try
                {
                    if (_serialPort != null)
                        _serialPort.Write(command, 0, command.Length);
                }
                catch { }
            }
            else
            {
                for (int i = 0; i <= checkedBox2.Length; i++)
                {
                    checkBox2.Checked = checkedBox2[CurrentLayout];
                }

                Layouts[currentLayout].drawAll(_serialPort);
            }

            listBox1.DataSource = Layouts[currentLayout].Apps;
            comboBox1.SelectedIndex = layoutNum;
            hotkeyBox.Text = Layouts[currentLayout].keyCommand[selectedButton];
            //TSComboBox1.ComboBox.SelectedIndex = comboBox1.SelectedIndex;
        }

        private void pictureBox_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.All;
            else
            {
                String[] strGetFormats = e.Data.GetFormats();
                e.Effect = DragDropEffects.None;
            }
        }

        private void pictureBox_DragDrop(object sender, DragEventArgs e)
        {
            var eventSource = (sender as PictureBox);
            string[] FileList = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            Bitmap image = new Bitmap(FileList[0]);
            image = ResizeBitmap(image, 64, 48);
            eventSource.Image = image;
            int keyNum = Int16.Parse(eventSource.Name.Substring(10));
            Layouts[currentLayout].oleds[keyNum - 1] = image;
            Layouts[currentLayout].drawAll(_serialPort);
        }

        private void listBox1_SelectedValueChanged(object sender, EventArgs e)
        {
            var eventSource = (sender as ListBox);
        }

        private void pictureBox_Click(object sender, EventArgs e)
        {
            var eventSource = (sender as PictureBox);
            pictureBox01.BorderStyle = BorderStyle.None;
            pictureBox02.BorderStyle = BorderStyle.None;
            pictureBox03.BorderStyle = BorderStyle.None;
            pictureBox04.BorderStyle = BorderStyle.None;
            pictureBox05.BorderStyle = BorderStyle.None;
            pictureBox06.BorderStyle = BorderStyle.None;
            pictureBox07.BorderStyle = BorderStyle.None;
            pictureBox08.BorderStyle = BorderStyle.None;
            pictureBox09.BorderStyle = BorderStyle.None;
            eventSource.BorderStyle = BorderStyle.Fixed3D;
            selectedButton = Int16.Parse(eventSource.Name.Substring(10)) - 1;
            hotkeyBox.Text = Layouts[currentLayout].keyCommand[selectedButton];
        }

        private void label4_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://docs.yandex.ru/docs/view?url=ya-disk-public%3A%2F%2FFKoS0OW6y3uOfBVJLJCwEmJdpspyhnnbHzfVIcrUIvthrvkZhWMFdk%2FWw71yCqzAq%2FJ6bpmRyOJonT3VoXnDag%3D%3D&name=Key.pdf&nosw=1");
        }

        private void label1_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://en.wikipedia.org/wiki/COM_(hardware_interface)");
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            var eventSource = (sender as ComboBox);
            switchToLayout(eventSource.SelectedIndex);
        }

        private void MainWindow_FormClosed(object sender, FormClosedEventArgs e) // Switch to Default Layout when Exiting
        {
            byte[] command = { (Byte)'D' };
            try
            {
                _serialPort.Write(command, 0, command.Length);
            }
            catch { }

            saveSettings();

        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e) // Change Brightness
        {
            var eventSource = (sender as CheckBox);
            if (eventSource.Checked)
            {
                byte[] command = { (Byte)'b' };
                _serialPort.Write(command, 0, command.Length);
            }
            else
            {
                byte[] command = { (Byte)'B' };
                _serialPort.Write(command, 0, command.Length);
            }

        }

        private void hotkeyBox_TextChanged(object sender, EventArgs e)
        {
            Layouts[currentLayout].keyCommand[selectedButton] = hotkeyBox.Text;
        }

        private void AddBox_Click(object sender, EventArgs e)
        {
            AppSelect f2 = new AppSelect(this);
            f2.ShowDialog();
        }

        private void RemoveBox_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem != null)
                Layouts[currentLayout].Apps.Remove(listBox1.SelectedItem.ToString());
            listBox1.DataSource = null;
            listBox1.DataSource = Layouts[currentLayout].Apps;
        }

        private void comboBox2_DropDown(object sender, EventArgs e)
        {
            ports = SerialPort.GetPortNames();
            comboBox2.DataSource = ports;
        }

        private void comboBox2_SelectionChangeCommitted(object sender, EventArgs e)
        {
            _serialPort.Close();
            _serialPort.PortName = comboBox2.SelectedItem.ToString();
            try
            {
                _serialPort.Open();
            }
            catch { }
        }

        private void saveSettings()
        {
            Properties.Settings.Default.portName = portName;
            Properties.Settings.Default.Layout1 = Layouts[1];
            Properties.Settings.Default.Layout2 = Layouts[2];
            Properties.Settings.Default.Layout3 = Layouts[3];
            Properties.Settings.Default.Layout4 = Layouts[4];
            Properties.Settings.Default.Layout5 = Layouts[5];
            Properties.Settings.Default.Layout6 = Layouts[6];
            Properties.Settings.Default.Layout7 = Layouts[7];
            Properties.Settings.Default.Layout8 = Layouts[8];
            Properties.Settings.Default.Layout9 = Layouts[9];
            Properties.Settings.Default.Layout10 = Layouts[10];
            Properties.Settings.Default.Layout11 = Layouts[11];
            Properties.Settings.Default.Layout12 = Layouts[12];
            Properties.Settings.Default.Layout13 = Layouts[13];
            Properties.Settings.Default.Layout14 = Layouts[14];
            Properties.Settings.Default.Layout15 = Layouts[15];

            Properties.Settings.Default.checkedBox1 = checkedBox2[1];
            Properties.Settings.Default.checkedBox2 = checkedBox2[2];
            Properties.Settings.Default.checkedBox3 = checkedBox2[3];
            Properties.Settings.Default.checkedBox4 = checkedBox2[4];
            Properties.Settings.Default.checkedBox5 = checkedBox2[5];
            Properties.Settings.Default.checkedBox6 = checkedBox2[6];
            Properties.Settings.Default.checkedBox7 = checkedBox2[7];
            Properties.Settings.Default.checkedBox8 = checkedBox2[8];
            Properties.Settings.Default.checkedBox9 = checkedBox2[9];
            Properties.Settings.Default.checkedBox10 = checkedBox2[10];
            Properties.Settings.Default.checkedBox11 = checkedBox2[11];
            Properties.Settings.Default.checkedBox12 = checkedBox2[12];
            Properties.Settings.Default.checkedBox13 = checkedBox2[13];
            Properties.Settings.Default.checkedBox14 = checkedBox2[14];
            Properties.Settings.Default.checkedBox15 = checkedBox2[15];

            Properties.Settings.Default.Save();
        }

        private void loadSettings()
        {
            Properties.Settings.Default.Reload();
            if (Properties.Settings.Default.portName != "") portName = Properties.Settings.Default.portName;
            if (Properties.Settings.Default.Layout1 != null) Layouts[1] = Properties.Settings.Default.Layout1;
            if (Properties.Settings.Default.Layout2 != null) Layouts[2] = Properties.Settings.Default.Layout2;
            if (Properties.Settings.Default.Layout3 != null) Layouts[3] = Properties.Settings.Default.Layout3;
            if (Properties.Settings.Default.Layout4 != null) Layouts[4] = Properties.Settings.Default.Layout4;
            if (Properties.Settings.Default.Layout5 != null) Layouts[5] = Properties.Settings.Default.Layout5;
            if (Properties.Settings.Default.Layout6 != null) Layouts[6] = Properties.Settings.Default.Layout6;
            if (Properties.Settings.Default.Layout7 != null) Layouts[7] = Properties.Settings.Default.Layout7;
            if (Properties.Settings.Default.Layout8 != null) Layouts[8] = Properties.Settings.Default.Layout8;
            if (Properties.Settings.Default.Layout9 != null) Layouts[9] = Properties.Settings.Default.Layout9;
            if (Properties.Settings.Default.Layout10 != null) Layouts[10] = Properties.Settings.Default.Layout10;
            if (Properties.Settings.Default.Layout11 != null) Layouts[11] = Properties.Settings.Default.Layout11;
            if (Properties.Settings.Default.Layout12 != null) Layouts[12] = Properties.Settings.Default.Layout12;
            if (Properties.Settings.Default.Layout13 != null) Layouts[13] = Properties.Settings.Default.Layout13;
            if (Properties.Settings.Default.Layout14 != null) Layouts[14] = Properties.Settings.Default.Layout14;
            if (Properties.Settings.Default.Layout15 != null) Layouts[15] = Properties.Settings.Default.Layout15;

            if (Properties.Settings.Default.checkedBox1 != false) checkedBox2[1] = Properties.Settings.Default.checkedBox1;
            if (Properties.Settings.Default.checkedBox2 != false) checkedBox2[2] = Properties.Settings.Default.checkedBox2;
            if (Properties.Settings.Default.checkedBox3 != false) checkedBox2[3] = Properties.Settings.Default.checkedBox3;
            if (Properties.Settings.Default.checkedBox4 != false) checkedBox2[4] = Properties.Settings.Default.checkedBox4;
            if (Properties.Settings.Default.checkedBox5 != false) checkedBox2[5] = Properties.Settings.Default.checkedBox5;
            if (Properties.Settings.Default.checkedBox6 != false) checkedBox2[6] = Properties.Settings.Default.checkedBox6;
            if (Properties.Settings.Default.checkedBox7 != false) checkedBox2[7] = Properties.Settings.Default.checkedBox7;
            if (Properties.Settings.Default.checkedBox8 != false) checkedBox2[8] = Properties.Settings.Default.checkedBox8;
            if (Properties.Settings.Default.checkedBox9 != false) checkedBox2[9] = Properties.Settings.Default.checkedBox9;
            if (Properties.Settings.Default.checkedBox10 != false) checkedBox2[10] = Properties.Settings.Default.checkedBox10;
            if (Properties.Settings.Default.checkedBox11 != false) checkedBox2[11] = Properties.Settings.Default.checkedBox11;
            if (Properties.Settings.Default.checkedBox12 != false) checkedBox2[12] = Properties.Settings.Default.checkedBox12;
            if (Properties.Settings.Default.checkedBox13 != false) checkedBox2[13] = Properties.Settings.Default.checkedBox13;
            if (Properties.Settings.Default.checkedBox14 != false) checkedBox2[14] = Properties.Settings.Default.checkedBox14;
            if (Properties.Settings.Default.checkedBox15 != false) checkedBox2[15] = Properties.Settings.Default.checkedBox15;
        }

        public Bitmap ResizeBitmap(Bitmap bmp, int width, int height)
        {
            Bitmap result = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(result))
            {
                g.DrawImage(bmp, 0, 0, width, height);
            }

            return result;
        }

        private void MainWindow_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                this.Hide();
                notifyIcon1.Visible = true;
                notifyIcon1.ShowBalloonTip(1000);
            }
            else if (FormWindowState.Normal == this.WindowState)
            { notifyIcon1.Visible = false; }
        }

        private void notifyIcon1_MouseDoubleClick_1(object sender, MouseEventArgs e)
        {
            this.Show();
            notifyIcon1.Visible = false;
            this.WindowState = FormWindowState.Normal;
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://www.youtube.com/watch?v=dQw4w9WgXcQ");
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            About f3 = new About(this);
            if (Application.OpenForms["About"] == null)
            f3.ShowDialog();
        }

        private void switchToDefaultToolStripMenuItem_Click(object sender, EventArgs e)
        {
            switchToLayout(0);
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            checkedBox2[currentLayout] = checkBox2.Checked;
        }
    }

    //---------------------------------------------------------------------------------------------------------------
    [SettingsSerializeAs(SettingsSerializeAs.Xml)]
    public class ScreenLayout
    {
        [XmlIgnore]
        public Bitmap[] oleds = { new Bitmap(64, 48),
                                  new Bitmap(64, 48),
                                  new Bitmap(64, 48),
                                  new Bitmap(64, 48),
                                  new Bitmap(64, 48),
                                  new Bitmap(64, 48),
                                  new Bitmap(64, 48),
                                  new Bitmap(64, 48),
                                  new Bitmap(64, 48)
        };

        [XmlElement("oleds")]
        public byte[][] oledsSerialized
        {
            get
            { // serialize
                if (oleds[0] == null)
                    return null;
                byte[][] _byte = new byte[9][];
                for (int i = 0; i < oleds.Length; i++)
                {
                    MemoryStream ms = new MemoryStream();
                    oleds[i].Save(ms, ImageFormat.Bmp);
                    _byte[i] = ms.ToArray();
                }
                return _byte;
            }
            set
            { // deserialize
                for (int i = 0; i < oleds.Length; i++)
                {
                    MemoryStream ms = new MemoryStream(value[i]);
                    oleds[i] = new Bitmap(ms);
                }
            }
        }

        public String[] keyCommand = new string[9];

        public List<string> Apps = new List<string>();


        public String name { get; set; }
        public ScreenLayout()
        {
            keyCommand[0] = "{1}";
            keyCommand[1] = "{2}";
            keyCommand[2] = "{3}";
            keyCommand[3] = "{4}";
            keyCommand[4] = "{5}";
            keyCommand[5] = "{6}";
            keyCommand[6] = "{7}";
            keyCommand[7] = "{8}";
            keyCommand[8] = "{9}";
        }
        public void drawAll(SerialPort _serialPort)
        {
            for (int i = 0; i < oleds.Length; i++)
                writeImageToSerial(_serialPort, i + 1, oleds[i]);
        }

        private void writeImageToSerial(SerialPort _serialPort, int keyNum, Bitmap bmp)
        {
            // Straight from https://docs.microsoft.com/de-de/dotnet/api/system.drawing.imaging.bitmapdata?redirectedfrom=MSDN&view=dotnet-plat-ext-5.0
            // Lock the bitmap's bits.  
            Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            System.Drawing.Imaging.BitmapData bmpData = bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite, bmp.PixelFormat);

            // Get the address of the first line.
            IntPtr ptr = bmpData.Scan0;

            // Declare an array to hold the bytes of the bitmap.
            int bytesb = Math.Abs(bmpData.Stride) * bmp.Height;
            byte[] rgbValues = new byte[bytesb];
            System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, bytesb);

            // Unlock the bits.
            bmp.UnlockBits(bmpData);

            BitArray bits = new BitArray(bmp.Width * bmp.Height);
            for (int i = 0; i < rgbValues.Length; i += 4)
            {
                double brightness = (rgbValues[i] + rgbValues[i + 1] + rgbValues[i + 2]) / 3;
                bits[i / 4] = (brightness >= 128);
            }

            var bytes = new byte[(bits.Length - 1) / 8 + 1];
            bits.CopyTo(bytes, 0);

            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = Reverse(bytes[i]);
            }

            byte commandbyte = (byte)(47 + keyNum); // starting with ASCII 0
            try
            {
                byte[] command = { commandbyte };
                _serialPort.Write(command, 0, command.Length);
                _serialPort.Write(bytes, 0, bytes.Length);
            }
            catch (Exception)
            {
                Console.WriteLine("TX problem");
            }
        }

        // Reverses bits in a byte (https://softwarejuancarlos.com/2013/05/05/byte_bits_reverse/)
        public static byte Reverse(byte inByte)
        {
            byte result = 0x00;

            for (byte mask = 0x80; Convert.ToInt32(mask) > 0; mask >>= 1)
            {
                // shift right current result
                result = (byte)(result >> 1);

                // tempbyte = 1 if there is a 1 in the current position
                var tempbyte = (byte)(inByte & mask);
                if (tempbyte != 0x00)
                {
                    // Insert a 1 in the left
                    result = (byte)(result | 0x80);
                }
            }

            return (result);
        }

    }

}

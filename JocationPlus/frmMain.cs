using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Device.Location;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using iMobileDevice;
using iMobileDevice.iDevice;
using iMobileDevice.Lockdown;
using iMobileDevice.Service;
using TestSqlite.sq;
using Microsoft.VisualBasic;

namespace LocationCleaned
{
    public partial class frmMain : Form
    {
        frmMap map = new frmMap();
        LocationService service;
        double speed = 10.5;
        double coordDiff = (10.5 * 1000 / 3600) / 111290.9197534;
        //public SqLiteHelper locationDB = new SqLiteHelper("locationDB.db");
        bool keepMoving = false;
        bool stopGPX = true;

        Location lastLocation = new Location();

        public frmMain()
        {
            CreateLocationDB();
            InitializeComponent();
            ReadLocationFromDB();
            PrintMessage("https://github.com/DoubleO31/JocationPlus");
            PrintMessage("开源软件，请勿用作非法用途^_^");
        }
        private void frmMain_Load(object sender, EventArgs e)
        {
            NativeLibraries.Load();
            service = LocationService.GetInstance();
            service.PrintMessageEvent = PrintMessage;
            service.ListeningDevice();
        }

        private void CreateLocationDB()
        {
            //SqLiteHelper locationDB = new SqLiteHelper("locationDB.db");
            try
            {
                //创建名为table1的数据表
                map.locationDB.CreateTable("location", new string[] { "NAME", "POSITION" }, new string[] { "TEXT primary key", "TEXT" });
                //locationDB.CloseConnection();
            }
            catch (Exception ex)
            {
                //locationDB.CloseConnection();
                MessageBox.Show(ex.Message);
            }
            finally
            {
                //locationDB.CloseConnection();
            }
        }

        private void InsertLocation(string name, string position)
        {
            //SqLiteHelper locationDB = new SqLiteHelper("locationDB.db");
            try
            {
                //创建名为table1的数据表
                //locationDB.CreateTable("location", new string[] { "NAME", "POSITION" }, new string[] { "TEXT primary key", "TEXT" });
                map.locationDB.InsertValues("location", new string[] { name, position });
                //locationDB.CloseConnection();
            }
            catch (Exception ex)
            {
                //locationDB.CloseConnection();
                MessageBox.Show(ex.Message);
            }
            finally
            {
                //locationDB.CloseConnection();
            }
        }

        private void ReadLocationFromDB()
        {
            txtLocation.Items.Clear();
            //SqLiteHelper locationDB = new SqLiteHelper("locationDB.db");
            SQLiteDataReader reader = map.locationDB.ReadFullTable("location");
            try
            {
                //连接数据库
                //locationDB = new SqLiteHelper("locationDB.db");
                //读取整张表
                //SQLiteDataReader reader = locationDB.ReadFullTable("location");
                while (reader.Read())
                {
                    //读取NAME与POSITION                    
                    txtLocation.Items.Add(reader.GetString(reader.GetOrdinal("NAME")) + "[" + reader.GetString(reader.GetOrdinal("POSITION")) + "]");
                }
                reader.Close();
                //locationDB.CloseConnection();

            }
            catch (Exception ex)
            {
                reader.Close();
                //locationDB.CloseConnection();
                MessageBox.Show(ex.Message);
            }
            finally
            {
                reader.Close();
                //locationDB.CloseConnection();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Location temp = new Location(map.Location.Latitude, map.Location.Longitude);
            map.ShowDialog();
            txtLocation.Text = $"{map.Location.Latitude}:{map.Location.Longitude}";
            txtLocation.Items.Clear();
            ReadLocationFromDB();
            map.Location = temp;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //var location = new Location(txtLocationTest.Text);
            //service.UpdateLocation(location);
            string locStr = txtLocation.Text;
            if (locStr.Contains("[") & locStr.Contains("]"))
            {
                int start = locStr.LastIndexOf("[");
                int end = locStr.LastIndexOf("]");
                locStr = locStr.Substring(start + 1, end - start - 1);
            }
            else if (locStr.Contains(","))
            {
                locStr = locStr.Replace(",", ":");
            }
            string[] loc = locStr.Split(new char[] { ':' });
            if (loc.Length == 2)
            {
                if (map.Location.Longitude == System.Convert.ToDouble(loc[1].Trim()) && map.Location.Latitude == System.Convert.ToDouble(loc[0].Trim())) { return; }
                distanceCal(System.Convert.ToDouble(loc[0].Trim()), System.Convert.ToDouble(loc[1].Trim()));
                lastLocation.Longitude = map.Location.Longitude;
                lastLocation.Latitude = map.Location.Latitude;
                map.Location.Longitude = System.Convert.ToDouble(loc[1].Trim());
                map.Location.Latitude = System.Convert.ToDouble(loc[0].Trim());
                service.UpdateLocation(map.Location);
            }
            else
            {
                PrintMessage($"The correct GPS format is：Lat:Lon or Lat:Lon.");
            }

        }

        private void distanceCal(double lat, double lon)
        {
            var oldCoord = new GeoCoordinate(map.Location.Latitude, map.Location.Longitude);
            var newCoord = new GeoCoordinate(lat, lon);
            double distance = Math.Round(oldCoord.GetDistanceTo(newCoord) / 1000, 2);
            PrintMessage($"The distance is {distance} km.");
            switch (distance)
            {
                case double n when (n <= 2):
                    PrintMessage($"The suggested cooldown time is 1 minute.");
                    break;
                case double n when (n <= 5):
                    PrintMessage($"The suggested cooldown time is 2 minutes.");
                    break;
                case double n when (n <= 7):
                    PrintMessage($"The suggested cooldown time is 5 minutes.");
                    break;
                case double n when (n <= 10):
                    PrintMessage($"The suggested cooldown time is 7 minutes.");
                    break;
                case double n when (n <= 12):
                    PrintMessage($"The suggested cooldown time is 8 minutes.");
                    break;
                case double n when (n <= 18):
                    PrintMessage($"The suggested cooldown time is 10 minutes.");
                    break;
                case double n when (n <= 26):
                    PrintMessage($"The suggested cooldown time is 15 minutes.");
                    break;
                case double n when (n <= 42):
                    PrintMessage($"The suggested cooldown time is 19 minutes.");
                    break;
                case double n when (n <= 65):
                    PrintMessage($"The suggested cooldown time is 22 minutes.");
                    break;
                case double n when (n <= 81):
                    PrintMessage($"The suggested cooldown time is 25 minutes.");
                    break;
                case double n when (n <= 100):
                    PrintMessage($"The suggested cooldown time is 35 minutes.");
                    break;
                case double n when (n <= 220):
                    PrintMessage($"The suggested cooldown time is 40 minutes.");
                    break;
                case double n when (n <= 250):
                    PrintMessage($"The suggested cooldown time is 45 minutes.");
                    break;
                case double n when (n <= 350):
                    PrintMessage($"The suggested cooldown time is 51 minutes.");
                    break;
                case double n when (n <= 375):
                    PrintMessage($"The suggested cooldown time is 54 minutes.");
                    break;
                case double n when (n <= 460):
                    PrintMessage($"The suggested cooldown time is 62 minutes.");
                    break;
                case double n when (n <= 500):
                    PrintMessage($"The suggested cooldown time is 65 minutes.");
                    break;
                case double n when (n <= 565):
                    PrintMessage($"The suggested cooldown time is 69 minutes.");
                    break;
                case double n when (n <= 700):
                    PrintMessage($"The suggested cooldown time is 78 minutes.");
                    break;
                case double n when (n <= 800):
                    PrintMessage($"The suggested cooldown time is 84 minutes.");
                    break;
                case double n when (n <= 900):
                    PrintMessage($"The suggested cooldown time is 92 minutes.");
                    break;
                case double n when (n <= 1000):
                    PrintMessage($"The suggested cooldown time is 99 minutes.");
                    break;
                case double n when (n <= 1100):
                    PrintMessage($"The suggested cooldown time is 107 minutes.");
                    break;
                case double n when (n <= 1200):
                    PrintMessage($"The suggested cooldown time is 114 minutes.");
                    break;
                case double n when (n <= 1300):
                    PrintMessage($"The suggested cooldown time is 117 minutes.");
                    break;
                case double n when (n > 1300):
                    PrintMessage($"The suggested cooldown time is 120 minutes.");
                    break;

            }
        }

        public void PrintMessage(string msg)
        {
            if (rtbxLog.InvokeRequired)
            {
                this.Invoke(new Action<string>(PrintMessage), msg);
            }
            else
            {
                rtbxLog.AppendText($"{DateTime.Now.ToString("HH:mm:ss")}： {msg}\r\n");
            }
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            service.ClearLocation();
        }

        private void rtbxLog_TextChanged(object sender, EventArgs e)
        {
            rtbxLog.SelectionStart = rtbxLog.Text.Length;
            rtbxLog.ScrollToCaret();
        }

        //↑
        private void button5_Click(object sender, EventArgs e)
        {

            PrintMessage($"向↑移动.");
            do
            {
                //111290.9197534m
                map.Location.Latitude += coordDiff;
                //map.Location.Longitude += 0.0005;
                service.UpdateLocation(map.Location);
                Delay(1000);
            } while (keepMoving);
        }

        // ←
        private void button3_Click(object sender, EventArgs e)
        {
            PrintMessage($"向←移动.");
            do
            {
                map.Location.Longitude -= coordDiff;
                service.UpdateLocation(map.Location);
                Delay(1000);
            } while (keepMoving);

        }

        //↓
        private void button4_Click(object sender, EventArgs e)
        {
            PrintMessage($"向↓移动.");
            do
            {
                map.Location.Latitude -= coordDiff;
                service.UpdateLocation(map.Location);
                Delay(1000);
            } while (keepMoving);

        }

        //→
        private void button6_Click(object sender, EventArgs e)
        {
            PrintMessage($"向→移动.");
            do
            {
                map.Location.Longitude += coordDiff;
                service.UpdateLocation(map.Location);
                Delay(1000);
            } while (keepMoving);

        }

        public static void Delay(int mm)
        {
            DateTime current = DateTime.Now;
            while (current.AddMilliseconds(mm) > DateTime.Now)
            {
                Application.DoEvents();
            }
            return;
        }

        private void frmMain_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Left:
                case Keys.NumPad4:
                    button3.PerformClick();
                    e.Handled = true;
                    break;
                case Keys.Right:
                case Keys.NumPad6:
                    button6.PerformClick();
                    e.Handled = true;
                    break;
                case Keys.Up:
                case Keys.NumPad8:
                    button5.PerformClick();
                    e.Handled = true;
                    break;
                case Keys.Down:
                case Keys.NumPad2:
                    button4.PerformClick();
                    e.Handled = true;
                    break;
                case Keys.NumPad7:
                    button9.PerformClick();
                    e.Handled = true;
                    break;
                case Keys.NumPad9:
                    button8.PerformClick();
                    e.Handled = true;
                    break;
                case Keys.NumPad1:
                    button10.PerformClick();
                    e.Handled = true;
                    break;
                case Keys.NumPad3:
                    button11.PerformClick();
                    e.Handled = true;
                    break;
                default:
                    break;
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            speed = System.Convert.ToDouble(textBox1.Text);
            coordDiff = (speed * 1000 / 3600) / 111290.9197534;
            PrintMessage($"速度修改为：{speed.ToString("0." + new string('#', 339))}");
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void txtLocation_TextChanged(object sender, EventArgs e)
        {

        }

        private void txtLocation_Click(object sender, EventArgs e)
        {

        }

        private void txtLocation_TextChanged_1(object sender, EventArgs e)
        {

        }

        private void txtLocation_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void txtLocation_SelectedIndexChanged(object sender, EventArgs e)
        {
            string locStr = txtLocation.Text;
            if (locStr.Contains("[") & locStr.Contains("]"))
            {
                int start = locStr.LastIndexOf("[");
                int end = locStr.LastIndexOf("]");
                locStr = locStr.Substring(start + 1, end - start - 1);
            }
            else if (locStr.Contains(","))
            {
                locStr = locStr.Replace(",", ":");
            }
            string[] loc = locStr.Split(new char[] { ':' });
            if (loc.Length == 2)
            {
                map.txtLocation.Longitude = System.Convert.ToDouble(loc[1].Trim());
                map.txtLocation.Latitude = System.Convert.ToDouble(loc[0].Trim());
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.CheckState == CheckState.Checked)
            {
                PrintMessage("开启持续移动，关闭请取消勾选!");
                keepMoving = true;
            }
            else
            {
                PrintMessage("已取消持续移动!");
                keepMoving = false;
            }
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void button7_Click(object sender, EventArgs e)
        {
            String locName = Interaction.InputBox("", "输入坐标别名", "", -1, -1);
            if (locName != "")
            {
                //MessageBox.Show(locName);
                InsertLocation(locName, map.Location.Latitude + ":" + map.Location.Longitude);
                ReadLocationFromDB();
                map.ReadNameFromDB();
            }
        }

        private void button9_Click(object sender, EventArgs e)
        {
            PrintMessage($"向↖移动.");
            do
            {
                map.Location.Latitude += coordDiff * Math.Sqrt(2) / 2;
                map.Location.Longitude -= coordDiff * Math.Sqrt(2) / 2;
                service.UpdateLocation(map.Location);
                Delay(1000);
            } while (keepMoving);
        }

        private void button8_Click(object sender, EventArgs e)
        {
            PrintMessage($"向↗移动.");
            do
            {
                map.Location.Latitude += coordDiff * Math.Sqrt(2) / 2;
                map.Location.Longitude += coordDiff * Math.Sqrt(2) / 2;
                service.UpdateLocation(map.Location);
                Delay(1000);
            } while (keepMoving);
        }

        private void button11_Click(object sender, EventArgs e)
        {
            PrintMessage($"向↘移动.");
            do
            {
                map.Location.Latitude -= coordDiff * Math.Sqrt(2) / 2;
                map.Location.Longitude += coordDiff * Math.Sqrt(2) / 2;
                service.UpdateLocation(map.Location);
                Delay(1000);
            } while (keepMoving);
        }

        private void button10_Click(object sender, EventArgs e)
        {
            PrintMessage($"向↙移动.");
            do
            {
                map.Location.Latitude -= coordDiff * Math.Sqrt(2) / 2;
                map.Location.Longitude -= coordDiff * Math.Sqrt(2) / 2;
                service.UpdateLocation(map.Location);
                Delay(1000);
            } while (keepMoving);
        }

        private void label4_DoubleClick(object sender, EventArgs e)
        {
            PrintMessage($"❤ ❤ ❤ 哇哦居然被发现了 ❤ ❤ ❤");
            PrintMessage($"https://github.com/DoubleO31/JocationPlus");
            PrintMessage($"❤ ❤ ❤ 免费开源欢迎比心 ❤ ❤ ❤");
            PrintMessage($"Current location is: {Math.Round(map.Location.Latitude, 2)},{Math.Round(map.Location.Longitude, 2)}");
        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void button12_Click(object sender, EventArgs e)
        {
            try
            {
                //Press again to stop the GPX
                if (!stopGPX)
                {
                    stopGPX = true;
                    button12.Text = "GPX";
                    return;
                }

                var fileContent = string.Empty;
                var filePath = string.Empty;

                using (OpenFileDialog openFileDialog = new OpenFileDialog())
                {
                    openFileDialog.InitialDirectory = "C:\\Users\\James\\Downloads";
                    openFileDialog.Filter = "gpx files (*.gpx)|*.gpx";
                    openFileDialog.FilterIndex = 2;
                    openFileDialog.RestoreDirectory = true;

                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        //Get the path of specified file
                        filePath = openFileDialog.FileName;

                        //Read the contents of the file into a stream
                        // var fileStream = openFileDialog.OpenFile();

                        // using (StreamReader reader = new StreamReader(fileStream))
                        // {
                        //     fileContent = reader.ReadToEnd();
                        // }
                    }
                }
                //PrintMessage($"{fileContent}");
                if (!string.IsNullOrEmpty(filePath))
                {
                    stopGPX = false;
                    button12.Text = "STOP";
                    PrintMessage($"GPX execution has begun for file{filePath}!");
                    foreach (XElement level1Element in XElement.Load(filePath).Elements("wpt"))
                    {
                        //PrintMessage($"{level1Element.Attribute("lat").Value},{level1Element.Attribute("lon").Value}");
                        map.Location.Latitude = Convert.ToDouble(level1Element.Attribute("lat").Value);
                        map.Location.Longitude = Convert.ToDouble(level1Element.Attribute("lon").Value);
                        service.UpdateLocation(map.Location);
                        Delay(3000);
                        if (stopGPX)
                        {
                            PrintMessage($"GPX execution has stopped!");
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                PrintMessage($"Ahhhh something went wrong!{ex.StackTrace}");
            }
        }

        private void button13_Click(object sender, EventArgs e)
        {
            if (lastLocation.Longitude == map.Location.Longitude && lastLocation.Latitude == map.Location.Latitude) { return; }
            distanceCal(lastLocation.Latitude, lastLocation.Longitude);
            map.Location.Longitude = lastLocation.Longitude;
            map.Location.Latitude = lastLocation.Latitude;
            service.UpdateLocation(map.Location);

        }
    }

}

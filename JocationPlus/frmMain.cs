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
        bool movingInPorgress = false;
        bool keepGPX = false;


        Location lastLocation = new Location();

        public frmMain()
        {
            CreateLocationDB();
            InitializeComponent();
            ReadLocationFromDB();
            PrintMessage("https://github.com/DoubleO31/JocationPlus");
            PrintMessage("This is an open source project. Please don't use it for any illegal activity.");
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
            map.Show();
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
                double newLat = System.Convert.ToDouble(loc[0].Trim());
                double newLon = System.Convert.ToDouble(loc[1].Trim());
                if (map.Location.Longitude == System.Convert.ToDouble(loc[1].Trim()) && map.Location.Latitude == System.Convert.ToDouble(loc[0].Trim())) { return; }
                if (!keepMoving)
                {

                    distanceCal(System.Convert.ToDouble(loc[0].Trim()), System.Convert.ToDouble(loc[1].Trim()), true);
                    lastLocation.Longitude = map.Location.Longitude;
                    lastLocation.Latitude = map.Location.Latitude;
                    map.Location.Longitude = newLon;
                    map.Location.Latitude = newLat;
                    service.UpdateLocation(map.Location);
                }
                else
                {
                    if (map.Location.Latitude == 0 && map.Location.Longitude == 0)
                    {
                        PrintMessage($"Please teleport to a location first!");
                        return;
                    }
                    PrintMessage($"Walking towards the coordinate!");
                    distanceCal(newLat, newLon, false);
                    lastLocation.Longitude = map.Location.Longitude;
                    lastLocation.Latitude = map.Location.Latitude;

                    processWalking(new Location(map.Location.Latitude, map.Location.Longitude),
                     new Location(newLat, newLon), ref keepMoving, "Walking");
                    if (keepMoving)
                    {
                        PrintMessage($"Reached the coordinate!");
                        PrintMessage($"Walking execution has stopped!");
                    }

                }
            }
            else
            {
                PrintMessage($"The correct GPS format is：Lat,Lon or Lat:Lon.");
            }

        }

        private void distanceCal(double lat, double lon, bool teleport)
        {
            var oldCoord = new GeoCoordinate(map.Location.Latitude, map.Location.Longitude);
            var newCoord = new GeoCoordinate(lat, lon);
            double distance = Math.Round(oldCoord.GetDistanceTo(newCoord) / 1000, 2);

            if (!teleport)
            {
                PrintMessage($"Distance: {distance} km. Walking time: { Math.Round(distance / speed * 3600, 0)}s");
                return;
            }
            if (map.Location.Latitude == 0 && map.Location.Longitude == 0) { return; }
            PrintMessage($"Distance: {distance} km.");
            switch (distance)
            {
                case double n when (n <= 2):
                    PrintMessage($"Suggested cooldown: 1 minute.");
                    break;
                case double n when (n <= 5):
                    PrintMessage($"Suggested cooldown: 2 minutes.");
                    break;
                case double n when (n <= 7):
                    PrintMessage($"Suggested cooldown: 5 minutes.");
                    break;
                case double n when (n <= 10):
                    PrintMessage($"Suggested cooldown: 7 minutes.");
                    break;
                case double n when (n <= 12):
                    PrintMessage($"Suggested cooldown: 8 minutes.");
                    break;
                case double n when (n <= 18):
                    PrintMessage($"Suggested cooldown: 10 minutes.");
                    break;
                case double n when (n <= 26):
                    PrintMessage($"Suggested cooldown: 15 minutes.");
                    break;
                case double n when (n <= 42):
                    PrintMessage($"Suggested cooldown: 19 minutes.");
                    break;
                case double n when (n <= 65):
                    PrintMessage($"Suggested cooldown: 22 minutes.");
                    break;
                case double n when (n <= 81):
                    PrintMessage($"Suggested cooldown: 25 minutes.");
                    break;
                case double n when (n <= 100):
                    PrintMessage($"Suggested cooldown: 35 minutes.");
                    break;
                case double n when (n <= 220):
                    PrintMessage($"Suggested cooldown: 40 minutes.");
                    break;
                case double n when (n <= 250):
                    PrintMessage($"Suggested cooldown: 45 minutes.");
                    break;
                case double n when (n <= 350):
                    PrintMessage($"Suggested cooldown: 51 minutes.");
                    break;
                case double n when (n <= 375):
                    PrintMessage($"Suggested cooldown: 54 minutes.");
                    break;
                case double n when (n <= 460):
                    PrintMessage($"Suggested cooldown: 62 minutes.");
                    break;
                case double n when (n <= 500):
                    PrintMessage($"Suggested cooldown: 65 minutes.");
                    break;
                case double n when (n <= 565):
                    PrintMessage($"Suggested cooldown: 69 minutes.");
                    break;
                case double n when (n <= 700):
                    PrintMessage($"Suggested cooldown: 78 minutes.");
                    break;
                case double n when (n <= 800):
                    PrintMessage($"Suggested cooldown: 84 minutes.");
                    break;
                case double n when (n <= 900):
                    PrintMessage($"Suggested cooldown: 92 minutes.");
                    break;
                case double n when (n <= 1000):
                    PrintMessage($"Suggested cooldown: 99 minutes.");
                    break;
                case double n when (n <= 1100):
                    PrintMessage($"Suggested cooldown: 107 minutes.");
                    break;
                case double n when (n <= 1200):
                    PrintMessage($"Suggested cooldown: 114 minutes.");
                    break;
                case double n when (n <= 1300):
                    PrintMessage($"Suggested cooldown: 117 minutes.");
                    break;
                case double n when (n > 1300):
                    PrintMessage($"Suggested cooldown: 120 minutes.");
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

            PrintMessage($"Moving ↑.");
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
            PrintMessage($"Moving ←.");
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
            PrintMessage($"Moving ↓.");
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
            PrintMessage($"Moving →.");
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
            PrintMessage($"Speed changed to：{speed.ToString("0." + new string('#', 339))}km/h");
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
                PrintMessage("Enabled continuous walking! Press any arrow key to start walking.");
                PrintMessage("Uncheck to disable it.");
                keepMoving = true;
                button2.Text = "Walking";
            }
            else
            {
                PrintMessage("Disabled continuous walking!");
                keepMoving = false;
                button2.Text = "Teleport";
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
            String locName = Interaction.InputBox("", "Enter coordinate name", "", -1, -1);
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
            PrintMessage($"Moving ↖.");
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
            PrintMessage($"Moving ↗.");
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
            PrintMessage($"Moving ↘.");
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
            PrintMessage($"Moving ↙.");
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
            PrintMessage($"❤ ❤ ❤ You found me! ❤ ❤ ❤");
            PrintMessage($"https://github.com/DoubleO31/JocationPlus");
            PrintMessage($"❤ ❤ ❤ Hope you like this app!❤ ❤ ❤");
            PrintMessage($"Current location is: {Math.Round(map.Location.Latitude, 5)},{Math.Round(map.Location.Longitude, 5)}");
        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void button12_Click(object sender, EventArgs e)
        {
            try
            {
                //Press again to stop the GPX
                if (keepGPX)
                {
                    keepGPX = false;
                    button12.Text = "GPX";
                    return;
                }

                var fileContent = string.Empty;
                var filePath = string.Empty;

                using (OpenFileDialog openFileDialog = new OpenFileDialog())
                {
                    openFileDialog.InitialDirectory = "C:\\";
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
                    keepGPX = true;
                    button12.Text = "STOP";
                    PrintMessage($"GPX execution has begun for file {filePath}!");
                    var locations = new List<Location>();
                    foreach (XElement level1Element in XElement.Load(filePath).Elements("wpt"))
                    {
                        locations.Add(new Location(Convert.ToDouble(level1Element.Attribute("lat").Value), Convert.ToDouble(level1Element.Attribute("lon").Value)));
                    }
                    processGPX(locations);
                }
            }
            catch (Exception ex)
            {
                PrintMessage($"Ahhhh something went wrong!{ex.StackTrace}");
            }
        }

        private void processGPX(List<Location> locations)
        {
            PrintMessage($"GPX routing is starting in 10 seconds!");
            Delay(10000);
            map.Location = locations[0];
            service.UpdateLocation(map.Location);
            for (int i = 1; i < locations.Count; i++)
            {
                PrintMessage($"Reached {AddOrdinal(i)} coordinate!");
                PrintMessage($"Waiting for 5 seconds.");
                Delay(5000);
                PrintMessage($"Moving to the next one!");
                Location lastLoc = locations[i - 1];
                Location nextLoc = locations[i];
                processWalking(lastLoc, nextLoc, ref keepGPX, "GPX");
                if (!keepGPX)
                {
                    return;
                }

            }
            PrintMessage($"Reached the last coordinate!");
            PrintMessage($"GPX execution has stopped!");
            keepGPX = false;
            button12.Text = "GPX";

        }

        private void processWalking(Location lastLoc, Location nextLoc, ref bool stop, string process)
        {
            var oldCoord = new GeoCoordinate(lastLoc.Latitude, lastLoc.Longitude);
            var newCoord = new GeoCoordinate(nextLoc.Latitude, nextLoc.Longitude);
            double distance = oldCoord.GetDistanceTo(newCoord);
            double interval = Math.Ceiling(distance / (speed * 1000 / 3600));
            double latDiff = (nextLoc.Latitude - lastLoc.Latitude) / interval;
            double lonDiff = (nextLoc.Longitude - lastLoc.Longitude) / interval;
            for (int n = 0; n < interval; n++)
            {
                if (!stop)
                {
                    PrintMessage($"{process} execution has stopped!");
                    return;
                }
                map.Location.Latitude += latDiff;
                map.Location.Longitude += lonDiff;
                service.UpdateLocation(map.Location);
                Delay(1000);

            }

        }

        private void button13_Click(object sender, EventArgs e)
        {
            if (lastLocation.Longitude == map.Location.Longitude && lastLocation.Latitude == map.Location.Latitude) { return; }
            distanceCal(lastLocation.Latitude, lastLocation.Longitude, true);
            map.Location.Longitude = lastLocation.Longitude;
            map.Location.Latitude = lastLocation.Latitude;
            PrintMessage($"Teleporting back to the last entered coordinate!");
            service.UpdateLocation(map.Location);

        }

        private static string AddOrdinal(int num)
        {
            if (num <= 0) return num.ToString();

            switch (num % 100)
            {
                case 11:
                case 12:
                case 13:
                    return num + "th";
            }

            switch (num % 10)
            {
                case 1:
                    return num + "st";
                case 2:
                    return num + "nd";
                case 3:
                    return num + "rd";
                default:
                    return num + "th";
            }
        }
    }

}

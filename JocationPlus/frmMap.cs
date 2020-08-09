using System;
using System.Configuration;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TestSqlite.sq;

namespace LocationCleaned
{
    [ComVisible(true)]
    public partial class frmMap : Form
    {
        /// <summary>
        /// 经纬度坐标
        /// </summary>
        public new Location Location { get; set; } = new Location();
        public SqLiteHelper locationDB { get; set; } = new SqLiteHelper("locationDB.db");
        public new Location txtLocation { get; set; } = new Location();
        public frmMap()
        {
            //this.locationDB = locationDB;
            CreateLocationDB();
            InitializeComponent();
            ReadNameFromDB();
        }

        private void frmMap_Load(object sender, EventArgs e)
        {   Console.WriteLine("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
            var google = @"<!DOCTYPE html>
<html>
  <head>
    <title>Places Search Box</title>
    <script src='https://polyfill.io/v3/polyfill.min.js?features=default'></script>
    <script
      src='https://maps.googleapis.com/maps/api/js?key="+ConfigurationManager.AppSettings.Get("GoogleMapAPIKey")+@"&callback=initAutocomplete&libraries=places&v=weekly'
      defer
    ></script>
    <style type='text/css'>
      /* Always set the map height explicitly to define the size of the div
       * element that contains the map. */
      #map {
        height: 100%;
      }

      /* Optional: Makes the sample page fill the window. */
      html,
      body {
        height: 100%;
        margin: 0;
        padding: 0;
      }

      #description {
        font-family: Roboto;
        font-size: 15px;
        font-weight: 300;
      }

      #pac-input {
        background-color: #fff;
        font-family: Roboto;
        font-size: 15px;
        font-weight: 300;
        margin-left: 12px;
        margin-top: 12px;
        padding: 0 11px 0 13px;
        text-overflow: ellipsis;
        width: 400px;
        Height: 20px;
      }

      #pac-input:focus {
        border-color: #4d90fe;
      }

      #title {
        color: #fff;
        background-color: #4d90fe;
        font-size: 25px;
        font-weight: 500;
        padding: 6px 12px;
      }

      #target {
        width: 345px;
      }
    </style>
    <script>
      'use strict';

      // This example adds a search box to a map, using the Google Place Autocomplete
      // feature. People can enter geographical searches. The search box will return a
      // pick list containing a mix of places and predicted search terms.
      // This example requires the Places library. Include the libraries=places
      // parameter when you first load the API. For example:
      // <script src='https://maps.googleapis.com/maps/api/js?key=YOUR_API_KEY&libraries=places'>
      function initAutocomplete() {
        var lat = "+GetLatitude()+@";
        var lon = "+GetLongitude()+@";
        if(lat == 0 && lon == 0){
            lat = -33.8688;
            lon = 151.2195;
        }
    const map = new google.maps.Map(document.getElementById('map'), {
            center: {
                lat: lat,
                lng: lon
            },
            zoom: 13,
            mapTypeId: 'roadmap',
            streetViewControl: false,
            mapTypeControl: false
        }); // Create the search box and link it to the UI element.

        const input = document.getElementById('pac-input');
        const searchBox = new google.maps.places.SearchBox(input);
        map.controls[google.maps.ControlPosition.TOP_LEFT].push(input); // Bias the SearchBox results towards current map's viewport.

        map.addListener('bounds_changed', () => {
          searchBox.setBounds(map.getBounds());
        });
        
        let markers = []; // Listen for the event fired when the user selects a prediction and retrieve
        var myLatLon = {lat: lat, lng: lon};
        markers.push(new google.maps.Marker({map, position: myLatLon}));
        window.external.notify(myLatLon.lat+';'+myLatLon.lng+'; ');
        
        map.addListener('click', function(mapsMouseEvent) {
          // Close the current InfoWindow.
          markers.forEach(marker => {
            marker.setMap(null);
          });
          markers = [];
          
          // Create a new InfoWindow.
          markers.push(new google.maps.Marker({map, position: mapsMouseEvent.latLng}));
          var coords = mapsMouseEvent.latLng.toString().slice(1,-1);
          window.external.notify(coords.split(',')[0]+';'+coords.split(',')[1]+'; ');
        });


        searchBox.addListener('places_changed', () => {
          const places = searchBox.getPlaces();

          if (places.length == 0) {
            return;
          } // Clear out the old markers.

          markers.forEach(marker => {
            marker.setMap(null);
          });
          markers = []; // For each place, get the icon, name and location.

          const bounds = new google.maps.LatLngBounds();
          places.forEach(place => {
            if (!place.geometry) {
              console.log('Returned place contains no geometry');
              return;
            }
            
            markers.push(
              new google.maps.Marker({
                map,
                title: place.name,
                position: place.geometry.location
              })
            );
            var coords = place.geometry.location.toString().slice(1,-1);
            window.external.notify(coords.split(',')[0]+';'+coords.split(',')[1]+';'+place.formatted_address);
            

            if (place.geometry.viewport) {
              // Only geocodes have viewport.
              bounds.union(place.geometry.viewport);
            } else {
              bounds.extend(place.geometry.location);
            }
          });
          map.fitBounds(bounds);
        });
      }
    </script>
  </head>
  <body>
    <input
      id='pac-input'
      class='controls'
      type='text'
      placeholder='Search Box'
    />
    <div id='map'></div>
  </body>
</html>";
            var test = @"<!DOCTYPE html>
<html>
<body>

<h1>My First Heading</h1>
<p>My first paragraph.</p>

</body>
</html>";
            //this.webView1.Refresh();
            this.webView1.NavigateToString(google);
            //this.webView1.Navigate("https://www.google.ca");
        }
        public void position(string a_0, string a_1, string b_0)
        {
            this.label3.Text = (double.Parse(a_1)).ToString();
            this.label4.Text = (double.Parse(a_0)).ToString();
            this.label5.Text = b_0;
            this.textBox1.Text = b_0;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            double lon = double.Parse(label3.Text);
            double lat = double.Parse(label4.Text);
            //Location location = LocationService.bd09_To_Gcj02(lat, lon);
            //location = LocationService.gcj_To_Gps84(location.Latitude, location.Longitude);
            this.Location = new Location(lat,lon);
            Close();

        }
        public void Alert(string msg)
        {
            MessageBox.Show(msg);
        }

        private void button2_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {

        }

        public void button2_Click_1(object sender, EventArgs e)
        {
            if (this.textBox1.Text.Trim() != "" && this.label3.Text.Trim() != "" && this.label4.Text.Trim() != "")
            {
                string name = this.textBox1.Text.Trim();
                string position = this.label4.Text.Trim() + ":" + this.label3.Text.Trim();
                //MessageBox.Show(name+"\n"+position);
                this.InsertLocation(name, position);
                this.ReadNameFromDB();
            }
            else
            {
                MessageBox.Show("别名和经纬度不能为空哦!");
            }

        }

        private void CreateLocationDB()
        {
            //SqLiteHelper locationDB = new SqLiteHelper("locationDB.db");
            try
            {
                //创建名为table1的数据表
                locationDB.CreateTable("location", new string[] { "NAME", "POSITION" }, new string[] { "TEXT primary key", "TEXT" });
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
        public string GetLatitude()
        {
            Console.WriteLine(Location.Latitude.ToString());
            Console.WriteLine(txtLocation.Latitude.ToString());
            if (Location.Latitude.ToString() == "0" && txtLocation.Latitude.ToString() == "0")
            {
                return "0";
            }
            return Location.Latitude.ToString() == "0" ? txtLocation.Latitude.ToString() : Location.Latitude.ToString();
        }

        public string GetLongitude()
        {
            Console.WriteLine(Location.Longitude.ToString());
            Console.WriteLine(txtLocation.Longitude.ToString());
            if (Location.Longitude.ToString() == "0" && txtLocation.Longitude.ToString() == "0")
            {
                return "0";
            }
            return Location.Longitude.ToString() == "0" ? txtLocation.Longitude.ToString() : Location.Longitude.ToString();
        }

        public void ReadNameFromDB()
        {
            comboBox1.Items.Clear();
            //SqLiteHelper locationDB = new SqLiteHelper("locationDB.db");
            SQLiteDataReader reader = locationDB.ReadFullTable("location");
            try
            {
                //连接数据库
                //locationDB = new SqLiteHelper("locationDB.db");
                //读取整张表
                //SQLiteDataReader reader = locationDB.ReadFullTable("location");
                while (reader.Read())
                {
                    //读取NAME与POSITION                    
                    comboBox1.Items.Add(reader.GetString(reader.GetOrdinal("NAME")));
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

        private void InsertLocation(string name, string position)
        {
            //SqLiteHelper locationDB = new SqLiteHelper("locationDB.db");
            try
            {
                //创建名为table1的数据表
                //locationDB.CreateTable("location", new string[] { "NAME", "POSITION" }, new string[] { "TEXT primary key", "TEXT" });
                locationDB.InsertValues("location", new string[] { name, position });
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

        private void DeleteLocation(string name)
        {
            //SqLiteHelper locationDB = new SqLiteHelper("locationDB.db");
            try
            {
                locationDB.DeleteValuesAND("location", new string[] { "NAME" }, new string[] { name }, new string[] { "=" });
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

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged_1(object sender, EventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged_1(object sender, EventArgs e)
        {

        }

        private void label8_Click(object sender, EventArgs e)
        {

        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            string name = comboBox1.Text.Trim();
            DeleteLocation(name);
            ReadNameFromDB();
        }

        private void webBrowser1_DocumentCompleted_1(object sender, WebBrowserDocumentCompletedEventArgs e)
        {

        }

        private void groupBox2_Enter(object sender, EventArgs e)
        {

        }

        private void webView1_ScriptNotify(object sender, Microsoft.Toolkit.Win32.UI.Controls.Interop.WinRT.WebViewControlScriptNotifyEventArgs e)
        {
            string coords = e.Value;
            position(coords.Split(';')[0], coords.Split(';')[1], coords.Split(';')[2]);
        }
    }
}

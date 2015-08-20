using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Support.UI;
using System.Xml;
using System.Web.Services;
using System.Reflection;
using GoogleMaps.LocationServices;
using Geocoding.Google;
using Geocoding;
using System.Data;
using System.Configuration;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web.Script.Serialization;




public partial class Direction : System.Web.UI.Page
{
    static List<string> marker_place = new List<string>();
    public static string[] waypoints_scenic = null;
    JavaScriptSerializer javaSerial = null;

    protected void Page_Load(object sender, EventArgs e)
    {

    }

    [System.Web.Services.WebMethod]
    public static void markers_address(object[] lat_long)
    {

        Dictionary<int, Dictionary<string, object>> struc = new Dictionary<int, Dictionary<string, object>>();
        Dictionary<string, object> inner = new Dictionary<string, object>();
        List<string> lat_long_pair = new List<string>();
        for (int i = 0; i < lat_long.Length; i++)
        {
            inner = (Dictionary<string, object>)lat_long.GetValue(i);
            foreach (var pair in inner)
            {
                string a = pair.Key;
                object b = pair.Value;
                string lat_lng_str = (string)Convert.ChangeType(b, typeof(string));
                lat_long_pair.Add(lat_lng_str);
            }
        }

        // convert lat-long to place
        var locationService = new GoogleLocationService();
        // List<string> marker_place = new List<string>();
        var count = 0;
        marker_place.Clear();
        for (int i = 0; i < lat_long_pair.Count; i = i + 2)
        {
            IGeocoder geocoder = new GoogleGeocoder() { };
            try
            {
                IEnumerable<Address> addresses = geocoder.ReverseGeocode(Convert.ToDouble(lat_long_pair[i]), Convert.ToDouble(lat_long_pair[i + 1]));


                foreach (Address adr in addresses)
                {
                    if (count == 0)
                    {
                        string address = adr.FormattedAddress;
                        marker_place.Add(address);
                    }
                    break;
                }
                count = 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }


    }

    public void DrawScenicDirection()
    {
        var locationService = new GoogleLocationService();
        var start = locationService.GetLatLongFromAddress("The University of Akron,OHIO");
    }

    public string[] CreateListScenicPlaces()
    {
        string connection_string = ConfigurationManager.ConnectionStrings["UA_NAVConnectionString"].ConnectionString;
        SqlConnection conn = new SqlConnection(connection_string);

        List<string> waypoints_list = new List<string>();

        SqlCommand cmd_1 = new SqlCommand();
        SqlDataReader reader_1;
        cmd_1.CommandText = "Scenic_Places";
        cmd_1.CommandType = CommandType.StoredProcedure;
        cmd_1.Connection = conn;
        cmd_1.Parameters.Add(new SqlParameter("@no_of_tables", marker_place.Count));
        conn.Open();
        reader_1 = cmd_1.ExecuteReader();
        conn.Close();

        SqlCommand cmd_2 = new SqlCommand();
        SqlDataReader reader_2;
        cmd_2.CommandText = @"SELECT TOP 8 * FROM [UA_NAV].[dbo].[Way_Points_Table]";
        cmd_2.Connection = conn;
        conn.Open();
        reader_2 = cmd_2.ExecuteReader();
        while (reader_2.Read())
        {
            waypoints_list.Add(reader_2["Address"].ToString());

        }
        waypoints_scenic = waypoints_list.ToArray();
        conn.Close();
        return waypoints_scenic;
    }



    public void WriteDataToDATABASE(DataSet ds)
    {
        string connection_string = ConfigurationManager.ConnectionStrings["UA_NAVConnectionString"].ConnectionString;
        SqlConnection conn = new SqlConnection(connection_string);

        SqlCommand cmd = new SqlCommand();
        SqlDataReader reader;

        cmd.CommandText = "StoredProc_tab_create";
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.Connection = conn;
        cmd.Parameters.Add(new SqlParameter("@no_of_tables", marker_place.Count));
        conn.Open();
        reader = cmd.ExecuteReader();
        conn.Close();
        using (SqlBulkCopy bc = new SqlBulkCopy(connection_string))
        {
            for (int i = 0; i < ds.Tables.Count; i++)
            {
                bc.DestinationTableName = ds.Tables[i].TableName;

                Console.WriteLine(DateTime.Now);
                try
                {
                    conn.Open();
                    bc.WriteToServer(ds.Tables[i]);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                finally
                {
                    conn.Close();
                }

                Console.WriteLine(DateTime.Now);
            }

        }

    }


    protected void Button4_Click(object sender, EventArgs e)
    {
        // Response.Write(marker_place);
        List<string> lst_zipscodes = new List<string>();
        IGeocoder geocoder = new GoogleGeocoder() { };
        DataSet ds = new DataSet("Sites_Collection");
        string connection_string = ConfigurationManager.ConnectionStrings["UA_NAVConnectionString"].ConnectionString;
        SqlConnection conn = new SqlConnection(connection_string);

        WeatherReference.WeatherSoapClient weather = new WeatherReference.WeatherSoapClient("WeatherSoap");


        // my source starting placeplace 

        for (int i = 0; i < marker_place.Count; i++)
        {
            string source = marker_place[i];
            string[] addr_string = source.Split(',');
            string[] zipcode = null;
            if (addr_string.Count() == 4)
            {
                zipcode = addr_string[2].Trim().Split(' ');
                source = addr_string[1] + "," + zipcode[0];
                lst_zipscodes.Add(zipcode[1]);
            }
            else
            {
                continue;
            }
            IWebDriver driver = null;
            try
            {
                driver = new FirefoxDriver();

                driver.Navigate().GoToUrl("http://www.nwf.org/naturefind.aspx");
                driver.Manage().Window.Maximize();

                var place_name = driver.FindElement(By.Id("content_0_txtBasicSearch"));
                place_name.Clear();
                place_name.SendKeys(source);
                driver.FindElement(By.Id("content_0_btnSearchSites")).Click();
                //driver.Manage().Timeouts().ImplicitlyWait(TimeSpan.FromSeconds(10));

                IWait<IWebDriver> wait = new OpenQA.Selenium.Support.UI.WebDriverWait(driver, TimeSpan.FromSeconds(15.00));
                // IWait<IWebDriver> wait = null;
                wait.Until(driver1 => ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete"));


                DataTable dt = new DataTable("Places_" + i);

                DataColumn place_Coulumn = new DataColumn("Scenic_Place_Name", Type.GetType("System.String"));
                DataColumn lat_Coulumn = new DataColumn("Latitude", Type.GetType("System.String"));
                DataColumn lng_Coulumn = new DataColumn("Longitude", Type.GetType("System.String"));
                DataColumn address_of_place = new DataColumn("Address", Type.GetType("System.String"));
                DataColumn Zipscode = new DataColumn("Zipcode", Type.GetType("System.String"));
                DataColumn weather_desc = new DataColumn("Weather", Type.GetType("System.String"));
                DataColumn temperature = new DataColumn("Temperature", Type.GetType("System.String"));
                DataColumn traffic = new DataColumn("Traffic", Type.GetType("System.String"));
                DataColumn safety = new DataColumn("Safety", Type.GetType("System.String"));
                dt.Columns.Add(place_Coulumn);
                dt.Columns.Add(lat_Coulumn);
                dt.Columns.Add(lng_Coulumn);
                dt.Columns.Add(address_of_place);
                dt.Columns.Add(Zipscode);
                dt.Columns.Add(weather_desc);
                dt.Columns.Add(temperature);
                dt.Columns.Add(traffic);
                dt.Columns.Add(safety);

                DataRow dr;
                int count1 = 0;

                try
                {

                    wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//div[@id='content_0_grdSiteList']//tr[@class='rgRow']//u")));
                    wait.Until(ExpectedConditions.ElementExists(By.XPath("//div[@id='content_0_grdSiteList']//tr[@class='rgRow']//u")));
                    IList<IWebElement> lst_places = driver.FindElements(By.XPath(".//div[@id='content_0_grdSiteList']//tr[@class='rgRow']//u"));

                    if (lst_places == null)
                        continue;
                    int count = 0;
                    foreach (IWebElement place in lst_places)
                    {
                        //   if (count1!= -1)
                        //    {
                        try
                        {
                            dr = dt.NewRow();
                            Thread.Sleep(200);
                            dr["Scenic_Place_Name"] = place.Text;
                            IEnumerable<Address> addresses = geocoder.Geocode(place.Text + "," + zipcode[0]);
                            string place_addr = null;
                            Location ltng = null;

                            foreach (Address adr in addresses)
                            {
                                if (count == 0)
                                {
                                    place_addr = adr.FormattedAddress;
                                    ltng = adr.Coordinates;
                                    dr["Address"] = place_addr;
                                    break;
                                }
                            }

                            dr["Latitude"] = ltng.Latitude;
                            dr["Longitude"] = ltng.Longitude;

                            //tokenize place address 

                            string[] array = place_addr.Split(',');
                            string[] waypoints = place_addr.Split(','); ///////*******************
                            string zip = array[array.Length - 2];
                            string[] arr = zip.Trim().Split(' ');
                            string webservicezip = null;
                            if (arr.Length == 1)
                            {
                                dr["Zipcode"] = zipcode[1];
                                webservicezip = zipcode[1];
                            }
                            else if (Regex.IsMatch(place_addr, @"\d"))
                            {
                                arr = zip.Trim().Split(' ');
                                dr["Zipcode"] = arr[1].Trim();
                                webservicezip = arr[1].Trim();
                            }

                            //weather update

                            WeatherReference.WeatherReturn weather_of_place = weather.GetCityWeatherByZIP(webservicezip);  //  arr[1].Trim()
                            dr["Weather"] = weather_of_place.Description;
                            dr["Temperature"] = weather_of_place.Temperature;

                            Random rnd = new Random();
                            dr["Traffic"] = rnd.Next(2, 5);
                            dr["Safety"] = rnd.Next(60, 100);
                            dt.Rows.Add(dr);
                            //break;
                        }

                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                            continue;
                        }
                    }

                }
                finally
                {

                    ds.Tables.Add(dt);
                }


            }

            finally
            {
                driver.Close();
                driver.Dispose();
            }
        }

        WriteDataToDATABASE(ds);
        string[] scenic_places = CreateListScenicPlaces();
        //    DrawScenicDirection();
        foreach (string s in scenic_places)
        {
            ClientScript.RegisterArrayDeclaration("scenic_places", "\"" + s + "\"");
        }

        ClientScript.RegisterStartupScript(Page.GetType(), "Scenic", "scenic_route();", true);



    }
}



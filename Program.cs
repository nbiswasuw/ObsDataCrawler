using HtmlAgilityPack;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace ObsDataCrawler
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("Realtime Ground Measured data downloader of SASWE Research Group");
            Console.WriteLine("Scripts developed by Nishan Kumar Biswas, contact: nbiswas@uw.edu, nishan.wre.buet@gmail.com");
            Console.WriteLine("----------------------------------------------------------");
            Console.ResetColor();

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Parameters and variables are initiating....");
            StringBuilder logText = new StringBuilder();
            StringBuilder rainfallData = new StringBuilder();
            StringBuilder waterLevelData = new StringBuilder();
            DirectoryInfo iniDi = new DirectoryInfo(@"C:\Users\nbiswas\Desktop\Nishan\SASWE\AutoCorrection");
            DateTime today = DateTime.Today.Date;
            //DateTime today = new DateTime(2016, 07, 21, 0, 0, 0);
            string plainDate = today.ToString("yyyyMMdd");
            string hiphenDate = today.ToString("yyyy-MM-dd");
            File.WriteAllText(@"C:\Users\nbiswas\Desktop\Nishan\SASWE\AutoCorrection\Programs\ExecutionFiles\Correction_Date.txt", hiphenDate);
            string preplainDate = today.AddDays(-1).ToString("yyyyMMdd");
            string prehiphenDate = today.AddDays(-1).ToString("yyyy-MM-dd");
            Directory.CreateDirectory(iniDi + @"\RawRainfall\WebData_" + hiphenDate);
            DirectoryInfo di = new DirectoryInfo(iniDi + @"\RawRainfall\WebData_" + hiphenDate);

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Realtime Data Downloading started for Date: " + hiphenDate);
            Console.ResetColor();
            logText.AppendLine("Data Download started for " + hiphenDate + ", started at " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            
            /////------------------------------------------------------------------------- CityWX New Stations from IMD front Page   ------------------------------------------------------
            try
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("1st Website, IMD front page: http://14.139.247.11/citywx/citywxnew.php");
                Directory.CreateDirectory(di + @"\WebPage1");
                Console.ResetColor();

                string[] stationInfo = File.ReadAllLines(iniDi + @"\NecessaryFiles\cityWXStationInfo.csv");
                string[] station = new string[stationInfo.Length - 1];
                string[] stationID = new string[stationInfo.Length - 1];
                for (int i = 0; i < stationInfo.Length - 1; i++)
                {
                    var dispers = stationInfo[i + 1].Split(',');
                    stationID[i] = dispers[0];
                    station[i] = dispers[1];
                }

                StringBuilder sb = new StringBuilder();
                WebClient client = new WebClient();

                int counter = 0;
                for (int i = 0; i < stationID.Length; i++)
                {
                    try
                    {
                        string htmlCode = client.DownloadString("http://14.139.247.11/citywx/citywxnew.php?id=" + stationID[i]);
                        File.WriteAllText(di + @"\WebPage1\" + stationID[i] + ".html", htmlCode);
                        HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                        doc.LoadHtml(htmlCode);
                        HtmlNodeCollection boldtext = doc.DocumentNode.SelectNodes(".//b");
                        DateTime webDate = DateTime.Parse(boldtext[1].InnerText.Substring(7, 12));
                        HtmlNodeCollection tables = doc.DocumentNode.SelectNodes("//table");
                        HtmlNodeCollection rows = tables[0].SelectNodes(".//tr");
                        HtmlNodeCollection col = rows[6].SelectNodes(".//td");
                        Console.WriteLine(webDate.ToString("yyyy-MM-dd HH:mm:ss") + "," + station[i] + "," + col[1].InnerText.Trim());
                        rainfallData.AppendLine(webDate.ToString("yyyy-MM-dd HH:mm:ss") + "," + station[i] + "," + col[1].InnerText.Trim());
                        counter = counter + 1;
                    }
                    catch (Exception error)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Problem in one station of 1st Website: IMD Front Page, Station ID: " + stationID[i] + ", data not available. Error: " + error.Message);
                        Console.ResetColor();
                        continue;
                    }
                }
                logText.AppendLine("1st Website, IMD front page, Page link: http://14.139.247.11/citywx/citywxnew.php, Downloaded Station: " + counter);
            }
            catch (Exception error)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Problem in 1st Website, IMD frontpage: http://14.139.247.11/citywx/citywxnew.php, cannot be accessed. Error: " + error.Message);
                Console.ResetColor();
                logText.AppendLine("Problem in 1st Website, IMD frontpage: http://14.139.247.11/citywx/citywxnew.php, Error: " + error.Message);
            }

            ///---------------------------------------------------City Weather Data----------------------------------------------------------
            try
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("2nd Website, CityWX Earlier Page: http://202.54.31.7/citywx/city_weather.php");
                Directory.CreateDirectory(di + @"\WebPage2");
                Console.ResetColor();

                StringBuilder sb = new StringBuilder();
                WebClient client = new WebClient();
                string[] stationID = new string[] { "42348", "42339", "42165", "42542", "42343", "42452", "42435", "42170", "42328", "42540", "42123", "42112", "42146", "42111", "42148", "99911", "42147", "42116", "99912", "42114", "99918", "42103", "42099", "42101", "42131", "99915", "42137", "00010", "99916", "42071", "99917", "42350", "42178", "42177", "42075", "42057", "42097", "42027", "42056", "42034", "42026", "42054", "42028", "42045", "42043", "42048", "42031", "42044", "42379", "42479", "42475", "42369", "42189", "42463", "42260", "42139", "42187", "42366", "42262", "42273", "42375", "42066", "42083", "42063", "42062", "42065", "8205", "42081", "42106", "42079" };
                string[] stationName = new string[] { "Jaipur", "Jodhpur", "Bikaner", "Udaipur", "Ajmer", "Kota", "Barmer", "Churu", "Jaisalmer", "Mount Abu", "Sriganganagar", "Mussorie", "Nainital", "Dehradun", "Pantnagar", "Pithoragarh", "Mukteshwar", "Joshimath", "Almora", "Tehri", "Haridwar", "Ambala", "Ludhiana", "Patiala", "Hissar", "Kurukshetra", "Karnal", "Chandigarh", "Sirsa", "Amritsar", "Anandpur Sahib", "Bhiwani", "Gurgaon", "Narnaul", "Jalandhar", "Pathankot", "Bhatinda", "Srinagar", "Jammu", "Leh", "Gulmarg", "Katra", "Pahalgam", "Banihal", "Batote", "Bhaderwah", "Kupwara", "Qazigund", "Gorakhpur", "Varanasi", "Allahabad", "Lucknow", "Bareilly", "Jhansi", "Agra", "Meerut", "Moradabad", "Kanpur", "Aligarh", "Bahraich", "Sultanpur", "Kalpa", "Shimla", "Keylong", "Dharamsala", "Manali", "Chamba", "Kullu", "Solan", "Sundernagar" };
                int count = 0;
                for (int i = 0; i < stationID.Length; i++)
                {
                    try
                    {
                        string htmlCode = client.DownloadString("http://14.139.247.11/citywx/city_weather.php?id=" + stationID[i]);
                        File.WriteAllText(di + @"\WebPage2\" + stationID[i] + ".html", htmlCode);
                        HtmlDocument doc = new HtmlDocument();
                        doc.LoadHtml(htmlCode);
                        HtmlNodeCollection boldtext = doc.DocumentNode.SelectNodes(".//b");
                        DateTime webDate = DateTime.Parse(boldtext[1].InnerText.Substring(7, 12));
                        HtmlNodeCollection tables = doc.DocumentNode.SelectNodes("//table");
                        HtmlNodeCollection rows = tables[0].SelectNodes(".//tr");
                        HtmlNodeCollection col = rows[6].SelectNodes(".//td");
                        //sb.AppendLine(DateTime.Today + "," + stationName[i] + "," + col[1].InnerText.Trim() + "," + "3");

                        rainfallData.AppendLine(webDate.ToString("yyyy-MM-dd HH:mm:ss") + "," + stationName[i] + "," + col[1].InnerText.Trim());
                        count = count + 1;
                        Console.WriteLine(webDate.ToString("yyyy-MM-dd HH:mm:ss") + "," + stationName[i] + "," + col[1].InnerText.Trim());
                    }
                    catch (Exception error)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Problem in one station of 2nd Website, CityWX Earlier Page, Station: " + stationName[i] + " data not available. Error: " + error.Message);
                        Console.ResetColor();
                        continue;
                    }
                }
                logText.AppendLine("2nd Website, CityWX Earlier Page: http://202.54.31.7/citywx/city_weather.php, Downloaded station: = " + count);

            }
            catch (Exception error)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Problem in 2nd Website, CityWX Earlier Page: http://202.54.31.7/citywx/city_weather.php, cannot be accessed. Error: " + error.Message);
                Console.ResetColor();
                logText.AppendLine("Problem in 2nd Website, CityWX Earlier Page: http://202.54.31.7/citywx/city_weather.php, cannot be accessed. Error: " + error.Message);
            }

            ///-----------------------------------------------City Weather1 Data----------------------------------------------------------------
            try
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("3rd Website, CityWX updated Page: http://202.54.31.7/citywx/city_weather1.php");
                Directory.CreateDirectory(di + @"\WebPage3");
                Console.ResetColor();

                StringBuilder sb = new StringBuilder();
                WebClient client = new WebClient();
                string[] stationID = new string[] { "30002", "42220", "42314", "42309", "42308", "42423", "42415", "42527", "42410", "42406", "42516", "42515", "42623", "42619", "42724", "42726" };
                string[] stationName = new string[] { "Anni", "Passighat", "Dibrugarh", "North Lakhimpur", "Itanagar", "Jorhat", "Tezpur", "Kohima", "Guwahati", "Dhubri", "Shillong", "Cherrapunji", "Imphal", "Silchar", "Agartala", "Aizwal" };
                int count = 0;
                for (int i = 0; i < stationID.Length; i++)
                {
                    try
                    {
                        string htmlCode = client.DownloadString("http://14.139.247.11/citywx/city_weather.php?id=" + stationID[i]);
                        File.WriteAllText(di + @"\WebPage3\" + stationID[i] + ".html", htmlCode);
                        HtmlDocument doc = new HtmlDocument();
                        doc.LoadHtml(htmlCode);
                        HtmlNodeCollection boldtext = doc.DocumentNode.SelectNodes(".//b");
                        DateTime webDate = DateTime.Parse(boldtext[1].InnerText.Substring(7, 12));
                        HtmlNodeCollection tables = doc.DocumentNode.SelectNodes("//table");
                        HtmlNodeCollection rows = tables[0].SelectNodes(".//tr");
                        HtmlNodeCollection col = rows[6].SelectNodes(".//td");
                        rainfallData.AppendLine(webDate.ToString("yyyy-MM-dd HH:mm:ss") + "," + stationName[i] + "," + col[1].InnerText.Trim());
                        Console.WriteLine(webDate.ToString("yyyy-MM-dd HH:mm:ss") + "," + stationName[i] + "," + col[1].InnerText.Trim());
                        count = count + 1;
                    }
                    catch (Exception error)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Problem in one station of 3rd Website, CityWX updated Page, Station Name:  " + stationName[i] + " data not  available. Error: " + error.Message);
                        Console.ResetColor();
                        continue;
                    }
                }
                logText.AppendLine("3rd Website, CityWX updated Page: http://202.54.31.7/citywx/city_weather1.php, Downloaded station: = " + count);

            }
            catch (Exception error)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Problem in 3rd Website, CityWX updated Page: http://202.54.31.7/citywx/city_weather1.php" + error.Message);
                Console.ResetColor();
                logText.AppendLine("Problem 3rd Website, CityWX updated Page: http://202.54.31.7/citywx/city_weather1.php, cannot be accessed, Error: " + error.Message);
            }


            ////---------------------------------------------------------------------- W underground Stations ------------------------------------------------------------------------------------------------------
            try
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("4th Website, W-Underground Page: http://www.wunderground.com");
                Directory.CreateDirectory(di + @"\WebPage4");
                Console.ResetColor();

                WebClient client = new WebClient();
                string[] stationMetadata = File.ReadAllLines(iniDi + @"\NecessaryFiles\WundergroundStationID.txt");
                string[] stationNo = new string[stationMetadata.Length];
                string[] stationName = new string[stationMetadata.Length];

                for (int index = 0; index < stationMetadata.Length; index++)
                {
                    var textParse = stationMetadata[index].Split(',');
                    stationNo[index] = textParse[0];
                    stationName[index] = textParse[1];
                }

                for (int i = 0; i < stationNo.Length; i++)
                {
                    try
                    {
                        string htmlCode = client.DownloadString("http://www.wunderground.com/history/station/" + stationNo[i] + "/" + today.Year + "/" + today.Month + "/" + today.Day + "/MonthlyHistory.html");
                        File.WriteAllText(di + @"\WebPage4\" + stationName[i] + ".html", htmlCode);
                        HtmlDocument doc = new HtmlDocument();
                        doc.LoadHtml(htmlCode);
                        HtmlNodeCollection tables = doc.DocumentNode.SelectNodes("//table");
                        HtmlNodeCollection rows = tables[3].SelectNodes(".//tr");
                        HtmlNodeCollection col = rows[today.Day + 1].SelectNodes(".//td");

                        rainfallData.AppendLine(today.ToString("yyyy-MM-dd HH:mm:ss") + "," + stationName[i].Trim() + "," + (float.Parse(col[19].InnerText.Trim()) * 25.4).ToString("0.00"));
                        Console.WriteLine(today.ToString("yyyy-MM-dd HH:mm:ss") + "," + stationName[i].Trim() + "," + (float.Parse(col[19].InnerText.Trim()) * 25.4).ToString("0.00"));
                    }
                    catch (Exception error)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Problem in one station of 4th Website, W-Underground Page, Station Name: " + stationName[i] + " data not available. Error: " + error.Message);
                        Console.ResetColor();
                    }


                }
                logText.AppendLine("4th Website, W-Underground Page: http://www.wunderground.com, Downloaded Station= " + (stationNo.Length));
            }
            catch (Exception err)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Problem 4th Website, W-Underground Page: http://www.wunderground.com, cannot be accessed, Error: " + err.Message);
                Console.ResetColor();
                logText.AppendLine("Problem 4th Website, W-Underground Page: http://www.wunderground.com, cannot be accessed, Error: " + err.Message);
            }

            ///-----------------------------------------------MFD Data---------------------------------------------------------------------
            try
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("5th Website, Meteorological Forecasting Division of Nepal, : http://www.mfd.gov.np/");
                Directory.CreateDirectory(di + @"\WebPage5");
                Console.ResetColor();

                StringBuilder sb = new StringBuilder();
                WebClient client = new WebClient();
                string htmlCode = client.DownloadString("http://www.mfd.gov.np/");
                File.WriteAllText(di + @"\WebPage5\mfd.gov.np.html", htmlCode);
                HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(htmlCode);
                HtmlNodeCollection tables = doc.DocumentNode.SelectNodes("//table");
                HtmlNodeCollection header = doc.DocumentNode.SelectNodes(".//em");
                string hour = header[2].InnerText.Trim().Substring(11, 2);

                DateTime webDate = DateTime.Parse(header[2].InnerText.Trim().Substring(0, 10)).AddHours(double.Parse(hour) + 5 / 4);
                HtmlNodeCollection rows = tables[0].SelectNodes(".//tr");
                HtmlNodeCollection col;
                for (int i = 1; i < rows.Count - 1; i++)
                {

                    col = rows[i].SelectNodes(".//td");

                    rainfallData.AppendLine(webDate.ToString("yyyy-MM-dd HH:mm:ss") + "," + col[0].InnerText.Trim() + "," + col[3].InnerText.Trim());
                    Console.WriteLine(webDate.ToString("yyyy-MM-dd HH:mm:ss") + "," + col[0].InnerText.Trim() + "," + col[3].InnerText.Trim());
                }
                logText.AppendLine("5th Website, Meteorological Forecasting Division of Nepal, : http://www.mfd.gov.np/, Downloaded Station= " + (rows.Count));
            }
            catch (Exception error)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Problem in 5th Website, Meteorological Forecasting Division of Nepal, : http://www.mfd.gov.np/ cannot be accessed, Error: " + error.Message);
                Console.ResetColor();
                logText.AppendLine("Problem in 5th Website, Meteorological Forecasting Division of Nepal, : http://www.mfd.gov.np/ cannot be accessed, Error: " + error.Message);
            }

            ///-----------------------------------------------------FFWC Rainfall Data---------------------------------------------------------------
            try
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("6th Website, Flood Forecasting and Warning Center, Banglaseh: http://www.ffwc.gov.bd/");
                Directory.CreateDirectory(di + @"\WebPage6");
                Console.ResetColor();

                StringBuilder sb = new StringBuilder();
                WebClient client = new WebClient();
                string htmlCode = client.DownloadString("http://www.ffwc.gov.bd/ffwc_charts/rainfall.php");
                File.WriteAllText(di + @"\WebPage6\FFWCRainfall.html", htmlCode);
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(htmlCode);
                HtmlNodeCollection tables = doc.DocumentNode.SelectNodes("//table");
                HtmlNodeCollection rows = tables[0].SelectNodes(".//tr");
                HtmlNodeCollection col = rows[1].SelectNodes(".//td");
                //MessageBox.Show(col[2].InnerText.Trim());
                DateTime webDate = new DateTime(int.Parse(col[2].InnerText.Trim().Substring(6, 4)), int.Parse(col[2].InnerText.Trim().Substring(3, 2)), int.Parse(col[2].InnerText.Trim().Substring(0, 2)));

                for (int i = 0; i < rows.Count - 3; ++i)
                {
                    HtmlNodeCollection cols = rows[i + 3].SelectNodes(".//td");
                    if (cols.Count > 4 && cols[4].InnerText != "NP")
                    {
                        rainfallData.AppendLine(webDate.AddHours(9).ToString("yyyy-MM-dd HH:mm:ss") + "," + cols[0].InnerText.Trim() + "," + cols[4].InnerText);
                        Console.WriteLine(webDate.AddHours(9).ToString("yyyy-MM-dd HH:mm:ss") + "," + cols[0].InnerText.Trim() + "," + cols[4].InnerText);
                    }
                }
                logText.AppendLine("6th Website, Flood Forecasting and Warning Center, Banglaseh: http://www.ffwc.gov.bd/, Downloaded Station= " + (rows.Count - 3));
            }
            catch (Exception errtor)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Problem in 6th Website, Flood Forecasting and Warning Center, Banglaseh: http://www.ffwc.gov.bd/ rainfall data cannot be accessed, Error: " + errtor.Message);
                Console.ResetColor();
                logText.AppendLine("Problem in 6th Website, Flood Forecasting and Warning Center, Banglaseh: http://www.ffwc.gov.bd/cannot rainfall data be accessed, Error: " + errtor.Message);
            }

            ////----------------------------------------------------------------- FFWC Water Level Data -----------------------------------------------------------------------------------------
            try
            {
                StringBuilder sb = new StringBuilder();
                WebClient client = new WebClient();
                string htmlCode = client.DownloadString("http://www.ffwc.gov.bd/ffwc_charts/waterlevel.php");
                File.WriteAllText(di + @"\WebPage6\FFWCWaterlevel.html", htmlCode);
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(htmlCode);

                HtmlNodeCollection tables = doc.DocumentNode.SelectNodes("//table");
                HtmlNodeCollection rows = tables[0].SelectNodes(".//tr");
                HtmlNodeCollection col = rows[1].SelectNodes(".//td");
                if (col[4].InnerText.Trim() != DateTime.Today.Day.ToString("00") + "-" + DateTime.Today.Month.ToString("00"))
                {
                    Console.WriteLine("Water Level data of " + DateTime.Today.ToString("yyyy-MM-dd") + " has not been updated on the FFWC Website.");
                }

                else
                {

                    for (int i = 0; i < rows.Count - 3; ++i)
                    {
                        HtmlNodeCollection cols = rows[i + 3].SelectNodes(".//td");
                        if (cols.Count > 4 && cols[4].InnerText != "NP")
                        {
                            string value = DateTime.Today.ToString("yyyy-MM-dd HH:mm:ss") + "," + cols[1].InnerText + "," + cols[4].InnerText;
                            Console.WriteLine(value);
                            waterLevelData.AppendLine(value);
                        }
                    }
                }
            }
            catch (Exception error)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Problem in 6th Website, Flood Forecasting and Warning Center, Banglaseh: http://www.ffwc.gov.bd/ water level data cannot be accessed, Error: " + error.Message);
                Console.ResetColor();
                logText.AppendLine("Problem in 6th Website, Flood Forecasting and Warning Center, Banglaseh: http://www.ffwc.gov.bd/cannot water level data be accessed, Error: " + error.Message);
            }

            ///----------------------------------------------------------------Hydrology Nepal Data---------------------------------------------------------
            try
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("7th Website, Department of Hydrology and Meteorology, Nepal: http://hydrology.gov.np/");
                Directory.CreateDirectory(di + @"\WebPage7");
                Console.ResetColor();

                StringBuilder sb = new StringBuilder();
                WebClient client = new WebClient();
                string[] deviceID = new string[] { "37", "56", "71", "72", "70", "69", "68", "28", "25", "26", "27", "23", "57", "58", "59", "60", "61", "64", "62", "63", "29", "30", "31", "33", "34", "35", "36", "67", "66", "65", "38", "39", "47", "45", "46", "44", "42", "43", "40", "94", "4", "9", "19", "15", "13", "16", "18", "5", "8", "51", "10", "20", "6", "7", "11", "17", "49", "74", "75", "76", "77", "78", "79", "80", "82", "91", "97", "53" };
                string[] stationID = new string[] { "68", "74", "84", "86", "87", "88", "89", "53", "54", "55", "56", "57", "71", "72", "73", "75", "76", "77", "78", "79", "60", "61", "62", "64", "65", "66", "67", "81", "82", "83", "43", "44", "45", "46", "47", "48", "49", "51", "52", "107", "19", "20", "22", "23", "25", "26", "27", "28", "30", "31", "32", "33", "34", "35", "36", "90", "70", "91", "92", "93", "94", "95", "96", "97", "99", "104", "108", "69" };
                string[] stationName = new string[] { "Karnali At Chisapani", "Bheri At Samaijighat", "Jajarkot", "Dailekh", "Karnali At Asaraghat", "Seti At Dipayal", "Mangalsen", "Babai At Chepang", "Gularia", "Tulsipur", "Ghorahi", "Rampur-Kalimati", "Ranijaruwa", "Salyan Bazar", "Luwamjyula", "Tharmare", "Jyamire", "Padampur", "Ambapur", "Ratmata", "West Rapti At Kusum", "West Rapti At Bagasoti", "Mari At Nayagaon", "Nepalgunj", "Dhakeri", "Lamahi", "Bijuwartar", "Libang Gaon", "Sulichour", "Swargadwari", "Budhigandaki At Arughat", "Kaligandaki At Kumalgaon", "Trishuli At Betrawati", "Narayani At Narayanghat", "Jomsom", "Beni", "Danda", "Gorkha", "East Rapti At Rajaiya", "Ghalekharkha", "Bagmati At Khokana", "Marin Khola At Kusumtar", "Thankot", "Godavari", "Babarmahal", "Nagarkot", "Budhanilkantha", "Lele", "Sindhulimadi", "Sindhuligadhi", "Bagmati At Bhorleni", "Bagmati At Karmaiya", "Chisapanigadhi", "Daman", "Garuda", "Sundarijal", "Koshi At Chatara", "Tamor At Mulghat", "Dhankuta", "Jiri", "Tamakoshi At Busti", "Sunkoshi At Pachuwarghat", "Tumlingtar", "Arun At Turkeghat", "Okhaldhunga", "Bhote Koshi At Bahrabise", "Dudh Koshi At Rabuawabazar", "Kankai At Mainachuli" };


                string date = DateTime.Today.ToString("yyyy-MM-dd");

                for (int i = 0; i < deviceID.Length; i++)
                {
                    try
                    {
                        string htmlCode = client.DownloadString("http://hydrology.gov.np/new/bull3/index.php/hydrology/station/graph_view?deviceId=" + deviceID[i] + "&stationId=" + stationID[i] + "&categoryId=5&startDate=" + date + "&type=daily");
                        File.WriteAllText(di + @"\WebPage7\" + stationName[i].Trim() + ".html", htmlCode);
                        HtmlDocument doc = new HtmlDocument();
                        doc.LoadHtml(htmlCode);
                        HtmlNodeCollection tables = doc.DocumentNode.SelectNodes("//table");
                        HtmlNodeCollection rows = tables[2].SelectNodes(".//tr");
                        HtmlNodeCollection col = rows[today.Day - 2].SelectNodes(".//td");

                        rainfallData.AppendLine(DateTime.Parse(col[0].InnerText.Trim()).AddHours(24).ToString("yyyy-MM-dd HH:mm:ss") + "," + stationName[i].Trim() + "," + col[1].InnerText.Trim());
                        Console.WriteLine(DateTime.Parse(col[0].InnerText.Trim()).AddHours(24).ToString("yyyy-MM-dd HH:mm:ss") + "," + stationName[i].Trim() + "," + col[1].InnerText.Trim());

                    }
                    catch (FileNotFoundException error)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Problem in one station of 7th Website, Department of Hydrology and Meteorology, Nepal, " + stationName[i] + " data not available. Error: " + error.Message);
                        Console.ResetColor();
                        continue;
                    }
                }
                logText.AppendLine("7th Website, Department of Hydrology and Meteorology, Nepal: http://hydrology.gov.np/, Downloaded Station= " + (deviceID.Length));
            }
            catch (Exception error)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Problem in 7th Website, Department of Hydrology and Meteorology, Nepal: http://hydrology.gov.np/ cannot be accessed, Error: " + error.Message);
                Console.ResetColor();
                logText.AppendLine("Problem in 7th Website, Department of Hydrology and Meteorology, Nepal: http://hydrology.gov.np/ cannot be accessed, Error: " + error.Message);
            }

            ///--------------------------------------------Data from AMSS-Delhi Webpage------------------------------------------------------
            try
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("8th Website, Regional Meteorological Center, New Delhi: http://amssdelhi.gov.in/");
                Directory.CreateDirectory(di + @"\WebPage8");
                Console.ResetColor();

                WebClient client = new WebClient();
                string htmlCode = client.DownloadString("http://amssdelhi.gov.in/dynamic/weather/wxtable.html");
                File.WriteAllText(di + @"\WebPage8\amssdelhi.gov.in.html", htmlCode);
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(htmlCode);

                HtmlNodeCollection date = doc.DocumentNode.SelectNodes(".//b");
                string titleText = date[1].InnerText;
                var tableTitle = titleText.Split(' ');
                DateTime webDate = DateTime.Parse(tableTitle[8] + " " + tableTitle[9]).AddHours(9);
                HtmlNodeCollection tables = doc.DocumentNode.SelectNodes("//table");
                HtmlNodeCollection rows = tables[0].SelectNodes(".//tr");
                for (int i = 4; i < rows.Count - 4; i++)
                {
                    HtmlNodeCollection col = rows[i].SelectNodes(".//td");

                    rainfallData.AppendLine(webDate.ToString("yyyy-MM-dd HH:mm:ss") + "," + col[0].InnerText.Trim() + "," + col[7].InnerText.Trim());
                    Console.WriteLine(webDate.ToString("yyyy-MM-dd HH:mm:ss") + "," + col[0].InnerText.Trim() + "," + col[7].InnerText.Trim());
                }
                logText.AppendLine("8th Website, Regional Meteorological Center, New Delhi: http://amssdelhi.gov.in/, Downloaded Station= " + (rows.Count - 6));

            }
            catch (Exception error)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Problem in 8th Website, Regional Meteorological Center, New Delhi: http://amssdelhi.gov.in/ cannot be accessed, Error: " + error.Message);
                Console.ResetColor();
                logText.AppendLine("Problem in 8th Website, Regional Meteorological Center, New Delhi: http://amssdelhi.gov.in/ cannot be accessed, Error: " + error.Message);
            }

            ///----------------------------------------------------Delhi Region Table----------------------------------------------
            try
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("9th Website, Regional Meteorological Center, New Delhi, Dynamic: http://121.241.116.157/dynamic/weather/delhiregion.html");
                Directory.CreateDirectory(di + @"\WebPage9");
                Console.ResetColor();

                WebClient client = new WebClient();
                string htmlCode = client.DownloadString("http://121.241.116.157/dynamic/weather/delhiregion.html");
                File.WriteAllText(di + @"\WebPage9\delhiregion.html", htmlCode);
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(htmlCode);

                HtmlNodeCollection tables = doc.DocumentNode.SelectNodes("//table");
                HtmlNodeCollection bigfont = tables[0].SelectNodes(".//b");
                //DateTime webDate = new DateTime();

                string[] dateforecast = doc.GetElementbyId("wb_Text2").InnerText.Split(' ');
                DateTime webDate = DateTime.ParseExact(dateforecast[6], "dd/MM/yyyy", CultureInfo.InvariantCulture).AddHours(9);

                HtmlNodeCollection rows = tables[0].SelectNodes(".//tr");
                for (int i = 1; i < rows.Count - 1; i++)
                {
                    HtmlNodeCollection col = rows[i].SelectNodes(".//td");
                    string stationname = col[0].InnerText.Trim();
                    if (stationname.Length > 7 && stationname.Substring(0, 6) == "NEW   ")
                    {
                        stationname = "NEW DELHI (PALAM AP)";
                    }
                    else if (stationname.Length > 7 && stationname.Substring(0, 7) == "UDAIPUR")
                    {
                        stationname = "UDAIPUR AP";
                    }
                    else if (stationname == @"&nbsp; BHUNTAR AP")
                    {
                        stationname = "BHUNTAR AP";
                    }

                    rainfallData.AppendLine(webDate.ToString("yyyy-MM-dd HH:mm:ss") + "," + stationname.Trim() + "," + col[5].InnerText.Trim());
                    Console.WriteLine(webDate.ToString("yyyy-MM-dd HH:mm:ss") + "," + stationname.Trim() + "," + col[5].InnerText.Trim());

                }
                logText.AppendLine("9th Website, Regional Meteorological Center, New Delhi, Dynamic: http://121.241.116.157/dynamic/weather/delhiregion.html, Downloaded Station: " + (rows.Count - 2));

            }
            catch (Exception error)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Problem in 9th Website, Regional Meteorological Center, New Delhi, Dynamic: http://121.241.116.157/dynamic/weather/delhiregion.html cannot be accessed, Error: " + error.Message);
                Console.ResetColor();
                logText.AppendLine("Problem in 9th Website, Regional Meteorological Center, New Delhi, Dynamic: http://121.241.116.157/dynamic/weather/delhiregion.html cannot be accessed, Error: " + error.Message);
            }

            ///----------------------------------------------------------- IMD Sikkim PDF Data----------------------------------------------
            try
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("10th Website, Meteorological Centre, Gangtok-Sikkim: http://www.imdsikkim.gov.in/daily_Forecast.pdf");
                Directory.CreateDirectory(di + @"\WebPage10");
                Console.ResetColor();

                StringBuilder sb = new StringBuilder();
                WebClient client = new WebClient();
                client.DownloadFile("http://www.imdsikkim.gov.in/daily_Forecast.pdf", di + @"\WebPage10\imdsikkim.pdf");
                PdfReader reader = new PdfReader(@"http://www.imdsikkim.gov.in/daily_Forecast.pdf"); // Grabbing .pdf file
                sb.Append(PdfTextExtractor.GetTextFromPage(reader, 2));
                string text = sb.ToString(); // Convert to string collected from the pdf
                var tableText = text.Split('\n');
                DateTime date = new DateTime();
                for (int i = 0; i < tableText.Length; i++)
                {
                    if (tableText[i].Contains("OBSERVATION RECORDED AT "))
                    {
                        var dateText = tableText[i].Split(' ');
                        date = DateTime.ParseExact(dateText[dateText.Length - 2], "dd-MM-yyyy", CultureInfo.InvariantCulture).AddHours(9); // Date Time obtaining
                    }
                }
                int counter = 0;
                for (int i = 11; i < tableText.Length - 2; i++)
                {
                    string station;
                    string rain;
                    var lines = tableText[i].Split(' ');
                    if (lines.Length == 5)
                    {
                        counter = counter + 1;
                        station = lines[0].Trim();
                        rain = lines[lines.Length - 2];

                        Console.WriteLine(date.ToString() + "," + station + "," + rain);
                    }
                    else if (lines.Length == 6)
                    {
                        counter = counter + 1;
                        station = lines[0].Trim() + " " + lines[1].Trim();
                        rain = lines[lines.Length - 2];
                        rainfallData.AppendLine(date.ToString("yyyy-MM-dd HH:mm:ss") + "," + station + "," + rain);
                        Console.WriteLine(date.ToString("yyyy-MM-dd HH:mm:ss") + "," + station + "," + rain);
                    }
                }
                logText.AppendLine("10th Website, Regional Meteorological Centre, Gangtok-Sikkim: http://www.imdsikkim.gov.in/daily_Forecast.pdf, Downloaded Station: " + counter);
            }

            catch (Exception error)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Problem in 10th Website, Meteorological Centre, Gangtok-Sikkim: http://www.imdsikkim.gov.in/daily_Forecast.pdf  cannot be accessed, Error: " + error.Message);
                Console.ResetColor();
                logText.AppendLine("Problem in 10th Website, Meteorological Centre, Gangtok-Sikkim: http://www.imdsikkim.gov.in/daily_Forecast.pdf  cannot be accessed, Error: " + error.Message);
            }

            ///---------------------------------------------------------- Weather Delhi PDF --------------------------------------------
            try
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("11th Website, Weather Report for NCR Delhi: http://121.241.116.157/dynamic/weather/Delhi.pdf");
                Directory.CreateDirectory(di + @"\WebPage11");
                Console.ResetColor();

                StringBuilder sb = new StringBuilder();
                WebClient client = new WebClient();
                client.DownloadFile("http://121.241.116.157/dynamic/weather/Delhi.pdf", di + @"\WebPage11\ncrdelhi.pdf");
                PdfReader reader = new PdfReader(@"http://121.241.116.157/dynamic/weather/Delhi.pdf");
                sb.Append(PdfTextExtractor.GetTextFromPage(reader, 1));
                string text = sb.ToString();
                string[] delimiters = new string[] { "\n", " ", ":" };
                var splittedText = text.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
                sb.Clear();

                List<string> stringlist = new List<string>(splittedText);
                stringlist.Remove("Sports");
                stringlist.Remove("Cplx");
                stringlist.Remove("Univ");
                stringlist.Remove("NOIDA");
                stringlist.Remove("Road");
                //--------------------------
                stringlist.Remove("TRTTRACE");
                //stringlist[stringlist.IndexOf("N")] = "Narela";
                stringlist.Remove("arela");
                stringlist.Remove("TRACE00.3T");
                stringlist.Remove("RACERACE");
                stringlist.Remove("00000.70TTT");
                stringlist.Remove("004.0**");
                stringlist.Remove("ungeshpur");
                //stringlist[stringlist.IndexOf("M")] = "Mungeshpur";
                //------------------------
                stringlist[stringlist.IndexOf("Yamuna")] = "Yamuna Sports Cplx";
                stringlist[stringlist.IndexOf("Delhi")] = "Delhi University";
                stringlist[stringlist.IndexOf("NCMRWF")] = "NCMRWF NOIDA";
                stringlist[stringlist.IndexOf("Lodhi")] = "Lodhi Road";
                string[] textDelhi = stringlist.ToArray();
                Console.WriteLine(textDelhi[18]);
                DateTime date = DateTime.ParseExact(textDelhi[18], "dd.MM.yyyy", System.Globalization.CultureInfo.InvariantCulture).AddHours(9);

                int counter = 0;
                int x = 0;
                for (int i = 26; i < textDelhi.Length; i++)
                {
                    if (textDelhi[i].Trim() != "Max" && textDelhi[i].Trim() != "R/F" && textDelhi[i].Trim() != "**" && textDelhi[i].Trim() != "Min" && textDelhi[i].Trim() != "trace" && textDelhi[i].Trim() != "TRACE" && textDelhi[i].Trim() != "Trace")
                    {
                        try
                        {
                            float foul = float.Parse(textDelhi[i].Trim());
                            continue;
                        }
                        catch (FormatException)
                        {

                            for (int j = x; j < textDelhi.Length; j++)
                            {
                                if (textDelhi[j].Trim() == "R/F")
                                {
                                    counter = counter + 1;
                                    rainfallData.AppendLine(date.ToString("yyyy-MM-dd HH:mm:ss") + "," + textDelhi[i].Trim() + "," + textDelhi[j + 1]);
                                    Console.WriteLine(date.ToString("yyyy-MM-dd HH:mm:ss") + "," + textDelhi[i].Trim() + "," + textDelhi[j + 1]);
                                    x = j + 1;
                                    break;
                                }
                            }

                            continue;
                        }
                    }
                    else
                    {
                        continue;
                    }
                }
                logText.AppendLine("11th Website, Weather Report for NCR Delhi: http://121.241.116.157/dynamic/weather/Delhi.pdf, Downloaded Station: " + counter);
            }
            catch (Exception error)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Problem in 11th Website, Weather Report for NCR Delhi: http://121.241.116.157/dynamic/weather/Delhi.pdf cannot be accessed, Error: " + error.Message);
                Console.ResetColor();
                logText.AppendLine("Problem in 11th Website, Weather Report for NCR Delhi: http://121.241.116.157/dynamic/weather/Delhi.pdf cannot be accessed, Error: " + error.Message);
            }


            ////--------------------------------------------------------- Guwahati PDF File ----------------------------------------------------------------------------------------------------

            try
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("12th Website, Guwahati PDF: http://www.imdguwahati.gov.in/dwr.pdf");
                Directory.CreateDirectory(di + @"\WebPage12");
                Console.ResetColor();

                StringBuilder sb = new StringBuilder();
                WebClient client = new WebClient();
                client.DownloadFile("http://www.imdguwahati.gov.in/dwr.pdf", di + @"\WebPage12\guwahati.pdf");
                PdfReader reader = new PdfReader("http://www.imdguwahati.gov.in/dwr.pdf"); //grabbing .pdf file using itextshap.pdf
                sb.Append(PdfTextExtractor.GetTextFromPage(reader, 1));  //getting all texts from .pdf file page number 1
                string text = sb.ToString(); //Extracting texts from stringbuilder
                var lines = text.Split(':');
                var spacedtext = lines[0].Split(' ');
                DateTime date = new DateTime();

                // Obtaining Date Time that was written in the PDF
                try
                {
                    if (spacedtext[22].Length == 9)
                    {
                        date = DateTime.ParseExact(spacedtext[22].Trim(), "d-MMM-yy", System.Globalization.CultureInfo.InvariantCulture).AddHours(9);
                    }

                    else if (spacedtext[22].Length >= 10)
                    {
                        date = DateTime.ParseExact(spacedtext[22].Trim().Substring(0, 9), "d-MMM-yy", System.Globalization.CultureInfo.InvariantCulture).AddHours(9);
                    }
                }
                catch (FormatException)
                {
                    date = DateTime.ParseExact(spacedtext[22].Trim().Substring(0, 8), "d-MMM-yy", System.Globalization.CultureInfo.InvariantCulture).AddHours(9);
                }

                char[] dispertext = new char[] { '=', ',', '&' };
                var stations = lines[1].Split(dispertext);
                sb.Clear();

                List<string> obtainedSt = new List<string>();
                List<string> obtainedRF = new List<string>();

                for (int i = 0; i < stations.Length - 1; i++)
                {
                    for (int j = i; j < stations.Length - 1; j++)
                    {
                        try
                        {
                            var values = stations[j].Trim().Split(' ');
                            int rain = int.Parse(values[0].Trim());
                            if (j == i || stations[i] == "")
                            {
                                break;
                            }
                            else
                            {
                                obtainedSt.Add(stations[i].Trim());
                                obtainedRF.Add((rain * 10).ToString());  //obatining rf value and station name by comparing with the existing station Name
                                break;
                            }
                        }
                        catch (FormatException)
                        {
                            continue;
                        }
                    }
                }

                for (int i = 0; i < obtainedRF.Count; i++)
                {
                    rainfallData.AppendLine((date.ToString("yyyy-MM-dd HH:mm:ss") + "," + obtainedSt[i].Trim() + "," + obtainedRF[i]));
                    Console.WriteLine(date.ToString("yyyy-MM-dd HH:mm:ss") + "," + obtainedSt[i].Trim() + "," + obtainedRF[i]);
                }
                logText.AppendLine("12th Website, Guwahati PDF: http://www.imdguwahati.gov.in/dwr.pdf, Downloaded Station: " + obtainedSt.Count);
            }
            catch (Exception error)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Problem in 12th Website, Guwahati PDF: http://www.imdguwahati.gov.in/dwr.pdf cannot be accessed, Error: " + error.Message);
                Console.ResetColor();
                logText.AppendLine("Problem in 12th Website, Guwahati PDF: http://www.imdguwahati.gov.in/dwr.pdf cannot be accessed, Error: " + error.Message);
            }


            ///---------------------------------------------------------------------Bangladesh Meteorological Board data--------------------------------------------------------------
            try
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("13th Website, Bangladesh Meterological Department: http://www.bmd.gov.bd/");
                Directory.CreateDirectory(di + @"\WebPage13");
                Console.ResetColor();

                StringBuilder sb = new StringBuilder();
                WebClient client = new WebClient();
                int[] stationIndex = new int[] { 42, 45, 57, 64, 65, 67, 68, 69, 70, 71, 72, 73, 74, 75, 76, 18, 58, 59, 61, 62, 63, 25, 78, 79, 80, 81, 83, 84, 85, 86, 87, 89, 90, 91, 92, 93, 95, 96, 97 };
                string[] stationName = new string[] { "Sandwip", "Sitakundu", "Tangail", "Rangamati", "Comilla", "Chandpur", "Maijdi Court", "Feni", "Hatiya", "Cox Bazar", "Kutubdia", "Teknaf", "Saint Martin", "Dighinala", "Bandarban", "Mymensingh", "Faridpur", "Madaripur", "Gopalganj", "Netrokona", "Nikli", "Srimangal", "Ishurdi", "Bogra", "Badalgachhi", "Tarash", "Dinajpur", "Sayedpur", "Rajarhat", "Dimla", "Tetulia", "Mongla", "Satkhira", "Jessore", "Chuadanga", "Kumarkhali", "Patuakhali", "Khepupara", "Bhola" };


                List<DateTime> stationDate = new List<DateTime>();
                List<float> stationRF = new List<float>();

                int counter = 0;
                for (int j = 0; j < stationIndex.Length; j++)
                {
                    try
                    {
                        string htmlCode = client.DownloadString("http://www.bmd.gov.bd/?/wchart/=" + stationIndex[j]);
                        File.WriteAllText(di + @"\WebPage13\" + stationName[j] + ".html", htmlCode);
                        HtmlDocument doc = new HtmlDocument();
                        doc.LoadHtml(htmlCode);
                        HtmlNodeCollection tables = doc.DocumentNode.SelectNodes("//script");
                        string[] text = tables[4].InnerText.Split('[', ']');
                        var dataText = text[2].Split(new string[] { "{", "}" }, StringSplitOptions.RemoveEmptyEntries);
                        for (int i = 0; i < dataText.Length; i++)
                        {
                            if (dataText[i].Length >= 2 && dataText[i].Substring(0, 2) != "  " && dataText[i].Substring(0, 1) != "\t")
                            {
                                var lastStepText = dataText[i].Split(',', ':');
                                stationDate.Add(DateTime.Parse(lastStepText[3].Trim().Substring(1, lastStepText[3].Trim().Length - 2)).AddHours(9));
                                stationRF.Add(float.Parse(lastStepText[1].Trim()));
                                counter = counter + 1;
                            }
                        }
                        if (stationDate.Contains(DateTime.Today.AddHours(9)) != true)
                        {
                            stationDate.Add(DateTime.Today.AddHours(9));
                            stationRF.Add(0f);
                        }
                        for (int i = 0; i < stationDate.Count; i++)
                        {
                            Console.WriteLine(stationDate[i].ToString("yyyy-MM-dd HH:mm:ss") + "," + stationName[j] + "-BMD" + "," + stationRF[i]);
                            rainfallData.AppendLine(stationDate[i].ToString("yyyy-MM-dd HH:mm:ss") + "," + stationName[j] + "-BMD" + "," + stationRF[i]);
                        }
                        stationDate.Clear();
                        stationRF.Clear();
                    }
                    catch (Exception eroor)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Problem in one of the stations of 13th Website, Bangladesh Meterological Department, Error: " + stationName[j] + " data not available. Error: " + eroor.Message);
                        Console.ResetColor();
                        continue;
                    }
                }
                logText.AppendLine("13th Website, Bangladesh Meterological Department: http://www.bmd.gov.bd/, Downloaded Station: " + stationIndex.Length);
            }
            catch (Exception error)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Problem in 13th Website, Bangladesh Meterological Department: http://www.bmd.gov.bd/ cannot be accessed, Error: " + error.Message);
                Console.ResetColor();
                logText.AppendLine("Problem in 13th Website, Bangladesh Meterological Department: http://www.bmd.gov.bd/ cannot be accessed, Error: " + error.Message);
            }

            ///------------------------------------------------Central Water Commission(CWC) Data Download----------------------------------------------/////
            try
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("14th Website, Central Water Commission, India: http://www.cwc.gov.in/");
                Directory.CreateDirectory(di + @"\WebPage14");
                Console.ResetColor();
                string[] stationCode = File.ReadAllLines(iniDi + @"\NecessaryFiles\CWCStationID.txt");
                foreach (string element in stationCode)
                {
                    try
                    {
                        WebRequest req = WebRequest.Create("http://www.india-water.gov.in/ffs/data-flow-list-based/flood-forecasted-site/");
                        string postData = "lstStation=" + element;

                        byte[] send = Encoding.Default.GetBytes(postData);
                        req.Method = "POST";
                        req.ContentType = "application/x-www-form-urlencoded";
                        req.ContentLength = send.Length;

                        Stream sout = req.GetRequestStream();
                        sout.Write(send, 0, send.Length);
                        sout.Flush();
                        sout.Close();

                        WebResponse res = req.GetResponse();
                        StreamReader sr = new StreamReader(res.GetResponseStream());
                        string returnvalue = sr.ReadToEnd();
                        File.WriteAllText(di + @"\WebPage14\" + element + ".html", returnvalue);
                        HtmlDocument doc = new HtmlDocument();
                        doc.LoadHtml(returnvalue);
                        HtmlNode header = doc.DocumentNode.SelectSingleNode("//h4");
                        string stationName = header.InnerText.Replace("Site Name : ", "").Trim() + "-CWC";

                        HtmlNodeCollection tables = doc.DocumentNode.SelectNodes("//table");
                        HtmlNodeCollection rows = tables[1].SelectNodes(".//tr");
                        HtmlNodeCollection colWL = rows[1].SelectNodes(".//td");
                        HtmlNodeCollection colRF = rows[3].SelectNodes(".//td");
                        if (colWL[0].InnerText.Trim() != "")
                        {
                            DateTime dateWL = DateTime.ParseExact(colWL[0].InnerText.Replace("Date: ", "").Trim(), "dd-MM-yyyy HH:mm", CultureInfo.InvariantCulture).AddHours(0.5);
                            float valueWL = float.Parse(colWL[1].InnerText.Replace("Value: ", "").Replace(" Meters (m)", ""));
                            //sb.AppendLine("WL-" + stationName + "," + dateWL.ToString("yyyy-MM-dd HH:mm:ss") + "," + valueWL);
                            Console.WriteLine(dateWL.ToString("yyyy-MM-dd HH:mm:ss") + "," + stationName + "," + "WL-" + valueWL);
                            waterLevelData.AppendLine(dateWL.ToString("yyyy-MM-dd HH:mm:ss") + "," + stationName + "," + valueWL);


                        }

                        if (colRF[0].InnerText.Trim() != "NOT AVAILABLE")
                        {
                            DateTime dateRF = DateTime.ParseExact(colRF[0].InnerText.Replace("Date: ", ""), "dd-MM-yyyy HH:mm", CultureInfo.InvariantCulture).AddHours(0.5);
                            float valueRF = float.Parse(colRF[1].InnerText.Replace("Value: ", "").Replace(" Milimiters (mm)", ""));
                            Console.WriteLine(dateRF.ToString("yyyy-MM-dd HH:mm:ss") + "," + stationName + "," + valueRF);
                            rainfallData.AppendLine(dateRF.ToString("yyyy-MM-dd HH:mm:ss") + "," + stationName + "," + valueRF);
                        }
                    }
                    catch (Exception error)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Problem in one of the stations of 14th Website, Central Water Commission, India, Station: " + element + " data not available. Error: " + error.Message);
                        Console.ResetColor();
                    }
                }
                logText.AppendLine("14th Website, Central Water Commission, India: http://www.cwc.gov.in/, Downloaded Station: " + stationCode.Length);
            }
            catch (Exception error)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Problem in 14th Website, Central Water Commission, India: http://www.cwc.gov.in/ cannot be accessed, Error: " + error.Message);
                Console.ResetColor();
                logText.AppendLine("Problem in 14th Website, Central Water Commission, India: http://www.cwc.gov.in/ cannot be accessed, Error: " + error.Message);
            }
       
            
            ///------------------------------------------------  Pakistan Meteorological Department Data Download  ----------------------------------------------/////
            try
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("15th Website, Pakistan Meteorological Department, Pakistan: http://www.ffd.pmd.gov.pk/ffd_rainfall/");
                Directory.CreateDirectory(di + @"\WebPage15");
                Console.ResetColor();
                string datestr = today.ToString("MMMyy").ToLower();

                WebClient client = new WebClient();
                Console.WriteLine("http://www.ffd.pmd.gov.pk/ffd_rainfall/rainfall" + datestr + ".htm");
                string htmlCode = client.DownloadString("http://www.ffd.pmd.gov.pk/ffd_rainfall/rainfall" +datestr + ".htm");
                File.WriteAllText(di + @"\WebPage15\rainfall" + datestr + ".htm", htmlCode);
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(htmlCode);

                int webDate = today.Day;
                Console.WriteLine(webDate);
                HtmlNodeCollection tables = doc.DocumentNode.SelectNodes("//table");
                HtmlNodeCollection rows = tables[0].SelectNodes(".//tr");
                
                for (int i = 3; i < rows.Count-1; i++)
                {
                    HtmlNodeCollection col = rows[i].SelectNodes(".//td");
                    if (col.Count()>38 && col[1].InnerText.Trim() != "&nbsp;")
                    {
                        rainfallData.AppendLine(today.ToString("yyyy-MM-dd HH:mm:ss") + "," + col[1].InnerText.Trim().Replace(",", " ") + "," + col[webDate + 1].InnerText.Trim());
                        Console.WriteLine(today.ToString("yyyy-MM-dd HH:mm:ss") + "," + col[1].InnerText.Trim().Replace(",", " ") + "," + col[webDate + 1].InnerText.Trim());
                    }

                }
                logText.AppendLine("15th Website, Pakistan Meteorological Department, Pakistan: http://www.ffd.pmd.gov.pk/, Downloaded Station= " + (rows.Count - 4));
            }
            catch (Exception error)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Problem in 15th Website, Pakistan Meteorological Department, Pakistan: http://www.ffd.pmd.gov.pk/. Error: " + error);
                Console.ResetColor();
            }

            File.WriteAllText(di + @"\Rainfall_" + hiphenDate + ".txt", rainfallData.ToString());
            File.WriteAllText(iniDi + @"\RawWL\WaterLevel_" + hiphenDate + ".txt", waterLevelData.ToString());
            logText.Append("All station data downloaded and saved successfully.");
            File.WriteAllText(iniDi + @"\Logfiles\DownloadStatus_" + hiphenDate + ".txt", logText.ToString());

            Process.Start(@"C:\Users\nbiswas\Desktop\Nishan\SASWE\AutoCorrection\Programs\ExecutionFiles\PrecipitationCorrection.exe");
        }
        
    }
}

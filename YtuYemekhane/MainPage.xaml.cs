/*
 Copyright (C) 2014 TowerLabs
 
 This program is free software; you can redistribute it and/or
 modify it under the terms of the GNU General Public License
 as published by the Free Software Foundation; either version 2
 of the License, or (at your option) any later version.
 
 This program is distributed in the hope that it will be useful,
 but WITHOUT ANY WARRANTY; without even the implied warranty of
 MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 GNU General Public License for more details.
 
 You should have received a copy of the GNU General Public License
 along with this program; if not, write to the
 Free Software Foundation, Inc., 51 Franklin Street,
 Fifth Floor, Boston, MA  02110-1301, USA.
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using System.Net.Http;
using System.Threading.Tasks;
using YtuYemekhane.Model;
using Windows.Storage;
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using Windows.UI.ViewManagement;

namespace YtuYemekhane
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Required; 
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            // Hide statusbar
            await StatusBar.GetForCurrentView().HideAsync();

            // get localfolder
            StorageFolder local = Windows.Storage.ApplicationData.Current.LocalFolder;

            // get today time
            DateTime date = DateTime.Today;

            String currentDate = date.ToString("MM/yyyy");
            currentDate = currentDate.Replace("/", "");
            var result = await local.GetItemsAsync();
            var menu = result.FirstOrDefault(x => x.Name == currentDate);
            if (menu == null)
            {
                var list = await getFoodList();
                await WriteToFile(local, list, currentDate);
                printToScreenMenuList(list, date.ToString("dd/MM/yyyy").Replace(".","/"));
            }
            else
            {
                String json = await ReadFile(local, currentDate);
                printToScreenMenuList(json, date.ToString("dd/MM/yyyy").Replace(".", "/"));
            }

            
        }
        private void printToScreenMenuList(String json,String date)
        {
            ObservableCollection<Menu> menuList = new ObservableCollection<Menu>();

            int result;
            String menuKey;

            // Day of month
            date = date.Substring(0, 2);

            // parse as Json array 
            var objects = JArray.Parse(json);  
            Regex reg = new Regex("[&#13,;]");

            foreach (JObject root in objects)
            {
                foreach (KeyValuePair<String, JToken> menu in root)
                {
                    //menuKey = menu.Key.ToString().Replace(".", "/");
                    menuKey = menu.Key.ToString().Substring(0, 2);
                    //int result = DateTime.Compare(DateTime.Parse(menuKey), DateTime.Parse(date));
                    result = Convert.ToInt32(menuKey) - Convert.ToInt32(date);

                    if(result>=0)
                    {
                        menuList.Add(new Menu
                        {
                            Date = menu.Key.ToString(),
                            main_dinner = reg.Replace(menu.Value["main_dinner"].ToString(),""),
                            main_lunch = reg.Replace(menu.Value["main_lunch"].ToString(),""),
                            alt_dinner = reg.Replace(menu.Value["alt_dinner"].ToString(),""),
                            alt_lunch = reg.Replace(menu.Value["alt_lunch"].ToString(),""),
                        });
                    }
                }
            }
            if(menuList.Count==0)
            {
                NoMenu.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }else
            {
                // List menu
                Menus.ItemsSource = menuList;
            }
        }
        private async Task<String> getFoodList()
        {
            List<Menu> menus = new List<Menu>();
            HttpClient client = new HttpClient(new HttpClientHandler());

            String json = await client.GetStringAsync("HERE Api Token");

            return json;

        }
        

        private async Task WriteToFile(StorageFolder local,String json, String date)
        {
            // Json to byteArray
            byte[] fileBytes = System.Text.Encoding.UTF8.GetBytes(json);

            // Create a file which name is date
            var file = await local.CreateFileAsync(date,
                 CreationCollisionOption.ReplaceExisting);

            // Write the data from fileBytes
            using (var s = await file.OpenStreamForWriteAsync())
            {
                s.Write(fileBytes, 0, fileBytes.Length);
            }
        }
        private async Task<String> ReadFile(StorageFolder local,String date)
        {
            String json;

            if (local != null)
            {

                // Get the file.
                var file = await local.OpenStreamForReadAsync(date);

                // Read the data.
                using (StreamReader streamReader = new StreamReader(file))
                {
                    json = streamReader.ReadToEnd();
                }
                return json;
            }
            return null;
        }

        private void ShowAboutSection_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            AboutSection.Visibility = Windows.UI.Xaml.Visibility.Visible;
            MenuList.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            PageTitle.Text = "Hakkında";
        }

        private void ShowMenuSection_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            AboutSection.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            MenuList.Visibility = Windows.UI.Xaml.Visibility.Visible;
            PageTitle.Text = "Yemek Listesi";
        }

        private void tTowerLabsLink_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            // Open website of towerlabs
            Uri uri = new Uri("http://www.towerlabs.co");
            Windows.System.Launcher.LaunchUriAsync(uri);
        }

        private void tProfil_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            // Open website of developer
            Uri uri = new Uri("http://www.twitter.com/SezginEge");
            Windows.System.Launcher.LaunchUriAsync(uri);
        }
    }
}
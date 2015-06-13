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
using Newtonsoft.Json;
using Windows.UI.Xaml;
using Windows.UI.Core;
using System.Net.NetworkInformation;
using Windows.UI.Popups;

namespace YtuYemekhane
{
    public sealed partial class MainPage : Page
    {
        private static StorageFolder LOCALFOLDER = Windows.Storage.ApplicationData.Current.LocalFolder;
        private DateTime date = DateTime.Today;
        private string currentDate;
        private List<Menu> menu = new List<Menu>();
        public MainPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;
        }

        private async Task Initialize()
        {
            Window.Current.Dispatcher.RunAsync(CoreDispatcherPriority.Low, async () =>
            {
                await StatusBar.GetForCurrentView().HideAsync();
                currentDate = date.ToString("MM/yyyy").Replace("/", "");

                var isExist = await IsLocalMenuExist();
                if (isExist)
                {
                    menu = await ReadMenuFromFileAsync(currentDate);
                }
                else
                {
                    var json = await GetMenuJsonFromApiAsync();
                    if (json != null)
                    {
                        menu = ConvertJsonToMenuList(json, currentDate);
                        WriteMenuJsonToLocalFileAsync(json, currentDate);
                    }
                }
                PrintMenuListToScreen(menu, date.ToString("dd/MM/yyyy").Replace(".", "/"));
                progress.IsActive = false;
            });
        }

        private async Task<string> GetMenuJsonFromApiAsync()
        {
            if (NetworkInterface.GetIsNetworkAvailable())
            {
                HttpClient client = new HttpClient(new HttpClientHandler());
                return await client.GetStringAsync(App.APIURL);
            }
            else
            {
                ShowInternetConnectionProblemDialog();
                return null;
            }
        }

        private void ShowInternetConnectionProblemDialog()
        {
            MessageDialog msg = new MessageDialog("Yemek listesinin indirilebilmesi için internet bağlantısı gerekmektedir.", "İnternet Erişim Problemi");

            UICommand okButton = new UICommand("OK");
            okButton.Invoked = OkButton_Click;
            msg.Commands.Add(okButton);

            msg.ShowAsync();
        }

        private void OkButton_Click(IUICommand command)
        {
            App.Current.Exit();            
        }

        private async Task<bool> IsLocalMenuExist()
        {
            var result = await LOCALFOLDER.GetItemsAsync();
            var menuFile = result.FirstOrDefault(x => x.Name == currentDate);
            return menuFile != null;
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            Initialize();
        }

        private void PrintMenuListToScreen(List<Menu> menu, String date)
        {
            if (menu.Count == 0)
            {
                NoMenu.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }
            else
            {
                Menus.ItemsSource = menu;
            }
        }

        private async Task WriteMenuJsonToLocalFileAsync(string json, String date)
        {
            var menu = ConvertJsonToMenuList(json, currentDate);

            // Json to byteArray
            byte[] fileBytes = System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(menu));

            // Create a file which name is date
            var file = await LOCALFOLDER.CreateFileAsync(date,
                 CreationCollisionOption.ReplaceExisting);

            // Write the data from fileBytes
            using (var s = await file.OpenStreamForWriteAsync())
            {
                s.Write(fileBytes, 0, fileBytes.Length);
            }
        }
        private async Task<List<Menu>> ReadMenuFromFileAsync(String date)
        {
            String json;

            // Get the file.
            var file = await LOCALFOLDER.OpenStreamForReadAsync(date);

            // Read the data.
            using (StreamReader streamReader = new StreamReader(file))
            {
                json = streamReader.ReadToEnd();
            }
            return JsonConvert.DeserializeObject<List<Menu>>(json);

        }

        private List<Menu> ConvertJsonToMenuList(string json, string date)
        {
            List<Menu> menuList = new List<Menu>();

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
                    menuKey = menu.Key.ToString().Substring(0, 2);
                    result = Convert.ToInt32(menuKey) - Convert.ToInt32(date);

                    if (result >= 0)
                    {
                        menuList.Add(new Menu
                        {
                            Date = menu.Key.ToString(),
                            MainDinner = reg.Replace(menu.Value["main_dinner"].ToString(), ""),
                            MainLunch = reg.Replace(menu.Value["main_lunch"].ToString(), ""),
                            AltDinner = reg.Replace(menu.Value["alt_dinner"].ToString(), ""),
                            AltLunch = reg.Replace(menu.Value["alt_lunch"].ToString(), ""),
                        });
                    }
                }
            }
            return menuList;
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

        private void TowerLabsLink_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            // Open website of towerlabs
            Uri uri = new Uri("http://www.towerlabs.co");
            Windows.System.Launcher.LaunchUriAsync(uri);
        }

        private void Profil_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            // Open website of developer
            Uri uri = new Uri("http://www.twitter.com/SezginEge");
            Windows.System.Launcher.LaunchUriAsync(uri);
        }
    }
}
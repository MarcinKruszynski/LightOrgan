﻿using LightOrganApp.Model;
using LightOrganApp.Services;
using System.Collections.Generic;
using Xamarin.Forms;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using System.Threading.Tasks;
using LightOrganApp.Resx;

namespace LightOrganApp
{
    public partial class FileListPage : ContentPage
    {
        private string searchText = null;

        List<MediaItem> allMediaItems;  
               

        public FileListPage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            var hasPermission = await CheckPermission();

            if (hasPermission)
            {
                allMediaItems = await DependencyService.Get<IMusicService>().GetItemsAsync();                  
            }
            else
            {
                allMediaItems = new List<MediaItem>();

                await DisplayAlert(AppResources.Permissions, string.Format(AppResources.PermissionDeniedMsg, GetPermissionName()), "OK");                
            }

            SearchFiles();
        }

        private async Task<bool> CheckPermission()
        {
            var status = PermissionStatus.Denied;

            if (Device.OS == TargetPlatform.Android)
               status = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Storage);
            else if (Device.OS == TargetPlatform.iOS)
               status = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Media);

            return status == PermissionStatus.Granted;
        }

        private string GetPermissionName()
        {
            if (Device.OS == TargetPlatform.Android)
                return "READ_EXTERNAL_STORAGE";
            else if (Device.OS == TargetPlatform.iOS)
                return "Media library access";

            return string.Empty;
        }

        private List<MediaItem> Filter(List<MediaItem> items, string query)
        {
            if (string.IsNullOrEmpty(query))
                return items;

            query = query.Trim().ToLower();

            var filteredList = new List<MediaItem>();
            foreach (var item in items)
            {
                var text1 = item.Title.ToString().Trim().ToLower();
                var text2 = item.Artist.ToString().Trim().ToLower();
                if (text1.Contains(query) || text2.Contains(query))
                {
                    filteredList.Add(item);
                }
            }
            return filteredList;
        }

        private void SearchFiles()
        {
            var filteredList = Filter(allMediaItems, searchText);
            listView.ItemsSource = filteredList;
        }

        private async void listView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            var mainPage = new MainPage();

            await Navigation.PushAsync(mainPage);
        }

        private void listView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            listView.SelectedItem = null;
        }

        private void SearchBar_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            searchText = e.NewTextValue;

            SearchFiles();
        }       
    }
}

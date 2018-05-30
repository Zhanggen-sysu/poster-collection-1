﻿using PosterCollection.Models;
using PosterCollection.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.Serialization.Json;
using System.Text;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace PosterCollection
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class CollectorItems : Page
    {
        private ViewModel viewModel;
        private Star selectedItem;
        public CollectorItems()
        {
            this.InitializeComponent();
            viewModel = ViewModel.Instance;
        }

        private async void Starlist_ItemClick(object sender, ItemClickEventArgs e)
        {
            var item = (Star)e.ClickedItem;
            if(item.type == 0)
            {
                String url = String.Format("https://api.themoviedb.org/3/movie/{0}?api_key=7888f0042a366f63289ff571b68b7ce0&append_to_response=casts", item.id);
                HttpClient client = new HttpClient();
                String Jresult = await client.GetStringAsync(url);
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(MovieDetail));
                MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(Jresult));
                viewModel.TheMovieDetail = (MovieDetail)serializer.ReadObject(ms);
                if (viewModel.TheMovieDetail.backdrop_path != null)
                {
                    viewModel.TheMovieDetail.backdrop_path = "https://image.tmdb.org/t/p/original" + viewModel.TheMovieDetail.backdrop_path;
                }
                else
                {
                    viewModel.TheMovieDetail.backdrop_path = "Assets/defaultBackground.png";
                }
                if (viewModel.TheMovieDetail.poster_path != null)
                {
                    viewModel.TheMovieDetail.poster_path = "https://image.tmdb.org/t/p/w500" + viewModel.TheMovieDetail.poster_path;
                }
                else
                {
                    viewModel.TheMovieDetail.poster_path = "Assets/defaultPoster.jpg";
                }

                foreach (var cast in viewModel.TheMovieDetail.casts.cast)
                {
                    if (cast.profile_path != null)
                    {
                        cast.profile_path = "https://image.tmdb.org/t/p/w500" + cast.profile_path;
                    }
                    else
                    {
                        cast.profile_path = "Assets/defaultPhoto.jpg";
                    }
                }
                this.Frame.Navigate(typeof(DetailPage), 0);
            }
            else
            {
                String url = String.Format("https://api.themoviedb.org/3/tv/{0}?api_key=7888f0042a366f63289ff571b68b7ce0&append_to_response=casts", item.id);
                HttpClient client = new HttpClient();
                String Jresult = await client.GetStringAsync(url);
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(TVDetail));
                MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(Jresult));
                viewModel.TheTVDetail = (TVDetail)serializer.ReadObject(ms);

                if (viewModel.TheTVDetail.backdrop_path != null)
                {
                    viewModel.TheTVDetail.backdrop_path = "https://image.tmdb.org/t/p/original" + viewModel.TheTVDetail.backdrop_path;
                }
                else
                {
                    viewModel.TheTVDetail.backdrop_path = "Assets/defaultBackground.png";
                }
                if (viewModel.TheTVDetail.poster_path != null)
                {
                    viewModel.TheTVDetail.poster_path = "https://image.tmdb.org/t/p/w500" + viewModel.TheTVDetail.poster_path;
                }
                else
                {
                    viewModel.TheTVDetail.poster_path = "Assets/defaultPoster.jpg";
                }

                foreach (var season in viewModel.TheTVDetail.seasons)
                {
                    if (season.poster_path != null)
                    {
                        season.poster_path = "https://image.tmdb.org/t/p/w500" + season.poster_path;
                    }
                    else
                    {
                        season.poster_path = "Assets/defaultPoster.jpg";
                    }
                }
                this.Frame.Navigate(typeof(DetailPage), 1);
            }  
        }

        private void Delete(object sender, RoutedEventArgs e)
        {
            dynamic temp = e.OriginalSource;
            Star s= temp.DataContext;
            viewModel.DeleteStar(s.id);
        }

        private void edit(object sender, RoutedEventArgs e)
        {
            dynamic temp = e.OriginalSource;
            selectedItem = temp.DataContext;
            com.Text = selectedItem.comment;
            comment.Visibility = Visibility.Visible;

        }

        private void ok(object sender, RoutedEventArgs e)
        {
            int id = selectedItem.id;

           viewModel.EditComment(id, com.Text);
            var db = App.conn;
            using (var TodoItem = db.Prepare(App.SQL_UPDATE))
            {
                TodoItem.Bind(1, com.Text);
                TodoItem.Bind(2, id);
               
                TodoItem.Step();


            }

            comment.Visibility = Visibility.Collapsed;
            selectedItem = null;
        }
    }
}

﻿using SQLitePCL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Notifications;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace PosterCollection
{
    /// <summary>
    /// 提供特定于应用程序的行为，以补充默认的应用程序类。
    /// </summary>
    sealed partial class App : Application
    {
        /// <summary>
        /// 初始化单一实例应用程序对象。这是执行的创作代码的第一行，
        /// 已执行，逻辑上等同于 main() 或 WinMain()。
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;

            conn = new SQLiteConnection(DB_NAME);


            using (var statement = conn.Prepare(SQL_CREATE_TABLE))
            {
                statement.Step();
            }


            using (var statement = conn.Prepare("CREATE TABLE IF NOT EXISTS Movies (Id INTEGER PRIMARY KEY NOT NULL,Title VARCHAR(100), overview VARCHAR(150),Type INTEGER);"))
            {
                statement.Step();
            }

            using (var statement = conn.Prepare(SQL_CREATE_USER_TABLE))
            {
                statement.Step();
            }
        }
        private static TileUpdater tileUpdate;
        static public SQLiteConnection conn { get; set; }
        public static String DB_NAME = "Collector.db";
        public static String TABLE_NAME = "Collection";
        public static String SQL_CREATE_TABLE = "CREATE TABLE IF NOT EXISTS " + TABLE_NAME + "(Id INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,MovieID INTEGER NOT NULL,Title VARCHAR(100),PATH VARCHAR(150),POSTER VERCHAR(150), COMMENT VARCHAR(150),TYPE INTEGER ,UID INTEGER, FOREIGN KEY(MovieID) REFERENCES Movies(Id),FOREIGN KEY(UID) REFERENCES UserTable(Id));";
        public static String SQL_INSERT = "INSERT INTO " + TABLE_NAME + "(MovieID,Title,PATH,POSTER,COMMENT,TYPE,UID) VALUES(?,?,?,?,?,?,?);";
        public static String SQL_QUERY_VALUE = "SELECT * FROM " + TABLE_NAME+" WHERE UID = (?)";
        public static String SQL_DELETE = "DELETE FROM " + TABLE_NAME + " WHERE MovieID = ? AND TYPE = ?";
        public static String SQL_UPDATE = "UPDATE " + TABLE_NAME + " SET COMMENT = ? WHERE MovieID = ? AND TYPE = ?";


        public static String USER_TABLE = "UserTable";
        public static String SQL_CREATE_USER_TABLE = "CREATE TABLE IF NOT EXISTS " + USER_TABLE + "(Id INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,Username VARCHAR(100),Password VARCHAR(100),Email VARCHAR(50),Phone VARCHAR(20), Role INTEGER);";
        public static String SQL_INSERT_USER = "INSERT INTO " + USER_TABLE + "(Username,Password,Email,Phone,Role) VALUES(?,?,?,?,?);";
        public static String SQL_QUERY_USER = "SELECT * FROM " + USER_TABLE;
        public static String SQL_DELETE_USER = "DELETE FROM " + USER_TABLE + " WHERE Id = ?";
        public static String SQL_UPDATE_USER = "UPDATE " + USER_TABLE + " SET Username = ?, Password = ?, Email = ?, Phone = ? WHERE Id = ?";
        private void BackRequested(object sender, BackRequestedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;
            if (rootFrame == null)
                return;
            if (rootFrame.CanGoBack && e.Handled == false)
            {
                e.Handled = true;
                rootFrame.GoBack();

            }
        }



        private void OnNavigated(object sender, NavigationEventArgs e)
        {
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = ((Frame)sender).CanGoBack && !((Frame)sender).CurrentSourcePageType.Equals(typeof(LoginPage)) ?
                AppViewBackButtonVisibility.Visible : Windows.UI.Core.AppViewBackButtonVisibility.Collapsed;
        }

        /// <summary>
        /// 在应用程序由最终用户正常启动时进行调用。
        /// 将在启动应用程序以打开特定文件等情况下使用。
        /// </summary>
        /// <param name="e">有关启动请求和过程的详细信息。</param>
        public static TileUpdater GetTileUpdater()
        {
            if (tileUpdate == null) tileUpdate = TileUpdateManager.CreateTileUpdaterForApplication();
            return tileUpdate;
        }

        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            tileUpdate = GetTileUpdater();
            tileUpdate.EnableNotificationQueue(true);
            tileUpdate.Clear();

            Frame rootFrame = Window.Current.Content as Frame;

            Windows.UI.Core.SystemNavigationManager.GetForCurrentView().BackRequested += BackRequested;

            // 不要在窗口已包含内容时重复应用程序初始化，
            // 只需确保窗口处于活动状态
            if (rootFrame == null)
            {
                // 创建要充当导航上下文的框架，并导航到第一页
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: 从之前挂起的应用程序加载状态
                }

                // 将框架放在当前窗口中
                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    // 当导航堆栈尚未还原时，导航到第一页，
                    // 并通过将所需信息作为导航参数传入来配置
                    // 参数
                    rootFrame.Navigate(typeof(LoginPage), e.Arguments);
                }
                // 确保当前窗口处于活动状态
                Window.Current.Activate();
                rootFrame.Navigated += OnNavigated;
            }
        }

        /// <summary>
        /// 导航到特定页失败时调用
        /// </summary>
        ///<param name="sender">导航失败的框架</param>
        ///<param name="e">有关导航失败的详细信息</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// 在将要挂起应用程序执行时调用。  在不知道应用程序
        /// 无需知道应用程序会被终止还是会恢复，
        /// 并让内存内容保持不变。
        /// </summary>
        /// <param name="sender">挂起的请求的源。</param>
        /// <param name="e">有关挂起请求的详细信息。</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: 保存应用程序状态并停止任何后台活动
            deferral.Complete();
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace myMediaPlayer
{
    public sealed partial class MainPage : Page
    {
        private MediaPlayer mediaPlayer = new MediaPlayer();                                                //  用于设置媒体播放器
        private MediaTimelineController mediaTimelineController = new MediaTimelineController();            //  用于控制进度
        private TimeSpan duration;                                                                          //  用于获取媒体项的总秒数
        private DispatcherTimer timer;

        public MainPage()
        {
            this.InitializeComponent();

            myMediaPlayer.SetMediaPlayer(mediaPlayer);                                                      //  将MediaPlayerElment元素与MediaPlayer对象绑定, 用于在XML中呈现媒体效果. 
                                                                                                            //  先set mediaPlayer再create media

            var mediaSource = MediaSource.CreateFromUri(new Uri("ms-appx:///Assets/梨花雨.mp4"));          
            mediaSource.OpenOperationCompleted += MediaSource_OpenOperationCompleted;
            myMediaPlayer.Source = mediaSource;

            mediaPlayer.CommandManager.IsEnabled = false;                                                   //  禁用CommandManager后才能使用TimeLineController
            mediaPlayer.TimelineController = mediaTimelineController;
        }

        private async void MediaSource_OpenOperationCompleted(MediaSource sender, MediaSourceOpenOperationCompletedEventArgs args)      //  媒体文件加载完后执行
        {
            duration = sender.Duration.GetValueOrDefault();

            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                timeLine.Minimum = 0;                           //  设置最小长度
                timeLine.Maximum = duration.TotalSeconds;       //  设置最大长度
                timeLine.StepFrequency = 0.5;                   //  设置更新频率
            });
        }

        private void startOrPause(object sender, RoutedEventArgs e)
        {
            if (timer == null)
            {
                timer = new DispatcherTimer();
                timer.Interval = TimeSpan.FromSeconds(1);
                timer.Tick += timer_Tick;
                timer.Start();

                EllStoryboard.Begin();              //  图片开始旋转
                mediaTimelineController.Start();

                var pauseIcon = new SymbolIcon(Symbol.Pause);
                startAndPauseButton.Icon = pauseIcon;
                startAndPauseButton.Label = "暂停";
            }
            else
            {
                if (mediaTimelineController.State == MediaTimelineControllerState.Running)
                {
                    EllStoryboard.Pause();          //  图片暂停旋转
                    mediaTimelineController.Pause();

                    var playIcon = new SymbolIcon(Symbol.Play);
                    startAndPauseButton.Icon = playIcon;
                    startAndPauseButton.Label = "播放";
                }
                else
                {
                    EllStoryboard.Begin();
                    mediaTimelineController.Resume();

                    var pauseIcon = new SymbolIcon(Symbol.Pause);
                    startAndPauseButton.Icon = pauseIcon;
                    startAndPauseButton.Label = "暂停";
                }
            }

        }

        void timer_Tick(object sender, object e)
        {
            timeLine.Value = ((TimeSpan)mediaTimelineController.Position).TotalSeconds;
            if (timeLine.Value == timeLine.Maximum)
            {
                mediaTimelineController.Position = TimeSpan.FromSeconds(0);
                mediaTimelineController.Pause();
            }
        }

        private void stop_Click(object sender, RoutedEventArgs e)
        {
            stopMediaPlayer();
        }

        private void stopMediaPlayer()
        {
            EllStoryboard.Stop();                       //  图片终止旋转
            mediaTimelineController.Position = TimeSpan.FromSeconds(0);
            mediaTimelineController.Pause();

            var playIcon = new SymbolIcon(Symbol.Play);
            startAndPauseButton.Icon = playIcon;
            startAndPauseButton.Label = "播放";
        }

        private async void add_Click(object sender, RoutedEventArgs e)
        {
            FileOpenPicker fileOpenPicker = new FileOpenPicker();
            fileOpenPicker.ViewMode = PickerViewMode.Thumbnail;
            fileOpenPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            fileOpenPicker.FileTypeFilter.Add(".mp3");
            fileOpenPicker.FileTypeFilter.Add(".mp4");
            fileOpenPicker.FileTypeFilter.Add(".wmv");
            fileOpenPicker.FileTypeFilter.Add(".wma");

            StorageFile file = await fileOpenPicker.PickSingleFileAsync();
            if (file != null)
            {
                myMediaPlayer.Source = MediaSource.CreateFromStorageFile(file);

                if (file.FileType == ".mp3" || file.FileType == ".wma")
                {
                    myMusicPlayer.Visibility = Visibility.Visible;
                }
                else
                {
                    myMusicPlayer.Visibility = Visibility.Collapsed;
                }

                stopMediaPlayer();      //  选取了新的视频后, 终止之前的视频
            }
        }

        private void ChangeMediaVolume(object sender, RangeBaseValueChangedEventArgs e)
        {
            mediaPlayer.Volume = (double)volumeSlider.Value;
        }

        private void display_Click(object sender, RoutedEventArgs e)
        {
            ApplicationView view = ApplicationView.GetForCurrentView();
            bool isInFullScreenMode = view.IsFullScreenMode;
            if (isInFullScreenMode)
            {
                view.ExitFullScreenMode();
            }
            else
            {
                view.TryEnterFullScreenMode();
            }
        }
    }
}

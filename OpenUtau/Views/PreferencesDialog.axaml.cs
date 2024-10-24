﻿using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using OpenUtau.App.ViewModels;
using OpenUtau.Core;
using Whisper.net.Ggml;
using OpenUtau.Core.Util;

namespace OpenUtau.App.Views {
    public partial class PreferencesDialog : Window {
        public PreferencesDialog() {
            InitializeComponent();
        }

        void ResetAddlSingersPath(object sender, RoutedEventArgs e) {
            ((PreferencesViewModel)DataContext!).SetAddlSingersPath(string.Empty);
        }

        async void SelectAddlSingersPath(object sender, RoutedEventArgs e) {
            var path = await FilePicker.OpenFolderAboutSinger(this, "prefs.paths.addlsinger");
            if (string.IsNullOrEmpty(path)) {
                return;
            }
            if (Directory.Exists(path)) {
                ((PreferencesViewModel)DataContext!).SetAddlSingersPath(path);
            }
        }

        async void ReloadSingers(object sender, RoutedEventArgs e) {
            MessageBox.ShowLoading(this);
            await Task.Run(() => {
                SingerManager.Inst.SearchAllSingers();
            });
            DocManager.Inst.ExecuteCmd(new SingersRefreshedNotification());
            MessageBox.CloseLoading();
        }

        async void DownloadGetLyricModule(object sender, RoutedEventArgs e) {
            MessageBox.ShowLoading(this);
            var modelPath = Path.Combine(PathManager.Inst.DependencyPath, "Whisper/model/ggml-large-v3-turbo.bin");
            if (!File.Exists(modelPath)) {
                if (!Directory.Exists(Path.Combine(PathManager.Inst.DependencyPath, "Whisper/model"))) {
                    Directory.CreateDirectory(Path.Combine(PathManager.Inst.DependencyPath, "Whisper/model"));
                }
                using var modelStream = await WhisperGgmlDownloader.GetGgmlModelAsync(GgmlType.LargeV3Turbo);
                using var fileWriter = File.OpenWrite(modelPath);
                await modelStream.CopyToAsync(fileWriter);
            }
            ((PreferencesViewModel)DataContext!).EnableGetLyricModuleButton = 
                (Preferences.Default.EnableGetLyricModule && !File.Exists(Path.Combine(PathManager.Inst.DependencyPath, "Whisper/model/ggml-large-v3-turbo.bin")));
            MessageBox.CloseLoading();
        }

        void ResetVLabelerPath(object sender, RoutedEventArgs e) {
            ((PreferencesViewModel)DataContext!).SetVLabelerPath(string.Empty);
        }

        async void SelectVLabelerPath(object sender, RoutedEventArgs e) {
            var type = OS.IsWindows() ? FilePicker.EXE : OS.IsMacOS() ? FilePicker.APP : FilePickerFileTypes.All;
            var path = await FilePicker.OpenFile(this, "prefs.advanced.vlabelerpath", type);
            if (string.IsNullOrEmpty(path)) {
                return;
            }
            if (OS.AppExists(path)) {
                ((PreferencesViewModel)DataContext!).SetVLabelerPath(path);
            }
        }

        void ResetSetParamPath(object sender, RoutedEventArgs e) {
            ((PreferencesViewModel)DataContext!).SetSetParamPath(string.Empty);
        }

        async void SelectSetParamPath(object sender, RoutedEventArgs e) {
            var path = await FilePicker.OpenFile(this, "prefs.otoeditor.setparampath", FilePicker.EXE);
            if (string.IsNullOrEmpty(path)) {
                return;
            }
            if (File.Exists(path)) {
                ((PreferencesViewModel)DataContext!).SetSetParamPath(path);
            }
        }
    }
}

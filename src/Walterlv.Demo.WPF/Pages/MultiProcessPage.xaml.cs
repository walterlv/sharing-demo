﻿using System;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace Walterlv.Demo.Pages
{
    public partial class MultiProcessPage : Page
    {
        public MultiProcessPage()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= OnLoaded;

            var currentFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
                                ?? Environment.CurrentDirectory;
#if DEBUG
            var path = Path.Combine(currentFolder, "..", "..", "..",
                "Extensions", "Walterlv.Extensions.WPF", "bin", "Debug");
            path = Path.GetFullPath(path);
#else
            var path = currentFolder;
#endif
        }
    }
}
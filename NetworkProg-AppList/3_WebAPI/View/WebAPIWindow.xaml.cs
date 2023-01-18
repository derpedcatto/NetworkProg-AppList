using System;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace NetworkProg_AppList._3_WebAPI.View
{
    /// <summary>
    /// Interaction logic for WebAPIWindow.xaml
    /// </summary>
    public partial class WebAPIWindow : Window
    {
        public ObservableCollection<Model.AssetModel> Assets { get; set; }
        private Color _activeGraphColor;
        private Random _random = new();

        public WebAPIWindow()
        {
            InitializeComponent();
            Assets = new();
            DataContext = this;
        }


        
        private void DrawGraphLine(double x1, double y1, double x2, double y2)
        {
            GraphCanvas.Children.Add(new Line
            {
                X1 = x1,
                Y1 = y1,
                X2 = x2,
                Y2 = y2,
                Stroke = new SolidColorBrush(_activeGraphColor),
                StrokeThickness = 2
            });
        }

        private void SetRandomGraphColor()
        {
            _activeGraphColor = Color.FromArgb(255, (byte)_random.Next(100, 200),
                                                    (byte)_random.Next(100, 200),
                                                    (byte)_random.Next(100, 200));
        }



        private async void GetAssets()
        {
            using var client = new HttpClient { BaseAddress = new Uri("https://api.coincap.io/") };
            String assets = await client.GetStringAsync("/v2/assets/");
            ProcessAssets(assets);
        }

        private void ProcessAssets(string assetsString)
        {
            var assetList = JsonSerializer.Deserialize<Model.AssetModelList>(assetsString);
            if (assetList is null) return;

            Dispatcher.Invoke(() =>
            {
                assetList.data.ForEach(Assets.Add); // Отображаем полученные ассеты путем помещения ссылок на них в наблюдаемую коллекцию Assets, связанную с ListView
            });                                     
        }



        private async void GetCoinHistory(string assetId)
        {
            using var client = new HttpClient { BaseAddress = new Uri("https://api.coincap.io/") };
            String history = await client.GetStringAsync($"/v2/assets/{assetId}/history?interval=d1");
            ProcessAssetHistory(history);
        }

        private void ProcessAssetHistory(string assetHistory)
        {
            var assetList = JsonSerializer.Deserialize<Model.AssetDateModelList>(assetHistory);
            if (assetList is null) return;

            /* Работаем над графиком:
            * по Х время (json.data[].time)
            * по Y курс (json.data[].priceUsd)
            * Данные нужно масштабировать, т.к. они явно не соответствуют
            * размерам холста. Для этого определяем максимальное и минимальное
            * значение по каждой координате
            */

            Int64 minTime, maxTime;
            Double minPrice, maxPrice;
            minTime = maxTime = assetList.data[0].time;
            minPrice = maxPrice = assetList.data[0].price;
            foreach (Model.AssetDateModel asset in assetList.data)
            {
                if (asset.time < minTime) minTime = asset.time;
                if (asset.time > maxTime) maxTime = asset.time;
                if (asset.price < minPrice) minPrice = asset.price;
                if (asset.price > maxPrice) maxPrice = asset.price;
            }

            /* Еще один цикл - масштабирует
            * точка на графике X: (time-minTime) - от нуля до (maxTime-minTime)
            *   ноль нас устраивает, но нужно чтобы максимальное значение 
            *   было шириной холста (Graph.ActualWidth):
            *   (time-minTime) / (maxTime-minTime) * Graph.ActualWidth
            * по Y аналогично, только с price и Graph.ActualHeight, но еще и "перевернуть",
            *  т.к. ось Y на холсте направлена вниз:
            *  y = Graph.ActualHeight - y
            * 
            * Для того чтобы проводить линии нужно помнить предыдущую точку и 
            *  соединять ее с текущей.
            */

            Double x1 = -1, y1 = -1;

            foreach (Model.AssetDateModel asset in assetList.data)
            {
                Double x2 = (asset.time - minTime) * GraphCanvas.ActualWidth / (maxTime - minTime);
                Double y2 = (asset.price - minPrice) * GraphCanvas.ActualHeight / (maxPrice - minPrice);
                y2 = GraphCanvas.ActualHeight - y2;   // инверсия по Y (вверх ногами)

                if (x1 != -1)   // не первая точка
                {
                    Dispatcher.Invoke(() => DrawGraphLine(x1, y1, x2, y2));
                }

                x1 = x2;
                y1 = y2;
            }
        }



        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Task.Run(GetAssets);
        }

        private void ListViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var item = ((ListViewItem)sender).Content as Model.AssetModel;

            if (ClearChartCheckBox.IsChecked == true)
            {
                DisplayedCoinsTextBlock.Text = String.Empty;
                GraphCanvas.Children.Clear();
            }

            SetRandomGraphColor();

            DisplayedCoinsTextBlock.Inlines.Add(new Run()
            {
                Text = $"{item!.name} ",
                Foreground = new SolidColorBrush(_activeGraphColor)
            });

            GetCoinHistory(item.id);
        }
    }
}
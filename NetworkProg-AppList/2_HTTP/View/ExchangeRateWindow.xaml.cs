using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Xml;

namespace NetworkProg_AppList._2_HTTP.View
{
    /// <summary>
    /// Interaction logic for ExchangeRateWindow.xaml
    /// </summary>
    public partial class ExchangeRateWindow : Window
    {
        public ExchangeRateWindow()
        {
            InitializeComponent();
            ExchangeRateDatePicker.SelectedDate = DateTime.Now;
            ExchangeRateDatePicker.DisplayDateStart = new DateTime(1996, 1, 6);
            ExchangeRateDatePicker.DisplayDateEnd = DateTime.Now;
        }


        private string GetDateURIString()
        {
            var date = ExchangeRateDatePicker.SelectedDate!.Value;
            return date.ToString("yyyy-MM-dd")
                       .Replace("-", "");
        }


        private void ParseJSONRates()
        {
            var ratesList = JsonSerializer.Deserialize<List<Model.RateJSON>>(RawDataTextBlock.Text);
            if (ratesList is null) return;

            RatesTreeView.Items.Clear();
            foreach(Model.RateJSON rate in ratesList)
            {
                TreeViewItem rateItem = new() { Header = rate.cc + " - " + rate.txt };
                rateItem.Items.Add(new TreeViewItem { Header = rate.txt });
                rateItem.Items.Add(new TreeViewItem { Header = "rate: " + rate.rate });
                rateItem.Items.Add(new TreeViewItem { Header = "r030: " + rate.r030 });
                rateItem.Items.Add(new TreeViewItem { Header = rate.exchangedate });

                RatesTreeView.Items.Add(rateItem);
            }
        }

        private void ParseXMLRates()
        {
            XmlDocument ratesDocument = new();
            ratesDocument.LoadXml(RawDataTextBlock.Text);   // Загрузка текста в документ для обработки

            XmlNodeList? currenciesNodeList = ratesDocument?    // Селектор - запрос на выборку узлов, соответствующим критериям;
                                      .DocumentElement?         // Сам контент (без строки заголовка <?xml...)
                                      .SelectNodes("currency"); // Отбор по имени тегов (<currency>)

            if (currenciesNodeList is null) return;

            RatesTreeView.Items.Clear();
            foreach (XmlNode node in currenciesNodeList)
            {
                /* node.InnerText - текст узла (без тегов), если у узла есть внутренние узлы, то все их InnerText соединяются в строку; 
                   node.ChildNodes - коллекция внутренних узлов, порядок их следования как в исходном док-те ([0] - r030, [1] - txt, [2] - rate,...)*/
                TreeViewItem treeItem = new()
                {
                    Header = node.ChildNodes[3]?.InnerText + " - " + node.ChildNodes[1]?.InnerText
                };

                treeItem.Items.Add(new TreeViewItem { Header = "r030: " + node.ChildNodes[0]?.InnerText });
                treeItem.Items.Add(new TreeViewItem { Header = "cc: " + node.ChildNodes[3]?.InnerText });
                treeItem.Items.Add(new TreeViewItem { Header = "rate: " + node.ChildNodes[2]?.InnerText });
                treeItem.Items.Add(new TreeViewItem
                {
                    Header = String.Format("1 {0} = {1:F2} UAH", node.ChildNodes[3]?.InnerText,
                                                                 node.ChildNodes[2]?.InnerText)
                });
                treeItem.Items.Add(new TreeViewItem
                {
                    Header = String.Format("1 UAH = {1:F2} {0}", node.ChildNodes[3]?.InnerText,                                     // F2 - Float with 2 digits after '.'
                                                                 1 / Convert.ToSingle(node.ChildNodes[2]?.InnerText,                // В ОС десятичная точка считается запятой.
                                                                                      CultureInfo.InvariantCulture.NumberFormat))   //Для 'сброса' этого правила выбираем InvariantCulture
                });

                RatesTreeView.Items.Add(treeItem);
            }
        }


        private void HTMLButton_Click(object sender, RoutedEventArgs e)
        {
            URITextBox.Text = "https://itstep.org";

            HttpClient httpClient = new() { BaseAddress = new Uri(URITextBox.Text) };
            httpClient.GetStringAsync("/")  // Отправляем запрос на домашнюю страницу (/)
                      .ContinueWith(t => Dispatcher.Invoke(() => 
                      { RawDataTextBlock.Text = t.Result; }));
        }

        // Использование await требует сделать метод async, но позваоляет не использовать Dispatcher
        private async void JSONButton_Click(object sender, RoutedEventArgs e)
        {
            URITextBox.Text = "https://bank.gov.ua/NBUStatService/v1/statdirectory/exchange?date=" + GetDateURIString() + "&json";

            /* Разделение базового адреса (bank.gov.ua) и запроса (/NBUStatService/v1/...) */
            string homeUri = "https://bank.gov.ua";

            using HttpClient httpClient = new() { BaseAddress = new Uri(homeUri) };
            RawDataTextBlock.Text = await httpClient.GetStringAsync("/NBUStatService/v1/statdirectory/exchange?date=" + GetDateURIString() + "&json");

            ParseJSONRates();
        }

        private void XMLButton_Click(object sender, RoutedEventArgs e)
        {
            URITextBox.Text = "https://bank.gov.ua/NBUStatService/v1/statdirectory/exchange?date=" + GetDateURIString() + "&xml";

            HttpClient httpClient = new();
            httpClient.GetStringAsync(URITextBox.Text)
                      .ContinueWith(t => Dispatcher.Invoke(() =>
                      {
                          RawDataTextBlock.Text = t.Result;
                          httpClient.Dispose();
                          ParseXMLRates();
                      }));
        }
    }
}

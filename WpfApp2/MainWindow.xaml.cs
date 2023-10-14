using Microsoft.Win32; // 使用 Microsoft 的 Windows 注冊表類別，用於檔案對話框
using System; // 引入基礎的 System 命名空間
using System.Collections.Generic; // 使用泛型集合，例如 Dictionary
using System.IO; // 使用文件輸入輸出
using System.Text;
using System.Windows; // WPF 基礎類
using System.Windows.Controls; // WPF 控件
using System.Windows.Data; // 用於數據綁定
using System.Windows.Documents;
using System.Windows.Media; // 繪圖和顏色

namespace WpfApp2// 命名空間
{
    public partial class MainWindow : Window // 主窗口類繼承自 Window
    {
        // 字典，用於儲存飲料名稱和價格
        Dictionary<string, int> drinks = new Dictionary<string, int>();

        // 字典，用於儲存訂單，包括飲料名稱和數量
        Dictionary<string, int> orders = new Dictionary<string, int>();

        // 用於儲存外帶選項的變數
        string takeout = "";

        public MainWindow()// 構造函數
        {
            InitializeComponent();// 初始化XAML元素

            // 將飲料項目添加到 'drinks' 字典中
            AddNewDrink(drinks);

            // 在UI上顯示飲料菜單
            DisplayDrinkMenu(drinks);
        }

        // 在UI上顯示飲料菜單
        private void DisplayDrinkMenu(Dictionary<string, int> myDrinks)
        {
            foreach (var drink in myDrinks)
            {
                // 創建水平排列的StackPanel
                var sp = new StackPanel
                {
                    Orientation = Orientation.Horizontal
                };

                // 為每種飲料創建一個核取方塊
                var cb = new CheckBox
                {
                    Content = $"{drink.Key} : {drink.Value}元",
                    Width = 200,
                    FontFamily = new FontFamily("Consolas"),
                    FontSize = 18,
                    Foreground = Brushes.Blue,
                    Margin = new Thickness(5)
                };

                // 創建一個滑塊以選擇數量
                var sl = new Slider
                {
                    Width = 100,
                    Value = 0,
                    Minimum = 0,
                    Maximum = 10,
                    VerticalAlignment = VerticalAlignment.Center,
                    IsSnapToTickEnabled = true
                };

                // 創建一個標籤以顯示所選數量
                var lb = new Label
                {
                    Width = 50,
                    Content = "0",
                    FontFamily = new FontFamily("Consolas"),
                    FontSize = 18,
                    Foreground = Brushes.Red// 設置文本的前景色為紅色。
                };

                // 將UI元素添加到StackPanel
                sp.Children.Add(cb);
                sp.Children.Add(sl);
                sp.Children.Add(lb);

                // 數據綁定以將滑塊值和標籤內容關聯起來
                Binding myBinding = new Binding("Value");
                myBinding.Source = sl;
                lb.SetBinding(ContentProperty, myBinding);

                // 將StackPanel添加到主UI StackPanel
                stackpanel_DrinkMenu.Children.Add(sp);
            }
        }

        // 從文件中添加新的飲料到 'drinks' 字典
        private void AddNewDrink(Dictionary<string, int> myDrinks)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();// 創建文件選擇對話框
            openFileDialog.Filter = "CSV文件|*.csv|文本文件|*.txt|所有文件|*.*";// 設置對話框的文件過濾選項
            if (openFileDialog.ShowDialog() == true)// 顯示對話框
            {
                string filename = openFileDialog.FileName;// 獲取選定文件的完整路徑
                string[] lines = File.ReadAllLines(filename);// 讀取文件的所有行
                foreach (var line in lines)// 遍歷每一行
                {
                    string[] tokens = line.Split(',');// 使用逗號分隔每一行
                    string drinkName = tokens[0];// 提取飲料名稱和價格
                    int price = Convert.ToInt32(tokens[1]); // 將價格從字符串轉為整數
                    myDrinks.Add(drinkName, price);// 添加到 'drinks' 字典
                }
            }
        }

        // "訂購"按鈕點擊事件處理程序
        private void OrderButton_Click(object sender, RoutedEventArgs e)
        {
            // 將選擇的飲料放入 'orders' 字典
            PlaceOrder(orders);

            // 在UI上顯示訂單詳細信息
            DisplayOrder(orders);

            // 將訂單寫入文本文件
            SaveOrderToFile(orders);
        }

        // 在UI上顯示訂單詳細信息
        private void DisplayOrder(Dictionary<string, int> myOrders)
        {
            displayTextBlock.Inlines.Clear();// 清空TextBlock的內容
            Run titleString = new Run // 創建用於顯示訂單標題的Run對象
            {
                Text = "您所訂購的飲品為 ",
                FontSize = 16,
                Foreground = Brushes.Blue
            };

            Run takeoutString = new Run // 創建用於顯示外帶選項的Run對象
            {
                Text = $"{takeout}",
                FontSize = 16,// 設置字體大小為 16。
                FontWeight = FontWeights.Bold// 設置字體權重為 Bold，使文本看起來更加粗體。
            };

            // 添加標題和外帶信息到顯示中
            displayTextBlock.Inlines.Add(titleString);// 將標題和外帶選項添加到TextBlock中
            displayTextBlock.Inlines.Add(takeoutString);
            displayTextBlock.Inlines.Add(new Run() { Text = " ，訂購明細如下: \n", FontSize = 16 });

            double total = 0.0;// 用於存儲總價格
            double sellPrice = 0.0; // 用於存儲售價
            string discountString = ""; // 用於存儲折扣信息

            int i = 1;
            foreach (var item in myOrders)
            {
                string drinkName = item.Key;// 獲取飲料名稱
                int quantity = myOrders[drinkName];// 獲取飲料數量
                int price = drinks[drinkName];// 獲取飲料價格
                total += price * quantity;// 更新總價格
                
                displayTextBlock.Inlines.Add(new Run() { Text = $"飲料品項{i}： {drinkName} X {quantity}杯，每杯{price}元，總共{price * quantity}元\n" });
                i++;// 顯示每個訂購的項目及其詳細信息
            }

            // 計算並顯示折扣和總價
            if (total >= 500)
            {
                discountString = "訂購滿500元以上者打8折";
                sellPrice = total * 0.8;
            }
            else if (total >= 300)
            {
                discountString = "訂購滿300元以上者打85折";
                sellPrice = total * 0.85;
            }
            else if (total >= 200)
            {
                discountString = "訂購滿200元以上者打9折";
                sellPrice = total * 0.9;
            }
            else
            {
                discountString = "訂購未滿200元以上者不打折";
                sellPrice = total;
            }

            // 創建一個新的 Italic 物件，其中包含一個 Run 物件來設定顯示文本。
            // Run 物件允許我們對單一的文字串進行豐富的格式設置。
            Italic summaryString = new Italic(new Run
            {
                Text = $"本次訂購總共{myOrders.Count}項，{discountString}，售價{sellPrice}元",// 使用字符串插值來組合本次訂購的摘要信息。
                FontSize = 16, // 設置字體大小為 16。
                FontWeight = FontWeights.Bold, // 設置字體權重為 Bold，使文本看起來更加粗體。
                Foreground = Brushes.Red // 設置文本的前景色為紅色。
            });
            displayTextBlock.Inlines.Add(summaryString);
            // 將創建的 Italic 物件（即 summaryString）添加到 displayTextBlock 的 Inlines 集合中。
            // 這樣可以將這段富文本顯示在 displayTextBlock 控件中。
        }

        // 將選擇的飲料放入 'orders' 字典
        private void PlaceOrder(Dictionary<string, int> myOrders)
        {
            
            myOrders.Clear();// 清空當前的訂單

            // 使用 for 迴圈遍歷 stackpanel_DrinkMenu 的所有子元件。
            // 這通常會是一些 StackPanel，其中包含飲料選項的 CheckBox 和 Slider。
            for (int i = 0; i < stackpanel_DrinkMenu.Children.Count; i++)// 遍歷所有子元件（飲料選項）在 stackpanel_DrinkMenu 中
            {
                // 從子元件列表中獲取第 i 個元件，並將其轉換為 StackPanel 類型。
                var sp = stackpanel_DrinkMenu.Children[i] as StackPanel;

                // 並將它們轉換為 CheckBox 和 Slider 類型。
                var cb = sp.Children[0] as CheckBox;
                var sl = sp.Children[1] as Slider;

                // 從 CheckBox 的 Content 屬性中提取飲料的名稱。
                // 我們假設名稱是前 4 個字元。
                string drinkName = cb.Content.ToString().Substring(0, 4);// 提取飲料名稱和數量
                int quantity = Convert.ToInt32(sl.Value); // 從 Slider 的 Value 屬性中獲取飲料的數量，並將其轉換為整數。

                if (cb.IsChecked == true && quantity != 0)  // 檢查 CheckBox 是否被選中，以及 Slider 的值（即數量）是否不為 0。
                {
                    myOrders.Add(drinkName, quantity);// 如果條件成立，則將這個飲料和其數量添加到 myOrders 字典中。
                }
            }
        }

        // 這是一個事件處理程序，專門用於處理 RadioButton 的 Checked 事件
        private void RadioButton_Checked(object sender, RoutedEventArgs e)// RadioButton 的事件處理程序，用於設定外帶選項
        {
            // 'sender' 參數是觸發此事件的 UI 元素，這裡是一個 RadioButton。
            
            var rb = sender as RadioButton;// 我們使用 'as' 運算符將它轉型為 RadioButton 類型。
            // 檢查 RadioButton 是否被選中。
            // 如果是，則將其內容（Content 屬性）轉換為字符串，
            // 並儲存到 'takeout' 變數中。
            if (rb.IsChecked == true) takeout = rb.Content.ToString();
            // 下面這行代碼是被註釋掉的，它會顯示一個消息框來顯示 'takeout' 變數的值。
            // 這主要用於調試 MessageBox.Show(takeout); 
        }

        // 將訂單保存到文件中
        private void SaveOrderToFile(Dictionary<string, int> myOrders)
        {
            string filename = "C:\\Users\\ADMIN\\source\\repos\\GWSG\\WpfApp3--Drinks-Yes-UI\\WpfApp2\\w3.txt"; // 設定要保存的文件的路徑

            try
            {
                // 開啟文件並寫入訂單信息
                // 使用 StreamWriter 來打開或創建一個指定編碼（UTF-8）的文件
                using (StreamWriter writer = new StreamWriter(filename, false, Encoding.UTF8))
                {
                    writer.WriteLine("訂單明細:");// 寫入訂單的標題到文件
                    foreach (var item in myOrders) // 遍歷訂單中的每一個項目（Dictionary 的每一個鍵值對）
                    {
                        string drinkName = item.Key;// 從 Dictionary 中獲取飲料名稱（Key）
                        int quantity = item.Value;// 從 Dictionary 中獲取數量（Value）

                        int price = drinks[drinkName];// 從另一個 Dictionary（drinks）中獲取該飲料的價格
                        writer.WriteLine($"{drinkName} X {quantity}杯，每杯{price}元，總共{price * quantity}元");// 寫入飲料的詳細信息到文件
                    }
                    writer.Close();// 關閉 StreamWriter 物件，釋放資源
                    MessageBox.Show("訂單已成功儲存到檔案: " + filename, "訂單儲存成功", MessageBoxButton.OK, MessageBoxImage.Information);// 顯示保存成功的消息
                }
            }
            catch (Exception ex)// 如果在嘗試寫入文件過程中發生異常（例如，文件不存在，沒有寫入權限等），則會執行此塊
            {
                MessageBox.Show("儲存訂單時發生錯誤: " + ex.Message, "錯誤", MessageBoxButton.OK, MessageBoxImage.Error);// 如果有錯誤，顯示錯誤信息
            }
        }
    }
}
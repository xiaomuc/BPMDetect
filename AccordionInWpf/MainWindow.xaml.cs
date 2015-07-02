using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AccordionInWpf
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            accitemUInfo.IsEnabled = false;
            listBox1.Items.Add("text");
        }

        private void btnSubmit_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(txtFN.Text) && !string.IsNullOrEmpty(txtLN.Text))
            {
                txtInfo.Text = "Welcom, " + txtFN.Text + " " + txtLN.Text;
                txtFN.Text = string.Empty;
                txtLN.Text = string.Empty;
                accitemUInfo.IsEnabled = true;
                accitemUInfo.IsSelected = true;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            listBox1.Items.Clear();
            
            Parallel.For(0, 10000, (i) => {
                listBox1.Dispatcher.BeginInvoke(new Action(() =>
                {
                    listBox1.Items.Add(i);
                }));
            });
        }
    }
}

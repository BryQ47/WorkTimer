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
using System.Windows.Shapes;

namespace WorkTimer
{
    /// <summary>
    /// Interaction logic for SummaryDialog.xaml
    /// </summary>
    public partial class SummaryDialog : Window
    {
        public string Summary
        {
            get { return txtSummary.Text; }
        }

        public SummaryDialog()
        {
            InitializeComponent();
        }

        private void Windows_ContentRendered(object sender, EventArgs e)
        {
            txtSummary.SelectAll();
            txtSummary.Focus();
        }

        private void btnSummaryOK_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
    }
}

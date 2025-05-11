using System.Windows;
using LittleHelper;

namespace LittleHelper {

    public partial class RevitCodeWindow: Window

    {
        public RevitCodeWindow(string apiCode) {
            InitializeComponent();

            RevitCodeTextBox.Text = apiCode;
        }
    }
}
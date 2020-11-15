using System.ComponentModel;
using Xamarin.Forms;
using Xamarin5Demo.ViewModels;

namespace Xamarin5Demo.Views
{
    public partial class ItemDetailPage : ContentPage
    {
        public ItemDetailPage()
        {
            InitializeComponent();
            BindingContext = new ItemDetailViewModel();
        }
    }
}
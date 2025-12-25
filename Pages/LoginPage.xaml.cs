using Microsoft.EntityFrameworkCore;
using Prak15Mensh.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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

namespace Prak15Mensh.Pages
{
    public partial class LoginPage : Page
    {
        private string _password = "1111";

        public string password 
        {
            get => _password;
            set
            {
                _password = value;
            }
        }


        public LoginPage()
        {
            InitializeComponent();
        }

        private void ManagerIn(object sender, RoutedEventArgs e)
        {
            if (SignInPassword.Password == password)
                NavigationService.Navigate(new MainPage(1234));
            else
                MessageBox.Show("Не верный пароль");
        }

        private void UserIn(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new MainPage());
        }
    }
}
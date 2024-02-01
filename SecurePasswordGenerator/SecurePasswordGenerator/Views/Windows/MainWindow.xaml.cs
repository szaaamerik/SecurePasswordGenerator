using System.Windows;
using System.Windows.Controls;
using SecurePasswordGenerator.Models;
using SecurePasswordGenerator.ViewModels.Windows;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace SecurePasswordGenerator.Views.Windows;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : INavigationWindow
{
    public MainWindowViewModel ViewModel { get; }
    
    public MainWindow(MainWindowViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = this;
        Wpf.Ui.Appearance.SystemThemeWatcher.Watch(this);
        InitializeComponent();
    }
    
    #region INavigationWindow methods

    public bool Navigate(Type pageType) => false;

    public void SetPageService(IPageService pageService) { }

    public void ShowWindow() => Show();

    public void CloseWindow() => Close();

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        Application.Current.Shutdown();
    }

    INavigationView INavigationWindow.GetNavigation()
    {
        throw new NotImplementedException();
    }

    public void SetServiceProvider(IServiceProvider serviceProvider)
    {
        throw new NotImplementedException();
    }
    
    #endregion INavigationWindow methods

    private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is not ListView listView)
        {
            return;
        }
        
        ViewModel.SelectedIndex = listView.SelectedIndex;
        ViewModel.NoEntrySelected = listView.SelectedItem == null;
        ViewModel.MultipleEntriesSelected = listView.SelectedItems.Count > 1;
        ViewModel.IsValidEntry = ViewModel is
        {
            NoEntrySelected: false,
            MultipleEntriesSelected: false,
            ListHasItems: true
        };
        
        if (!ViewModel.MultipleEntriesSelected)
        {
            return;
        }
        
        ViewModel.SelectedPasswords.Clear();
        foreach (SecurePassword element in listView.SelectedItems)
        {
            ViewModel.SelectedPasswords.Add(element);
        }
    }
}
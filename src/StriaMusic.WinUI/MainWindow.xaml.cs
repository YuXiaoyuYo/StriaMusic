using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using StriaMusic.Desktop.Share.Storage;
using StriaMusic.WinUI.Pages;
using Windows.Graphics;

namespace StriaMusic.WinUI;

/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MainWindow
{
    public static MainWindow Window { get; private set; } = null!;

    public MainWindow()
    {
        Window = this;
        
        InitializeComponent();
        
        // 设置系统背景，在Win10为Acrylic，在Win11为MicaAlt
        MainWindow_SetSystemBackDrop();
        
        // 自定义标题栏
        MainWindow_SetTitleBar();

        // 设置窗口大小和位置
        MainWindow_MoveAndResizeFromPrevious();

        // 设置关闭窗口时事件
        AppWindow.Destroying += MainWindow_Destroying;
        
        // 设置默认导航页面
        MainFrame.Navigate(typeof(HomePage), null, new EntranceNavigationTransitionInfo());
    }

    private void MainWindow_SetTitleBar()
    {
        // 设置标题栏
        ExtendsContentIntoTitleBar = true;
        Title = "Stria Music";
        SetTitleBar(AppTitleBar);
    }

    private void MainWindow_SetSystemBackDrop()
    {
        if (MicaController.IsSupported())
            SystemBackdrop = new MicaBackdrop { Kind = MicaKind.BaseAlt };
        else if (DesktopAcrylicController.IsSupported())
            SystemBackdrop = new DesktopAcrylicBackdrop();
    }
    
    private void RootNavigationView_OnDisplayModeChanged(NavigationView sender, object args)
    {
        AppTitleBar.Margin = RootNavigationView.DisplayMode is NavigationViewDisplayMode.Minimal 
            ? new Thickness(94, 0, 0, 0) 
            : new Thickness(44, 0, 0, 0);
    }

    private static RectInt32 CachedWindowSize { get; set; }

    private void MainWindow_SizeChanged(object sender, object e)
    {
        // 暂存窗口大小和位置、防止最大化后直接保存最大化时的大小
        if ((OverlappedPresenter)AppWindow.Presenter is { State: OverlappedPresenterState.Maximized })
            return;

        CachedWindowSize = CachedWindowSize with
        {
            X = AppWindow.Position.X,
            Y = AppWindow.Position.Y,
            Width = AppWindow.Size.Width,
            Height = AppWindow.Size.Height,
        };
    }
    
    private void MainWindow_Destroying(object sender, object e)
    {
        // 保存窗口大小和位置
        Preferences.Set("X", CachedWindowSize.X, "Window");
        Preferences.Set("Y", CachedWindowSize.Y, "Window");
        Preferences.Set("Width", CachedWindowSize.Width, "Window");
        Preferences.Set("Height", CachedWindowSize.Height, "Window");
        Preferences.Set("IsMaximized", 
            (OverlappedPresenter)AppWindow.Presenter is { State: OverlappedPresenterState.Maximized },
            "Window");
    }

    private void MainWindow_MoveAndResizeFromPrevious()
    {
        // 设置窗口最小大小
        ((OverlappedPresenter)AppWindow.Presenter).PreferredMinimumHeight = 400;
        ((OverlappedPresenter)AppWindow.Presenter).PreferredMinimumWidth = 500;
        
        // 从上次关闭的位置和大小恢复窗口
        AppWindow.MoveAndResize(new RectInt32(
            Preferences.Get("X", AppWindow.Position.X, "Window"),
            Preferences.Get("Y", AppWindow.Position.Y, "Window"), 
            Preferences.Get("Width", AppWindow.Size.Width, "Window"),
            Preferences.Get("Height", AppWindow.Size.Height, "Window")
            ));
        
        if (Preferences.Get("IsMaximized", false, "Window"))
            ((OverlappedPresenter)AppWindow.Presenter).Maximize();
    }
    
    private void RootNavigationView_OnBackRequested(object sender, object args)
    {
        if (MainFrame.CanGoBack)
            MainFrame.GoBack();
    }
    
    private void RootNavigationView_OnForwardRequested(object sender, object args)
    {
        if (MainFrame.CanGoForward)
            MainFrame.GoForward();
    }

    private void MainFrame_OnNavigated(object sender, NavigationEventArgs e)
    {
        RootNavigationView.IsBackEnabled = MainFrame.CanGoBack;
        
        if (MainFrame.SourcePageType == typeof(SettingsPage))
        {
            // SettingsItem is not part of NavView.MenuItems, and doesn't have a Tag.
            RootNavigationView.SelectedItem = (NavigationViewItem)RootNavigationView.SettingsItem;
        }
        else if (MainFrame.SourcePageType != null)
        {
            RootNavigationView.SelectedItem = RootNavigationView.MenuItems
                .OfType<NavigationViewItem>()
                .FirstOrDefault(n => n.Tag.Equals(MainFrame.SourcePageType.Name));
        }
    }

    private void RootNavigationView_OnItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
    {
        // 防止重复导航
        if ((string)args.InvokedItemContainer.Tag == MainFrame.SourcePageType.Name)
            return;
        
        if (args.IsSettingsInvoked && MainFrame.SourcePageType != typeof(SettingsPage))
            MainFrame.Navigate(typeof(SettingsPage), null, args.RecommendedNavigationTransitionInfo);
        
        else if (args.InvokedItemContainer?.Tag is not null)
            switch (args.InvokedItemContainer.Tag)
            {
                case "HomePage":
                    MainFrame.Navigate(typeof(HomePage), null, args.RecommendedNavigationTransitionInfo);
                    break;
            }
    }
}


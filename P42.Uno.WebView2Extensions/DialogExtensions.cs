using Windows.Storage.Pickers;
using Microsoft.UI.Xaml.Shapes;
#if BROWSERWASM
using Log = System.Console;
#else
using Log = System.Diagnostics.Debug;
#endif

namespace P42.Uno;

public static class DialogExtensions
{
    public static async Task<StorageFile?> RequestStorageFileAsync(XamlRoot xamlRoot, string type,  string suggestedName, params List<string> suffixes)
    {

        if (!string.IsNullOrEmpty(suggestedName) && suggestedName.Contains('.'))
        {
            var extension = System.IO.Path.GetExtension(suggestedName);
            suffixes.Insert(0, extension);
            suggestedName = System.IO.Path.GetFileNameWithoutExtension(suggestedName);
        }
        
        #if __IOS__
        var picker = new CustomFileSavePicker(xamlRoot)
        #else
        var picker = new FileSavePicker
        #endif
        {
            SuggestedStartLocation = PickerLocationId.Downloads,
            SuggestedFileName = suggestedName
        };

        if (suffixes is not { Count: > 0 })
            return await picker.PickSaveFileAsync() ?? throw new TaskCanceledException();

        for (var i = suffixes.Count - 1; i >= 0; i--)
        {
            var suffix = suffixes[i].TrimStart('.');
            if (string.IsNullOrWhiteSpace(suffix))
                suffixes.RemoveAt(i);
            else
                suffixes[i] = $".{suffix}";
        }

        picker.FileTypeChoices.Add(type, suffixes);



#if WINDOWS
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(WebView2Extensions.WinUiMainWindow);
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
#elif BROWSERWASM
        // TODO: Switch to LocalFolderCache path?
        var path = Windows.Storage.ApplicationData.Current.LocalCacheFolder.Path;
        Log.WriteLine($"LocalCacheFolder.Path = {path}");
        
        if (!Directory.Exists("/cache"))
            Directory.CreateDirectory("/cache");
#endif

        return await picker.PickSaveFileAsync() ??  throw new TaskCanceledException();

    }

    public static Task ShowExceptionDialogAsync(XamlRoot xamlRoot, string title, Exception e)
        => ShowErrorDialogAsync(xamlRoot, title, e is TaskCanceledException ? "Task Cancelled" : e.ToString());
    
    public static async Task ShowErrorDialogAsync(XamlRoot xamlRoot, string title, string? error)
    {
        ContentDialog cd = new ()
        {
            XamlRoot = xamlRoot,
            Title = title,
            Content = string.IsNullOrWhiteSpace(error)
                ? "Unknown failure"
                : new ScrollViewer
                    {
                        Content = new TextBlock
                        {
                            Text = error,
                            TextWrapping = TextWrapping.WrapWholeWords,
                            MaxLines = 1000
                        },
                        HorizontalScrollBarVisibility = ScrollBarVisibility.Visible,
                        VerticalScrollBarVisibility = ScrollBarVisibility.Visible
                    },
            PrimaryButtonText = "OK"
        };
        await cd.ShowAsync();
    }

    internal class BusyDialog
    {
        #region  Fields

        private readonly TaskCompletionSource<bool> _tcs = new();
        private readonly CancellationToken _cancellationToken;

        private readonly Func<CancellationToken, Task> _function;
        
        private readonly ContentDialog _contentDialog;
        private readonly bool _hideAfterOnLoadedTaskComplete;
        
        #endregion

        public static async Task Create(
            XamlRoot xamlRoot, 
            string title,
            Func<CancellationToken, Task> processFunction, 
            bool hasCancelButton = true,
            bool hideAfterOnContentLoadedTaskComplete = false,
            CancellationToken cancellationToken = default)
        {
            var processor = new BusyDialog(xamlRoot, title, processFunction, hasCancelButton, hideAfterOnContentLoadedTaskComplete, cancellationToken);
            await processor.ProcessAsync();
        }
        
        private BusyDialog(
            XamlRoot xamlRoot, 
            string title, 
            Func<CancellationToken, Task> processFunction,
            bool hasCancelButton = false, 
            bool hideAfterOnContentLoadedTaskComplete = false,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(xamlRoot);
            ArgumentNullException.ThrowIfNull(processFunction);
            
            
            CancellationTokenSource uiCancellationTokenSource = new ();
            _hideAfterOnLoadedTaskComplete = hideAfterOnContentLoadedTaskComplete;
            _cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(uiCancellationTokenSource.Token, cancellationToken).Token;

            _function = processFunction;
            var title1 = title;
            
            _contentDialog = new ContentDialog
            {            
                XamlRoot = xamlRoot,
                Title = title1,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Margin = new Thickness(0),
                Padding = new Thickness(0),
                Content =  new ProgressRing
                    {
                        IsActive = true
                    },
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center
            };

            _contentDialog.Loaded += OnLoaded;

            if (!hasCancelButton)
                return;

            _contentDialog.CloseButtonText = "CANCEL";
            _contentDialog.CloseButtonCommand = new RelayCommand(_ => uiCancellationTokenSource.Cancel());
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                await _function(_cancellationToken);
                _contentDialog.Content = "COMPLETED";
                _contentDialog.CloseButtonText = "OK";
                _tcs.TrySetResult(true);
                if (!_hideAfterOnLoadedTaskComplete)
                    await Task.Delay(3000, _cancellationToken);
            }
            catch (Exception ex)
            {
                _tcs.TrySetException(ex);
            }
            finally
            {
                _contentDialog.CloseButtonText = "OK";
                _contentDialog.Hide();
                _contentDialog.Loaded -= OnLoaded;
            }
        }

        private async Task ProcessAsync()
        {
            await _contentDialog.ShowAsync();
            await _tcs.Task;
        }

    }

    internal class AuxiliaryWebViewAsyncProcessor<T> 
    {
        
        #region  Fields

        private readonly TaskCompletionSource<T> _tcs = new();
        private readonly CancellationToken _cancellationToken;

        private readonly Func<WebView2, CancellationToken, Task<T>> _onContentLoadedTask;
        private readonly Func<WebView2, CancellationToken, Task> _loadContentAction;
        
        private readonly ContentDialog _contentDialog;

        private readonly WebView2 _webView2 = new()
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch
        };
        private readonly ProgressRing _progressRing;

        private readonly bool _showWebContent;
        private readonly bool _hideAfterOnLoadedTaskComplete;
        #endregion

        public static async Task<T> Create(
            XamlRoot xamlRoot, 
            string html,
            Func<WebView2, CancellationToken, Task<T>> onContentLoadedTask, 
            bool showWebContent = false,
            bool hasCancelButton = true,
            bool hideAfterOnContentLoadedTaskComplete = false,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(html))
                throw new ArgumentNullException(nameof(html));
            
            var processor = new AuxiliaryWebViewAsyncProcessor<T>(xamlRoot, LoadHtml, onContentLoadedTask, showWebContent, hasCancelButton, hideAfterOnContentLoadedTaskComplete, cancellationToken);
            return await processor.ProcessAsync();
            
            Task LoadHtml(WebView2 webView, CancellationToken localToken)
            {
                webView.CoreWebView2.NavigateToString(html);
                return Task.CompletedTask;
            }

        }

        public static async Task<T> Create(
            XamlRoot xamlRoot,
            Func<WebView2, Task> beforeContentSetTask,
            string html,
            Func<WebView2, CancellationToken, Task<T>> onContentLoadedTask,
            bool showWebContent = false,
            bool hasCancelButton = true,
            bool hideAfterOnContentLoadedTaskComplete = false,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(html))
                throw new ArgumentNullException(nameof(html));

            var processor = new AuxiliaryWebViewAsyncProcessor<T>(xamlRoot, LoadHtml, onContentLoadedTask, showWebContent, hasCancelButton, hideAfterOnContentLoadedTaskComplete, cancellationToken);
            return await processor.ProcessAsync();

            async Task LoadHtml(WebView2 webView, CancellationToken localToken)
            {
                await beforeContentSetTask(webView);
                webView.CoreWebView2.NavigateToString(html);
            }

        }


        private AuxiliaryWebViewAsyncProcessor(
            XamlRoot xamlRoot, 
            Func<WebView2, CancellationToken, Task> loadContentAction, 
            Func<WebView2, CancellationToken, Task<T>> onContentLoadedTask, 
            bool showWebContent, 
            bool hasCancelButton, 
            bool hideAfterOnContentLoadedTaskComplete,
            CancellationToken cancellationToken) 
        {
            ArgumentNullException.ThrowIfNull(xamlRoot);
            ArgumentNullException.ThrowIfNull(loadContentAction);
            ArgumentNullException.ThrowIfNull(onContentLoadedTask);

            CancellationTokenSource uiCancellationTokenSource = new ();
            _cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(uiCancellationTokenSource.Token, cancellationToken).Token;

            _loadContentAction = loadContentAction;
            _onContentLoadedTask = onContentLoadedTask;
            _showWebContent = showWebContent;
            _hideAfterOnLoadedTaskComplete = hideAfterOnContentLoadedTaskComplete;

            var rectangle = new Rectangle
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Fill = (Brush)Application.Current.Resources["ContentDialogBackground"],
                Visibility = showWebContent ? Visibility.Collapsed : Visibility.Visible
            };
            
            _progressRing = new ProgressRing { IsActive = true };
            
            _contentDialog = new ContentDialog
            {            
                XamlRoot = xamlRoot,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Margin = new Thickness(0),
                Padding = new Thickness(0),
                Content = new Grid
                    {
                        Children = { _webView2,rectangle, _progressRing }
                    }
            };

            _webView2.Opacity = showWebContent ? 1 : 0.01;
            
#if WINDOWS
            if (showWebContent)
            {
                _contentDialog.Resources["ContentDialogPadding"] = new Thickness(4);
                _contentDialog.Resources["ContentDialogMaxWidth"] = xamlRoot.Size.Width * 0.75;
                _contentDialog.Resources["ContentDialogMaxHeight"] = xamlRoot.Size.Height * 0.75;
                _contentDialog.Resources["ContentDialogMinWidth"] = xamlRoot.Size.Width * 0.75;
                _contentDialog.Resources["ContentDialogMinHeight"] = xamlRoot.Size.Height * 0.75;
                _contentDialog.Resources["HorizontalContentAlignment"] = HorizontalAlignment.Stretch;
                _contentDialog.Resources["VerticalContentAlignment"] = VerticalAlignment.Stretch;
            }
#endif
            _webView2.Loaded += OnLoaded;
                
            if (!hasCancelButton)
                return;

            _contentDialog.CloseButtonText = "CANCEL";
            _contentDialog.CloseButtonCommand = new RelayCommand(_ => uiCancellationTokenSource.Cancel());
            
        }

        private async Task<T> ProcessAsync()
        {
            await _contentDialog.ShowAsync();
            return await _tcs.Task;
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                await Task.Delay(1000, _cancellationToken);  // without this, WinSdk version crashes

                await _webView2.EnsureCoreWebView2Async().AsTask(_cancellationToken);
                await _loadContentAction.Invoke(_webView2, _cancellationToken);

                await _webView2.WaitForDocumentLoadedAsync(_cancellationToken);

                if (_showWebContent)
                    _progressRing.Visibility = Visibility.Collapsed;

                var result = await _onContentLoadedTask(_webView2, _cancellationToken);
                _contentDialog.Content = "COMPLETED";
                _contentDialog.CloseButtonText = "OK";
                _tcs.TrySetResult(result);
                if (!_hideAfterOnLoadedTaskComplete)
                    await Task.Delay(3000, _cancellationToken);
            }
            catch (Exception ex)
            {
                _tcs.TrySetException(ex);
            }
            finally
            {
                _contentDialog.Hide();
                _contentDialog.Loaded -= OnLoaded;
            }
        }


    }
    
    
    internal class RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
        : ICommand
    {
        private readonly Action<object?> _execute = execute ?? throw new ArgumentNullException(nameof(execute));

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter) => canExecute?.Invoke(parameter) ?? true;

        public void Execute(object? parameter) => _execute(parameter);

        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

}

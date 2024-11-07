using MahApps.Metro.Controls;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Minimal.Mvvm.Windows
{
    /// <summary>
    /// The MetroTabbedDocumentService class is responsible for managing tabbed documents within a UI that utilizes the Metro design language. 
    /// It extends the DocumentServiceBase and implements interfaces for asynchronous document management and disposal. 
    /// This service allows for the creation, binding, and lifecycle management of tabbed documents within MetroTabControl.
    /// </summary>
    public sealed class MetroTabbedDocumentService: DocumentServiceBase<MetroTabControl>, IAsyncDocumentManagerService, IAsyncDisposable
    {
        #region TabbedDocument

        private class TabbedDocument : AsyncDisposable, IAsyncDocument
        {
            private readonly AsyncLifetime _lifetime = new(continueOnCapturedContext: true);
            private bool _isClosing;

            public TabbedDocument(MetroTabbedDocumentService owner, MetroTabItem tab)
            {
                _ = owner ?? throw new ArgumentNullException(nameof(owner));
                Tab = tab ?? throw new ArgumentNullException(nameof(tab));

                _lifetime.AddBracket(() => owner._documents.Add(this), () => owner._documents.Remove(this));
                _lifetime.Add(() => TabControl?.Items.Remove(Tab));//second, remove tab item
                _lifetime.Add(() => Tab.ClearStyle());//first, clear tab item style
                _lifetime.Add(DetachContent);//second, detach content
                _lifetime.AddAsync(DisposeViewModelAsync);//first, dispose vm
                _lifetime.AddBracket(() => SetDocument(Tab, this), () => SetDocument(Tab, null));
                _lifetime.AddBracket(
                    () => Tab.IsVisibleChanged += OnTabIsVisibleChanged,
                    () => Tab.IsVisibleChanged -= OnTabIsVisibleChanged);
                _lifetime.AddBracket(
                    () => ViewModelHelper.SetViewTitleBinding(Tab.Content, HeaderedContentControl.HeaderProperty, Tab),
                    () => ViewModelHelper.ClearViewTitleBinding(HeaderedContentControl.HeaderProperty, Tab));

                var dpd = DependencyPropertyDescriptor.FromProperty(HeaderedContentControl.HeaderProperty, typeof(HeaderedContentControl));
                Debug.Assert(dpd != null);
                if (dpd != null)
                {
                    _lifetime.AddBracket(
                        () => dpd.AddValueChanged(Tab, OnTabHeaderChanged),
                        () => dpd.RemoveValueChanged(Tab, OnTabHeaderChanged));
                }
            }

            #region Properties

            public bool DisposeOnClose { get; set; }

            public object Id { get; set; } = null!;

            private MetroTabItem Tab { get; }

            private MetroTabControl? TabControl => Tab.Parent as MetroTabControl;

            public string? Title
            {
                get => Tab.Header?.ToString();
                set => Tab.Header = value;
            }

            #endregion

            #region Event Handlers

            private void OnTabHeaderChanged(object? sender, EventArgs e)
            {
                OnPropertyChanged(nameof(Title));
            }

            private void OnTabIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
            {
                if (Tab.Content is UIElement element)
                {
                    element.Visibility = Tab.Visibility;
                }
            }

            #endregion

            #region Methods

            public async ValueTask CloseAsync(bool force = true)
            {
                if (_isClosing || IsDisposing)
                {
                    return;
                }
                if (_lifetime.IsTerminated)
                {
                    return;
                }
                try
                {
                    _isClosing = true;
                    if (!force)
                    {
                        var viewModel = ViewModelHelper.GetViewModelFromView(Tab.Content);
                        if (viewModel is DocumentContentViewModelBase viewModelBase && viewModelBase.CanClose() == false)
                        {
                            return;
                        }
                    }
                    CloseTab();
                    await DisposeAsync().ConfigureAwait(false);
                }
                finally
                {
                    _isClosing = false;
                }
            }

            private void CloseTab()
            {
                Tab.Visibility = Visibility.Collapsed;
                if (Tab.CloseTabCommand != null)
                {
                    if (Tab.CloseTabCommand is RoutedCommand command)
                    {
                        command.Execute(Tab.CloseTabCommandParameter, Tab);
                    }
                    else if (Tab.CloseTabCommand.CanExecute(Tab.CloseTabCommandParameter))
                    {
                        Tab.CloseTabCommand.Execute(Tab.CloseTabCommandParameter);
                    }
                }
            }

            private void DetachContent()
            {
                var view = Tab.Content;
                Debug.Assert(view != null);
                //First, detach DataContext from view
                ViewModelHelper.DetachViewModel(view);
                //Second, detach Content from tab item
                Tab.Content = null;
            }

            private async ValueTask DisposeViewModelAsync()
            {
                var viewModel = ViewModelHelper.GetViewModelFromView(Tab.Content);
                Debug.Assert(viewModel != null);
                if (DisposeOnClose && viewModel is IAsyncDisposable asyncDisposable)
                {
                    await asyncDisposable.DisposeAsync().ConfigureAwait(false);
                }
            }

            protected override ValueTask OnDisposeAsync()
            {
                return _lifetime.DisposeAsync();
            }

            public void Hide()
            {
                if (Tab.Visibility != Visibility.Collapsed)
                {
                    Tab.Visibility = Visibility.Collapsed;
                }
            }

            public void Show()
            {
                if (Tab.Visibility != Visibility.Visible)
                {
                    Tab.Visibility = Visibility.Visible;
                }
                Tab.IsSelected = true;
            }

            #endregion
        }

        #endregion

        private readonly ObservableCollection<IAsyncDocument> _documents = new();
        private bool _isActiveDocumentChanging;
        private IDisposable? _subscription;

        #region Dependency Properties

        public static readonly DependencyProperty ActiveDocumentProperty = DependencyProperty.Register(
            nameof(ActiveDocument), typeof(IAsyncDocument), typeof(MetroTabbedDocumentService), 
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, (d, e) => ((MetroTabbedDocumentService)d).OnActiveDocumentChanged(e.OldValue as IAsyncDocument, e.NewValue as IAsyncDocument)));

        public static readonly DependencyProperty CloseButtonEnabledProperty = DependencyProperty.Register(
            nameof(CloseButtonEnabled), typeof(bool), typeof(MetroTabbedDocumentService), 
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure));

        public static readonly DependencyProperty UnresolvedViewTypeProperty = DependencyProperty.Register(
            nameof(UnresolvedViewType), typeof(Type), typeof(MetroTabbedDocumentService));

        #endregion

        public MetroTabbedDocumentService()
        {
            if (ViewModelBase.IsInDesignMode) return;
            (_documents as INotifyPropertyChanged).PropertyChanged += OnDocumentsPropertyChanged;
        }

        #region Events

        public event EventHandler<ActiveDocumentChangedEventArgs>? ActiveDocumentChanged;

        #endregion

        #region Properties

        public IAsyncDocument? ActiveDocument
        {
            get => (IAsyncDocument)GetValue(ActiveDocumentProperty);
            set => SetValue(ActiveDocumentProperty, value);
        }

        public IEnumerable<IAsyncDocument> Documents => _documents;

        public bool CloseButtonEnabled
        {
            get => (bool)GetValue(CloseButtonEnabledProperty);
            set => SetValue(CloseButtonEnabledProperty, value);
        }

        public int Count => _documents.Count;

        public Type? UnresolvedViewType
        {
            get => (Type)GetValue(UnresolvedViewTypeProperty);
            set => SetValue(UnresolvedViewTypeProperty, value);
        }

        #endregion

        #region Event Handlers

        private void OnActiveDocumentChanged(IAsyncDocument? oldValue, IAsyncDocument? newValue)
        {
            if (!_isActiveDocumentChanging)
            {
                try
                {
                    _isActiveDocumentChanging = true;
                    newValue?.Show();
                }
                finally
                {
                    _isActiveDocumentChanging = false;
                }
            }
            ActiveDocumentChanged?.Invoke(this, new ActiveDocumentChangedEventArgs(oldValue, newValue));
        }

        private void OnDocumentsPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(_documents.Count))
            {
                OnPropertyChanged(nameof(Count));
            }
        }

        private static async void OnTabControlItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                case NotifyCollectionChangedAction.Remove:
                    if (e.OldItems is { Count: > 0 })
                    {
                        foreach (var item in e.OldItems)
                        {
                            if (item is not MetroTabItem tab)
                            {
                                continue;
                            }
                            var document = GetDocument(tab);
                            if (document is not null)
                            {
                                await document.CloseAsync(force: true);
                            }
                        }
                    }
                    break;
            }
        }

        private void OnTabControlLoaded(object sender, RoutedEventArgs e)
        {
            Debug.Assert(Equals(sender, AssociatedObject));
            if (sender is FrameworkElement fe)
            {
                fe.Loaded -= OnTabControlLoaded;
            }
            Debug.Assert(_subscription == null);
            Disposable.DisposeAndNull(ref _subscription);
            _subscription = SubscribeTabControl(AssociatedObject);
        }

        private void OnTabControlSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isActiveDocumentChanging)
            {
                return;
            }
            Debug.Assert(Equals(sender, AssociatedObject));
            if (sender is MetroTabControl tabControl)
            {
                try
                {
                    _isActiveDocumentChanging = true;
                    ActiveDocument = (tabControl.SelectedItem is TabItem tabItem) ? GetDocument(tabItem) : null;
                }
                finally
                {
                    _isActiveDocumentChanging = false;
                }
            }
        }

        private static void OnTabControlTabItemClosingEvent(object? sender, BaseMetroTabControl.TabItemClosingEventArgs e)
        {
            var viewModel = ViewModelHelper.GetViewModelFromView(e.ClosingTabItem.Content);
            if (viewModel is DocumentContentViewModelBase viewModelBase)
            {
                e.Cancel = viewModelBase.CanClose() == false;
            }
        }

        #endregion

        #region Methods

        public async ValueTask<IAsyncDocument> CreateDocumentAsync(string? documentType, object? viewModel, object? parentViewModel, object? parameter, CancellationToken cancellationToken = default)
        {
            Throw.IfNull(AssociatedObject);
            cancellationToken.ThrowIfCancellationRequested();
            object? view;
            if (documentType == null && ViewTemplate == null && ViewTemplateSelector == null)
            {
                view = GetUnresolvedViewType() ?? await GetViewLocator().GetOrCreateViewAsync(documentType, cancellationToken);
                await GetViewLocator().InitializeViewAsync(view, viewModel, parentViewModel, parameter, cancellationToken);
            }
            else
            {
                view = await CreateAndInitializeViewAsync(documentType, viewModel, parentViewModel, parameter, cancellationToken);
            }
            var tab = new MetroTabItem
            {
                Header = "Item",
                Content = view,
                CloseButtonEnabled = CloseButtonEnabled
            };
            AssociatedObject.Items.Add(tab);
            var document = new TabbedDocument(this, tab);
            return document;
        }

        public async ValueTask DisposeAsync()
        {
            try
            {
                if (_documents.Count == 0)
                {
                    return;
                }
                await Task.WhenAll(_documents.ToList().Select(x => x.CloseAsync().AsTask()));
                if (_documents.Count == 0)
                {
                    return;
                }

                var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

                void OnDocumentsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
                {
                    if (_documents.Count == 0)
                    {
                        tcs.TrySetResult(true);
                    }
                }

                _documents.CollectionChanged += OnDocumentsCollectionChanged;
                try
                {
                    if (_documents.Count == 0)
                    {
                        tcs.TrySetResult(true);
                    }
                    await tcs.Task.ConfigureAwait(false);
                }
                finally
                {
                    _documents.CollectionChanged -= OnDocumentsCollectionChanged;
                }
            }
            finally
            {
                (_documents as INotifyPropertyChanged).PropertyChanged -= OnDocumentsPropertyChanged;
            }
        }

        private object? GetUnresolvedViewType()
        {
            return UnresolvedViewType == null ? null : Activator.CreateInstance(UnresolvedViewType);
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            Debug.Assert(_subscription == null);
            if (AssociatedObject!.IsLoaded)
            {
                Disposable.DisposeAndNull(ref _subscription);
                _subscription = SubscribeTabControl(AssociatedObject);
            }
            else
            {
                AssociatedObject.Loaded += OnTabControlLoaded;
            }
        }

        protected override void OnDetaching()
        {
            AssociatedObject!.Loaded -= OnTabControlLoaded;
            Debug.Assert(_subscription != null);
            Disposable.DisposeAndNull(ref _subscription);
            base.OnDetaching();
        }

        private Lifetime? SubscribeTabControl(BaseMetroTabControl? tabControl)
        {
            if (tabControl == null)
            {
                return null;
            }
            if (tabControl.ItemsSource != null)
            {
                throw new InvalidOperationException("Can't use not null ItemsSource in this service");
            }
            var lifetime = new Lifetime();
            if (tabControl.Items is INotifyCollectionChanged collection)
            {
                lifetime.AddBracket(() => collection.CollectionChanged += OnTabControlItemsCollectionChanged,
                    () => collection.CollectionChanged -= OnTabControlItemsCollectionChanged);
            }
            lifetime.AddBracket(() => tabControl.SelectionChanged += OnTabControlSelectionChanged,
                () => tabControl.SelectionChanged -= OnTabControlSelectionChanged);
            lifetime.AddBracket(() => tabControl.TabItemClosingEvent += OnTabControlTabItemClosingEvent,
                () => tabControl.TabItemClosingEvent -= OnTabControlTabItemClosingEvent);
            return lifetime;
        }

        #endregion
    }
}

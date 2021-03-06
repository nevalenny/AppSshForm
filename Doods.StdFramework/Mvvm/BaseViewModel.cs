﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Doods.StdFramework.Interfaces;
using Doods.StdFramework.Mvvm;

namespace Doods.StdFramework
{
    /// <summary>
    /// Base view model.
    /// </summary>
    public class BaseViewModel : ObservableObject, IViewModel, IBuzy
    {
        private bool _isLoad;
        protected bool ReloadOnAppearing;
        private int _busyCount;


        public int BusyCount
        {
            get => _busyCount;
            set
            {
                SetProperty(ref _busyCount, value);
                SetPropertyChanged(nameof(IsBusy));
            }
        }

        private ViewModelState _viewModelState;

        public ViewModelState ViewModelState
        {
            get => _viewModelState;
            set => SetProperty(ref _viewModelState, value);
        }

        private CancellationTokenSource _cts = new CancellationTokenSource();
        protected ILogger Logger { get; }
        protected CancellationToken Token => _cts.Token;

        protected BaseViewModel(ILogger logger)
        {
            Title = GetType().Name.Replace("ViewModel", "");
            Logger = logger;
            Logger.Info($"{Title} : opened.");
        }

        private string _title = string.Empty;

        /// <summary>
        /// Obtient ou définit le titre.
        /// </summary>
        /// <value>LE titre.</value>
        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        private string subtitle = string.Empty;

        /// <summary>
        ///Obtient ou définit le sous-titre.
        /// </summary>
        /// <value>Le sous-titre..</value>
        public string Subtitle
        {
            get => subtitle;
            set => SetProperty(ref subtitle, value);
        }

        private string icon = string.Empty;

        /// <summary>
        ///Obtient ou définit l'icône.
        /// </summary>
        /// <value>L'icône.</value>
        public string Icon
        {
            get => icon;
            set => SetProperty(ref icon, value);
        }


        /// <summary>
        /// Obtient ou définit une valeur indiquant si cette instance est occupée.
        /// </summary>
        /// <value><c>true</c> Si cette instance est occupée; autrement,<c>false</c>.</value>
        public bool IsBusy => BusyCount > 0;


        /// <summary>
        /// Obtient ou définit une valeur indiquant si cette instance n'est pas occupée.
        /// </summary>
        /// <value><c>True</c> si cette instance n'est pas occupée; autrement, <c>false</c>.</value>
        public bool IsNotBusy => !IsBusy;


        private bool canLoadMore = true;

        /// <summary>
        /// Obtient ou définit une valeur indiquant si cette instance peut charger plus.
        /// </summary>
        /// <value><c>true</c> si cette instance peut charger plus; autrement, <c>false</c>.</value>
        public bool CanLoadMore
        {
            get => canLoadMore;
            set => SetProperty(ref canLoadMore, value);
        }


        private string _header = string.Empty;

        /// <summary>
        /// Obtient ou définit l'en-tête.
        /// </summary>
        /// <value>L'en-tête.</value>
        public string Header
        {
            get => _header;
            set => SetProperty(ref _header, value);
        }

        private string _footer = string.Empty;

        /// <summary>
        /// Obtient ou définit le pied de page.
        /// </summary>
        /// <value>Le pied de page.</value>
        public string Footer
        {
            get => _footer;
            set => SetProperty(ref _footer, value);
        }

        public async Task OnAppearing()
        {
            if (_cts.IsCancellationRequested) _cts = new CancellationTokenSource();

            if (!_isLoad || ReloadOnAppearing)
            {
                _isLoad = true;
                await StartLoading();
            }

            await OnInternalAppearing();
        }

        /// <summary>
        /// Méthode appelé si le lors du premier appairage ou si l’on force l’option <c>ReloadOnAppearing</c> à true.
        /// </summary>
        /// <returns></returns>
        public async Task StartLoading()
        {
            ViewModelState = ViewModelState.Loading;
            BusyCount++;

            try
            {
                var delayTask = Task.Delay(TimeSpan.FromMilliseconds(50));
                var executeTask = ExecuteAsync(token => Load(), true);

                await Task.WhenAll(delayTask, executeTask);

                if (ViewModelState == ViewModelState.Loading)
                    ViewModelState = ViewModelState.Loaded;
            }
            catch (Exception e)
            {
                ViewModelState = ViewModelState.Failed;
                Logger.Error(e);
                throw;
            }
            finally
            {
                OnLoadingCallBack();
                BusyCount--;
            }
        }

        protected async Task ExecuteAsync(Func<CancellationToken, Task> action, bool safeExecution = false)
        {
            try
            {
                Token.ThrowIfCancellationRequested();
                await action(Token);
            }
            catch (AggregateException e)
            {
                var innerException = e.InnerException;
                while (innerException.InnerException != null)
                    innerException = innerException.InnerException;

                //TODO : LOG + HokeyApp
                if (!safeExecution) throw;
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
                if (!safeExecution) throw;
            }
        }

        /// <summary>
        /// Appelé par le méthode <c>StartLoading</c>.
        /// </summary>
        /// <returns></returns>
        protected virtual Task Load()
        {
            return Task.FromResult(0);
        }

        protected virtual Task OnInternalAppearing()
        {
            return Task.FromResult(0);
        }

        private void OnLoadingCallBack()
        {
            OnInternalLoadingCallBack();
        }

        protected virtual void OnInternalLoadingCallBack()
        {
            //NP
        }

        public Task OnDisappearing()
        {
            CancelExecutions();

            return OnInternalDisappearing();
        }

        protected virtual Task OnInternalDisappearing()
        {
            return Task.FromResult(0);
        }

        public void CancelExecutions()
        {
            if (Token.CanBeCanceled && !_cts.IsCancellationRequested)
                _cts.Cancel();
        }
    }
}
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Doods.StdFramework.Navigation
{
    // TODO THE rendre ca exploitable ;).
    public  class BaseNavigationService
    {
        private static bool _isNavigating;

        public static INavigation Navigation { get; set; }

        public static Page CurrentPage
        {
            get { return Navigation.NavigationStack.FirstOrDefault(); }
        }

        public static Page CurrentModalPage
        {
            get { return Navigation.ModalStack.FirstOrDefault(); }
        }

        public static void RemovePageFromHistory(Type type)
        {
            var last = Navigation.NavigationStack.ToList().First(p => p.GetType() == type);
            Navigation.RemovePage(last);
        }

        public static void ClearHistory()
        {
            foreach (var page in Navigation.NavigationStack.ToList())
                Navigation.RemovePage(page);
        }

        private static async Task PushAsync(INavigation navigation, Page page, bool animate = true)
        {
            if (_isNavigating) return;
            _isNavigating = true;

            await navigation.PushAsync(page, animate);
            _isNavigating = false;


        }

        private static async Task PushModalAsync(INavigation navigation, Page page, bool animate = true)
        {
            if (_isNavigating) return;
            _isNavigating = true;

            await navigation.PushModalAsync(page, animate);
            _isNavigating = false;
        }

        private static async Task PopAsync(INavigation navigation, bool animate = true)
        {
            if (_isNavigating) return;
            _isNavigating = true;

            await navigation.PopAsync(animate);
            _isNavigating = false;
        }

        private static async Task PopModalAsync(INavigation navigation, bool animate = true)
        {
            if (_isNavigating) return;
            _isNavigating = true;

            await navigation.PopModalAsync(animate);
            _isNavigating = false;
        }

        private static async Task PopToRootAsync(INavigation navigation, bool animate = true)
        {
            if (_isNavigating) return;
            _isNavigating = true;

            await navigation.PopToRootAsync(animate);
            _isNavigating = false;
        }

        public static async Task GoBackToRoot()
        {
            await PopToRootAsync(Navigation);
        }



     
        public static Task GoBack()
        {
            return PopAsync(Navigation);
        }
    }
}

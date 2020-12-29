﻿using System;
using System.Linq;
using System.Reflection;
using GS.Server.Main;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using GS.Shared;


namespace GS.Server
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    [ComVisible(false)]
    public partial class App: IDisposable
    {
        // give the mutex a  unique name
        private const string MutexName = "Green Swamp Server";
        // declare the mutex
        private readonly Mutex _mutex;
        // overload the constructor
        private readonly bool createdNew;
        public App()
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomainOnAssemblyResolve;

            // overloaded mutex constructor which outs a boolean
            // telling if the mutex is new or not.
            // see http://msdn.microsoft.com/en-us/library/System.Threading.Mutex.aspx
            _mutex = new Mutex(true, MutexName, out createdNew);
            if (createdNew) return;
            // if the mutex already exists, notify and quit
            MessageBox.Show("GS Server is already running", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            Current.Shutdown(0);
        }

        /// <summary>
        /// Hack to get around the strong name and material design not load correctly
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        private Assembly CurrentDomainOnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            var assemblyName = new AssemblyName(args.Name);
            var assembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => a.GetName().Name == assemblyName.Name);
            return assembly;
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            if (!createdNew) return;
            if (Server.Properties.Server.Default.DisableHardwareAcceleration)
            {
                RenderOptions.ProcessRenderMode = System.Windows.Interop.RenderMode.SoftwareOnly;
            }
            base.OnStartup(e);
            Languages.SetLanguageDictionary(false, LanguageApp.GSServer);
            var app = new MainWindow();
            var context = new MainWindowVM();
            app.DataContext = context;
            app.Show();
        }


        #region Dispose
        public void Dispose()
        {
            Dispose(true);
            // GC.SuppressFinalize(this);
        }
        // NOTE: Leave out the finalizer altogether if this class doesn't
        // own unmanaged resources itself, but leave the other methods
        // exactly as they are.
        ~App()
        {
            // Finalizer calls Dispose(false)
            Dispose(false);
        }
        // The bulk of the clean-up code is implemented in Dispose(bool)
        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                _mutex?.Dispose();
            }
            // free native resources if there are any.
            //if (nativeResource != IntPtr.Zero)
            //{
            //    Marshal.FreeHGlobal(nativeResource);
            //    nativeResource = IntPtr.Zero;
            //}
        }
        #endregion
    }
}

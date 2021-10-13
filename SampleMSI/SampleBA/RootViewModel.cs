using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using Microsoft.Tools.WindowsInstallerXml.Bootstrapper;

namespace SampleBA
{
    public enum Error
    {
        UserCancelled = 1223,
    }
    public class RootViewModel : PropertyNotifyBase
    {
        private ICommand cancelCommand;
        private ICommand closeCommand;



        private bool canceled;
        private InstallationState installState;
        private DetectionState detectState;

        public InstallationViewModel installationViewModel { get; private set; }
        public UpdateViewModel updateViewModel { get; private set; }
        public ProgressViewModel progressViewModel { get; private set; }

        public RootViewModel()
        {
            installationViewModel = new InstallationViewModel(this);
            updateViewModel = new UpdateViewModel(this);
            progressViewModel = new ProgressViewModel(this);
        }


        public IntPtr ViewWindowHandle { get; set; }

        public ICommand CancelCommand
        {
            get
            {
                if (this.cancelCommand == null)
                {
                    this.cancelCommand = new RelayCommand(param =>
                    {
                        lock (this)
                        {
                            this.Canceled = (MessageBoxResult.Yes == MessageBox.Show(CustomBA.View, "Are you sure you want to cancel?", "Custom installer", MessageBoxButton.YesNo, MessageBoxImage.Error));
                        }
                    },
                    param => this.installState == InstallationState.Applying );
                }
                return this.cancelCommand;
            }
        }

        public ICommand CloseCommand
        {
            get
            {
                if (this.closeCommand == null)
                {
                    this.closeCommand = new RelayCommand(param =>
                    {
                        CustomBA.View.Close();
                    });
                }
                return this.closeCommand;
            }
        }

        public bool CancelEnabled
        {
            get
            {
                return this.CancelCommand.CanExecute(this);
            }
        }

        public bool Canceled
        {
            get
            {
                return this.canceled;
            }

            set
            {
                if (this.canceled != value)
                {
                    this.canceled = value;
                    base.OnPropertyChanged("Canceled");
                }
            }
        }


        public InstallationState InstallState
        {
            get
            {
                return this.installState;
            }

            set
            {
                if (this.installState != value)
                {
                    this.installState = value;
                    base.OnPropertyChanged("InstallState");
                    base.OnPropertyChanged("CancelEnabled");
                }
            }
        }

        public DetectionState DetectState
        {
            get
            {
                return this.detectState;
            }
            set
            {
                if (this.detectState != value)
                {
                    this.detectState = value;
                    base.OnPropertyChanged("DetectState");
                    base.OnPropertyChanged("CancelEnabled");
                }
            }
        }

        /// <summary>
        /// Gets and sets the state of the view's model before apply begins in order to return to that state if cancel or rollback occurs.
        /// </summary>
        public InstallationState PreApplyState { get; set; }

        /// <summary>
        /// Gets and sets the path where the bundle is currently installed or will be installed.
        /// </summary>
        public string InstallDirectory
        {
            get
            {
                return CustomBA.Model.InstallDirectory;
            }

            set
            {
                if (CustomBA.Model.InstallDirectory != value)
                {
                    CustomBA.Model.InstallDirectory = value;
                    base.OnPropertyChanged("InstallDirectory");
                }
            }
        }



    }
}

using System;
using System.Windows.Input;
using System.Windows;
using Microsoft.Tools.WindowsInstallerXml.Bootstrapper;
using IO = System.IO;
using System.Reflection;

namespace SampleBA
{
    public enum InstallationState
    {
        Initializing,
        Detecting,
        Waiting,
        Planning,
        Applying,
        Applied,
        Failed,
    }

    public enum DetectionState
    {
        Absent,
        Present,
        Newer,
    }

    public class InstallationViewModel : PropertyNotifyBase
    {
        private RootViewModel root;
        private bool welcomeEnabled = true;
        private bool licenseEnabled = false;
        private bool settingsEnabled = false;
        private bool installEnabled = false;
        private bool finishEnabled = false;


        //private Dictionary<string, int> downloadRetries;
        private bool downgrade;

        //private ICommand completeCommand;
        private ICommand licenseCommand;
        private ICommand installCommand;
        private ICommand repairCommand;
        private ICommand uninstallCommand;
        private ICommand tryAgainCommand;

        private string message;
        private DateTime cachePackageStart;
        private DateTime executePackageStart;

        public InstallationViewModel(RootViewModel root)
        {
            this.root = root;
        }

        //public ICommand CompleteCommand
        //{

        //}

        public string Message
        {
            get
            {
                return this.message;
            }

            set
            {
                if (this.message != value)
                {
                    this.message = value;
                    base.OnPropertyChanged("Message");
                }
            }
        }

        public bool WelcomeEnabled
        {
            get { return this.welcomeEnabled; }
            set
            {
                if (this.welcomeEnabled != value)
                {
                    this.welcomeEnabled = value;
                    base.OnPropertyChanged("WelcomeEnabled");
                }
            }
        }
        public bool LicenseEnabled
        {
            get { return this.licenseEnabled; }
            set
            {
                if (this.licenseEnabled != value)
                {
                    this.licenseEnabled = value;
                    base.OnPropertyChanged("LicenseEnabled");
                }
            }
        }
        public bool SettingsEnabled
        {
            get { return this.settingsEnabled; }
            set
            {
                if (this.settingsEnabled != value)
                {
                    this.settingsEnabled = value;
                    base.OnPropertyChanged("SettingsEnabled");
                }
            }
        }
        public bool InstallEnabled
        {
            get { return this.installEnabled; }
            set
            {
                if (this.installEnabled != value)
                {
                    this.installEnabled = value;
                    base.OnPropertyChanged("InstallEnabled");
                }
            }
        }
        public bool FinishEnabled
        {
            get { return this.finishEnabled; }
            set
            {
                if (this.finishEnabled != value)
                {
                    this.finishEnabled = value;
                    base.OnPropertyChanged("FinishEnabled");
                }
            }
        }

        public bool Downgrade
        {
            get
            {
                return this.downgrade;
            }

            set
            {
                if (this.downgrade != value)
                {
                    this.downgrade = value;
                    base.OnPropertyChanged("Downgrade");
                }
            }
        }

        public ICommand LicenseCommand
        {
            get
            {
                if (this.licenseCommand == null)
                {
                    this.licenseCommand = new RelayCommand(param => this.LaunchLicense(), param => true);
                }

                return this.licenseCommand;
            }
        }

        //public bool LicenseEnabled
        //{
        //    get { return this.LicenseCommand.CanExecute(this); }
        //}

        public ICommand CloseCommand
        {
            get { return this.root.CloseCommand; }
        }

        public bool ExitEnabled
        {
            get { return this.root.InstallState != InstallationState.Applying; }
        }

        public ICommand InstallCommand
        {
            get
            {
                if (this.installCommand == null)
                {
                    this.installCommand = new RelayCommand(param => CustomBA.Plan(LaunchAction.Install), param => this.root.DetectState == DetectionState.Absent && this.root.InstallState == InstallationState.Waiting);
                }

                return this.installCommand;
            }
        }

        //public bool InstallEnabled
        //{
        //    get { return this.InstallCommand.CanExecute(this); }
        //}

        public ICommand RepairCommand
        {
            get
            {
                if (this.repairCommand == null)
                {
                    this.repairCommand = new RelayCommand(param => CustomBA.Plan(LaunchAction.Repair), param => this.root.DetectState == DetectionState.Present && this.root.InstallState == InstallationState.Waiting);
                }

                return this.repairCommand;
            }
        }

        public bool RepairEnabled
        {
            get { return this.RepairCommand.CanExecute(this); }
        }

        public bool CompleteEnabled
        {
            get { return this.root.InstallState == InstallationState.Applied; }
        }

        public ICommand UninstallCommand
        {
            get
            {
                if (this.uninstallCommand == null)
                {
                    this.uninstallCommand = new RelayCommand(param => CustomBA.Plan(LaunchAction.Uninstall), param => this.root.DetectState == DetectionState.Present && this.root.InstallState == InstallationState.Waiting);
                }

                return this.uninstallCommand;
            }
        }

        public bool UninstallEnabled
        {
            get { return this.UninstallCommand.CanExecute(this); }
        }

        public ICommand TryAgainCommand
        {
            get
            {
                if (this.tryAgainCommand == null)
                {
                    this.tryAgainCommand = new RelayCommand(param =>
                    {
                        this.root.Canceled = false;
                        CustomBA.Plan(CustomBA.Model.PlannedAction);
                    }, param => this.root.InstallState == InstallationState.Failed);
                }

                return this.tryAgainCommand;
            }
        }

        public bool TryAgainEnabled
        {
            get { return this.TryAgainCommand.CanExecute(this); }
        }

        public string Title
        {
            get
            {
                switch (this.root.InstallState)
                {
                    case InstallationState.Initializing:
                        return "Initializing...";
                    case InstallationState.Waiting:
                        switch (this.root.DetectState)
                        {

                            case DetectionState.Present:
                                return "Installed";

                            case DetectionState.Newer:
                                return "Newer version installed";

                            case DetectionState.Absent:
                                return "Not installed";
                            default:
                                return "Unexpected Detection state";
                        }
                    case InstallationState.Applying:
                        switch (CustomBA.Model.PlannedAction)
                        {
                            case LaunchAction.Layout:
                                return "Laying out...";

                            case LaunchAction.Install:
                                return "Installing...";

                            case LaunchAction.Repair:
                                return "Repairing...";

                            case LaunchAction.Uninstall:
                                return "Uninstalling...";

                            case LaunchAction.UpdateReplace:
                            case LaunchAction.UpdateReplaceEmbedded:
                                return "Updating...";

                            default:
                                return "Unexpected action state";
                        }

                    case InstallationState.Applied:
                        switch (CustomBA.Model.PlannedAction)
                        {
                            case LaunchAction.Layout:
                                return "Successfully created layout";

                            case LaunchAction.Install:
                                return "Successfully installed";

                            case LaunchAction.Repair:
                                return "Successfully repaired";

                            case LaunchAction.Uninstall:
                                return "Successfully uninstalled";

                            case LaunchAction.UpdateReplace:
                            case LaunchAction.UpdateReplaceEmbedded:
                                return "Successfully updated";

                            default:
                                return "Unexpected action state";
                        }

                    case InstallationState.Failed:
                        if (this.root.Canceled)
                        {
                            return "Canceled";
                        }
                        else if (LaunchAction.Unknown != CustomBA.Model.PlannedAction)
                        {
                            switch (CustomBA.Model.PlannedAction)
                            {
                                case LaunchAction.Layout:
                                    return "Failed to create layout";

                                case LaunchAction.Install:
                                    return "Failed to install";

                                case LaunchAction.Repair:
                                    return "Failed to repair";

                                case LaunchAction.Uninstall:
                                    return "Failed to uninstall";

                                case LaunchAction.UpdateReplace:
                                case LaunchAction.UpdateReplaceEmbedded:
                                    return "Failed to update";

                                default:
                                    return "Unexpected action state";
                            }
                        }
                        else
                        {
                            return "Unexpected failure";
                        }

                    default:
                        return "Unknown view model state";
                }
            }
        }

        private void LaunchLicense()
        {
            string folder = IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string LicenseFile = IO.Path.Combine(folder, "License.txt");
        }

        private void DetectBegin(object sender, DetectBeginEventArgs e)
        {
            this.root.DetectState = e.Installed ? DetectionState.Present : DetectionState.Absent;
            CustomBA.Model.PlannedAction = LaunchAction.Unknown;
        }

        private void DetectedRelatedBundle(object sender, DetectRelatedBundleEventArgs e)
        {
            if (e.Operation == RelatedOperation.Downgrade)
            {
                this.Downgrade = true;
            }
        }

        private void DetectComplete(object sender, DetectCompleteEventArgs e)
        {
            // Parse the command line string before any planning.
            this.ParseCommandLine();
            this.root.InstallState = InstallationState.Waiting;

            if (LaunchAction.Uninstall == CustomBA.Model.Command.Action)
            {
                CustomBA.Model.Engine.Log(LogLevel.Verbose, "Invoking automatic plan for uninstall");
                CustomBA.Plan(LaunchAction.Uninstall);
            }
            else if (Hresult.Succeeded(e.Status))
            {
                // block if CLR v2 isn't available; sorry, it's needed for the MSBuild tasks
                if (CustomBA.Model.Engine.EvaluateCondition("NOT NETFRAMEWORK35_SP_LEVEL"))
                {
                    string message = "WiX Toolset requires the .NET Framework 3.5.1 Windows feature to be enabled.";
                    CustomBA.Model.Engine.Log(LogLevel.Verbose, message);

                    if (Display.Full == CustomBA.Model.Command.Display)
                    {
                        CustomBA.Dispatcher.Invoke((Action)delegate()
                        {
                            MessageBox.Show(message, "WiX Toolset", MessageBoxButton.OK, MessageBoxImage.Error);
                            if (null != CustomBA.View)
                            {
                                CustomBA.View.Close();
                            }
                        }
                        );
                    }

                    this.root.InstallState = InstallationState.Failed;
                    return;
                }

                if (this.Downgrade)
                {
                    // TODO: What behavior do we want for downgrade?
                    this.root.DetectState = DetectionState.Newer;
                }

                if (LaunchAction.Layout == CustomBA.Model.Command.Action)
                {
                    CustomBA.PlanLayout();
                }
                else if (CustomBA.Model.Command.Display != Display.Full)
                {
                    // If we're not waiting for the user to click install, dispatch plan with the default action.
                    CustomBA.Model.Engine.Log(LogLevel.Verbose, "Invoking automatic plan for non-interactive mode.");
                    CustomBA.Plan(CustomBA.Model.Command.Action);
                }
            }
            else
            {
                this.root.InstallState = InstallationState.Failed;
            }

            // Force all commands to reevaluate CanExecute.
            // InvalidateRequerySuggested must be run on the UI thread.
            CustomBA.Dispatcher.Invoke(new Action(CommandManager.InvalidateRequerySuggested));
        }

        private void PlanPackageBegin(object sender, PlanPackageBeginEventArgs e)
        {
            if (CustomBA.Model.Engine.StringVariables.Contains("MbaNetfxPackageId") && e.PackageId.Equals(CustomBA.Model.Engine.StringVariables["MbaNetfxPackageId"], StringComparison.Ordinal))
            {
                e.State = RequestState.None;
            }
        }

        private void PlanComplete(object sender, PlanCompleteEventArgs e)
        {
            if (Hresult.Succeeded(e.Status))
            {
                this.root.PreApplyState = this.root.InstallState;
                this.root.InstallState = InstallationState.Applying;
                CustomBA.Model.Engine.Apply(this.root.ViewWindowHandle);
            }
            else
            {
                this.root.InstallState = InstallationState.Failed;
            }
        }

        private void ApplyBegin(object sender, ApplyBeginEventArgs e)
        {
            //this.downloadRetries.Clear();
        }

        private void CacheAcquireBegin(object sender, CacheAcquireBeginEventArgs e)
        {
            this.cachePackageStart = DateTime.Now;
        }

        private void CacheAcquireComplete(object sender, CacheAcquireCompleteEventArgs e)
        {
            this.AddPackageTelemetry("Cache", e.PackageOrContainerId ?? String.Empty, DateTime.Now.Subtract(this.cachePackageStart).TotalMilliseconds, e.Status);
        }

        private void ExecutePackageBegin(object sender, ExecutePackageBeginEventArgs e)
        {
            this.executePackageStart = e.ShouldExecute ? DateTime.Now : DateTime.MinValue;
        }

        private void ExecutePackageComplete(object sender, ExecutePackageCompleteEventArgs e)
        {
            if (DateTime.MinValue < this.executePackageStart)
            {
                this.AddPackageTelemetry("Execute", e.PackageId ?? String.Empty, DateTime.Now.Subtract(this.executePackageStart).TotalMilliseconds, e.Status);
                this.executePackageStart = DateTime.MinValue;
            }
        }

        private void ExecuteError(object sender, ErrorEventArgs e)
        {
            lock (this)
            {
                if (!this.root.Canceled)
                {
                    // If the error is a cancel coming from the engine during apply we want to go back to the preapply state.
                    if (InstallationState.Applying == this.root.InstallState && (int)Error.UserCancelled == e.ErrorCode)
                    {
                        this.root.InstallState = this.root.PreApplyState;
                    }
                    else
                    {
                        this.Message = e.ErrorMessage;

                        if (Display.Full == CustomBA.Model.Command.Display)
                        {
                            // On HTTP authentication errors, have the engine try to do authentication for us.
                            if (ErrorType.HttpServerAuthentication == e.ErrorType || ErrorType.HttpProxyAuthentication == e.ErrorType)
                            {
                                e.Result = Result.TryAgain;
                            }
                            else // show an error dialog.
                            {
                                MessageBoxButton msgbox = MessageBoxButton.OK;
                                switch (e.UIHint & 0xF)
                                {
                                    case 0:
                                        msgbox = MessageBoxButton.OK;
                                        break;
                                    case 1:
                                        msgbox = MessageBoxButton.OKCancel;
                                        break;
                                    // There is no 2! That would have been MB_ABORTRETRYIGNORE.
                                    case 3:
                                        msgbox = MessageBoxButton.YesNoCancel;
                                        break;
                                    case 4:
                                        msgbox = MessageBoxButton.YesNo;
                                        break;
                                    // default: stay with MBOK since an exact match is not available.
                                }

                                MessageBoxResult result = MessageBoxResult.None;
                                CustomBA.View.Dispatcher.Invoke((Action)delegate()
                                {
                                    result = MessageBox.Show(CustomBA.View, e.ErrorMessage, "WiX Toolset", msgbox, MessageBoxImage.Error);
                                }
                                    );

                                // If there was a match from the UI hint to the msgbox value, use the result from the
                                // message box. Otherwise, we'll ignore it and return the default to Burn.
                                if ((e.UIHint & 0xF) == (int)msgbox)
                                {
                                    e.Result = (Result)result;
                                }
                            }
                        }
                    }
                }
                else // canceled, so always return cancel.
                {
                    e.Result = Result.Cancel;
                }
            }
        }

        private void ResolveSource(object sender, ResolveSourceEventArgs e)
        {
            int retries = 0;

            //this.downloadRetries.TryGetValue(e.PackageOrContainerId, out retries);
            //this.downloadRetries[e.PackageOrContainerId] = retries + 1;

            e.Result = retries < 3 && !String.IsNullOrEmpty(e.DownloadSource) ? Result.Download : Result.Ok;
        }

        private void ApplyComplete(object sender, ApplyCompleteEventArgs e)
        {
            CustomBA.Model.Result = e.Status; // remember the final result of the apply.

            // Set the state to applied or failed unless the state has already been set back to the preapply state
            // which means we need to show the UI as it was before the apply started.
            if (this.root.InstallState != this.root.PreApplyState)
            {
                this.root.InstallState = Hresult.Succeeded(e.Status) ? InstallationState.Applied : InstallationState.Failed;
            }

            // If we're not in Full UI mode, we need to alert the dispatcher to stop and close the window for passive.
            if (Display.Full != CustomBA.Model.Command.Display)
            {
                // If its passive, send a message to the window to close.
                if (Display.Passive == CustomBA.Model.Command.Display)
                {
                    CustomBA.Model.Engine.Log(LogLevel.Verbose, "Automatically closing the window for non-interactive install");
                    CustomBA.Dispatcher.BeginInvoke(new Action(CustomBA.View.Close));
                }
                else
                {
                    CustomBA.Dispatcher.InvokeShutdown();
                }
                return;
            }
            else if (Hresult.Succeeded(e.Status) && LaunchAction.UpdateReplace == CustomBA.Model.PlannedAction) // if we successfully applied an update close the window since the new Bundle should be running now.
            {
                CustomBA.Model.Engine.Log(LogLevel.Verbose, "Automatically closing the window since update successful.");
                CustomBA.Dispatcher.BeginInvoke(new Action(CustomBA.View.Close));
                return;
            }

            // Force all commands to reevaluate CanExecute.
            // InvalidateRequerySuggested must be run on the UI thread.
            CustomBA.Dispatcher.Invoke(new Action(CommandManager.InvalidateRequerySuggested));
        }

        private void ParseCommandLine()
        {
            // Get array of arguments based on the system parsing algorithm.
            string[] args = CustomBA.Model.Command.GetCommandLineArgs();
            for (int i = 0; i < args.Length; ++i)
            {
                if (args[i].StartsWith("InstallFolder=", StringComparison.InvariantCultureIgnoreCase))
                {
                    // Allow relative directory paths. Also validates.
                    string[] param = args[i].Split(new char[] { '=' }, 2);
                    this.root.InstallDirectory = IO.Path.Combine(Environment.CurrentDirectory, param[1]);
                }
            }
        }

        private void AddPackageTelemetry(string prefix, string id, double time, int result)
        {
            lock (this)
            {
                string key = String.Format("{0}Time_{1}", prefix, id);
                string value = time.ToString();
               // CustomBA.Model.Telemetry.Add(new KeyValuePair<string, string>(key, value));

                key = String.Format("{0}Result_{1}", prefix, id);
                value = String.Concat("0x", result.ToString("x"));
                //CustomBA.Model.Telemetry.Add(new KeyValuePair<string, string>(key, value));
            }
        }

        private void WireEventsHandlers()
        {

        }
    }
}

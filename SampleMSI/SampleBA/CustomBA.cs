using System;
using System.IO;
using Microsoft.Tools.WindowsInstallerXml.Bootstrapper;
using Threading = System.Windows.Threading;
using WinForms = System.Windows.Forms;

namespace SampleBA
{
    public class CustomBA : BootstrapperApplication
    {
        public static Model Model { get; private set; }

        public static RootView View { get; private set; }

        public static Threading.Dispatcher Dispatcher { get; private set; }

        protected override void Run()
        {
            this.Engine.Log(LogLevel.Standard, "Wix Custom BA started...");

            CustomBA.Model = new Model(this);
            CustomBA.Dispatcher = Threading.Dispatcher.CurrentDispatcher;
            RootViewModel viewModel = new RootViewModel();

            // Kick off detect which will populate the view models.
            this.Engine.Detect();
            
            if(CustomBA.Model.Command.Display == Display.Passive ||
                CustomBA.Model.Command.Display == Display.Full)
            {
                this.Engine.Log(LogLevel.Standard, "UI enabled...");
                CustomBA.View = new RootView(viewModel);
                CustomBA.View.Show();
            }

            Threading.Dispatcher.Run();

            this.Engine.Quit(CustomBA.Model.Result);

        }

        public static void Plan(LaunchAction action)
        {
            CustomBA.Model.PlannedAction = action;
            CustomBA.Model.Engine.Plan(CustomBA.Model.PlannedAction);
        }

        public static void PlanLayout()
        {
            // Either default or set the layout directory
            if (String.IsNullOrEmpty(CustomBA.Model.Command.LayoutDirectory))
            {
                CustomBA.Model.LayoutDirectory = Directory.GetCurrentDirectory();

                // Ask the user for layout folder if one wasn't provided and we're in full UI mode
                if (CustomBA.Model.Command.Display == Display.Full)
                {
                    CustomBA.Dispatcher.Invoke((Action)delegate()
                    {
                        WinForms.FolderBrowserDialog browserDialog = new WinForms.FolderBrowserDialog();
                        browserDialog.RootFolder = Environment.SpecialFolder.MyComputer;

                        // Default to the current directory.
                        browserDialog.SelectedPath = CustomBA.Model.LayoutDirectory;
                        WinForms.DialogResult result = browserDialog.ShowDialog();

                        if (WinForms.DialogResult.OK == result)
                        {
                            CustomBA.Model.LayoutDirectory = browserDialog.SelectedPath;
                            CustomBA.Plan(CustomBA.Model.Command.Action);
                        }
                        else
                        {
                            CustomBA.View.Close();
                        }
                    }
                    );
                }
            }
            else
            {
                CustomBA.Model.LayoutDirectory = CustomBA.Model.Command.LayoutDirectory;
                CustomBA.Plan(CustomBA.Model.Command.Action);
            }
        }
    }
}

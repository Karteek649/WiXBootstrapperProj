using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Tools.WindowsInstallerXml.Bootstrapper;

namespace SampleBA
{
    public class Model
    {
        private Version version;
        private const string BurnBundleInstallDirectoryVariable = "InstallFolder";
        private const string BurnBundleLayoutDirectoryVariable = "WixBundleLayoutDirectory";
        private const string BurnBundleVersionVariable = "WixBundleVersion";

        public BootstrapperApplication Bootstrapper { get; private set; }

        public Model(BootstrapperApplication bootStrapper)
        {
            this.Bootstrapper = bootStrapper;

        }
        public Engine Engine { get { return this.Bootstrapper.Engine; } }

        public Command Command { get { return this.Bootstrapper.Command; } }

        public int Result { get; set; }     //Final Result of Installation

        /// Get or set the path where the bundle is installed.
        public string InstallDirectory
        {
            get
            {
                if (!this.Engine.StringVariables.Contains(BurnBundleInstallDirectoryVariable))
                {
                    return null;
                }

                return this.Engine.StringVariables[BurnBundleInstallDirectoryVariable];
            }

            set
            {
                this.Engine.StringVariables[BurnBundleInstallDirectoryVariable] = value;
            }
        }


        /// Get the version of the install.
        public Version Version
        {
            get
            {
                if (null == this.version)
                {
                    this.version = this.Engine.VersionVariables[BurnBundleVersionVariable];
                }

                return this.version;
            }
        }

        /// Get or set the path for the layout to be created.
        public string LayoutDirectory
        {
            get
            {
                if (!this.Engine.StringVariables.Contains(BurnBundleLayoutDirectoryVariable))
                {
                    return null;
                }

                return this.Engine.StringVariables[BurnBundleLayoutDirectoryVariable];
            }

            set
            {
                this.Engine.StringVariables[BurnBundleLayoutDirectoryVariable] = value;
            }
        }


        public LaunchAction PlannedAction { get; set; }
    }
}

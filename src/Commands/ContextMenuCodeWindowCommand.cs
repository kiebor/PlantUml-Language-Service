using System;
using System.ComponentModel.Design;
using System.Linq;
using Microsoft.VisualStudio.Shell;
using PlantUmlLanguageService.Settings;

namespace PlantUmlLanguageService.Commands
{
    internal sealed class ContextMenuCodeWindowCommand
    {
        public const int CodeWindowContextMenuCommandId = 0x0100;

        public static readonly Guid CommandSet = new Guid("0c1acc31-15ac-417c-86b2-eefdc669e8bf");

        private readonly Package package;

        private OleMenuCommand OlemenuItem;

        private ContextMenuCodeWindowCommand(Package package)
        {
            this.package = package ?? throw new ArgumentNullException("package");

            OleMenuCommandService commandService = this.ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (commandService != null)
            {
                var menuCommandID = new CommandID(CommandSet, CodeWindowContextMenuCommandId);
                OlemenuItem = new OleMenuCommand(this.MenuItemCallback, menuCommandID)
                {
                    Visible = false
                };
                OlemenuItem.BeforeQueryStatus += EnableForPlantUmlMenuItems;
                commandService.AddCommand(OlemenuItem);
            }
        }

        public static ContextMenuCodeWindowCommand Instance
        {
            get;
            private set;
        }

        private void EnableForPlantUmlMenuItems(object sender, EventArgs e)
        {
            EnvDTE.DTE dte = (EnvDTE.DTE)ServiceProvider.GetService(typeof(EnvDTE.DTE));
            EnvDTE.Document activeDocument = dte.ActiveDocument;

            if (activeDocument != null && Constants.FileTypes.Contains($".{activeDocument.FullName.Split('.').Last()}"))
            {

                OlemenuItem.Visible = true;
            }
            else
            {
                OlemenuItem.Visible = false;
            }
        }

        private IServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        public static void Initialize(Package package)
        {
            Instance = new ContextMenuCodeWindowCommand(package);
        }

        private void MenuItemCallback(object sender, EventArgs e)
        {
            EnvDTE.DTE dte;
            EnvDTE.Document activeDocument;

            Global.BaseUrl = GetBaseUrl();

            dte = (EnvDTE.DTE)ServiceProvider.GetService(typeof(EnvDTE.DTE));
            activeDocument = dte.ActiveDocument;
            activeDocument.Save();
            ServiceProvider.PreviewFileContent(activeDocument.FullName);
        }

        private string GetBaseUrl()
        {
            var settingsPage = (PlantUmlServerSettings)package.GetDialogPage(typeof(PlantUmlServerSettings));
            var baseUrl = settingsPage.BaseUrl;
            if (string.IsNullOrEmpty(baseUrl))
                baseUrl = Constants.DefaultBaseUrl;
            return baseUrl;
        }
    }
}
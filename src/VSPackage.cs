using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using PlantUmlLanguageService.Commands;
using PlantUmlLanguageService.Settings;
//using PlantUmlLanguageService.Intellisense;
using PlantUmlLanguageService.ToolWindow;
using Task = System.Threading.Tasks.Task;

namespace PlantUmlLanguageService
{
    //[ProvideEditorFactory(typeof(EditorFactory), 101)]
    //[ProvideEditorExtension(typeof(EditorFactory), ".plantuml", 32, NameResourceID = 101)]
    //[ProvideEditorExtension(typeof(EditorFactory), ".plant", 32, NameResourceID = 101)]
    //[ProvideEditorExtension(typeof(EditorFactory), ".puml", 32, NameResourceID = 101)]
    //[ProvideEditorExtension(typeof(EditorFactory), ".pu", 32, NameResourceID = 101)]
    //[ProvideEditorExtension(typeof(EditorFactory), ".iuml", 32, NameResourceID = 101)]
    //[ProvideEditorExtension(typeof(EditorFactory), ".uml", 32, NameResourceID = 101)]
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)] // Info on this package for Help/About
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    [ProvideToolWindow(typeof(ReferenceWindow), Style = VsDockStyle.Tabbed, Window = ReferenceWindow.WindowGuidString)]
    [Guid(VSPackage.PackageGuidString)]
    [ProvideToolWindow(typeof(DiagramPreviewToolWindow),Style =VsDockStyle.MDI)]
    [ProvideAutoLoad(UIContextGuids.SolutionExists, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideOptionPage(typeof(PlantUmlServerSettings), "PlantUML", "PlantUML Server Settings", 0, 0, true)]
    public sealed class VSPackage : AsyncPackage
    {
        public const string PackageGuidString = "c097b9f6-5f54-40b4-aa9f-5fe227fc3bb1"; //"ff4f80de-da63-4ca8-9f09-acf70fdc5cb5";
        //private EditorFactory editorFactory;
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {

            //this.editorFactory = new EditorFactory(this);
            //base.RegisterEditorFactory(this.editorFactory);

            // Switch to main thread to register commands
            await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            ContextMenuItemCommand.Initialize(this);
            ContextMenuCodeWindowCommand.Initialize(this);
            DiagramPreviewToolWindowCommand.Initialize(this);
            await ReferenceWindowsCommand.InitializeAsync(this);
        }

        public override IVsAsyncToolWindowFactory GetAsyncToolWindowFactory(Guid toolWindowType)
        {
            if (toolWindowType.Equals(new Guid(ReferenceWindow.WindowGuidString)))
            {
                return this;
            }

            return null;
        }

        protected override string GetToolWindowTitle(Type toolWindowType, int id)
        {
            if (toolWindowType == typeof(ReferenceWindow))
            {
                return ReferenceWindow.Title;
            }

            return base.GetToolWindowTitle(toolWindowType, id);
        }

        /// <summary>
        /// Returns an object that is passed into the constructor of <see cref="ReferenceWindow"/>.
        /// </summary>
        protected override async Task<object> InitializeToolWindowAsync(Type toolWindowType, int id, CancellationToken cancellationToken)
        {
            await Task.Delay(4000); // simulate long running initialization
            return ReferenceWindow.Title;
        }

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        //protected override void Initialize()
        //{
        //    base.Initialize();
        //    DiagramPreviewToolWindowCommand.Initialize(this);
        //}

    }
}

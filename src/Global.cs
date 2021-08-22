using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using PlantUmlLanguageService.Control;
using PlantUmlLanguageService.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace PlantUmlLanguageService
{
    public static class Global
    {
        internal static string BaseUrl = string.Empty;

        internal static string DiagramUrl = string.Empty;

        internal static string CurrentFile = string.Empty;

        internal static string CurrentFilePath = string.Empty;

        private static Validator validator = new Validator();

        internal static List<string> Warnings = new List<string>();

        internal static Validator Validator { get => validator; set => validator = value; }

        internal static void OpenDiagramPreviewer()
        {
            var dte = Package.GetGlobalService(typeof(EnvDTE.DTE)) as EnvDTE.DTE;
            if (dte == null) return;

            var window = dte.Windows.Item("{7e1da659-a22a-47af-8078-9debafc6aeff}");
            window.Visible = true;
        }

        internal static string GetResourceString(string folder, string name, string extension)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            StreamReader textStreamReader = new StreamReader(assembly.GetManifestResourceStream($"PlantUmlLanguageService.{folder}.{name}.{extension}"));

            return textStreamReader.ReadToEnd();
        }

        internal static void PreviewFileContent(this IServiceProvider serviceprovider, string path)
        {
            if (Constants.FileTypes.Contains($".{path.Split('.').Last()}"))
            {
                Warnings.Clear();
                CurrentFilePath = path;
                CurrentFile = Path.GetFileNameWithoutExtension(path);
                DiagramUrl =
                    DiagramService.GetImageUrlForSource(
                        MacroService.CheckImports(File.ReadAllLines(path).IncludeFiles()),
                        "svg"
                    );
                OpenDiagramPreviewer();
            }
            else
            {
                VsShellUtilities.ShowMessageBox(
                serviceprovider,
                $"{path} is not a recognised plant uml file.",
                "PlantUml Preview",
                OLEMSGICON.OLEMSGICON_INFO,
                OLEMSGBUTTON.OLEMSGBUTTON_OK,
                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
            }
        }

        static internal string[] GetProjectFilesInSolution()
        {
            return SolutionService.Projects().Where(p => !string.IsNullOrEmpty(p.FullName)).Select((p) => p.FullName).ToArray();
        }

        static internal void WriteOutput()
        {
            IVsOutputWindow outWindow = Package.GetGlobalService(typeof(SVsOutputWindow)) as IVsOutputWindow;

            Guid customGuid = new Guid("0fa4dc72-e10a-4d36-8822-26112d3cbdea");
            string customTitle = "Plant Uml";
            outWindow.CreatePane(ref customGuid, customTitle, 1, 1);

            IVsOutputWindowPane customPane;
            outWindow.GetPane(ref customGuid, out customPane);

            customPane.Clear();

            if (DiagramUrl != Constants.NoImageBase64)
            {
                string output = 
                    $"\nMarkdown:\r{string.Format(Constants.UrlFormatMd,CurrentFile, DiagramUrl)}" +
                    $"\n\rHtml:\r{string.Format(Constants.UrlFormatSrc, DiagramUrl, CurrentFile)}";

                customPane.OutputString(output);

            }

            customPane.Activate();
        }

    }

    internal class Controls
    {
        public static PlantUmlViewer PumlViewer = new PlantUmlViewer() {ScriptErrorsSuppressed = true,ContextMenuStrip = new ViewerContextMenu() };
    }
}

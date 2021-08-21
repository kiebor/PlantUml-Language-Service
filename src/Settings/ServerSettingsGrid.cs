using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;

namespace PlantUmlLanguageService.Settings
{
    public class PlantUmlServerSettings: DialogPage
    {
        [Category("PlantUML")]
        [DisplayName("BaseUrl")]
        [Description("PlantUML Base Server URL (i.e http://plantuml.com/plantuml/")]
        public string BaseUrl { get; set; }
    }
}

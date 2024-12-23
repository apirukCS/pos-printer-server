using System;
using System.Text;

namespace PosPrintServer
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.cd
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            ApplicationConfiguration.Initialize();
            Application.Run(new Form1());
        }
    }
}
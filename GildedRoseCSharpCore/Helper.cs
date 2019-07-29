using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace GildedRoseCSharpCore
{
    public static class Helper
    {        
        /// <summary>
        /// Method to get Configuration from settings file
        /// </summary>
        public static IConfigurationRoot GetConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            return builder.Build();
        }        

        /// <summary>
        /// Method to handle the global exceptions
        /// </summary>
        public static void UnhandledExceptionTrapper(object sender, UnhandledExceptionEventArgs e)
        {
            Console.WriteLine(e.ExceptionObject.ToString());
            Console.WriteLine("Press Enter to continue");
            Console.ReadLine();
            Environment.Exit(1);
        }
    }
}

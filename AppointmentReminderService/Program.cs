using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AppointmentReminderService
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            //Application.ThreadException += Application_ThreadException;

            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new ReminderService()
            };
            ServiceBase.Run(ServicesToRun);
        }

        private static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}

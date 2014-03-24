using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkerRole
{
    public static class PDFPrinter
    {
        public const string HtmlToPdfExePath = "approot/wkhtmltopdf.exe";

        public static bool GeneratePdf(string commandLocation, StreamReader html, Stream pdf)
        {
            Process p;
            StreamWriter stdin;
            ProcessStartInfo psi = new ProcessStartInfo();

            psi.FileName = Path.Combine(commandLocation, HtmlToPdfExePath);
            psi.WorkingDirectory = Path.GetDirectoryName(psi.FileName);

            Trace.TraceInformation("working dir: {0}", psi.WorkingDirectory);
            // run the conversion utility
            psi.UseShellExecute = false;
            psi.CreateNoWindow = true;
            psi.RedirectStandardInput = true;
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;

            // note: that we tell wkhtmltopdf to be quiet and not run scripts
            psi.Arguments = "-q -n --disable-smart-shrinking --page-width 210mm --page-height 297mm  - -";

            p = Process.Start(psi);

            try
            {
                stdin = p.StandardInput;
                stdin.AutoFlush = true;
                stdin.Write(html.ReadToEnd());
                stdin.Dispose();

                CopyStream(p.StandardOutput.BaseStream, pdf);
                p.StandardOutput.Close();
                pdf.Position = 0;

                p.WaitForExit(10000);

                return true;
            }
            catch
            {
                return false;

            }
            finally
            {
                p.Dispose();
            }
        }

        public static void CopyStream(Stream input, Stream output)
        {
            byte[] buffer = new byte[32768];
            int read;
            while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                output.Write(buffer, 0, read);
            }
        }
    }
}

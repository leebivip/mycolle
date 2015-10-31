using System;
using System.Windows.Forms;
using Excel = Microsoft.Office.Interop.Excel;

namespace xlstopng
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            // open file
            Console.WriteLine("xlstopng: please wait...");

            var eapp = new Excel.Application();
            eapp.Visible = false;

            try
            {
                var wb = eapp.Workbooks.Open(args[0]);

                // read sheet
                Excel.Worksheet sht = wb.Sheets[4];
                sht.Select();

                // set times, will auto calculate
                sht.get_Range("C29").Value = sht.get_Range("C29").Value + 1;

                // copy pic
                Excel.Range rng = sht.get_Range("H4:M11");
                rng.CopyPicture(Excel.XlPictureAppearance.xlScreen, Excel.XlCopyPictureFormat.xlBitmap);

                // export pic
                var img = Clipboard.GetImage();
                if (img != null) { img.Save(args[1]); img.Dispose(); }                
                else { Console.WriteLine("xlstopng: image not copied"); }

                // close file                
                wb.Close(true);
                Console.WriteLine("xlstopng: done");
            }
            catch (Exception ec) { Console.WriteLine("xlstopng: " + ec.GetType() + ": " + ec.Message); }
            finally { eapp.Quit(); }
        }
    }
}

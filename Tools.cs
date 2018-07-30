using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace RevitFamilyBrowser
{
    public static class Tools
    {
        public static Workset GetActiveWorkset(Document doc)
        {
            WorksetTable table = doc.GetWorksetTable();
            WorksetId activeId = table.GetActiveWorksetId();
            Workset workset = table.GetWorkset(activeId);
            return workset;
        }

        private static FilteredElementCollector WorksetElements(Document doc, Workset workset)
        {
            FilteredElementCollector elementCollector = new FilteredElementCollector(doc).OfClass(typeof(Family));

            ElementWorksetFilter elementWorksetFilter = new ElementWorksetFilter(workset.Id, false);

            return elementCollector.WherePasses(elementWorksetFilter);
        }
        private static IList<Workset> GetAllWorksets(Document doc)
        {
            string message = string.Empty;

            FilteredWorksetCollector collector = new FilteredWorksetCollector(doc);

            collector.OfKind(WorksetKind.FamilyWorkset);

            IList<Workset> worksets = collector.ToWorksets();

            return worksets;
        }

        private static string GetFamiliesElements(FilteredElementCollector elementCollector)
        {
            string temp = string.Empty;

            foreach (Element element in elementCollector)
            {
                if (!(element.Name.Contains("Standart") ||
                      element.Name.Contains("Mullion") ||
                      element.Name.Contains("Tag")))
                {
                    Family family = element as Family;

                    temp += element.Name;

                    ISet<ElementId> familySymbolId = family.GetFamilySymbolIds();

                    foreach (ElementId id in familySymbolId)
                    {
                        var symbol = family.Document.GetElement(id) as FamilySymbol;

                        if (symbol != null) temp += "#" + symbol.Name;
                    }
                    temp += "\n";
                }
            }
            return temp;
        }

        public static void CollectFamilyData(Autodesk.Revit.DB.Document doc)
        {
            Properties.Settings.Default.CollectedData = string.Empty;

            if (GetAllWorksets(doc).Count == 0)
            {
                FilteredElementCollector FamilyCollector = new FilteredElementCollector(doc).OfClass(typeof(Family));

                Properties.Settings.Default.CollectedData = GetFamiliesElements(FamilyCollector);
            }
            else
            {
                string temp = string.Empty;

                foreach (Workset workset in GetAllWorksets(doc))
                {
                    if (workset.IsEditable)
                    {
                        temp += GetFamiliesElements(WorksetElements(doc, workset));
                    }
                }
                Properties.Settings.Default.CollectedData = temp;
            }
        }

        public static void CreateImages(Autodesk.Revit.DB.Document doc)
        {
            var collector = new FilteredElementCollector(doc).OfClass(typeof(FamilyInstance));

            foreach (FamilyInstance fi in collector)
            {
                ElementId typeId = fi.GetTypeId();

                ElementType type = doc.GetElement(typeId) as ElementType;

                string TempImgFolder = Path.GetTempPath() + "FamilyBrowser\\";

                if (!Directory.Exists(TempImgFolder))
                {
                    Directory.CreateDirectory(TempImgFolder);
                }

                string filename = Path.Combine(TempImgFolder + type.FamilyName + ".bmp");

                if (File.Exists(filename))
                {
                    continue;
                }
                else
                {
                    System.Drawing.Size imgSize = new System.Drawing.Size(200, 200);

                    Bitmap image = type.GetPreviewImage(imgSize);

                    BitmapEncoder encoder = new BmpBitmapEncoder();

                    if (image == null)
                    {
                        image = new Bitmap(Properties.Resources.RevitLogo, imgSize);
                    }

                    encoder.Frames.Add(BitmapFrame.Create(ConvertBitmapToBitmapSource(image)));

                    FileStream file = new FileStream(filename, FileMode.Create, FileAccess.Write);

                    encoder.Save(file);

                    file.Close();
                }
            }
        }

        public static BitmapSource ConvertBitmapToBitmapSource(Bitmap bmp)
        {
            return System.Windows.Interop.Imaging
                .CreateBitmapSourceFromHBitmap(
                    bmp.GetHbitmap(),
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
        }

        public static BitmapSource GetImage(IntPtr bm)
        {
            BitmapSource bmSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                bm,
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
            return bmSource;
        }

        public static void ShowMessageBox(string message)
        {
            System.Windows.Forms.MessageBox.Show(message, "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}

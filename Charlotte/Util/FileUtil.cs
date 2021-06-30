using ESRI.ArcGIS.DataSourcesFile;
using ESRI.ArcGIS.Geodatabase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Charlotte.Util
{
    static class FileUtil
    {
        private static void DeleteShpFile(string pathName, string fileName)
        {
            IWorkspaceFactory pWSF = new ShapefileWorkspaceFactory();
            IFeatureWorkspace pFWS = pWSF.OpenFromFile(pathName, 0) as IFeatureWorkspace;
            IFeatureClass pFeatureClass = pFWS.OpenFeatureClass(fileName);
            IDataset pFeatureDataset = pFeatureClass as IDataset;
            pFeatureDataset.Delete();
        }

        public static void ExtractFileNameFromFullPath(string fullPath, ref string directory, ref string fileName)
        {
            fullPath = fullPath.Replace('\\', '/');
            int lastSeparatorIndex = fullPath.LastIndexOf('/');
            directory = fullPath.Substring(0, lastSeparatorIndex);
            fileName = fullPath.Substring(lastSeparatorIndex, fullPath.Length - lastSeparatorIndex);
        }

        /// <summary>
        /// Open a file dialog for selecting file to OPEN
        /// </summary>
        /// <param name="title">title of dialog</param>
        /// <param name="filter">specify filter</param>
        /// <returns>path</returns>
        public static string SelectOpenPath(string title, string filter, bool multiSelect = false)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = filter;
            openFileDialog.Title = title;
            openFileDialog.Multiselect = multiSelect;
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                return multiSelect ? String.Join(";", openFileDialog.FileNames) : openFileDialog.FileName;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Open a file dialog for selecting file to SAVE
        /// </summary>
        /// <param name="title">title of dialog</param>
        /// <param name="filter">specify filter</param>
        /// <returns>path</returns>
        public static string SelectSavePath(string title, string filter, string defaultFile = "")
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = filter;
            saveFileDialog.Title = title;
            saveFileDialog.AddExtension = true;
            saveFileDialog.FileName = defaultFile;
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                return saveFileDialog.FileName;
            }
            else
            {
                return null;
            }
        }
    }
}


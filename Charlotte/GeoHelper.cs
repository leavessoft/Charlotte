using AmapAPITool.AmapAPI.Entity;
using ESRI.ArcGIS.DataSourcesFile;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Charlotte
{
    class GeoHelper
    {
        public static Field CreateField(string name, esriFieldType type, int length = 100)
        {
            Field field = new Field();
            IFieldEdit pFieldEdit = field as IFieldEdit;
            pFieldEdit.Length_2 = length;
            pFieldEdit.Name_2 = name;
            pFieldEdit.Type_2 = type;
            return field;
        }

        public static void CreateShpFile(string filePath, string fileName, Field[] fields, ISpatialReference spatialReference, Func<IFeatureClass, bool> featureCreationCallback)
        {
            fileName = fileName.Replace("/", "");
            IWorkspaceFactory pWorkspaceFactory = new ShapefileWorkspaceFactory();
            IFields pFields = new Fields();
            IFieldsEdit pFieldsEdit = pFields as IFieldsEdit;
            IField pField = new Field();
            IFieldEdit pFieldEdit = pField as IFieldEdit;
            pFieldEdit.Name_2 = "Shape";
            pFieldEdit.Type_2 = esriFieldType.esriFieldTypeGeometry;
            IGeometryDef pGeometryDef = new GeometryDef();
            IGeometryDefEdit pGeometryDefEdit = pGeometryDef as IGeometryDefEdit;
            pGeometryDefEdit.GeometryType_2 = esriGeometryType.esriGeometryPoint;
            pGeometryDefEdit.SpatialReference_2 = spatialReference;
            pFieldEdit.GeometryDef_2 = pGeometryDef;
            pFieldsEdit.AddField(pField);

            foreach (Field f in fields)
            {
                pFieldsEdit.AddField(f);
            }

            IFeatureWorkspace pFeatureWorkspace = pWorkspaceFactory.OpenFromFile(filePath, 0) as IFeatureWorkspace;
            int i = fileName.IndexOf(".shp");
            if (i == -1)
            {
                pFeatureWorkspace.CreateFeatureClass(fileName + ".shp", pFields, null, null,
                    esriFeatureType.esriFTSimple, "Shape", "");
            }
            else
            {
                pFeatureWorkspace.CreateFeatureClass(fileName, pFields,
                    null, null, esriFeatureType.esriFTSimple, "Shape", "");
            }

            //if (featureCreationCallback == null)
            //{
            IFeatureClass featureClass = pFeatureWorkspace.OpenFeatureClass(fileName);
            featureCreationCallback(featureClass);
            //}

        }

        public static void DeleteShpFile(string pathName, string fileName)
        {
            IWorkspaceFactory pWSF = new ShapefileWorkspaceFactory();
            IFeatureWorkspace pFWS = pWSF.OpenFromFile(pathName, 0) as IFeatureWorkspace;
            IFeatureClass pFeatureClass = pFWS.OpenFeatureClass(fileName);
            IDataset pFeatureDataset = pFeatureClass as IDataset;
            pFeatureDataset.Delete();
        }
    }
}

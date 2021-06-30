using Charlotte.Util;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.DataSourcesFile;
using ESRI.ArcGIS.DataSourcesRaster;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geoprocessing;
using ESRI.ArcGIS.Geoprocessor;
using ESRI.ArcGIS.SpatialAnalystTools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Charlotte
{
    public partial class MapView : Form
    {
        private const String defaultStatusText = "Ready";

        Workspace workspace { get; set; }

        String targetFeaturePath = null;

        public MapView(Workspace workspace)
        {
            this.workspace = workspace;

            ESRI.ArcGIS.RuntimeManager.Bind(ESRI.ArcGIS.ProductCode.EngineOrDesktop);
            ESRI.ArcGIS.RuntimeManager.BindLicense(ESRI.ArcGIS.ProductCode.EngineOrDesktop);

            InitializeComponent();
        }

        private void MapView_Load(object sender, EventArgs e)
        {
            if (MessageBox.Show(this, "Add POIs to layer? This might take a while.", "Charlotte", MessageBoxButtons.YesNo) == DialogResult.Yes) {
                // Export POI to shapefile in a new thread to avoid freezing
                new Thread(() =>
                {
                    Thread.CurrentThread.IsBackground = true;

                    toolStripStatusLabel1.Text = "Preparing POI shapefile, please wait...";
                    ExportPoiToTemp();
                    toolStripStatusLabel1.Text = defaultStatusText;
                }).Start();
            }
        }

        private void MapView_ResizeEnd(object sender, EventArgs e)
        {
        
        }

        private void MapView_SizeChanged(object sender, EventArgs e)
        {
            axToolbarControl1.Width = this.Width - axToolbarControl1.Left - (this.Width - axToolbarControl1.Left - axToolbarControl1.Width);
            axTOCControl1.Height = this.Height - axTOCControl1.Top - (this.Height - axTOCControl1.Height - axTOCControl1.Top);
        }

        private void ExportPoiToTemp()
        {
            DirectoryInfo tempDir = Directory.CreateDirectory("temp");
            // Remove previous temp files
            foreach (var file in tempDir.GetFiles())
            {
                file.Delete();
            }

            // Export the current one
            String shapefilePath = tempDir.FullName + "/temp.shp";
            workspace.Export(shapefilePath);

            // Load to map
            AddShapeFileToView(shapefilePath);

            targetFeaturePath = shapefilePath;
        }



        // MARK: - Menu items

        private void addLayerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string path = FileUtil.SelectOpenPath("Add shapefile", "ESRI Shapefile(*.shp)|*.shp", false);
            if (path != null)
            {
                AddShapeFileToView(path);
            }
        }

        private void reloadPOILayerToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void clipToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show(this, "This algorithm clips a vector layer using the features of an additional polygon layer. Only the parts of the features in the Input layer that fall within the polygons of the Overlay layer will be added to the resulting layer.", "Clip tool");
            string input1 = FileUtil.SelectOpenPath("Select [Input] file", "Shapefile(.shp)|*.shp");
            string input2 = FileUtil.SelectOpenPath("Select [Overlay] file", "Shapefile(.shp)|*.shp");
            string output = FileUtil.SelectSavePath("Save as..", "Shapefile(.shp)|*.shp", "clip.shp");
            Clip(new string[] { input2, input1 }, output);
        }

        private void intersectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show(this, "This algorithm extracts the overlapping portions of features in the Input and Overlay layers. Features in the output Intersection layer are assigned the attributes of the overlapping features from both the Input and Overlay layers.", "Intersection tool");
            string input1 = FileUtil.SelectOpenPath("Select [Input] file", "Shapefile(.shp)|*.shp");
            string input2 = FileUtil.SelectOpenPath("Select [Overlay] file", "Shapefile(.shp)|*.shp");
            string output = FileUtil.SelectSavePath("Save as..", "Shapefile(.shp)|*.shp", "intersect.shp");
            Intersect(new string[] { input2, input1 }, output);
        }

        private void krigingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (targetFeaturePath == null)
            {
                if (MessageBox.Show(this, "No target feature class specified, would you like to choose one?", "Charlotte", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    String path = FileUtil.SelectOpenPath("Select shapefile", "ESRI Shapefile(*.shp)|*.shp");
                    if (path != null)
                    {
                        targetFeaturePath = path;
                    } 
                    else
                    {
                        return;
                    }
                }
                else
                {
                    return;
                }
            }

            string tiffPath = "temp/kriging.tif";
            Geoprocessor gp = new Geoprocessor();
            Kriging kriging = new Kriging();
            kriging.in_point_features = targetFeaturePath;
            kriging.out_surface_raster = tiffPath;
            kriging.z_field = "additional";
            kriging.semiVariogram_props = "Spherical";
            gp.Execute(kriging, null);
            AddRasterToView(tiffPath);
        }

        // MARK: - Utils
        IFeatureClass LoadFeatureClassFromPath(string path)
        {
            IWorkspaceFactory pWorkspaceFactory = new ShapefileWorkspaceFactory();
            string inputDir = "";
            string inputName = "";
            FileUtil.ExtractFileNameFromFullPath(path, ref inputDir, ref inputName);
            IFeatureWorkspace pFeatureWorkspace = pWorkspaceFactory.OpenFromFile(inputDir, 0) as IFeatureWorkspace;

            return pFeatureWorkspace.OpenFeatureClass(inputName);
        }

        void AddShapeFileToView(string filePath)
        {
            string inputDir = "";
            string inputName = "";
            FileUtil.ExtractFileNameFromFullPath(filePath, ref inputDir, ref inputName);
            axMapControl1.AddShapeFile(inputDir, inputName);
        }

        void AddRasterToView(string filePath)
        {
            String dir = "";
            String name = "";
            FileUtil.ExtractFileNameFromFullPath(filePath, ref dir, ref name);

            IWorkspaceFactory pWorkspaceFactory = new RasterWorkspaceFactory();
            IRasterWorkspace pRasterWorkspace = (IRasterWorkspace)pWorkspaceFactory.OpenFromFile(dir, 0);
            IRasterDataset pRasterDataset = (IRasterDataset)pRasterWorkspace.OpenRasterDataset(name);
            IRasterLayer pRasterLayer = new RasterLayer();
            pRasterLayer.CreateFromDataset(pRasterDataset);
            axMapControl1.Map.AddLayer(pRasterLayer);
            axMapControl1.ActiveView.Refresh();
        }


        // Mark: - Geoprocessing

        void Intersect(string[] inputFeatures, string outputFilePath)
        {
            Geoprocessor geoprocessor = new Geoprocessor();
            ESRI.ArcGIS.AnalysisTools.Intersect intersect = new ESRI.ArcGIS.AnalysisTools.Intersect();

            IGpValueTableObject vt = new GpValueTableObject();
            vt.SetColumns(2);

            vt.AddRow(LoadFeatureClassFromPath(inputFeatures[0]));
            vt.AddRow(LoadFeatureClassFromPath(inputFeatures[1]));
            intersect.in_features = vt;
            intersect.out_feature_class = outputFilePath;
            intersect.join_attributes = "ALL";
            geoprocessor.Execute(intersect, null);
            AddShapeFileToView(outputFilePath);
        }

        void Clip(string[] inputFeatures, string outputFilePath)
        {
            Geoprocessor geoprocessor = new Geoprocessor();
            ESRI.ArcGIS.AnalysisTools.Clip clip = new ESRI.ArcGIS.AnalysisTools.Clip();

            clip.in_features = inputFeatures[0];
            clip.clip_features = inputFeatures[1];
            clip.out_feature_class = outputFilePath;
            geoprocessor.Execute(clip, null);
            AddShapeFileToView(outputFilePath);
        }

    }
}

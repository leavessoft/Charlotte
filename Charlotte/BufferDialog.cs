using Charlotte.Util;
using ESRI.ArcGIS.Geoprocessor;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Charlotte
{
    public partial class BufferDialog : Form
    {
        private Func<string, string, bool> outputCallback;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="outputCallback">with two args for input and output shapefile path</param>
        public BufferDialog(Func<string, string, bool> outputCallback, string defaultInputPath = "", string defaultOutputPath = "")
        {
            InitializeComponent();
            this.outputCallback = outputCallback;
            textBox1.Text = defaultInputPath;
            textBox3.Text = defaultOutputPath;
        }

        private void BufferDialog_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            textBox1.Text = FileUtil.SelectOpenPath("Select Input Shapefile", "Shapefile(.shp)|*.shp");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            textBox3.Text = FileUtil.SelectSavePath("Specify Output Shapefile", "Shapefile(.shp)|*.shp");
        }

        private void button4_Click(object sender, EventArgs e)
        {
            double distance = 0;
            try
            {
                distance = double.Parse(textBox2.Text);
            }
            catch (Exception)
            {
                MessageBox.Show(this, "Invalid distance: " + textBox2.Text, "Charlotte", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            try
            {
                Buffer(textBox1.Text, distance, textBox3.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Failed executing geoprocessing: \n" + ex.ToString(), "Charlotte", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Generate buffer area for input shapefile
        /// </summary>
        /// <param name="inputFilePath"></param>
        /// <param name="bufferDistance"></param>
        /// <param name="outputFilePath"></param>
        void Buffer(string inputFilePath, double bufferDistance, string outputFilePath)
        {
            Geoprocessor geoprocessor = new Geoprocessor();
            ESRI.ArcGIS.AnalysisTools.Buffer buffer = new ESRI.ArcGIS.AnalysisTools.Buffer();
            buffer.in_features = inputFilePath;
            buffer.out_feature_class = outputFilePath;
            buffer.buffer_distance_or_field = bufferDistance;
            geoprocessor.Execute(buffer, null);

            outputCallback(inputFilePath, outputFilePath);
        }
    }
}

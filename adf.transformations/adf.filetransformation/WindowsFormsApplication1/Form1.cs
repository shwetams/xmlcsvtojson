using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using adf.filetransformation;
using System.IO;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            TransformToJSON xmlJson = new TransformToJSON();
            //GetJSONFromXML
            string xmltext = File.ReadAllText(@"C:\Users\shg\PCBackup_29thAugust2014\Work\Beckman\PoC\xmldata\100194_294975392_1430994622388.DATA");
            //string csvText = File.ReadAllText(@"C:\Users\shg\PCBackup_29thAugust2014\Work\Beckman\SampleDocuments\AU680_Sample CSV files\PLCode.csv");
            xmlJson.ProcessXMLToJSON(xmltext, null);
            //xmlJson.ProcessCSVFile(csvText, false,null);
        }
    }
}

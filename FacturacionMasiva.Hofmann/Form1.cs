using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FacturacionMasiva.Hofmann
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void generarFactura(DataTable dtFactura,string NumFactura)
        {

            using (StringWriter sw = new StringWriter())
            {
                foreach (DataRow row in dtFactura.Rows)
                {
                    sw.WriteLine(row[1]);
                    using (FileStream fs = new FileStream(@"\\zeus\Emisión Offline IC - Producción\Entrada\33_NPG_" + NumFactura+"+_.txt", FileMode.OpenOrCreate, FileAccess.Write))
                    {
                        byte[] buffer = System.Text.Encoding.Default.GetBytes(sw.ToString());
                        fs.Write(buffer, 0, buffer.Length);

                    }
                }
            }



        }

        private void btnProcesar_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            for (int i = 0; i < dgvPicking.RowCount; i++)
            {

                using (SqlConnection cn = new SqlConnection("Data Source=200.6.76.250;Initial Catalog=SLAPPHOFMANN;;Persist Security Info=True;User ID=us"))
                {
                    cn.Open();

                    SqlCommand cm = new SqlCommand("exec xsp_LE_Obtener_Correlativo 'FEL','DHOFMANN'", cn);
                    int correlativo = Convert.ToInt32(cm.ExecuteScalar());

                    string NumFactura = "F" + correlativo.ToString("0000000");


                    SqlDataAdapter da = new SqlDataAdapter("exec xsp_GenerarFactura '" + dgvPicking.Rows[i].Cells[0].Value.ToString() + "','" + NumFactura + "'", cn);
                    DataTable dtFactura = new DataTable();
                    da.Fill(dtFactura);

                    generarFactura(dtFactura, NumFactura);

                    dgvFacturas.Rows.Add(dgvPicking.Rows[i].Cells[0].Value.ToString(), NumFactura);




                }
            }


            
            dgvPicking.DataSource = "";
            dgvPicking.DataSource = null;
            Cursor.Current = Cursors.Default;
            MessageBox.Show("Proceso Concluido");
        }

        private void btnOpenExcel_Click(object sender, EventArgs e)
        {

            dialog.Filter = "Excel files (*.xls)|*.xls|All files (*.*)|*.*";
            dialog.InitialDirectory = "C:";
            dialog.Title = "Seleccione un Excel Version 2003";

            string strFileName;

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                strFileName = dialog.FileName;
                textBox1.Text = strFileName;
            }
            else
            {
                textBox1.Text = "";
                return;

            }
            //if (strFileName == String.Empty)
            //{
            //    return; //user didn't select a file to opena
            //}
            sConnectionString = "Provider=Microsoft.Jet.OleDb.4.0; Data Source = " + strFileName + "; Extended Properties = \"Excel 8.0;HDR=Yes;IMEX=1\";";

            //sConnectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + strFileName + ";Extended Properties=Excel 12.0 Xml;HDR=YES;";
        }


        OpenFileDialog dialog = new OpenFileDialog();
        String input = string.Empty;
        string sConnectionString;
        private void btnCargarExcel_Click(object sender, EventArgs e)
        {

            OleDbConnection cnx = new OleDbConnection(sConnectionString);
            OleDbDataAdapter da = new OleDbDataAdapter("select * from [Hoja1$]", cnx);
            DataTable dt = new DataTable();
            da.Fill(dt);

            dgvPicking.DataSource = dt;
        }
    }
}

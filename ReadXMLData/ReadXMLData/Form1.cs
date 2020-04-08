using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;
using System.Xml;
using System.IO;
using System.Data.Odbc;
using System.Configuration;
using System.Data.SqlClient;

namespace ReadXMLData
{
    public partial class Form1 : Form
    {
        DataTable dt = null;
        string connectionString = "Dsn=XMLMapping";

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {

            DataSet xmldata = GetData();

            SQLInsert(xmldata);

        }

        #region DataTable
        private DataTable CreateDataTable()
        {
            // New table.
            dt = new DataTable();

            dt.Columns.Add("Mapping Type", typeof(string));
            dt.Columns.Add("Table Name", typeof(string));
            dt.Columns.Add("Table ID", typeof(string));
            dt.Columns.Add("FLD_DATA_CL_ID", typeof(string));
            dt.Columns.Add("FLD_ID", typeof(string));

            return dt;
        }
        #endregion

        #region GetData

        private DataSet GetData()
        {
            #region "Reading XML with LINQ"
            string defaultValue = null;
            string mappingtype = string.Empty;
            DataSet ds = new DataSet();

            FileInfo file = new FileInfo(@"S:\Config.xml");
            if (file.Exists)
            {
                XElement xmlFile = XElement.Load(file.FullName);
                IEnumerable<XElement> Mappings = from element in xmlFile.Elements("Mapping")
                                                 select element;
                CreateDataTable();

                foreach (var mappingnodes in Mappings)
                {

                    foreach (XAttribute attribute in mappingnodes.Attributes("type"))
                    {
                        mappingtype = attribute.Value;

                    }
                    IEnumerable<XElement> tables = from el in mappingnodes.Elements("Table")
                                                   select el;

                    foreach (var tablenodes in tables)
                    {
                        DataRow row = dt.NewRow();

                        XAttribute tablename = tablenodes.Attribute("tableName");
                        XAttribute tableid = tablenodes.Attribute("tableId");

                        row["Table Name"] = tablename.Value;
                        row["Table ID"] = tableid.Value;
                        row["Mapping Type"] = mappingtype;

                        IEnumerable<XElement> entrynode = from tbl in tablenodes.Elements("Entry") select tbl;

                        foreach (var defaultEntry in entrynode)
                        {
                            IEnumerable<XElement> defaultentries = from ent in entrynode.Elements("DefaultEntry") select ent;

                            foreach (var col in defaultentries)
                            {
                                IEnumerable<XElement> columns = from co in col.Elements("Column") select co;

                                IEnumerable<XElement> fieldidcolumns = (from c in columns
                                                                        where ((c.Attribute("columnName").Value.Equals("FLD_ID")))
                                                                        select c);
                                IEnumerable<XElement> fielddataclcolumns = (from c in columns
                                                                            where ((c.Attribute("columnName").Value.Equals("FLD_DATA_CL_ID")))
                                                                            select c);

                                XAttribute defval;

                                if (fieldidcolumns.Count() != 0)
                                {
                                    defval = fieldidcolumns.Attributes("defaultValue").FirstOrDefault();

                                    if (defval != null)
                                    {
                                        defaultValue = defval.Value;

                                        if (defval.PreviousAttribute.Value.Equals("FLD_ID"))
                                        {
                                            row["FLD_ID"] = defaultValue;
                                        }

                                    }

                                }
                                else if (fielddataclcolumns.Count() != 0)
                                {
                                    defval = fielddataclcolumns.Attributes("defaultValue").FirstOrDefault();

                                    if (defval != null)
                                    {
                                        defaultValue = defval.Value;

                                        if (defval.PreviousAttribute.Value.Equals("FLD_DATA_CL_ID"))
                                        {
                                            row["FLD_DATA_CL_ID"] = defaultValue;
                                        }
                                    }
                                }
                                else
                                {
                                    row["FLD_ID"] = string.Empty;
                                    row["FLD_DATA_CL_ID"] = string.Empty;
                                }

                            }

                        }
                        dt.Rows.Add(row);
                    }
                }
                ds.Tables.Add(dt);
            }
            else
                throw new FileNotFoundException(file.FullName);
            return ds;
            #endregion
        }


        #endregion


        #region SQL Insert
        private void SQLInsert(DataSet ds)
        {
            string TruncateCommand = "TRUNCATE TABLE [XMLMapping]";

            if (ds.Tables[0] != null && ds.Tables[0].Rows.Count > 0)
            {
                using (OdbcConnection con = new OdbcConnection(connectionString))
                {

                    using (OdbcCommand cmd = new OdbcCommand(TruncateCommand, con))
                    {
                        con.Open();

                        cmd.ExecuteNonQuery();
                        con.Close();
                    }

                    string InsertCommand = "INSERT INTO [XMLMapping] (MappingType, TableName, TableId, FLD_DATA_CL_ID, FLD_ID) VALUES (?, ?, ?, ?, ?)";

                    using (OdbcCommand cmd = new OdbcCommand(InsertCommand, con))
                    {
                        using (OdbcDataAdapter odbcAdapter = new OdbcDataAdapter(cmd))
                        {
                            try
                            {
                                con.Open();

                                cmd.Parameters.AddWithValue("@p1", "");
                                cmd.Parameters.AddWithValue("@p2", "");
                                cmd.Parameters.AddWithValue("@p3", "");
                                cmd.Parameters.AddWithValue("@p4", "");
                                cmd.Parameters.AddWithValue("@p5", "");

                                foreach (DataRow r in ds.Tables[0].Rows)
                                {
                                    cmd.Parameters["@p1"].Value = String.IsNullOrEmpty(r["Mapping Type"].ToString()) ? string.Empty : r["Mapping Type"].ToString();
                                    cmd.Parameters["@p2"].Value = String.IsNullOrEmpty(r["Table Name"].ToString()) ? string.Empty : r["Table Name"].ToString();
                                    cmd.Parameters["@p3"].Value = String.IsNullOrEmpty(r["Table ID"].ToString()) ? string.Empty : r["Table ID"].ToString();
                                    cmd.Parameters["@p4"].Value = String.IsNullOrEmpty(r["FLD_DATA_CL_ID"].ToString()) ? string.Empty : r["FLD_DATA_CL_ID"].ToString();
                                    cmd.Parameters["@p5"].Value = String.IsNullOrEmpty(r["FLD_ID"].ToString()) ? string.Empty : r["FLD_ID"].ToString();
                                    cmd.ExecuteNonQuery();
                                }
                                MessageBox.Show("Done.");

                            }
                            catch (System.Exception ex)
                            {
                                MessageBox.Show("Exception catch here - details  : " + ex.ToString());
                            }

                        }
                    }
                }
            }
        }
        #endregion

    }
}


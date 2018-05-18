using SqlDbInteraction;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

using System.Windows.Input;
using System.Configuration;
using System.IO;
using System.Windows.Forms;

namespace ModelGenerator
{
  
    public partial class MainWindow : Window
    {

        StringBuilder stringBuilder = new StringBuilder();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {                

                SqlDbUtils util = new SqlDbUtils();
                DataTable dtTableColumn = new DataTable();

                string conStr = ConfigurationManager.ConnectionStrings["connection"].ConnectionString;
                SqlConnection scon = new SqlConnection(conStr);

                string tableName = cmbTable.Text.ToString();
                string schemaName = cmbTable.SelectedValue.ToString();
                string sqlStr = "select  COLUMN_NAME,DATA_TYPE  from information_schema.columns    where TABLE_NAME= '" + tableName + "' and Table_schema= '" + schemaName + "'";
                string errorMsg = "";


                tableName = GetQualifyName(tableName);

                dtTableColumn = util.FetchDataTable(scon, sqlStr, out errorMsg, null, CommandType.Text, null);
                 StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.AppendLine("using System;");
                stringBuilder.AppendLine("using System.Collections.Generic;");
                stringBuilder.AppendLine("using System.Data.SqlClient;");
                stringBuilder.AppendLine("using System.Data;");       
                stringBuilder.AppendLine("using System.Linq;");
                stringBuilder.AppendLine("using MCLSystem;");
                stringBuilder.AppendLine("using System.Web;");
                 stringBuilder.AppendLine("using System.ComponentModel.DataAnnotations;");
                  stringBuilder.AppendLine("using System.ComponentModel;");
                  stringBuilder.AppendLine("using System.Web.Mvc;");
                stringBuilder.AppendLine("\r\n public  class  " + tableName + "\r\n {\r\n");
                string dataType = null;
                string columnName = null;
               
                for (int i=0;i<dtTableColumn.Rows.Count;i++)
                {
                    columnName = Convert.ToString(dtTableColumn.Rows[i][0]);
                    dataType = Convert.ToString(dtTableColumn.Rows[i][1]);

                    stringBuilder.AppendLine("\n public  " + GetDataType(dataType, true) + " " + GetQualifyName(columnName) + " { get; set; }");

                }

               

                stringBuilder.AppendLine("\r\n}");



                GenerateFiles(stringBuilder.ToString(), txtPath.Text + "\\" + tableName + "ViewModel.cs");


                stringBuilder.Clear();
                System.Windows.MessageBox.Show("Succsessfully generated !");

             

            }
            catch (Exception ex)
            {


            }



        }

        private void ComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                SqlDbUtils util = new SqlDbUtils();
                DataTable dtTable = new DataTable();

                string conStr = ConfigurationManager.ConnectionStrings["connection"].ConnectionString;
                SqlConnection scon = new SqlConnection(conStr);
                //string sqlStr = " select Table_schema,Table_name from information_schema.tables  where Table_schema not in('dbo','dbutil') order by TABLE_NAME";
                string sqlStr = " select Table_schema,Table_name from information_schema.tables  order by TABLE_NAME";
            
                string errorMsg = "";
                dtTable = util.FetchDataTable(scon, sqlStr,out errorMsg , null, CommandType.Text, null);

                cmbTable.DisplayMemberPath = dtTable.Columns[1].ToString();
                cmbTable.SelectedValuePath = dtTable.Columns[0].ToString();
                DataRow newRow = dtTable.NewRow();
                newRow[1] = "---Select---";
                dtTable.Rows.InsertAt(newRow, 0);
                cmbTable.ItemsSource = dtTable.DefaultView;
                cmbTable.SelectedIndex = 0;

            }
            catch (Exception ex)
            {
               
               
            }
        }




        private static string GetDataType(string SqlDatatype, bool isNullable)
        {
          
          
            if (SqlDatatype == "binary" || SqlDatatype == "image" || SqlDatatype == "rowversion" || SqlDatatype == "timestamp" || SqlDatatype == "varbinary")
            {
              
                return "byte[]";
            }
            if (SqlDatatype == "bit")
            {
                
                if (isNullable)
                {
                    return "bool?";
                }
                return "bool";
            }
            if (SqlDatatype == "time")
            {

                return "TimeSpan?";
            }
            if (SqlDatatype == "date" || SqlDatatype == "datetime" || SqlDatatype == "smalldatetime")
            {
               
                return "DateTime?";
            }
            if (SqlDatatype == "datetimeoffset")
            {
               
                return "DateTimeOffset?";
            }
            if (SqlDatatype == "float")
            {
              
                if (isNullable)
                {
                    return "double?";
                }
                return "double";
            }

            if (SqlDatatype == "int")
            {
               
                if (isNullable)
                {
                    return "Int32?";
                }
                return "Int32";
            }
            if (SqlDatatype == "bigint")
            {

                if (isNullable)
                {
                    return "Int64?";
                }
                return "Int64";
            }
            if (SqlDatatype == "smallint")
            {

                if (isNullable)
                {
                    return "Int16?";
                }
                return "Int16";
            }
            if (SqlDatatype == "tinyint")
            {

                if (isNullable)
                {
                    return "Int16?";
                }
                return "Int16";
            }

            if (SqlDatatype == "decimal" || SqlDatatype == "money" || SqlDatatype == "numeric" || SqlDatatype == "smallmoney")
            {
                
                if (isNullable)
                {
                    return "decimal?";
                }
                return "decimal";
            }

            if (SqlDatatype == "real")
            {
                
                if (isNullable)
                {
                    return "single?";
                }
                return "single";
            }

          

           
            
            return "string";
        }






       
        private  string GetQualifyName(string objcetname)
        {
            if (objcetname == null)
            {
                return null;
            }


            string[] objcetarray = objcetname.Split(new char[]
	           {  	'_'            });
            objcetname = "";
            string[] TextArray = objcetarray;
            for (int i = 0; i < TextArray.Length; i++)
            {
                string text2 = TextArray[i];
                if (!string.IsNullOrWhiteSpace(text2))
                {
                    string str = text2.Substring(1);
                    objcetname = objcetname + text2[0].ToString().ToUpper() + str;
                }
            }
            objcetname = objcetname.Trim().Replace(" ", "_");
            return objcetname;
        }










        private static void GenerateFiles(string dataCode, string Filename)
        {

            try
            {
                StreamWriter streamWriter = File.CreateText(Filename);
                streamWriter.Write(dataCode);
                streamWriter.Flush();
                streamWriter.Dispose();
            }
            catch(Exception ex)
            { }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog folder = new FolderBrowserDialog();

            DialogResult dr = folder.ShowDialog();

            txtPath.Text = folder.SelectedPath.ToString();
            
             
           

        }

        private void SpGenerate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SqlDbUtils util = new SqlDbUtils();
                DataTable dtTableColumn = new DataTable();

                string conStr = ConfigurationManager.ConnectionStrings["connection"].ConnectionString;
                SqlConnection scon = new SqlConnection(conStr);

                string tableName = cmbTable.Text.ToString();
                string schemaName = cmbTable.SelectedValue.ToString();


                string sqlStr = "select Table_schema, COLUMN_NAME,DATA_TYPE ,CHARACTER_MAXIMUM_LENGTH from information_schema.columns    where TABLE_NAME= '" + tableName + "' and Table_schema= '" + schemaName + "' ";
               
                
                
                string errorMsg = "";


            

                dtTableColumn = util.FetchDataTable(scon, sqlStr, out errorMsg, null, CommandType.Text, null);
               
                InsertSpGenerate(dtTableColumn, tableName);

                stringBuilder.AppendLine("\r\n-- update-----------");

                updateSpGenerate(dtTableColumn, tableName);


                stringBuilder.AppendLine("\r\n-- delete-----------");


                deleteSpGenerate(dtTableColumn, tableName);
                

                stringBuilder.AppendLine("\r\n-- Exists Validate-----------");


                ExistValidate(dtTableColumn, tableName);

                      stringBuilder.AppendLine("\r\n-- Get----------");


                      GetFunctionGenerate(dtTableColumn, tableName);

             
                stringBuilder.AppendLine("\r\n");

                GenerateFiles(stringBuilder.ToString(), txtPath.Text + "\\" + tableName + ".txt");
                stringBuilder.Clear();
                System.Windows.MessageBox.Show("Succsessfully generated !");



            }
            catch (Exception ex)
            {


            }
        }








        private void InsertSpGenerate(DataTable dtTableColumn, string tableName)
        {

            try
            {
                string schemaName = Convert.ToString(dtTableColumn.Rows[0][0]);
                string parameterDeclaration = null;
                string allparameterName = null;
                string allColumnName = null;
                string Spname = schemaName + "." + tableName + "_create";
                stringBuilder.AppendLine("create proc " + Spname);
                //  stringBuilder.AppendLine("\n");
                string Colsize = null;

                string dataType = null;
                string columnName = null;
                parameterDeclaration = "";
                stringBuilder.AppendLine("(\n");
                int k = 0;

                for (int i =1; i < dtTableColumn.Rows.Count; i++)
                {

                    columnName = Convert.ToString(dtTableColumn.Rows[i][1]);
                    dataType = Convert.ToString(dtTableColumn.Rows[i][2]);
                    Colsize = Convert.ToString(dtTableColumn.Rows[i][3]);
                    if (Colsize=="-1")
                    {
                        Colsize = "200";
                    }
                    if ((dataType == "varchar") || (dataType == "char") || (dataType == "nvarchar"))
                    {
                        if (k == 0)
                        {
                            allColumnName += columnName;
                            allparameterName += "@" + columnName;
                            parameterDeclaration += "@" + columnName + " " + dataType + "(" + Colsize + ")" + " = Null ";

                        }
                        else
                        {
                            allColumnName += "," + columnName;
                            allparameterName += "," + "@" + columnName;
                            parameterDeclaration += "," + "@" + columnName + " " + dataType + "(" + Colsize + ")" + " = Null ";

                        }

                    }
                    else
                    {
                        if (k == 0)
                        {
                            allColumnName += columnName;
                            allparameterName += "@" + columnName;
                            parameterDeclaration += "@" + columnName + " " + dataType + " = Null ";

                        }


                        else
                        {

                            allColumnName += "," + columnName;
                            allparameterName += "," + "@" + columnName;
                            parameterDeclaration += "," + "@" + columnName + " " + dataType + " = Null ";


                        }

                    }



                    k = k + 1;


                }
                stringBuilder.AppendLine(parameterDeclaration + "\r\n");
                stringBuilder.AppendLine(")\n As \r\n");
                stringBuilder.AppendLine("Begin \r\n");
                stringBuilder.AppendLine("BEGIN TRY \r\n");
                stringBuilder.AppendLine("insert into " + schemaName + "." + tableName + " (" + allColumnName + " ) \r\n");
                stringBuilder.AppendLine("Values (" + allparameterName + " ) \r\n");
                stringBuilder.AppendLine(" END TRY    \r\n BEGIN CATCH    \r\n");


                stringBuilder.AppendLine("RAISERROR('Insert Failed !', 16, 2)  \r\n");

                stringBuilder.AppendLine("  END CATCH     \r\n");
                stringBuilder.AppendLine("  END    \r\n");



            



            }
            catch (Exception ex)
            {


            }
        }




        private void updateSpGenerate(DataTable dtTableColumn, string tableName)
        {

            try
            {
              

                string schemaName = Convert.ToString(dtTableColumn.Rows[0][0]);
                string parameterDeclaration = null;
              
                string allColumnName = null;
                string Spname = schemaName + "." + tableName + "_update";
                stringBuilder.AppendLine("create proc " + Spname);
                //  stringBuilder.AppendLine("\n");
                string Colsize = null;

                string dataType = null;
                string columnName = null;
                parameterDeclaration = "";
                stringBuilder.AppendLine("(\n");
                int k = 0;

                string AutoColumnName = Convert.ToString(dtTableColumn.Rows[0][1]);
                string AutoColumnType = Convert.ToString(dtTableColumn.Rows[0][2]);
                string AutoColumnSize = Convert.ToString(dtTableColumn.Rows[0][3]);

                if ((AutoColumnType == "varchar") || (AutoColumnType == "char") || (AutoColumnType == "nvarchar"))
                {
                    parameterDeclaration += "@" + AutoColumnName + " " + AutoColumnType + "(" + AutoColumnSize + ")" + " = Null ,";
                }
                else
                {
                    parameterDeclaration += "@" + AutoColumnName + " " + AutoColumnType + " = Null ,";
                }
                for (int i = 1; i < dtTableColumn.Rows.Count; i++)
                {

                    columnName = Convert.ToString(dtTableColumn.Rows[i][1]);
                    dataType = Convert.ToString(dtTableColumn.Rows[i][2]);
                    Colsize = Convert.ToString(dtTableColumn.Rows[i][3]);
                    if (Colsize == "-1")
                    {
                        Colsize = "200";
                    }
                    if ((dataType == "varchar") || (dataType == "char") || (dataType == "nvarchar"))
                    {
                        if (k == 0)
                        {
                            allColumnName += columnName + "=@" + columnName;
                           
                            parameterDeclaration += "@" + columnName + " " + dataType + "(" + Colsize + ")" + " = Null ";

                        }
                        else
                        {
                           
                                allColumnName += "," + columnName + "=@" + columnName;
                           
                            parameterDeclaration += "," + "@" + columnName + " " + dataType + "(" + Colsize + ")" + " = Null ";

                        }

                    }
                    else
                    {
                        if (k == 0)
                        {
                           
                                allColumnName += columnName + "=@" + columnName;
                           
                            parameterDeclaration += "@" + columnName + " " + dataType + " = Null ";

                        }


                        else
                        {
                           
                                allColumnName += "," + columnName + "=@" + columnName;
                           
                            parameterDeclaration += "," + "@" + columnName + " " + dataType + " = Null ";


                        }

                    }



                    k = k + 1;


                }
                stringBuilder.AppendLine(parameterDeclaration + "\r\n");
                stringBuilder.AppendLine(")\n As \r\n");
                stringBuilder.AppendLine("Begin \r\n");
                stringBuilder.AppendLine("BEGIN TRY \r\n");
                stringBuilder.AppendLine("Update  " + schemaName + "." + tableName + " set " + allColumnName + " where  "+AutoColumnName+"=@"+AutoColumnName  +"\r\n");
              
                stringBuilder.AppendLine(" END TRY    \r\n BEGIN CATCH    \r\n");


                stringBuilder.AppendLine("RAISERROR('Update Failed !', 16, 2)  \r\n");

                stringBuilder.AppendLine("  END CATCH     \r\n");
                stringBuilder.AppendLine("  END    \r\n");



              


            }
            catch (Exception ex)
            {


            }
        }



        private void deleteSpGenerate(DataTable dtTableColumn, string tableName)
        {

            try
            {


                string schemaName = Convert.ToString(dtTableColumn.Rows[0][0]);
                string parameterDeclaration = null;

             
                string Spname = schemaName + "." + tableName + "_delete";
                stringBuilder.AppendLine("create proc " + Spname);
               

               
                parameterDeclaration = "";
                stringBuilder.AppendLine("(\n");
                int k = 0;

                string AutoColumnName = Convert.ToString(dtTableColumn.Rows[0][1]);
                string AutoColumnType = Convert.ToString(dtTableColumn.Rows[0][2]);
                string AutoColumnSize = Convert.ToString(dtTableColumn.Rows[0][3]);

                if ((AutoColumnType == "varchar") || (AutoColumnType == "char") || (AutoColumnType == "nvarchar"))
                {
                    parameterDeclaration += "@" + AutoColumnName + " " + AutoColumnType + "(" + AutoColumnSize + ")" + " = Null ";
                }
                else
                {
                    parameterDeclaration += "@" + AutoColumnName + " " + AutoColumnType + " = Null ";
                }
              
                stringBuilder.AppendLine(parameterDeclaration + "\r\n");
                stringBuilder.AppendLine(")\n As \r\n");
                stringBuilder.AppendLine("Begin \r\n");
                stringBuilder.AppendLine("BEGIN TRY \r\n");
                stringBuilder.AppendLine("delete from  " + schemaName + "." + tableName + " where  " + AutoColumnName + "=@" + AutoColumnName + "\r\n");

                stringBuilder.AppendLine(" END TRY    \r\n BEGIN CATCH    \r\n");


                stringBuilder.AppendLine("RAISERROR('Delete Failed !', 16, 2)  \r\n");

                stringBuilder.AppendLine("  END CATCH     \r\n");
                stringBuilder.AppendLine("  END    \r\n");






            }
            catch (Exception ex)
            {


            }
        }












        private void ExistValidate(DataTable dtTableColumn, string tableName)
        {

            try
            {


                string schemaName = Convert.ToString(dtTableColumn.Rows[0][0]);
                string parameterDeclaration = null;


                string Spname = schemaName + "." + tableName + "_exists_validate";
                stringBuilder.AppendLine("create function " + Spname);



                parameterDeclaration = "";
                stringBuilder.AppendLine("(\n");
                int k = 0;

                string AutoColumnName = Convert.ToString(dtTableColumn.Rows[1][1]);
                string AutoColumnType = Convert.ToString(dtTableColumn.Rows[1][2]);
                string AutoColumnSize = Convert.ToString(dtTableColumn.Rows[1][3]);

                if ((AutoColumnType == "varchar") || (AutoColumnType == "char") || (AutoColumnType == "nvarchar"))
                {
                    parameterDeclaration += "@" + AutoColumnName + " " + AutoColumnType + "(" + AutoColumnSize + ")" + " = Null ";
                }
                else
                {
                    parameterDeclaration += "@" + AutoColumnName + " " + AutoColumnType + " = Null ";
                }

                stringBuilder.AppendLine(parameterDeclaration + "\r\n )\n");
              
                stringBuilder.AppendLine("  returns int  \r\n");
                stringBuilder.AppendLine(" As \r\n");
                stringBuilder.AppendLine("Begin \r\n");
            
                  stringBuilder.AppendLine("declare @count int  \r\n");
                stringBuilder.AppendLine("select @count=COUNT(*) from  " + schemaName + "." + tableName + " where  " + AutoColumnName + "=@" + AutoColumnName + "\r\n");
                stringBuilder.AppendLine("return @count    \r\n");
 

             
                stringBuilder.AppendLine("  END    \r\n");






            }
            catch (Exception ex)
            {


            }
        }





        private void GetFunctionGenerate(DataTable dtTableColumn, string tableName)
        {

            try
            {


                string schemaName = Convert.ToString(dtTableColumn.Rows[0][0]);
              

                string Spname = schemaName + "." + tableName + "_get";
                stringBuilder.AppendLine("create function " + Spname + " ( ) ");



            
              

                string columnName, allColumnName="";
                int k = 0;
                for (int i = 0; i < dtTableColumn.Rows.Count; i++)
                {

                    columnName = Convert.ToString(dtTableColumn.Rows[i][1]);
                   
               
                        if (k == 0)
                        {
                            allColumnName += columnName + " as " + GetQualifyName(columnName);
                          

                        }


                        else
                        {

                            allColumnName += "," + columnName + " as " + GetQualifyName(columnName);
                        


                        }

                 



                    k = k + 1;


                }



              

               
                stringBuilder.AppendLine("   returns table  \r\n");
                stringBuilder.AppendLine("   return  ( \r\n");
             

              
                stringBuilder.AppendLine("   select " + allColumnName + "  from  " + schemaName + "." + tableName + "\r\n   )");
              


               





            }
            catch (Exception ex)
            {


            }
        }



    }




   

}

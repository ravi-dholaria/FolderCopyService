using FolderCopyService.Business;
using FolderCopyService.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FolderCopyService.DAL
{
    internal class FCS_DAL
    {
        //connection string
        public static string connectionstring = ConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString;
        FolderCopyService_WorkerClass folderCopyService_WorkerClass = new FolderCopyService_WorkerClass();

        #region Insert Folder Details
        public void InsertFolderDetails(FolderDetailsModel folderDetails) 
        {
            try
            {

            using (SqlConnection sqlConnection = new SqlConnection(connectionstring))
            {
                sqlConnection.Open();
                //Query
                string query = "INSERT INTO FolderDetails (FolderName, FileCount, FolderCount,ParentFolder,OriginalLocation,WatchStatus) VALUES (@FolderName, @FileCount, @FolderCount, @ParentFolder, @OriginalLocation, @WatchStatus)";

                // Create a parameterized SqlCommand to prevent SQL injection
                using (SqlCommand command = new SqlCommand(query, sqlConnection))
                {
                    // Add parameters and their values
                    command.Parameters.AddWithValue("@FolderName",folderDetails.FolderName);
                    command.Parameters.AddWithValue("@FileCount",folderDetails.FileCount);
                    command.Parameters.AddWithValue("@FolderCount",folderDetails.FolderCount);
                    command.Parameters.AddWithValue("@ParentFolder",folderDetails.ParentFolder);
                    command.Parameters.AddWithValue("@OriginalLocation",folderDetails.OriginalLocation);
                    command.Parameters.AddWithValue("@WatchStatus",folderDetails.Watchstatus);
                    command.ExecuteNonQuery();
                }
            }
            }catch (Exception ex)
            {
                folderCopyService_WorkerClass.WriteToFile(ex.Message);
            }
        }
        #endregion

        #region updateFileDetails 
        public void updateFileDetails(FileDetailsModel fileDetails)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionstring))
                {
                    string query = "UPDATE FileDetails " + "SET FileStatus = @FileStatus, " + "ModificationDate = @ModificationDate " + "WHERE FilePath = @FilePath";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        connection.Open();
                        command.Parameters.AddWithValue("@FileStatus", fileDetails.FileStatus);
                        command.Parameters.AddWithValue("@ModificationDate", fileDetails.ModificationDate);
                        command.Parameters.AddWithValue("@FilePath", fileDetails.FilePath);
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                folderCopyService_WorkerClass.WriteToFile(ex.Message);
            }
        }
        #endregion

        #region Get Folder ID
        public int GetFolderId(string path)
        {
            try
            {
                using (SqlConnection sqlConnection = new SqlConnection(connectionstring))
                {
                    //Query
                    string query = "Select Id From FolderDetails where OriginalLocation = @OriginalLocation";

                    // Create a parameterized SqlCommand to prevent SQL injection
                    using (SqlCommand command = new SqlCommand(query, sqlConnection))
                    {
                        sqlConnection.Open();
                        command.Parameters.AddWithValue("@OriginalLocation", path);
                        object result = command.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            return Convert.ToInt32(result);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                folderCopyService_WorkerClass.WriteToFile(ex.Message);
            }
            return -1;
        }
        #endregion

        #region Insert File Details
        public void InsertFileDetails(FileDetailsModel fileDetails)
        {
            try
            {
                using(SqlConnection connection = new SqlConnection(connectionstring))
                {
                    string query = "Insert Into FileDetails (FolderId, FileName, FileExtension, FileStatus, FilePath,ModificationDate) values (@FolderId, @FileName, @FileExtension, @FileStatus, @FilePath, @ModificationDate)";
                    using (SqlCommand command = new SqlCommand(query,connection))
                    {
                        connection.Open();
                        command.Parameters.AddWithValue("@FolderId", fileDetails.FolderId);
                        command.Parameters.AddWithValue("@FileName", fileDetails.FileName);
                        command.Parameters.AddWithValue("@FileExtension", fileDetails.FileExtension);
                        command.Parameters.AddWithValue("@FileStatus", fileDetails.FileStatus);
                        command.Parameters.AddWithValue("@FilePath", fileDetails.FilePath);
                        command.Parameters.AddWithValue("@ModificationDate", fileDetails.ModificationDate);
                        command.ExecuteNonQuery();
                    }
                }
            }catch (Exception ex) 
            {
                folderCopyService_WorkerClass.WriteToFile(ex.Message);
            }
        }
        #endregion

        #region IsFilePresent
        public int isFilePresent(string filepath)
        {
            try
            {
                using(SqlConnection conn = new SqlConnection(connectionstring))
                {
                    string Query = "Select FileStatus,ModificationDate From FileDetails Where FilePath = @filepath";
                    using (SqlCommand command = new SqlCommand(Query,conn))
                    {
                        conn.Open();
                        command.Parameters.AddWithValue("@filepath", filepath);
                        // Execute the query and read the result
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // Check if FileStatus is 5
                                if (reader["FileStatus"] != DBNull.Value)
                                {
                                    bool flag1 = Convert.ToInt32(reader["FileStatus"]) == 5;
                                    bool flag = Convert.ToString(new FileInfo(filepath).LastWriteTime) == Convert.ToString(reader["ModificationDate"]);
                                    return (flag1 && flag)? 1 : -1;
                                    //folderCopyService_WorkerClass.WriteToFile(Convert.ToString(new FileInfo(filepath).LastWriteTime));
                                    //folderCopyService_WorkerClass.WriteToFile(Convert.ToString(Convert.ToDateTime(reader["ModificationDate"])));
                                }
                            }
                        }
                    }
                }
            } catch (Exception ex) {
                folderCopyService_WorkerClass.WriteToFile(ex.StackTrace);
            }
            return 0;
        }
        #endregion

        #region IsFolderPresent
        public bool isFolderPresent(string folderpath)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionstring))
                {
                    string Query = "Select Id From FolderDetails Where OriginalLocation = @folderpath";
                    using (SqlCommand command = new SqlCommand(Query, conn))
                    {
                        conn.Open();
                        command.Parameters.AddWithValue("@folderpath", folderpath);
                        Object res = command.ExecuteScalar();
                        if (res != null) return true;

                    }
                }
            }
            catch (Exception ex)
            {
                folderCopyService_WorkerClass.WriteToFile(ex.StackTrace);
            }
            return false;
        }
        #endregion

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Globalization;
using System.Diagnostics;
using Microsoft.Azure.Management.DataFactories.Models;
using Microsoft.DataFactories.Runtime;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using adf.filetransformation;

namespace adf.filetransformation
{
    public class XMLCSVToJSON : IDotNetActivity
    {
        public IDictionary<string, string> Execute(IEnumerable<ResolvedTable> filesToProcessTables,IEnumerable<ResolvedTable> transformedFilesTables,IDictionary<string, string> filesProperties,IActivityLogger logger)
        {
            string output = string.Empty;
            
            foreach(ResolvedTable filetoProcessTable in filesToProcessTables)
            {
                string storageConnectionString = GetConnectionString(filetoProcessTable.LinkedService);
                string folderPath = GetFolderPath(filetoProcessTable.Table);
                if (String.IsNullOrEmpty(storageConnectionString) || String.IsNullOrEmpty(folderPath))
                {
                    continue;
                }
                logger.Write(TraceEventType.Information, "Reading file from: {0}", folderPath);
                CloudStorageAccount inputStorageAccount = CloudStorageAccount.Parse(storageConnectionString);
                CloudBlobClient inputClient = inputStorageAccount.CreateCloudBlobClient();

                BlobContinuationToken continuationToken = null;
                
                do
                {
                    
                    BlobResultSegment result = inputClient.ListBlobsSegmented(folderPath, true, BlobListingDetails.All, null, continuationToken, null, null);
                    foreach (IListBlobItem listBlobItem in result.Results)
                    {
                        CloudBlockBlob inputBlob = listBlobItem as CloudBlockBlob;
                        string JSONOutputString = "";
                        string inputBlobString = "";
                        int count = 0;
                        if (inputBlob != null)
                        {

                            string blobType = "xml";
                            //string blobType = inputBlob.Metadata["FileType"];
                            IDictionary<string, string> blobMetadata = inputBlob.Metadata;
                            // Identify the type of file
                            using (StreamReader sr = new StreamReader(inputBlob.OpenRead()))
                            {
                                while (!sr.EndOfStream)
                                {
                                    string line = sr.ReadLine();
                                    inputBlobString = inputBlobString + line;
                                }

                            }
                            // Call process xml
                            if (blobType.ToLower() == "xml")
                            {
                                logger.Write(TraceEventType.Information, "Identified file {0} as XML", inputBlob.Name);
                                JSONOutputString = GetJSONFromXML(inputBlobString, inputBlob.Metadata);
                                logger.Write(TraceEventType.Information, "Processed file{0} XML to JSON", inputBlob.Name);
                            }

                            // Call process csv
                            if(blobType.ToLower() == "csv")
                            {
                                logger.Write(TraceEventType.Information, "Identified file {0} as CSV", inputBlob.Name);
                                JSONOutputString = GetJSONFromCSV(inputBlobString, inputBlob.Metadata);
                                logger.Write(TraceEventType.Information, "Processed file{0} CSV to JSON", inputBlob.Name);
                            }

                            // Create JSON file

                            foreach (ResolvedTable transformedFilesTable in transformedFilesTables)
                            {
                                string connectionString = GetConnectionString(transformedFilesTable.LinkedService);
                                string outputfolderPath = GetFolderPath(transformedFilesTable.Table);

                                if (String.IsNullOrEmpty(connectionString) ||
                                    String.IsNullOrEmpty(folderPath))
                                {
                                    continue;
                                }
                                logger.Write(TraceEventType.Information, "Writing blob to: {0}", folderPath);
                                string blobName = Guid.NewGuid().ToString();
                                CloudStorageAccount outputStorageAccount = CloudStorageAccount.Parse(connectionString);
                                Uri outputBlobUri = new Uri(outputStorageAccount.BlobEndpoint, outputfolderPath + "/" + blobName + ".json");

                                CloudBlockBlob outputBlob = new CloudBlockBlob(outputBlobUri, outputStorageAccount.Credentials);
                                //outputBlob.Metadata["InstrumentID"] = inputBlob.Metadata["InstrumentID"];
                                //outputBlob.Metadata["InstrumentType"] = inputBlob.Metadata["InstrumentType"];
                                //outputBlob.Metadata["InstrumentID"] = "TestInstrumentID";
                                //outputBlob.Metadata["TestInstrumentType"] = "TestInstrumentType";
                                //outputBlob.SetMetadata();
                                outputBlob.UploadText(JSONOutputString);


        
                            }

                            

                            

                            

                            //Upload to blob storage 


                        }
                    }
                } while (continuationToken != null);
                

                

               

            }

            return new Dictionary<string, string>();
        }
        private static string GetJSONFromXML(string XMLValue, IDictionary<string,string> metaData)
        {
            string JSONString = "";
            XMLToJSON xmltojson = new XMLToJSON();
            JSONString = xmltojson.ProcessXMLToJSON(XMLValue, metaData);
            return JSONString;
        }
        private static string GetJSONFromCSV(string CSVValue, IDictionary<string,string> metaData)
        {
            string JSONString = "";

            return JSONString;
        }
        private static string GetConnectionString(LinkedService asset)
        {
            AzureStorageLinkedService storageAsset;
            if (asset == null)
            {
                return null;
            }

            storageAsset = asset.Properties as AzureStorageLinkedService;
            if (storageAsset == null)
            {
                return null;
            }

            return storageAsset.ConnectionString;
        }
        private static string GetFolderPath(Table dataArtifact)
        {
            AzureBlobLocation blobLocation;
            if (dataArtifact == null || dataArtifact.Properties == null)
            {
                return null;
            }

            blobLocation = dataArtifact.Properties.Location as AzureBlobLocation;
            if (blobLocation == null)
            {
                return null;
            }

            return blobLocation.FolderPath;
        }
    }
}

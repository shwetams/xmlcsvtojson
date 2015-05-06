using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Xml;
using System.IO;
using Com.StellmanGreene.CSVReader;
using System.Data;
using Newtonsoft.Json;

namespace adf.filetransformation
{
    public class FileDetails
    {
        public string DirectoryPath;
        public string FileName;
        public string FileExtension;
    }
   public class TransformToJSON
    {
        public string ProcessXMLToJSON(string xmlValue, IDictionary<string,string> metaData)
        {
            string jsonString = "";

            XmlDocument xDoc = new XmlDocument();
            xDoc = LoadXMLAndIgnoreComments(xmlValue);
            XmlElement rootNode = xDoc.DocumentElement;
            string metaDataString = CreateMetaDataString(metaData);
            if (metaDataString.Length > 0 && metaDataString != null)
            {
                jsonString = jsonString + "[{" + metaDataString + ","; 
            }
            
            jsonString = jsonString + "\"TriggerTimeStamp\":" + "\"" + DateTime.Now + "\",";
            jsonString = jsonString + ProcessComplexTag(rootNode);
            jsonString = jsonString + "}]";
            return jsonString;
        }

        public string CreateMetaDataString(IDictionary<string, string> metaData)
        {
            string metaDataString = "";
            if (metaData != null)
            {
                metaDataString = "\"MetaData\":{";
                bool isFirstValue = true;
                foreach (var data in metaData)
                {
                    if (isFirstValue)
                    {
                        bool isNumberBoolDateTime = CheckNumberBoolDateTime(data.Value);
                        if (isNumberBoolDateTime)
                        {
                            metaDataString = "\"" + data.Key + "\":" + data.Value;
                        }
                        else
                        {
                            metaDataString = "\"" + data.Key + "\":" + "\"" + data.Value + "\"";
                        }

                        isFirstValue = false;
                    }
                    else
                    {
                        bool isNumberBoolDateTime = CheckNumberBoolDateTime(data.Value);
                        if (isNumberBoolDateTime)
                        {
                            metaDataString = "," + "\"" + data.Key + "\":" + data.Value;
                        }
                        else
                        {
                            metaDataString = "," + "\"" + data.Key + "\":" + "\"" + data.Value + "\"";
                        }

                        isFirstValue = false;
                    }
                }
                metaDataString = metaDataString + "}";

            }
            else
            {
                metaDataString = "\"Metadata\":{\"testkey1\":\"testvalue1\",\"testkey2\":\"testvalue2\"}";
            }
            return metaDataString;
        }
        public XmlDocument LoadXMLAndIgnoreComments(String xmlValue)
        {
            // Create XML reader settings
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreComments = true;                         // Exclude comments
            MemoryStream memStream = new MemoryStream(Encoding.UTF8.GetBytes(xmlValue));
            StreamReader rd = new StreamReader(memStream);
                        
            // Create reader based on settings
            XmlReader reader = XmlReader.Create(rd, settings);
            // Will throw exception if document is invalid
            XmlDocument document = new XmlDocument();
            document.Load(reader);
            return document;


        }
        public string ProcessCSVFile(string csvFileData, bool isFirstRowHeader, IDictionary<string, string> metaData)
        {
            string jsonString = "[";

            var dataTable = CSVReader.ReadCSVData(csvFileData, isFirstRowHeader);
            
            bool isFirstLine = true;
            string metaDataString = CreateMetaDataString(metaData);
            foreach(DataRow row in dataTable.Rows)
            {
                int i = 0;
                if (isFirstLine == true)
                        {
                            jsonString = jsonString + "{";
                            isFirstLine = false;
                        }
                        else
                        {
                            jsonString = jsonString + ",{";
                        }
                bool isFirstValueLine = true;
                foreach(DataColumn column in dataTable.Columns)
                {
                    
                    if(isFirstRowHeader == true)
                    {
                        string rowValue = row[column.ColumnName].ToString();
                        bool checkNumberBoolDate = CheckNumberBoolDateTime(rowValue);
                       
                        if(checkNumberBoolDate)
                        {
                            if (isFirstValueLine == true)
                            {

                                jsonString = jsonString + "\"RowTimeStamp\":\"" + DateTime.Now.ToString() + "\",";
                                jsonString = jsonString + metaDataString + ",";
                                jsonString = jsonString + "\"" + column.ColumnName + "\":" + "\"" + rowValue.Trim() + "\"";
                                isFirstValueLine = false;
                            }
                            else
                            {
                                jsonString = jsonString + ",\"" + column.ColumnName + "\":" + "\"" + rowValue.Trim() + "\"";
                                isFirstValueLine = false;
                            }
                            
                        }
                        else
                        {
                            if (isFirstValueLine == true)
                            {
                                jsonString = jsonString + "\"RowTimeStamp\":\"" + DateTime.Now.ToString() + "\",";
                                jsonString = jsonString + metaDataString + ",";
                                jsonString = jsonString + "\"" + column.ColumnName + "\":" + "\""  +rowValue.Trim() + "\"";
                                isFirstValueLine = false;
                            }
                            else
                            {
                                jsonString = jsonString + ",\"" + column.ColumnName + "\":" + "\"" + rowValue.Trim() + "\"";
                                isFirstValueLine = false;
                            }
                        }
                    }
                    else
                    {
                        string rowValue = row[i].ToString();
                        bool checkNumberBoolDate = CheckNumberBoolDateTime(rowValue.Trim());
                        if (checkNumberBoolDate)
                        {
                            if (isFirstValueLine == true)
                            {
                                jsonString = jsonString + "\"RowTimeStamp\":\"" + DateTime.Now.ToString() + "\",";
                                jsonString = jsonString + metaDataString + ",";
                                jsonString = jsonString + "\"" + "key" + i + "\":" + rowValue.Trim() + "\"";
                                isFirstValueLine = false;
                            }
                            else
                            {
                                jsonString = jsonString + ",\"" + "key" + i + "\":" + "\"" + rowValue.Trim() + "\"";
                                isFirstValueLine = false;
                            }

                        }
                        else
                        {
                            if (isFirstValueLine == true)
                            {
                                jsonString = jsonString + "\"RowTimeStamp\":\"" + DateTime.Now.ToString() + "\",";
                                jsonString = jsonString + metaDataString + ",";
                                jsonString = jsonString + "\"" + "key" + i + "\":" + "\"" + rowValue.Trim() + "\"";
                                isFirstValueLine = false;
                            }
                            else
                            {
                                jsonString = jsonString + ",\"" + "key" + i + "\":" + "\"" + rowValue.Trim() + "\"";
                                isFirstValueLine = false;
                            }
                        }

                    }
                    i++;
                }
                jsonString = jsonString + "}";
             
            }

            //var reader = new StreamReader(File.OpenRead(@"C:\Users\shg\PCBackup_29thAugust2014\Work\Beckman\SampleDocuments\AU680_Sample CSV files\PLCode.csv"));
            
            jsonString = jsonString + "]";
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.StringEscapeHandling = StringEscapeHandling.EscapeHtml;
            var jsonObj = JsonConvert.SerializeObject(jsonString);
            string JsonFormattedString = JsonConvert.DeserializeObject<string>(jsonObj,settings);
            return JsonFormattedString;
        }
        public  string ProcessSimpleTag(XmlNode node)
        {
            string simpleTagJsonString = "";

            simpleTagJsonString = simpleTagJsonString + "\"" + node.LocalName + "\":"; // +"'" + node.Value + "'}";
            if (CheckNumberBoolDateTime(node.InnerText))
            {
                if (node.InnerText.Length <= 0)
                {
                    simpleTagJsonString = simpleTagJsonString + "\"\"";
                }
                else
                {
                    simpleTagJsonString = simpleTagJsonString + "\"" + node.InnerText + "\"";
                }

            }
            else
            {
                if (node.InnerText.Length <= 0)
                {
                    simpleTagJsonString = simpleTagJsonString + "\"\"";
                }
                else
                {
                    simpleTagJsonString = simpleTagJsonString + "\"" + node.InnerText + "\"";
                }

            }
            return simpleTagJsonString;
        }
        public  string ProcessComplexTag(XmlNode node, bool rootNode = false)
        {
            string complexTagJsonString = "";
            if (rootNode == false)
            { complexTagJsonString = complexTagJsonString + "\"" + node.LocalName + "\"" + ":" + "{"; }
            else
            { complexTagJsonString = "{"; }
            bool isFirstValue = true;
            if (node.Attributes.Count > 0)
            {
                XmlAttributeCollection attributes = node.Attributes;
                foreach (XmlAttribute attribute in attributes)
                {
                    if (isFirstValue)
                    {
                        complexTagJsonString = complexTagJsonString + ProcessAttributes(attribute);
                        isFirstValue = false;
                    }
                    else
                    {
                        complexTagJsonString = complexTagJsonString + "," + ProcessAttributes(attribute);
                        isFirstValue = false;
                    }
                }
            }

            if (node.ChildNodes.Count > 1 || (node.ChildNodes.Count == 1 && node.InnerText == null))
            {

                XmlNodeList childNodes = node.ChildNodes;
                List<string> processedChildNodeNames = new List<string>();

                foreach (XmlNode childNode in childNodes)
                {
                    //If there are multiple child nodes of the same local name, then create an array
                    XmlNodeList sameChildNodes = node.SelectNodes(childNode.LocalName);  //Ideally, you must check with NS too

                    if (sameChildNodes.Count == 1)
                    {

                        if ((childNode.ChildNodes.Count > 1 || (childNode.ChildNodes.Count == 1 && childNode.InnerText == null)) || childNode.Attributes.Count > 0)
                        {
                            if (isFirstValue)
                            {
                                complexTagJsonString = complexTagJsonString + ProcessComplexTag(childNode);
                                isFirstValue = false;
                            }
                            else
                            {
                                complexTagJsonString = complexTagJsonString + "," + ProcessComplexTag(childNode);
                                isFirstValue = false;
                            }
                        }
                        else
                        {
                            if (isFirstValue)
                            {
                                complexTagJsonString = complexTagJsonString + ProcessSimpleTag(childNode);
                                isFirstValue = false;
                            }
                            else
                            {
                                complexTagJsonString = complexTagJsonString + "," + ProcessSimpleTag(childNode);
                                isFirstValue = false;
                            }

                        }

                    }
                    else
                    {
                        bool isNotPresent = processedChildNodeNames.Contains(childNode.LocalName);
                        if (isNotPresent == false)
                        {
                            XmlNodeList sameChildNodesList = node.SelectNodes(childNode.LocalName);
                            if (isFirstValue)
                            {
                                complexTagJsonString = complexTagJsonString + "\"" + childNode.LocalName + "\"" + ":" + "[";
                                isFirstValue = false;
                            }
                            else
                            {
                                complexTagJsonString = complexTagJsonString + "," + "\"" + childNode.LocalName + "\"" + ":" + "[";
                                isFirstValue = false;
                            }
                            bool isFirstChildValue = true;
                            foreach (XmlNode sameChildNode in sameChildNodesList)
                            {
                                if ((sameChildNode.ChildNodes.Count > 1 || (sameChildNode.ChildNodes.Count == 1 && sameChildNode.InnerText == null)) || sameChildNode.Attributes.Count > 0)
                                {
                                    if (isFirstChildValue)
                                    {
                                        complexTagJsonString = complexTagJsonString + ProcessComplexTag(sameChildNode, rootNode: true);
                                        isFirstChildValue = false;
                                    }
                                    else
                                    {
                                        complexTagJsonString = complexTagJsonString + "," + ProcessComplexTag(sameChildNode, rootNode: true);
                                        isFirstChildValue = false;
                                    }
                                }
                                else
                                {
                                    if (isFirstValue)
                                    {
                                        complexTagJsonString = complexTagJsonString + ProcessSimpleTag(sameChildNode);
                                        isFirstValue = false;
                                    }
                                    else
                                    {
                                        complexTagJsonString = complexTagJsonString + "," + ProcessSimpleTag(sameChildNode);
                                        isFirstValue = false;
                                    }

                                }

                            }
                            processedChildNodeNames.Add(childNode.LocalName);
                            if (isFirstValue)
                            {
                                complexTagJsonString = complexTagJsonString + "]";
                                isFirstValue = false;
                            }
                            else
                            {
                                complexTagJsonString = complexTagJsonString + "]";
                                isFirstValue = false;
                            }
                        }
                    }
                }
            }
            else
            {
                bool isNodeValueNumber = CheckNumberBoolDateTime(node.InnerText);
                if (isFirstValue)
                {
                    if (isNodeValueNumber)
                    {
                        complexTagJsonString = complexTagJsonString + "\"value1\":" + "\"" + node.InnerText + "\"";
                    }
                    else
                    {
                        complexTagJsonString = complexTagJsonString + "\"value1\":" + "\"" + node.InnerText + "\"";
                    }

                    isFirstValue = false;
                }
                else
                {
                    if (isNodeValueNumber)
                    {
                        complexTagJsonString = complexTagJsonString + ",\"value1\":" + "\"" + node.InnerText + "\"";
                    }
                    else
                    {
                        complexTagJsonString = complexTagJsonString + ",\"value1\":" + "\"" + node.InnerText + "\"";
                    }


                    isFirstValue = false;
                }

            }

            complexTagJsonString = complexTagJsonString + "}";
            return complexTagJsonString;
        }
        public string ProcessAttributes(XmlAttribute attribute)
        {
            string jsonAttributeString = "";
            jsonAttributeString = jsonAttributeString + "\"" + attribute.LocalName + "\":";
            if (CheckNumberBoolDateTime(attribute.Value))
            {
                jsonAttributeString = jsonAttributeString + "\"" + attribute.Value + "\"";
            }
            else
            {
                jsonAttributeString = jsonAttributeString + "\"" + attribute.Value + "\"";
            }
            return jsonAttributeString;
        }
        public  bool CheckNumberBoolDateTime(string value)
        {
            bool isNumberBoolDateTime = false;
            if (value.Length > 0)
            {
                char firstChar = value.ToCharArray()[0];
                if (firstChar != '0')
                {
                    bool isAllDigit = value.All(char.IsDigit);
                    if (isAllDigit)
                    {
                        isNumberBoolDateTime = true;
                    }
                    else
                    {
                        double res; // Ensuring that doubles are also taken care of

                        if (double.TryParse(value, out res))
                        {
                            isNumberBoolDateTime = true;
                        }
                    }

                    if (isNumberBoolDateTime == false)
                    {
                        bool boolRes;
                        if (bool.TryParse(value, out boolRes))
                        {
                            isNumberBoolDateTime = true;
                        }
                    }
                    // Commenting out the date time parsing, as some values are being parsed in .Net and not being processed well in JSON
                    /*
                    if (isNumberBoolDateTime == false)
                    {
                        DateTime dtRes;
                        if (DateTime.TryParse(value, out dtRes))
                        {
                            isNumberBoolDateTime = true;
                        }
                    }*/
                }
            }
            return isNumberBoolDateTime;
        }
        public  FileDetails SplitFilePath(string filePath)
        {
            FileDetails fileDet = new FileDetails();
            string[] filesplitpath = filePath.Split('\\');
            fileDet.FileName = filesplitpath[filesplitpath.Length - 1];
            string dirPath = "";
            for (int i = 0; i < filesplitpath.Length - 1; i++)
            {
                if (dirPath.Length == 0)
                { dirPath = filesplitpath[i]; }
                else { dirPath = dirPath + "\\" + filesplitpath[i]; }

            }
            string[] fileNameSplit = fileDet.FileName.Split('.');
            string fileExt = fileNameSplit[fileNameSplit.Length - 1];
            fileDet.FileExtension = fileExt;
            fileDet.DirectoryPath = dirPath;
            return fileDet;
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

using System.IO;
using System.Xml.Serialization;

namespace NModel.Tools.GML
{
    public class GraphmlCodeGen
    {
        private StringBuilder codeModel = null;        
        private string _modelName;
        private string _inDirName;
        private string _outDirName;
        private string _namespace;

        public GraphmlCodeGen(string modelName, string inDirName, string outDirName)
        {
            if ((outDirName == null) || (outDirName.Equals("")))
                throw new Exception("Missing output directory!");

            if ( ((modelName == null) || (modelName.Equals(""))) &&
                ((inDirName == null) || (inDirName.Equals(""))))
                throw new Exception("Missing input graphml file or directory!");

            _modelName = modelName;
            _inDirName = inDirName;
            _outDirName = outDirName;
            string[] path = outDirName.Split('\\');
            _namespace = path[path.Length - 1];
        }

        public void GenerateClasses()
        {
            if (_inDirName != null)
            {
                DirectoryInfo di = new DirectoryInfo(_inDirName);
                FileInfo[] rgFiles = di.GetFiles("*.graphml");

                if (((_modelName == null) || (_modelName.Equals(""))) &&
                (rgFiles.Length == 0) )
                    throw new Exception("Missing input graphml file or the input directory is empty!");

                foreach (FileInfo fi in rgFiles)
                {
                    processModel(fi.FullName);
                }
                _namespace = di.Name;
            }
            else
            {
                processModel(_modelName);
            }
        }

        private void processModel(string modelName)
        {
            string className = getClassName(modelName);
            string outFileName = _outDirName + "\\" + className + ".cs";
            TextWriter tw = new StreamWriter(outFileName);
            StreamReader sReader;
            sReader = File.OpenText(modelName);
            StringBuilder strModel = new StringBuilder();
            string line;
            while ((line = sReader.ReadLine()) != null)
            {
                if (line.Contains("xmlns"))
                {
                    // Try to replace the namespaces lines that breaks XmlSerializer
                    // If it won't work
                    // you should replace the namespaces lines in each yED's graphml file with the line below, manually!
                    strModel.AppendLine("<graphml xmlns=\"http://graphml.graphdrawing.org/xmlns/graphml\"  xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:schemaLocation=\"http://graphml.graphdrawing.org/xmlns/graphml http://www.yworks.com/xml/schema/graphml/1.0/ygraphml.xsd\" xmlns:y=\"http://www.yworks.com/xml/graphml\">");
                }
                else
                    strModel.AppendLine(line);
            }
            tw.Write(ParseModel(strModel.ToString(), className).ToString());
            tw.Close();
        }

        private string getClassName(string modelName)
        {
            string[] Path = modelName.Split('\\');
            string fullName = Path[Path.Length - 1];
            string[] SeperateExt = fullName.Split('.');
            return (SeperateExt[0]);
        }

        private string[] getBlock(string[] strCode, string blockName)
        {
            ArrayList block = new ArrayList();
            int curLine = 0;
            while (curLine < strCode.Length)
            {
                if (strCode[curLine].ToLower().Trim() == ("<" + blockName + ">"))
                {
                    try
                    {
                        string line;
                        while (((line = strCode[++curLine]).ToLower()) != ("</" + blockName + ">"))
                        {
                            block.Add(line);
                        }
                        curLine = curLine + 1;
                        break;
                    }
                    catch (IndexOutOfRangeException)
                    {
                        throw new Exception("Missing closing " + blockName);
                    }
                }
                curLine = curLine + 1;
            }
            string[] strBlock = new string[block.Count];
            block.CopyTo(strBlock, 0);
            return (strBlock);                           
        }

        private void AppendCommentsInBlock(string[] block)
        {
            for (int curLine = 0; curLine < block.Length; ++curLine)
            {
                if (block[curLine].Trim().StartsWith("//"))
                {
                    codeModel.AppendLine("            " + block[curLine].Trim());
                }
            }
        }

        private void SetBooleanBlock(string[] block)
        {                        
            codeModel.AppendLine("            return(");
            for (int curLine = 0; curLine < block.Length; ++curLine)
            {
                // Remove ';'
                string strLine = block[curLine].Trim().Replace(";", "");
                if ((strLine != "") && (!strLine.StartsWith("//")))
                {
                    codeModel.AppendLine("            (" + strLine + ") &&");
                }
            }
        }

        private StringBuilder ParseModel(string model, string className)
        {
            Dictionary<string, string> states = new Dictionary<string, string>();
            ArrayList acceptingState = new ArrayList();
            string endStateID = null;

            string modifier = "static";

            StringReader strModel = new StringReader(model);
            // Get the graph tab of the xml into the graphmlGraph object 
            XmlSerializer graphmlSerializer = new XmlSerializer(typeof(graphml));
            graphml Model = (graphml)graphmlSerializer.Deserialize(strModel);
            graphmlGraph graph = null;
            foreach (object item in Model.Items)
            {
                if (item.GetType().Name.Equals("graphmlGraph"))
                {
                    graph = (graphmlGraph)item;
                    break;
                }
            }


            // Start creating the C# code in a StringBuilder object
            codeModel = new StringBuilder();

            codeModel.AppendLine("using System;");
            codeModel.AppendLine("using System.Collections;");
            codeModel.AppendLine("using System.Collections.Generic;");
            codeModel.AppendLine("using NModel.Attributes;");
            codeModel.AppendLine("using NModel;");
            codeModel.AppendLine("using NModel.Execution;");
            codeModel.AppendLine("");
            codeModel.AppendLine("namespace " + _namespace);
            codeModel.AppendLine("{");
            codeModel.AppendLine("    /// <summary>");
            codeModel.AppendLine("    /// Model States:");
            codeModel.AppendLine("    /// <summary>");
            codeModel.Append("    public enum ModelStates {");

            foreach (graphmlGraphNode state in graph.node)
            {

                foreach (data dt in state.data)
                {
                    foreach (ShapeNodeNodeLabel nodeLabel in dt.ShapeNode.NodeLabel)
                    {
                        string[] strState = nodeLabel.Value.Split('\n'); // Remove the 'INDEX'
                        string stateName = strState[0].ToLower();
                        if ((!stateName.Equals("start")) &&
                            (!stateName.Equals("end")) &&
                            (!stateName.Equals("model_configured")))
                        {
                            if (states.Count > 0)
                                codeModel.Append(", ");
                            codeModel.Append(stateName);
                            states.Add(state.id, stateName);
                        }
                        else if (stateName.Equals("end"))
                            endStateID = state.id;
                    }
                }
            }
            codeModel.Append("}");
            codeModel.AppendLine("");
            codeModel.AppendLine("");

            // Make sure that Model_Config is parsed first
            foreach (graphmlGraphEdge edge in graph.edge)
            {
                foreach (data dt in edge.data)
                {
                    if ((dt.PolyLineEdge != null) && (dt.PolyLineEdge.EdgeLabel != null))
                    {
                        foreach (PolyLineEdgeEdgeLabel edgeLabel in dt.PolyLineEdge.EdgeLabel)
                        {
                            if (edgeLabel.Value != null)
                            {
                                string[] strCode = edgeLabel.Value.Split('\n');

                                string methodName = getBlock(strCode, "name")[0].ToLower();

                                if (methodName.Equals("model_config"))
                                {
                                    string[] configBlock = getBlock(strCode, "config");
                                    for (int curLine = 0; curLine < configBlock.Length; ++curLine)
                                    {
                                        if (configBlock[curLine].Trim().StartsWith("modifier"))
                                            modifier = (configBlock[curLine].Split(':')[1].Trim());
                                    }
                                }
                            }
                        }
                    }
                }
            }

            // Class modifier and name:
            codeModel.AppendLine("    " + modifier + " class " + className);
            codeModel.AppendLine("    {");

            // Make sure that Model_Init is parsed second
            foreach (graphmlGraphEdge edge in graph.edge)
            {
                foreach (data dt in edge.data)
                {
                    if ((dt.PolyLineEdge != null) && (dt.PolyLineEdge.EdgeLabel != null))
                    {
                        foreach (PolyLineEdgeEdgeLabel edgeLabel in dt.PolyLineEdge.EdgeLabel)
                        {
                            if (edgeLabel.Value != null)
                            {
                                string[] strCode = edgeLabel.Value.Split('\n');
                                string methodName = getBlock(strCode, "name")[0].ToLower();
                                if (methodName.Equals("model_init"))
                                {
                                    codeModel.AppendLine("        " + modifier + " ModelStates _curState = ModelStates." + states[edge.target].Trim() + ";");
                                    string[] initBlock = getBlock(strCode, "init");
                                    for (int curLine = 0; curLine < initBlock.Length; ++curLine)
                                    {
                                        if (initBlock[curLine].Trim() != "")
                                            codeModel.AppendLine("        " + initBlock[curLine].Trim());
                                    }
                                    codeModel.AppendLine("");                                    
                                }
                            }
                        }
                    }
                }
            }


            foreach (graphmlGraphEdge edge in graph.edge)
            {
                foreach (data dt in edge.data)
                {
                    if ((dt.PolyLineEdge != null) && (dt.PolyLineEdge.EdgeLabel != null))
                    {
                        foreach (PolyLineEdgeEdgeLabel edgeLabel in dt.PolyLineEdge.EdgeLabel)
                        {
                            if (edgeLabel.Value != null)
                            {                                
                                string[] strCode = edgeLabel.Value.Split('\n');

                                string methodName = getBlock(strCode, "name")[0].ToLower();
                                string[] parametersBlock = getBlock(strCode, "parameters");
                                string[] reqBlock = getBlock(strCode, "req");
                                string[] guardBlock = getBlock(strCode, "guard");
                                string[] actionBlock = getBlock(strCode, "action");

                                if ((!methodName.Equals("model_config")) && (!methodName.Equals("model_init")))
                                {
                                    codeModel.AppendLine("        [Action]");
                                    for (int curLine = 0; curLine < reqBlock.Length; ++curLine)
                                    {
                                        if (reqBlock[curLine].Trim() != "")
                                            codeModel.AppendLine("        [Requirement(" + reqBlock[curLine].Trim() + ")]");
                                    }                                    

                                    codeModel.AppendLine("        " + modifier + " void " + methodName + "(");
                                    for (int curLine = 0; curLine < parametersBlock.Length; ++curLine)
                                    {
                                        if (parametersBlock[curLine].Trim() != "")
                                            codeModel.AppendLine("                " + parametersBlock[curLine].Trim());
                                    }
                                    codeModel.AppendLine("        )");

                                    codeModel.AppendLine("        {");

                                    for (int curLine = 0; curLine < actionBlock.Length; ++curLine)
                                    {
                                        if (actionBlock[curLine].Trim() != "")
                                            codeModel.AppendLine("            " + actionBlock[curLine].Trim());
                                    }

                                    if (states.ContainsKey(edge.target))
                                        codeModel.AppendLine("            _curState = ModelStates." + states[edge.target].Trim() + ";");

                                    codeModel.AppendLine("        }");

                                    codeModel.AppendLine("        " + modifier + " bool " + methodName.Trim() + "Enabled" + "(");
                                    for (int curLine = 0; curLine < parametersBlock.Length; ++curLine)
                                    {
                                        if (parametersBlock[curLine].Trim() != "")
                                            codeModel.AppendLine("                " + parametersBlock[curLine].Trim());
                                    }
                                    codeModel.AppendLine("        )");

                                    codeModel.AppendLine("        {");
                                    AppendCommentsInBlock(guardBlock);
                                    SetBooleanBlock(guardBlock);
                                    if (states.ContainsKey(edge.source)) // No key for Start and Model_Configured
                                        codeModel.AppendLine("            (_curState == ModelStates." + states[edge.source].Trim() + ")");
                                    codeModel.AppendLine("            );");
                                    codeModel.AppendLine("        }");
                                    codeModel.AppendLine("");
                                }                                
                            }
                            else if (edge.target.Equals(endStateID))
                            {
                                acceptingState.Add("_curState == ModelStates." + states[edge.source].Trim());
                            }
                        }
                    }
                }
            }

            // Set the accepting state
            if (acceptingState.Count > 0)
            {
                codeModel.AppendLine("        [AcceptingStateCondition]");
                codeModel.AppendLine("        " + modifier + " bool IsAcceptingState()");
                codeModel.AppendLine("        {");
                codeModel.AppendLine("            return(");

                string[] acceptingStateArray = new string[acceptingState.Count];
                acceptingState.CopyTo(acceptingStateArray, 0);
                for (int curLine = 0; curLine < acceptingStateArray.Length; ++curLine)
                {                    
                    codeModel.AppendLine("            (" + acceptingStateArray[curLine].Trim() + ") ||");
                }
                codeModel.AppendLine("            false);");
                codeModel.AppendLine("        }");
                codeModel.AppendLine("");
            }


            codeModel.AppendLine("    }");
            codeModel.AppendLine("}");
            return (codeModel);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Text;

namespace NModel.Tools.GML
{
    /// <summary>
    /// Main program for the Code Generator from GraphML utility.
    /// 
    /// GraphML main page: http://graphml.graphdrawing.org/
    /// GraphML is a comprehensive and easy-to-use file format for graphs. 
    /// It consists of a language core to describe the structural properties of 
    /// a graph and a flexible extension mechanism to add application-specific data.
    /// GraphML XSD: http://graphml.graphdrawing.org/xmlns/1.1/graphml.xsd
    /// 
    /// yEd Graph Editor: http://www.yworks.com/en/products_yed_about.html
    /// yEd is a freely available powerful diagram editor that can be used to 
    /// quickly and effectively generate high-quality drawings of diagrams
    /// that can be exported to standard GraphML format.
    /// 
    /// 
    /// If XmlSerializer breaks try to replace the namespaces lines (usually the second line) in each yED model with the following line:
    /// <graphml xmlns="http://graphml.graphdrawing.org/xmlns/graphml"  xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xsi:schemaLocation="http://graphml.graphdrawing.org/xmlns/graphml http://www.yworks.com/xml/schema/graphml/1.0/ygraphml.xsd" xmlns:y="http://www.yworks.com/xml/graphml">
    ///
    /// 
    /// </summary>
    class Program
    {
        /// <summary>
        /// Code Generator from GraphML utility
        /// </summary>
        /// <param name="args">GraphMl file or folder with several GraphMl files, path to folder where to save the generated class files</param>
        /// <returns>0 if application returns normally, returns -1 otherwise</returns>
        static int Main(string[] args)
        {
            GML_CommandLine.RunWithCommandLineArguments(args);
            return 0;
        }
    }
}

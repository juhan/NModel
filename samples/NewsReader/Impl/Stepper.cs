using System;
using NModel.Conformance;
using NModel.Terms;
namespace NewsReader.Impl{    
    public class Stepper: IStepper    {	
        public CompoundTerm DoAction(CompoundTerm action)	{	    
            switch (action.FunctionSymbol.ToString())	    {	        
                case("Tests"): return null; // first action in test seq.	        
                case ("SelectMessages"): 		    
                    NewsReaderUI.SelectMessages(); return null;	        
                case ("SelectTopics"): 		    
                    NewsReaderUI.SelectTopics(); return null;	        
                case ("ShowTitles"): 		    
                    NewsReaderUI.ShowTitles(); return null;	        
                case ("ShowText"): 		    
                    NewsReaderUI.ShowText(); 
                    return null;	        
                case ("SortByFirst"): 		    
                    NewsReaderUI.SortByFirst(); 
                    return null;	        
                case ("SortByMostRecent"): 		    
                    NewsReaderUI.SortByMostRecent(); 
                    return null;                
                default: 
                    throw new Exception("Unexpected action " + action);            
            }        
        }        
        public void Reset()        
        {	    
            NewsReaderUI.Reset();        
        }        
        
        public static IStepper Make()        
        {            
            return new Stepper();        
        }    
    }
}
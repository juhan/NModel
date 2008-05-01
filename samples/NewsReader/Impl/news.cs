// news.cs - news reader GUI, simple FSM

using System;

namespace NewsReader.Impl {
  public static class NewsReaderUI {

    // Types for each state variable
    enum Page { Topics, Messages };
    enum Style { WithText, TitlesOnly };
    enum Sort { ByFirst, ByMostRecent };

    // State variables, initial state, make public so Stepper can reset
    static Page page = Page.Topics;
    static Style style = Style.WithText;
    static Sort sort = Sort.ByMostRecent;

      // Reset needed by test harness
      public static void Reset() {
	  page = Page.Topics; style = Style.WithText; sort = Sort.ByMostRecent;
      }

    // Actions
    public static void SelectMessages() {
      if (page == Page.Topics) 
	{
	    Console.WriteLine("Impl: SelectMessages");
	    page = Page.Messages;
	}
      else throw new ApplicationException("SelectMessages not enabled");
    }
    
    public static void SelectTopics() {
      if (page == Page.Messages) 
	{
	    Console.WriteLine("Impl: SelectTopics");
	    // BUG! State not updated, should be  page = Page.Topics;
	}
      else throw new ApplicationException("SelectTopics not enabled");
    }

    public static void ShowTitles() {
      if (page == Page.Topics
	  && style == Style.WithText) 
	{
	    Console.WriteLine("Impl: ShowTitles");
	    style = Style.TitlesOnly;
	}
      else throw new ApplicationException("ShowTitles not enabled");
    }

    public static void ShowText() {
      if (page == Page.Topics
	  && style == Style.TitlesOnly) 
	{
	    Console.WriteLine("Impl: ShowText");
	    style = Style.WithText;
	}
      else throw new ApplicationException("ShowText not enabled");
    }
  
    public static void SortByFirst() {
      if (page == Page.Topics
	  && style == Style.TitlesOnly
	  && sort == Sort.ByMostRecent) 
	{
	    Console.WriteLine("Impl: SortByFirst");
	    sort = Sort.ByFirst;
	}
      else throw new ApplicationException("SortByFirst not enabled");
    }

    public static void SortByMostRecent() {
      if (page == Page.Topics
	  && style == Style.TitlesOnly
	  && sort == Sort.ByFirst)
	{
	    Console.WriteLine("Impl: SortByMostRecent");
	    sort = Sort.ByMostRecent;
	}
      else throw new ApplicationException("SortByMostRecent not enabled");
    }
       
  } // end class 
} // end namespace


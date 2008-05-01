using System;

using System.Text;

using System.Collections.Generic;

using NModel;

using NModel.Attributes;

using NModel.Execution;



namespace NewsReader

{

    public static class NewsReaderUI

    {

	// Types for each state variable

	public enum Page { Topics, Messages };

	public enum Style { WithText, TitlesOnly };

	public enum Sort { ByFirst, ByMostRecent };



	// State variables, initial state

	public static Page page = Page.Topics;

	public static Style style = Style.WithText;

	public static Sort sort = Sort.ByMostRecent;



	// Helper, progress messages

	static void Show()

	{

	    Console.Write("{0}, {1}, {2}: ", page, style, sort);

	}



	// Actions: enabling condition, then action method



	public static bool SelectMessagesEnabled() 

	{ 

	    return (page == Page.Topics);

	}



	[Action]

	public static void SelectMessages()

	{

	    Show(); Console.WriteLine("SelectMessages");

	    page = Page.Messages;

	}



	public static bool SelectTopicsEnabled() 

	{ 

	    return (page == Page.Messages); 

	}



	[Action]

	public static void SelectTopics()

	{

	    Show(); Console.WriteLine("SelectTopics");

	    page = Page.Topics;

	}

 

	public static bool ShowTitlesEnabled() 

	{

           return (page == Page.Topics && style == Style.WithText); 

	}



	[Action]

	public static void ShowTitles()

	{

	    Show(); Console.WriteLine("ShowTitles");

	    style = Style.TitlesOnly;

	}



	public static bool ShowTextEnabled() 

	{ 

	    return (page == Page.Topics && style == Style.TitlesOnly);

	}



	[Action]

	public static void ShowText()

	{

	    Show(); Console.WriteLine("ShowText");

	    style = Style.WithText;

	}



	public static bool SortByFirstEnabled() 

	{ 

	    return (page == Page.Topics && style == Style.TitlesOnly

		    && sort == Sort.ByMostRecent);

	}



	[Action]

	public static void SortByFirst()

	{

	    Show(); Console.WriteLine("SortByFirst");

	    sort = Sort.ByFirst;

	}



	public static bool SortByMostRecentEnabled() 

	{ 

	    return (page == Page.Topics && style == Style.TitlesOnly

		    && sort == Sort.ByFirst);

	}



	[Action]

	public static void SortByMostRecent()

	{

	    Show(); Console.WriteLine("SortByMostRecent");

	    sort = Sort.ByMostRecent;

	}



    }



    public static class Factory

    {

        /// <summary>

        /// Factory method

        /// </summary>

        public static ModelProgram CreateNewsReader()

        {

            return new LibraryModelProgram(typeof(Factory).Assembly, "NewsReader");

        }

    }

}


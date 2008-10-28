using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;
using System.Xml;
using System.Text.RegularExpressions;
//using System.Web.SessionState;

using WebModel;

namespace Adapter
{
    public class Session
    {
        public static void AddStandardHeaders(HttpWebRequest request)
        {
            request.UserAgent = "Mozilla/5.0";
            request.Accept = "text/xml,application/xml,application/xhtml+xml,text/html;q=0.9,text/plain;q=0.8,image/png,*/*;q=0.5";
            request.Expect = null;
        }


        public TextWriter logWriter = Console.Error;

        private CookieCollection cookies;

        private Encoding encoding = new UTF8Encoding();


        public string getQuery(string url, string vars)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url + (vars.Length > 0 ? "?" + vars : ""));
            AddStandardHeaders(request);

            request.Method = "GET";
            request.ContentType = "text/xml; encoding='utf-8'";
            request.CookieContainer = new CookieContainer();
            if (cookies != null)
                foreach (Cookie c in cookies)
                {
                    request.CookieContainer.Add(c);
                }
            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                if (response.Cookies.Count > 0)
                    cookies = response.Cookies;
                Stream responseStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(responseStream);

                string page = "";
                while (!reader.EndOfStream)
                    page += reader.ReadLine();
                reader.Close();
                response.Close();
                return page;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public string postQuery(string url, string referer, string vars)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

            AddStandardHeaders(request);
            request.ContentType = "application/x-www-form-urlencoded";
            request.Method = "POST";
            request.KeepAlive = true;
            request.Referer = referer;


            request.CookieContainer = new CookieContainer();
            if (cookies != null)
                foreach (Cookie c in cookies)
                {
                    request.CookieContainer.Add(c);
                }

            byte[] postBytes = encoding.GetBytes(vars);
            request.ContentLength = postBytes.Length;
            try
            {
                Stream postStream = request.GetRequestStream();
                postStream.Write(postBytes, 0, postBytes.Length);
                postStream.Close();
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream responseStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(responseStream);
                string page = "";
                while (!reader.EndOfStream)
                    page += reader.ReadLine();
                reader.Close();
                response.Close();
                return page;
            }

            catch (Exception ex)
            {
                logWriter.Flush();
                throw new Exception(ex.Message);
            }

        }


        /// <summary>
        /// Just a scenario to test whether the adapter actually does something useful.
        /// This is helpful to try out the adapter without calling ct.exe, the conformance
        /// tester of NModel.
        /// </summary>
        /// <param name="args"></param>
        public static void Main(String[] args)
        {

            Session session = new Session();

            string currentURL = "http://192.168.32.128/doStuff.php";
            string currentUser = "user";
            string currentPassword = "123";

            

            string page = "";
            page = session.getQuery(currentURL, "");

            string queryString = "username=" + currentUser + "&" +
                     "password=" + currentPassword;

            page = session.postQuery(currentURL, currentURL, queryString);



            Console.WriteLine(page);


        }

    }
}

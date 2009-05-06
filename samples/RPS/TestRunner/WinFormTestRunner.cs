// Reflection-Based UI control

using System;
using System.Reflection;
using System.Windows.Forms;
using System.Threading;
using System.Drawing;
using System.Diagnostics;

namespace WinFormTestRunner
{
    public class TestRunner
    {
        static BindingFlags flags = BindingFlags.Public |
                                    BindingFlags.NonPublic |
                                    BindingFlags.Static |
                                    BindingFlags.Instance;
        static AutoResetEvent resetEvent = new AutoResetEvent(false);

        private static TestRunner m_runnerInstance = null;
        private string m_formName;
        private string m_implPath;
        Form m_form = null;

        public static TestRunner getTestRunner(string formName, string implPath)
        {
            if (m_runnerInstance == null)            
                m_runnerInstance = new TestRunner(formName, implPath);
            return m_runnerInstance;
        }

        private TestRunner(string formName, string implPath)
        {
            m_formName = formName;
            m_implPath = implPath;            
        }

        [STAThread]
        public void LaunchApp()
        {            
            Assembly a = Assembly.LoadFrom(m_implPath);
            Type t = a.GetType(m_formName);
            m_form = (Form)a.CreateInstance(t.FullName);
            AppState aps = new AppState(m_form);
            ThreadStart ts = new ThreadStart(aps.RunApp);
            Thread thread = new Thread(ts);
            thread.Start();            
        }

        public void closeImplWindow()
        {            
            //Process[] processes = Process.GetProcessesByName("WinFormImpl");
            //foreach (Process p in processes)
            //{
            //    p.CloseMainWindow();
            //}
            this.clickButton("Exit");
            m_form = null;
        }

        private class AppState
        {
            public readonly Form formToRun;
            public AppState(Form f)
            {
                this.formToRun = f;
            }
            public void RunApp()
            {
                Application.Run(formToRun);
            }
        }

        public void moveForm(Point targetPoint)
        {
            SetFormPropertyValue("Location", targetPoint);
        }

        public void setPlayer(string playerName, string value)
        {
            string txtBoxName;
            string btnName;

            if (playerName == "Player1")
            {
                txtBoxName = "tbPlayer1";
                btnName = "Submit1";
            }
            else if (playerName == "Player2")
            {
                txtBoxName = "tbPlayer2";
                btnName = "Submit2";
            }
            else
                throw new Exception("No such player");

            SetControlPropertyValue(txtBoxName, "Text", value);
            clickButton(btnName);
        }

        public string getLastResult()
        {
            return ( (string)
                GetControlPropertyValue("tbResults", "Text") );                                                    
        }

        private void clickButton(string buttonName)
        {
            object[] parms = new object[] { null, EventArgs.Empty };

            if (buttonName == "Submit1")
                InvokeMethod("Submit1_Click", parms);

            else if (buttonName == "Submit2")
                InvokeMethod("Submit2_Click", parms);
            
            else if (buttonName == "Exit")            
                InvokeMethod("exitToolStripMenuItem_Click", parms);
            
            else
                throw new Exception("No such button");
        }

        delegate void SetFormPropertyValueHandler(string propertyName, object newValue);
        private void SetFormPropertyValue(string propertyName,object newValue)
        {
            if (m_form.InvokeRequired)
            {
                Delegate d = new SetFormPropertyValueHandler(SetFormPropertyValue);
                object[] o = new object[] { propertyName, newValue };                
                m_form.Invoke(d, o);               
                resetEvent.WaitOne();                
            }
            else
            {
                Type t = m_form.GetType();
                PropertyInfo pi = t.GetProperty(propertyName);
                pi.SetValue(m_form, newValue, null);
                resetEvent.Set();
            }
        }

        delegate void SetControlPropertyValueHandler(string controlName, string propertyName, object newValue);
        private void SetControlPropertyValue(string controlName,string propertyName, object newValue)
        {
            if (m_form.InvokeRequired)
            {
                Delegate d = new SetControlPropertyValueHandler(SetControlPropertyValue);
                object[] o = new object[] { controlName, propertyName, newValue };
                m_form.Invoke(d, o);
                resetEvent.WaitOne();
            }
            else
            {
                Type t1 = m_form.GetType();
                FieldInfo fi = t1.GetField(controlName, flags);
                object ctrl = fi.GetValue(m_form);
                Type t2 = ctrl.GetType();
                PropertyInfo pi = t2.GetProperty(propertyName);
                pi.SetValue(ctrl, newValue, null);
                resetEvent.Set();
            }
        }

        delegate void InvokeMethodHandler(string methodName,params object[] parms);
        private void InvokeMethod(string methodName,params object[] parms)
        {
            if (m_form.InvokeRequired)
            {
                Delegate d = new InvokeMethodHandler(InvokeMethod);
                m_form.Invoke(d, new object[] { methodName, parms });
                resetEvent.WaitOne();
            }
            else
            {
                Type t = m_form.GetType();
                MethodInfo mi = t.GetMethod(methodName, flags);
                mi.Invoke(m_form, parms);
                resetEvent.Set();
            }
        }

        delegate object GetControlPropertyValueHandler(string controlName, string propertyName);
        private object GetControlPropertyValue(string controlName,string propertyName)
        {
            if (m_form.InvokeRequired)
            {
                Delegate d = new GetControlPropertyValueHandler(GetControlPropertyValue);
                object[] o = new object[] { controlName, propertyName };
                object iResult = m_form.Invoke(d, o);
                resetEvent.WaitOne();
                return iResult;
            }
            else
            {
                Type t1 = m_form.GetType();
                FieldInfo fi = t1.GetField(controlName, flags);
                object ctrl = fi.GetValue(m_form);
                Type t2 = ctrl.GetType();
                PropertyInfo pi = t2.GetProperty(propertyName);
                object gResult = pi.GetValue(ctrl, null);
                resetEvent.Set();
                return gResult;
            }
        }
    }
}

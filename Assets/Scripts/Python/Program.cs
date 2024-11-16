using UnityEngine;
using Python.Runtime;

public class Program : MonoBehaviour
{
    static void RunScript(string scriptName)
    {
        PythonEngine.Exec("import sys; sys.modules.clear()");
        using (Py.GIL())
        {
            var pythonScript = Py.Import(scriptName);
            PythonEngine.Exec($"import {scriptName}");
            var message = new PyString("Message from Nick.");
            var result = pythonScript.InvokeMethod("fuck", new PyObject[] {message});
            Debug.Log(result.ToString());
        }
    }

    private void Start()
    {
        RunScript("mypythonscript");
    }
}

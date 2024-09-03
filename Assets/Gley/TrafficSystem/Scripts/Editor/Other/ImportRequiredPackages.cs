using UnityEditor;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.Events;

namespace Gley.TrafficSystem.Editor
{
    public class ImportRequiredPackages
    {
        private static AddRequest _request;
        private static UnityAction<string> _updateMethod;


        public static void ImportPackages(UnityAction<string> UpdateMethod)
        {
            _updateMethod = UpdateMethod;
            Debug.Log("Installation started. Please wait");
            _request = UnityEditor.PackageManager.Client.Add("com.unity.burst");
            EditorApplication.update += Progress;
        }


        private static void Progress()
        {
            _updateMethod(_request.Status.ToString());
            if (_request.IsCompleted)
            {
                if (_request.Status == UnityEditor.PackageManager.StatusCode.Success)
                {
                    Debug.Log("Installed: " + _request.Result.packageId);
                    _updateMethod("Installed: " + _request.Result.packageId);
                }
                else
                {
                    if (_request.Status >= UnityEditor.PackageManager.StatusCode.Failure)
                    {
                        Debug.Log(_request.Error.message);
                        _updateMethod(_request.Error.message);

                    }
                }
                EditorApplication.update -= Progress;
            }
        }
    }
}

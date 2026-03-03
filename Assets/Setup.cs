/*using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace ArcticWolves.3D.Setup.Files {

public class Setup : MonoBehaviour {
    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }
}

static class Packages {
    static AddRequest Request;
    static Queue<string> packagesToInstall = new();

    public static void Install(string[] packages) {
        foreach (var package in packages) {
            packagesToInstall.Enqueue(package);
        }
        if (packagesToInstall.Count > 0) {
            Request = Client.Add(packagesToInstall.Dequeue());
            EditorApplication.update += Progress;
        }
    }
    static async void Progress() {
        if (Request.IsCompleted) {
            if (Request.Status == StatusCode.Success) {
                Debug.Log("Installed: " + Request.Result.packageId);
            } else if (Request.Status >= StatusCode.Failure)
                Debug.Log(Request.Error.message);

            EditorApplication.update -= Progress;

            if (packagesToInstall.Count > 0) {

                await Task.Delay(1000);

                Request = Client.Add(packagesToInstall.Dequeue());
                EditorApplication.update += Progress;
            }
        }
    }
}
}*/
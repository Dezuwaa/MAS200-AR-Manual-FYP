using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class PdfOpener : MonoBehaviour
{
    // Must match the exact file name in StreamingAssets
    private string pdfFileName = "Body Supply Station Manual.pdf";

    void OnEnable()
    {
        EventBus.OnPdfButtonClicked += OpenPDF;
    }

    void OnDisable()
    {
        EventBus.OnPdfButtonClicked -= OpenPDF;
    }

    private void OpenPDF()
    {
        // Start the coroutine to handle the asynchronous extraction
        StartCoroutine(ExtractAndOpenPDF());
    }

    private IEnumerator ExtractAndOpenPDF()
    {
        string sourcePath = Path.Combine(Application.streamingAssetsPath, pdfFileName);
        string persistentPath = Path.Combine(Application.persistentDataPath, pdfFileName);

        // On Android, StreamingAssets are inside the APK, requiring UnityWebRequest to extract
        if (sourcePath.Contains("://") || sourcePath.Contains(":///"))
        {
            using (UnityWebRequest www = UnityWebRequest.Get(sourcePath))
            {
                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"Failed to load PDF: {www.error}");
                    yield break;
                }

                // Save the file to persistentDataPath where external apps can access it
                File.WriteAllBytes(persistentPath, www.downloadHandler.data);
            }
        }
        else
        {
            // On the Unity Editor or iOS, we can copy the file directly
            if (!File.Exists(persistentPath))
            {
                File.Copy(sourcePath, persistentPath);
            }
        }

        // Optional: Good for debugging in Android Logcat to ensure the button event fired
        Debug.Log($"Successfully extracted MAS200 manual to: {persistentPath}");
        
        // Use Native Share to safely pass the file to the OS
        new NativeShare()
        .AddFile(persistentPath, "application/pdf")
        .Share();
    }
}
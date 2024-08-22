using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Gameplay
{
    public class FirestoreManager : MonoBehaviour
    {
        private const string PROJECT_ID = "econagar";
        private const string API_KEY = "BDazzRM-2cG4o4LehnIJC-Hz8hYj_cfcJVTE9itcxIniTz-Btw2P4EZgH153E5KPDPY8dU9N0RdIvvkMR3fuckk";

        public static FirestoreManager I;
        public ScoreData[] topScores;

        [Serializable]
        public class ScoreData
        {
            public string name;
            public int score;
        }

        [Serializable]
        public class FirestoreResponse
        {
            public List<FirestoreDocument> documents;
        }
        
        [Serializable]
        public class FirebaseResponse
        {
            public string name;
            public string bucket;
            public string generation;
            public string metageneration;
            public string contentType;
            public string timeCreated;
            public string updated;
            public string storageClass;
            public string size;
            public string md5Hash;
            public string contentEncoding;
            public string contentDisposition;
            public string crc32c;
            public string etag;
            public string downloadTokens;
            public string mediaLink;  // This is the direct URL to the file
        }

        [Serializable]
        public class FirestoreDocument
        {
            public FirestoreFields fields;
        }

        [Serializable]
        public class FirestoreFields
        {
            public FirestoreValue name;
            public FirestoreValue score;
        }

        [Serializable]
        public class FirestoreValue
        {
            public string stringValue;
            public long integerValue;
        }
    
        private void Awake()
        {
            I = this;
            GetTopScores();
        }
        
        public void UploadScreenshot(byte[] imageBytes, string fileName, Action<string> callback)
        {
            string storageUrl = $"https://firebasestorage.googleapis.com/v0/b/{PROJECT_ID}.appspot.com/o?uploadType=media&name={fileName}&key={API_KEY}";
            StartCoroutine(UploadImageRequest(storageUrl, imageBytes, callback));
        }

        private IEnumerator UploadImageRequest(string url, byte[] imageBytes, Action<string> callback)
        {
            UnityWebRequest www = UnityWebRequest.PostWwwForm(url, UnityWebRequest.kHttpVerbPOST);
            UploadHandlerRaw uploadHandler = new UploadHandlerRaw(imageBytes);
            uploadHandler.contentType = "image/png";
            www.uploadHandler = uploadHandler;

            Debug.Log($"Uploading screenshot to {url}");

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError(www.error);
            }
            else
            {
                Debug.Log("Screenshot uploaded successfully!");
                callback?.Invoke(www.downloadHandler.text);
            }
        }

        
        public void AddScore(string name, int score, Action<string> callback)
        {
            string url = $"https://firestore.googleapis.com/v1/projects/{PROJECT_ID}/databases/(default)/documents/scores?key={API_KEY}";

            string json = $"{{\"fields\": {{\"name\": {{\"stringValue\": \"{name}\"}}, \"score\": {{\"integerValue\": {score}}}}}}}";

            StartCoroutine(PostRequest(url, json, callback));
        }
    
        public void GetTopScores()
        {
            string url = $"https://firestore.googleapis.com/v1/projects/{PROJECT_ID}/databases/(default)/documents/scores?pageSize=10&orderBy=score desc&key={API_KEY}";
            StartCoroutine(GetRequest(url));
        }

        private IEnumerator GetRequest(string url)
        {
            UnityWebRequest www = UnityWebRequest.Get(url);
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
            }
            else
            {
                List<ScoreData> scores = ParseScores(www.downloadHandler.text);
                topScores = scores.ToArray();
            }
        }
        private IEnumerator PostRequest(string url, string json, Action<string> callback = null)
        {
            byte[] body = System.Text.Encoding.UTF8.GetBytes(json);
        
            UnityWebRequest www = new UnityWebRequest(url, "POST");
            www.uploadHandler = new UploadHandlerRaw(body);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            Debug.Log(json);
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
            }
            else
            {
                callback?.Invoke(www.downloadHandler.text);
                Debug.Log("Score added successfully!");
            }
        }
    
        private List<ScoreData> ParseScores(string json)
        {
            List<ScoreData> scores = new List<ScoreData>();
            FirestoreResponse response = JsonUtility.FromJson<FirestoreResponse>(json);

            foreach (FirestoreDocument document in response.documents)
            {
                string name = document.fields.name.stringValue;
                long score = document.fields.score.integerValue;
                scores.Add(new ScoreData { name = name, score = (int)score });
            }

            return scores;
        }
    }
}
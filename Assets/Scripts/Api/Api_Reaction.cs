using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;

public class Api_Reaction : MonoBehaviour
{
    [System.Serializable]
    public class Result
    {
        public bool trigger;
        public string effect;
    }

    private string apiUrl = "http://192.168.0.64:8000/process-form";
    public string chatText = "너무 귀여워요! 앵콜!";

    void Start()
    {
        SendToServer(chatText); // 자동으로
    }

    public void OnSendButtonClicked() // 버튼 클릭시
    {
        SendToServer(chatText);
    }

    public void SendToServer(string chatText, string gestureType = "", string audioPath = "")
    {
        StartCoroutine(SendRequest(chatText, gestureType, audioPath));
    }

    private IEnumerator SendRequest(string chatText, string gestureType, string audioPath)
    {
        WWWForm form = new WWWForm();
        form.AddField("chat_text", chatText);

        if (!string.IsNullOrEmpty(gestureType))
            form.AddField("gesture_type", gestureType);

        if (!string.IsNullOrEmpty(audioPath))
            form.AddField("audio_path", audioPath);

        using (UnityWebRequest www = UnityWebRequest.Post(apiUrl, form))
        {
            www.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("서버 통신 실패: " + www.error);
            }
            else
            {
                string json = www.downloadHandler.text;
                Debug.Log("서버 응답: " + json);

                Result result = JsonConvert.DeserializeObject<Result>(json);
                if (result.trigger)
                {
                    Debug.Log("이펙트 발동: " + result.effect);
                    // 실제 이펙트 재생 코드 여기에 추가
                }
            }
        }
    }
}
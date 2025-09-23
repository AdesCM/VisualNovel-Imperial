using UnityEngine;
using System.IO;

public class SaveDebugLogOnly : MonoBehaviour
{
    string logPath;

    void OnEnable()
    {
        // 저장될 경로 (프로젝트 폴더 내부)
        logPath = Application.dataPath + "/DebugOnlyLogs.txt";

        // 기존 파일이 있으면 삭제해서 새로 시작
        if (File.Exists(logPath))
            File.Delete(logPath);

        // 로그 이벤트 등록
        Application.logMessageReceived += HandleLog;
    }

    void OnDisable()
    {
        // 로그 이벤트 해제
        Application.logMessageReceived -= HandleLog;
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        // Debug.Log 로 출력된 메시지만 저장
        if (type == LogType.Log)
        {
            File.AppendAllText(logPath, logString + "\n");
        }
    }
}

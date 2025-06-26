using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Compilation;
#endif

public class CompileFixer : MonoBehaviour
{
    #if UNITY_EDITOR
    [MenuItem("Tools/Fix Compilation Issues")]
    public static void FixCompilation()
    {
        // 1. 모든 스크립트 재컴파일
        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        
        // 2. 컴파일 요청
        CompilationPipeline.RequestScriptCompilation();
        
        // 3. 에디터 설정 저장
        EditorUtility.SetDirty(Selection.activeObject);
        AssetDatabase.SaveAssets();
        
        Debug.Log("Compilation fix attempted. Please wait for Unity to recompile...");
    }
    
    [MenuItem("Tools/Clear Script Cache")]
    public static void ClearScriptCache()
    {
        // Library 폴더의 스크립트 캐시 삭제
        System.IO.Directory.Delete("Library/ScriptAssemblies", true);
        System.IO.Directory.Delete("Library/ShaderCache", true);
        
        AssetDatabase.Refresh();
        Debug.Log("Script cache cleared. Unity will now reimport all scripts.");
    }
    #endif
}
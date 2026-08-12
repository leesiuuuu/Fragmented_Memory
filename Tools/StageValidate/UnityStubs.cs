using System;

// 유니티 없이 StageGenerator를 돌리기 위한 최소 스텁.
// 생성기가 UnityEngine에서 실제로 쓰는 것은 Debug.LogError와 Mathf.Max뿐이다.
namespace UnityEngine
{
    public class ScriptableObject
    {
        public string name = "";
    }

    public class GameObject
    {
        public string name = "";
        public GameObject(string n) { name = n; }
    }

    public static class Debug
    {
        public static void Log(object m) => Console.WriteLine(m);
        public static void LogError(object m) => Console.WriteLine("[Unity.LogError] " + m);
        public static void LogWarning(object m) => Console.WriteLine("[Unity.LogWarning] " + m);
    }

    // EnemyStats를 Setup() 단위로 검사하기 위한 최소 표면.
    // 생명주기(Awake/Start)는 유니티가 부르는 것이므로 여기서는 흉내내지 않는다.
    public class MonoBehaviour
    {
        public string name = "";
    }

    public static class Mathf
    {
        public static int Max(int a, int b) => Math.Max(a, b);
        public static int Min(int a, int b) => Math.Min(a, b);
        public static float Pow(float a, float b) => (float)Math.Pow(a, b);
        public static int RoundToInt(float f) => (int)Math.Round(f, MidpointRounding.ToEven);
    }

    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    public class HeaderAttribute : Attribute
    {
        public HeaderAttribute(string header) { }
    }

    [AttributeUsage(AttributeTargets.All)]
    public class TooltipAttribute : Attribute
    {
        public TooltipAttribute(string tooltip) { }
    }

    [AttributeUsage(AttributeTargets.All)]
    public class SerializeFieldAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.All)]
    public class RangeAttribute : Attribute
    {
        public RangeAttribute(float min, float max) { }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class CreateAssetMenuAttribute : Attribute
    {
        public string fileName;
        public string menuName;
        public int order;
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using TMPro;

[AddComponentMenu("XR/XR Origin")]
[DisallowMultipleComponent]
public class control: MonoBehaviour
{
    [SerializeField]
    Text m_LogText;

    [SerializeField]
    Text m_LocText;
 
    AROcclusionManager m_occlusionManager;

    public GameObject Sphere;
    public GameObject dummy; // navi模型的轴向不对，嵌套了一个空对象方便对轴。。
    public Vector3 aim; // 要指向的目标，upload里调用
    // public GameObject TypeText;

    [Serializable]
    public class Loc
    {
        public long timestamp;
        public string x;
        public string y;
        public string z;
    }

    [Serializable]
    public class Device
    {
        public string mac;
        public string type;
        public float x;
        public float y;
        public float z;
    }
    
    public Text logText
    {
        get { return m_LogText; }
        set { m_LogText = value; }
    }

    public Text locText
    {
        get { return m_LocText; }
        set { m_LocText = value; }
    }

    void Log(string message)
    {
        m_LogText.text += $"{message}" + "\n";
    }

    void LocLog(string message)
    {
        m_LocText.text = $"{message}";
    }

    /// <summary>
    /// Unity call Android
    /// https://docs.unity.cn/cn/current/ScriptReference/AndroidJavaObject.CallStatic.html
    /// </summary>
    public void SendVIO(string json)
    {
        AndroidJavaObject javaObject = new AndroidJavaObject("com.unity3d.player.UnityPlayerActivity");
        javaObject.Call("SendVIO", json);
    }


    /// <summary>
    ///  根据Android发送的定位 新建预制件实例
    /// </summary>
    public void Locator(string str)
    {
        Log(str);
        Device device = JsonUtility.FromJson<Device>(str);
        // TODO 根据type，套不同模型。点击能显示文本么？或者模型上能体现种类
        Log("发现新的IoT设备：" + $"{device.mac}" + " " + $"{device.type}" + " " + $"{device.x}" + " " + $"{device.y}" + " " + $"{device.z}");
        
        // https://blog.csdn.net/weixin_43913272/article/details/90246161
        Instantiate(Sphere, new Vector3(device.x, device.y, device.z), Quaternion.identity);

        // GameObject textInstance = Instantiate(TypeText);
        // // 获取 TextMeshProUGUI 组件
        // TextMeshProUGUI textComponent = textInstance.GetComponent<TextMeshProUGUI>();
        // // 设置文本内容和位置
        // textComponent.text = device.type;
        // textInstance.transform.position = new Vector3(device.x, device.y + (float)0.15, device.z);
    }

    // 修改Aimer，这样navi指向就变了
    // 偷懒，直接用Device了
    public void Aimer(string str)
    {
        Log(str);
        Device device = JsonUtility.FromJson<Device>(str);
        aim = new Vector3(device.x, device.y, device.z);
    }


    // Start is called before the first frame update
    void Start()
    {
        // Locator("{ \"mac\":\"ff:ff:ff:ff:ff:ff\", \"type\": \"camera\", \"x\": 0.1541, \"y\": 0, \"z\": -0.1541 }");
        // Locator("{ \"mac\":\"ff:ff:ff:ff:ff:ff\", \"type\": \"camera\", \"x\": -0.1541, \"y\": 0, \"z\": -0.1541 }");
        m_occlusionManager = (Camera.main).GetComponent<AROcclusionManager>();
        dummy = GameObject.Find("dummy");
        aim = new Vector3(0, 0, 0);
    }

    void Occlusion() {
        // https://docs.unity3d.com/Packages/com.unity.xr.arsubsystems@4.1/api/UnityEngine.XR.ARSubsystems.EnvironmentDepthMode.html
        if((int)m_occlusionManager.requestedEnvironmentDepthMode != 0)
        {
            m_occlusionManager.requestedEnvironmentDepthMode = (EnvironmentDepthMode)0;
        } else {
            m_occlusionManager.requestedEnvironmentDepthMode = (EnvironmentDepthMode)1;
        }
    }


    // Update is called once per frame
    void Update()
    {
        var mc = Camera.main;
        var trans = mc.transform;
        var curPos = trans.localPosition;

        // https://blog.csdn.net/Uqiumu/article/details/106668615
        // var msg = ((DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000) + ": " + "\n" + curPos.x + ", " + curPos.y + ", " + curPos.z;
        // SendVIO(msg);

        dummy.transform.LookAt(aim);
        // Vector3 direction = aim - dummy.transform.position;
        // dummy.transform.rotation = Quaternion.LookRotation(direction);

        var loc = new Loc {
            timestamp = (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000,
            x = curPos.x.ToString("F3"),
            y = curPos.y.ToString("F3"),
            z = curPos.z.ToString("F3")
        };
        string locStr = JsonUtility.ToJson(loc);

        Loc loc2 = JsonUtility.FromJson<Loc>(locStr);
        LocLog("当前坐标(camera)：" + " " + loc2.x + ", " + loc2.y + ", " + loc2.z);

        SendVIO(locStr);
    }
}
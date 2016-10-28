using UnityEngine;
using System.Collections;

public class UILoginPopupController : MonoBehaviour {

    public UILabel stringMessage = null;

    public GameObject popupPanelObj = null;
    public GameObject loginPanelObj = null;

    private UILoginController component;

    // Use this for initialization
    void Start()
    {
        try
        {
            loginPanelObj = GameObject.Find("LoginObject");
            component = loginPanelObj.GetComponent<UILoginController>();
        }
        catch (System.Exception e)
        {
            Debug.Log(e.Message);  // 예외의 메시지를 출력
            Debug.Log(e.StackTrace);
        }
    
    }
   
    // Update is called once per frame
    void Update()
    {
        if (popupPanelObj.activeSelf == true)
        {
            stringMessage.text = component.getAlertMessage();
        }
    }

    public void OnClick_OKButton()
    {
        if (stringMessage.text.Equals("로그인 성공"))
        {
            PK_NetMgr.Ins.Connect();
        }
        else
        {
            popupPanelObj.SetActive(false);
            loginPanelObj.SetActive(true);
        }
    }
}

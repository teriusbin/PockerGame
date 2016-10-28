using UnityEngine;
using System.Collections;

public class UIJoinPopupController : MonoBehaviour
{

    public UILabel stringMessage = null;

    public GameObject popupPanelObj = null;
    public GameObject joinPanelObj = null;

    private UIJoinController component;

    // Use this for initialization
    void Start()
    {
        try
        {
            joinPanelObj = GameObject.Find("LoginObject");
            component = joinPanelObj.GetComponent<UIJoinController>();
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
        popupPanelObj.SetActive(false);
        joinPanelObj.SetActive(true);

    }
}

using UnityEngine;
using System.Collections;
using System.Text;
using SimpleJSON;
//using Newtonsoft.Json;
//using Newtonsoft.Json.Linq;

public class UIJoinController : MonoBehaviour
{
    public UIInput idInput = null;
    public UIInput pwdInput = null;
    public UIInput confirmPwdInput = null;
    public UIInput email = null;
    public UIInput nickName = null;

    public GameObject loginPanelObj = null;
    public GameObject joinPanelObj = null;
    public GameObject popupPanelObj = null;

    private bool idConfirmFlag = false;
    private string alertMessage = null;

    // Use this for initialization
    void Start()
    {
        idInput.value = null;
        pwdInput.value = null;
        confirmPwdInput.value = null;
        email.value = null;
        nickName.value = null;
    }

    // Update is called once per frame
    void Update()
    {
        HttpManager.Ins.Update();
    }
   
    public void OnClick_JoinButton()
    {
        
        if(idInput.value == "" || 
           pwdInput.value == "" || 
           confirmPwdInput.value == "" ||
           email.value == "" || 
           nickName.value == "")
        {
            setAlertMessage("모든 사항을 기입하세요.");
            popupAlertWindow();
        }
        else
        {
            if (pwdInput.value == confirmPwdInput.value )
            {
                if(idConfirmFlag == false)
                {
                    setAlertMessage("아이디 중복확인 하세요.");
                    popupAlertWindow();
                }
                else
                {
                    StringBuilder dataParams = new StringBuilder();
                    dataParams.Append("id=" + idInput.value);
                    dataParams.Append("&pwd=" + pwdInput.value);
                    dataParams.Append("&email=" + email.value);
                    dataParams.Append("&nickName=" + nickName.value);

                    string reqURL = string.Format("http://{0}", "172.30.154.7/poker/user_join.php");
                    HttpManager.Ins.SendRequest(reqURL,
                        dataParams.ToString(), "none", 10.0f, OnResponse, OnRequestTimeout);
                }
            }
            else
            {
                setAlertMessage("패스워드가 일치하지 않습니다.");
                popupAlertWindow();
            }

        }
    }

    public void OnClick_ConfirmButton()
    {
        StringBuilder dataParams = new StringBuilder();
        dataParams.Append("id=" + idInput.value);
      
        string reqURL = string.Format("http://{0}", "172.30.154.7/poker/id_confirm.php");
        HttpManager.Ins.SendRequest(reqURL,
            dataParams.ToString(), "none", 10.0f, OnResponse, OnRequestTimeout);
    }

    public void OnClick_BackButton()
    {
        joinPanelObj.SetActive(false);
        loginPanelObj.SetActive(true);
    }

    public void OnClick_CloseButton()
    {

    }
    public string getAlertMessage()
    {
        return alertMessage;
    }

    public void setAlertMessage(string inputMsg)
    {
        alertMessage = inputMsg;
    }

    public void popupAlertWindow()
    {
        popupPanelObj.SetActive(true);
    }

    //콜백 함수
    void OnResponse(int task_id, string param, int code, string response)
    {
        
        var result = JSON.Parse(response);
       
        if (result["rsponseMsg"].Value.Equals("confirm_ok"))
        {
            setAlertMessage("사용할 수 있는 아이디");
            idConfirmFlag = true;
            popupAlertWindow();
        }
        else if (result["rsponseMsg"].Value.Equals("confirm_no"))
        {
            setAlertMessage("이미 존재하는 아이디");
            idConfirmFlag = false;
            popupAlertWindow();
        }
        else if (result["rsponseMsg"].Value.Equals("join_succes"))
        {
            setAlertMessage("회원가입 성공");
          
            idInput.value = null;
            pwdInput.value = null;
            confirmPwdInput.value = null;
            email.value = null;
            nickName.value = null;

            joinPanelObj.SetActive(false);
            loginPanelObj.SetActive(true);
        }
        else if (result["rsponseMsg"].Value.Equals("join_fail"))
        {
            setAlertMessage("회원가입 실패");
            popupAlertWindow();
        }
    }

    void OnRequestTimeout(int task_id, string param)
    {
        Debug.Log("OnRequestTimeout Start");
        Debug.Log("task_id : " + task_id + ", param : " + param);
        Debug.Log("OnRequestTimeout End");
    }

 
}
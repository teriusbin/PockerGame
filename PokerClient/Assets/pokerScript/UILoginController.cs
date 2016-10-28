using UnityEngine;
using System.Collections;
using System.Text;
using SimpleJSON;


public class UILoginController : MonoBehaviour {

    public UIInput idInput = null; 
    public UIInput pwdInput = null;
   
    public GameObject loginPanelObj = null;
    public GameObject joinPanelObj = null;
    public GameObject popupPanelObj = null;

    private string alertMessage = null;

    string sucssesUserId = null;
    // Use this for initialization
    void Start () {
        idInput.value = null;
        pwdInput.value = null;
    }
	
	// Update is called once per frame
	void Update () {
        HttpManager.Ins.Update();
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

    public void OnClick_LoginButton()
    {
       
        if(idInput.value == "" || pwdInput.value == "")
        {
            setAlertMessage("모든 사항을 기입하세요");
            popupAlertWindow();
        }
        else
        {
            StringBuilder dataParams = new StringBuilder();

            dataParams.Append("id=" + idInput.value);
            dataParams.Append("&pwd=" + pwdInput.value);

            sucssesUserId = idInput.value;

            string reqURL = string.Format("http://{0}", "172.30.154.7/poker/user_login.php");
            HttpManager.Ins.SendRequest(reqURL,
                dataParams.ToString(), "none", 10.0f, OnResponse, OnRequestTimeout);
        }
  
    }

    public void OnClick_JoinButton()
    {
        idInput.value = null;
        pwdInput.value = null;

        loginPanelObj.SetActive(false);
        joinPanelObj.SetActive(true);
    }

    void OnResponse(int task_id, string param, int code, string response)
    {
        var result = JSON.Parse(response);
     
        if (result["rsponseMsg"].Value.Equals("login_succses"))
        {
            setAlertMessage("로그인 성공");
            Debug.Log("내가 접속한 아이디는 " + sucssesUserId);
            PK_NetMgr.Ins.userId = sucssesUserId;
            PK_NetMgr.Ins.userHasMoney = result["money"].Value;
            PK_NetMgr.Ins.userWin = int.Parse(result["win"].Value);
            PK_NetMgr.Ins.userLose = int.Parse(result["lose"].Value);

            sucssesUserId = null;
            popupAlertWindow();
        }
        else if (result["rsponseMsg"].Value.Equals("pwd_notMatch"))
        {
            setAlertMessage("잘못된 패스워드");
            popupAlertWindow();
        }
        else if (result["rsponseMsg"].Value.Equals("noexist"))
        {
            setAlertMessage("잘못된 아이디");
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

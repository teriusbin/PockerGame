using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class UIRoomMakePopupController : MonoBehaviour {

    public UIInput roomNameinput = null;

    public GameObject popupPanelObj = null;
    public GameObject lobbyPanelObj = null;

    private UILobbyController component;
    private string bettingMoney = null;

    // Use this for initialization
    void Start () {
        try
        {
            lobbyPanelObj = GameObject.Find("LobbyObject");
            component = lobbyPanelObj.GetComponent<UILobbyController>();
        }
        catch (System.Exception e)
        {
            Debug.Log(e.Message);  // 예외의 메시지를 출력
            Debug.Log(e.StackTrace);
        }
     
    }
	
	// Update is called once per frame
	void Update () {
	

	}

    public void OnCheck_BettingMoney()
    {
        string temp = UIButton.current.transform.GetChild(2).GetComponent<UILabel>().text;

        if (temp.Equals(" 10만")) this.bettingMoney = "100000";
        else if(temp.Equals(" 100만")) this.bettingMoney = "1000000";
        else if (temp.Equals(" 1000만")) this.bettingMoney = "10000000";
        else if (temp.Equals(" 5000만")) this.bettingMoney = "50000000";
        else if (temp.Equals(" 1억")) this.bettingMoney = "100000000";
        else if (temp.Equals(" 10억")) this.bettingMoney = "1000000000";
        
           // UILabel noLabel = createItemObj.transform.FindChild("Label - No").GetComponent<UILabel>();
    }

    public void OnClick_OKButton()
    {

        string roomName = roomNameinput.value.ToString(); 

        if (!roomName.Equals("") && !this.bettingMoney.Equals("") && Int64.Parse(PK_NetMgr.Ins.userHasMoney) > Int64.Parse(this.bettingMoney))
        {
            component.setRoomInfo(roomName, this.bettingMoney);

            roomNameinput.value = null;

            popupPanelObj.SetActive(false);
            lobbyPanelObj.SetActive(true);
        }
        else
        {
            popupPanelObj.SetActive(false);
        }
        
    }
}

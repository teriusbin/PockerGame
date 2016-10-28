using UnityEngine;
using System;
using System.Collections;
using FreeNet;
using FreeNetUnity;
using VirusWarGameServer;
//using Newtonsoft.Json;
public class CNetworkManager : MonoBehaviour {

	CFreeNetUnityService gameserver;
	string received_msg;

	void Awake()
	{
		this.received_msg = "";

		// ��Ʈ��ũ ����� ���� CFreeNetUnityService��ü�� �߰��մϴ�.
		this.gameserver = gameObject.AddComponent<CFreeNetUnityService>();

		// ���� ��ȭ(����, �����)�� �뺸 ���� ��������Ʈ ����.
		this.gameserver.appcallback_on_status_changed += on_status_changed;

		// ��Ŷ ���� ��������Ʈ ����.
		this.gameserver.appcallback_on_message += on_message;
	}

	// Use this for initialization
	void Start()
	{
		connect();
	}

	void connect()
	{
		this.gameserver.connect("172.30.154.7", 7979);
	}

	/// <summary>
	/// ��Ʈ��ũ ���� ����� ȣ��� �ݹ� �żҵ�.
	/// </summary>
	/// <param name="server_token"></param>
	void on_status_changed(NETWORK_EVENT status)
	{
		switch (status)
		{
				// ���� ����.
			case NETWORK_EVENT.connected:
				{
					CLogManager.log("on connected");
					this.received_msg += "on connected\n";

					//CPacket msg = CPacket.create((short)PROTOCOL.CHAT_MSG_REQ);
					//msg.push("Hello!!!");
					//this.gameserver.send(msg);
				}
				break;

				// ���� ����.
			case NETWORK_EVENT.disconnected:
				CLogManager.log("disconnected");
				this.received_msg += "disconnected\n";
				break;
		}
	}

	void on_message(CPacket msg)
	{
		//// ���� ���� �������� ���̵� �����´�.
		//PROTOCOL protocol_id = (PROTOCOL)msg.pop_protocol_id();

		//// �������ݿ� ���� �б� ó��.
		//switch (protocol_id)
		//{
		//	//case PROTOCOL.CHAT_MSG_ACK:
		//	//	{
		//	//		string text = msg.pop_string();
		//	//		GameObject.Find("GameMain").GetComponent<CGameMain>().on_receive_chat_msg(text);
		//	//	}
		//	//	break;
		//}
	}

	public void send(CPacket msg)
	{
		this.gameserver.send(msg);
	}
}

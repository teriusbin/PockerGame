using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.IO;


public class HttpSession
{
    public int id = 0;
    private const int BUFFERSIZE = 4096;
    private byte[] readBuffer = new byte[4096];
    //private HttpManager m_HttpMgr;
    private byte[] requestData;
    private HttpWebRequest httpRequest;
    private HttpManager.OnResponseDelegate onResponse;
    private HttpManager.OnRequestTimeoutDelegate onTimeout;
    public StringBuilder responseData;
    public HttpWebResponse response;
    public float fTimeout;
    public bool bCompleted = false;
    public string param = "none";
	public string contentType = "application/Json";

	public HttpSession(int id, string method, string url, string requestData, 
	                   string param, float fTimeout, string contentType = "application/Json", 
        				HttpManager.OnResponseDelegate onResponse = null, HttpManager.OnRequestTimeoutDelegate onTimeout = null)
    {
        this.id = id;
        this.httpRequest = (HttpWebRequest)WebRequest.Create(url);
        this.httpRequest.Method = method;
        this.requestData = Encoding.UTF8.GetBytes(requestData);
        this.bCompleted = false;
        this.param = param;
        this.fTimeout = fTimeout;
        this.onResponse = onResponse;
        this.onTimeout = onTimeout;
        this.httpRequest.KeepAlive = false;
        this.responseData = new StringBuilder(string.Empty);
        this.response = null;
		this.contentType = contentType;
    }

    public void Start()
    {
        if (this.requestData != null && this.requestData.Length > 0)
        {
            this.httpRequest.ContentLength = (long)this.requestData.Length;
            this.httpRequest.ContentType = this.contentType;
            this.httpRequest.BeginGetRequestStream(new AsyncCallback(this.ConnectedCallback), null);
        }
        else
        {
            this.httpRequest.BeginGetResponse(new AsyncCallback(this.RespCallback), null);
        }
    }

    public void Stop()
    {
        if (this.httpRequest != null)
            this.httpRequest.Abort();
        if (this.response != null)
            this.response.Close();
        this.httpRequest = (HttpWebRequest)null;
        this.response = (HttpWebResponse)null;
    }

    public void Update(float deltaTime)
    {
        if (this.bCompleted || (double)this.fTimeout <= 0.0)
            return;

        this.fTimeout -= deltaTime;
        if ((double)this.fTimeout > 0.0)
            return;

        this.bCompleted = true;
    }

    private void ConnectedCallback(IAsyncResult ar)
    {
        Stream requestStream = this.httpRequest.EndGetRequestStream(ar);
        requestStream.Write(this.requestData, 0, this.requestData.Length);
        requestStream.Flush();
        requestStream.Close();

        this.httpRequest.BeginGetResponse(new AsyncCallback(this.RespCallback), null);
    }

    private void RespCallback(IAsyncResult ar)
    {
        this.response = (HttpWebResponse)this.httpRequest.EndGetResponse(ar);
        Debug.Log(("RespCallback - " + this.response.StatusCode));

        this.response.GetResponseStream().BeginRead(
			this.readBuffer, 0, 2048, new AsyncCallback(this.ReadCallBack), null);
    }

    private void ReadCallBack(IAsyncResult ar)
    {
        Debug.Log(("ReadCallBack - " + this.response.StatusCode));
        int count = this.response.GetResponseStream().EndRead(ar);
        if (count > 0)
        {
            this.responseData.Append(Encoding.UTF8.GetString(this.readBuffer, 0, count));
            this.response.GetResponseStream().BeginRead(
				this.readBuffer, 0, 2048, new AsyncCallback(this.ReadCallBack), null);
        }
        else
        {
            this.response.GetResponseStream().Close();
            this.bCompleted = true;
        }
    }

    public void Callback()
    {
        if (this.response != null)
        {
            if (this.response.StatusCode == HttpStatusCode.OK)
            {
                if (this.onResponse == null)
                    return;
                this.onResponse(this.id, this.param, 0, this.responseData.ToString());
            }
            else
            {
                if (this.onResponse == null)
                    return;
                this.onResponse(this.id, this.param, (int)this.response.StatusCode, this.responseData.ToString());
            }
        }
        else
        {
            if (this.onTimeout == null)
                return;
            this.onTimeout(this.id, this.param);
        }
    }
}

public class HttpManager
{
    private static HttpManager instance = null;

    public static HttpManager Ins
    {
        get
        {
            if (instance == null)
                instance = new HttpManager();

            return instance;
        }
    }

    public delegate void OnResponseDelegate(int task_id, string param, int code, string response);

    public delegate void OnRequestTimeoutDelegate(int task_id, string param);

    private List<HttpSession> sessionList = new List<HttpSession>();

    private int m_iCounter = 0;
    public int SendRequest(string url, string request, string param, float timeout, 
        HttpManager.OnResponseDelegate onResponse, HttpManager.OnRequestTimeoutDelegate onTimeout)
    {
        int id = this.GenerateId();
        Debug.Log("Url:" + url);
        Debug.Log("Request:" + request);
        HttpSession session = 
			new HttpSession(id, "POST", url, request, param, timeout,
                            "application/x-www-form-urlencoded; charset=UTF-8", onResponse, onTimeout);
        session.Start();
        this.sessionList.Add(session);
        return id;
    }
 
    public void CancelRequest(int id)
    {
        for (int i = 0; i < sessionList.Count; ++i)
        {
            if (sessionList[i].id == id)
            {
                sessionList[i].Stop();
                sessionList.Remove(sessionList[i]);
                break;
            }
        }
    }

    private int GenerateId()
    {
        int returnCounter = this.m_iCounter;
        ++this.m_iCounter;
        return returnCounter;
    }

    public void Update()
    {
        if (this.sessionList.Count <= 0)
            return;

        HttpSession httpSession = sessionList[0];
        httpSession.Update(Time.deltaTime);
        if (httpSession.bCompleted)
        {
            httpSession.Callback();
            httpSession.Stop();
            sessionList.RemoveAt(0);
        }
    }

    public void Clear()
    {
        for (int i = 0; i < sessionList.Count; ++i)
        {
            sessionList[i].Stop();
            sessionList[i].Callback();
        }

        sessionList.Clear();
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MessageBoxManager : MonoBehaviour
{
    public static MessageBoxManager Instance;

    public class MessageBoxQueue
    {
        public string Message;
        public MessageBoxCallBack OKCallback, CancelCallback;
        public MessageBoxQueue()
        {

        }
        public MessageBoxQueue(string Message, MessageBoxCallBack OKCallback = null, MessageBoxCallBack CancelCallback = null)
        {
            this.Message = Message;
            this.OKCallback = OKCallback;
            this.CancelCallback = CancelCallback;
        }
    }

    public Text Message;
    public GameObject Canvas;

    private List<MessageBoxQueue> Queue = new List<MessageBoxQueue>();
    private MessageBoxCallBack NextOKCall = null, NextCancelCall = null;

    public delegate void MessageBoxCallBack();

    private void Start()
    {
        Instance = this;
    }
    public void ShowMessage(string Message, MessageBoxCallBack OKCallback = null, MessageBoxCallBack CancelCallback = null)
    {
        Queue.Add(new MessageBoxQueue(Message, OKCallback, CancelCallback));
    }
    public void OK()
    {
        if (NextOKCall != null) NextOKCall();
        Canvas.SetActive(false);
    }
    public void Cancel()
    {
        if (NextCancelCall != null) NextCancelCall();
        Canvas.SetActive(false);
    }
    private void Update()
    {
        if (Canvas.activeInHierarchy == true) return;
        if (Queue.Count == 0) return;
        NextOKCall = Queue[0].OKCallback;
        NextCancelCall = Queue[0].CancelCallback;
        Message.text = Queue[0].Message;
        Queue.RemoveAt(0);
        Canvas.SetActive(true);
    }
}

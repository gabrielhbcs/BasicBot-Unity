using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using UnityEngine;

public class BasicBot : MonoBehaviour {
    TcpClient tcpClient;
    StreamReader reader;
    StreamWriter writer;

    string username, password, channelName, prefixForSendingChatMessages;
    DateTime lastMessageSendTime;

    Queue<string> sendMessageQueue;

    public void Start()
    {
        sendMessageQueue = new Queue<string>();
        this.username = "gabrielhbcs".ToLower();
        this.channelName = username;
        this.password = "";
        prefixForSendingChatMessages = String.Format(":{0}!{0}@{0}.tmi.twitch.tv PRIVMSG #{1} :", username, channelName);

        Reconnect();
    }

    void Reconnect()
    {
        tcpClient = new TcpClient("irc.twitch.tv", 6667);
        reader = new StreamReader(tcpClient.GetStream());
        writer = new StreamWriter(tcpClient.GetStream());
        writer.AutoFlush = true;

        writer.WriteLine(String.Format("PASS {0}\r\nNick {1}\r\nUser {1} 8 * :{1}", password, username));
        writer.WriteLine("JOIN #" + channelName);
        lastMessageSendTime = DateTime.Now;

    }


    public void SendTwitchMessage(string message)
    {
        sendMessageQueue.Enqueue(message);
    }

    void Update()
    {
        if (!tcpClient.Connected)
        {
            Reconnect();
        }

        TryReceiveMessage();
        TrySendingMessage();
    }

    void TryReceiveMessage()
    {
        if(tcpClient.Available > 0)
        {
            var message = reader.ReadLine();
            //print(String.Format("\r\nMensagem: {0}", message));

            var iCollon = message.IndexOf(":", 1);
            if(iCollon > 0)
            {
                var command = message.Substring(1, iCollon);
                if (command.Contains("PRIVMSG #"))
                {
                    var iBang = command.IndexOf("!");
                    if(iBang > 0)
                    {
                        var speaker = command.Substring(0, iBang);
                        var chatMessage = message.Substring(iCollon + 1);

                        ReceiveMessage(speaker, chatMessage);
                    }
                }
            }
        }
    }

    void ReceiveMessage(string speaker, string message)
    {
        print(String.Format("\r\n{0}: {1}", speaker, message));

        //comandos entram aqui ~
        if (message.StartsWith("!hi"))
        {
            print("teste");
            SendTwitchMessage(String.Format("Hello {0}!", speaker));
        }
    }

    void TrySendingMessage()
    {
        if (DateTime.Now - lastMessageSendTime > TimeSpan.FromSeconds(2))
        {
            if (sendMessageQueue.Count > 0)
            {
                var message = sendMessageQueue.Dequeue();
                writer.WriteLine(String.Format("{0}{1}", prefixForSendingChatMessages, message));
                lastMessageSendTime = DateTime.Now;
            }
        }
    }








	

}

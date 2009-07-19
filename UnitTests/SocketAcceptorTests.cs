﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using QuickFIX.NET.Transport;
using QuickFIX.NET;
using QuickFIX.NET.Config;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Diagnostics;
using QuickFIX.NET.Applications;

namespace UnitTests
{
    [TestFixture]
    public class SocketAcceptorTests
    {
        [Test]
        public void TestAcceptor()
        {
            Application acceptorApp = new FIX4Application();
            Settings acceptorSettings = new Settings();
            acceptorSettings.SocketAcceptPort = 54123;
            acceptorSettings.SocketAcceptHost = "127.0.0.1";
            SocketAcceptor acceptor = new SocketAcceptor(acceptorApp, acceptorSettings);
            acceptor.DataReceivedFromClient += new SocketAcceptor.DataReceivedFromClientHandler(acceptor_DataReceivedFromClient);
            acceptor.Start();
            Assert.That(acceptor.NumberOfClientsConnected, Is.EqualTo(0));

            TcpClient client = new TcpClient();
            client.Connect("127.0.0.1", 54123);
            Thread.Sleep(100);
            Assert.That(client.Connected, Is.True);
            Assert.That(acceptor.NumberOfClientsConnected, Is.EqualTo(1));

            Stream strm = client.GetStream();

            const string testData = "8=FIX.4.2\x01" + "9=46\x01" + "35=0\x01" + "34=3\x01" + "49=TW\x01" +
    "52=20000426-12:05:06\x01" + "56=ISLD\x01" + "1=acct123\x01" + "10=000\x01";

            ASCIIEncoding asen = new ASCIIEncoding();
            byte[] ba = asen.GetBytes(testData + "\n");

            strm.Write(ba, 0, ba.Length);
            Thread.Sleep(200);

            Assert.That(_lastReceivedData.Length, Is.EqualTo(testData.Length));
            Assert.That(_lastReceivedData.Substring(0, testData.Length), Is.EqualTo(testData));
            acceptor.ForceShutdown();
            Thread.Sleep(1000);
            Assert.That(acceptor.NumberOfClientsConnected, Is.EqualTo(0));
        }

        private string _lastReceivedData = string.Empty;

        void acceptor_DataReceivedFromClient(object sender, string data)
        {
            _lastReceivedData = data;
        }
    }
}
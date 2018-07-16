﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Xml;
using System.Xml.Serialization;

namespace NewsCrawler
{
    public partial class Service1 : ServiceBase
    {
        XmlDocument rssXmlDoc;
        XmlSerializer serializer;
        List<NewsItem> NewsList;
        System.Timers.Timer servicetimer;
        string IOPath,logfile,urlOne,urlTwo,outputxmlpath;

        public Service1()
        {
            InitializeComponent();
            //Service will call workfunction after 5 Minutes.   
            servicetimer = new System.Timers.Timer(1000 * 60 * 5);
            rssXmlDoc = new XmlDocument();
            serializer = new XmlSerializer(typeof(NewsItem));
            NewsList = new List<NewsItem>();

            
        }
        public void writetoLog(string msg)
        {
            using (StreamWriter sw = File.AppendText(logfile))
            {
                sw.WriteLine(msg + " " + System.DateTime.Now);
            }
        }
        protected override void OnStart(string[] args)
        {
            //Intializing Variables

            servicetimer.Elapsed += new System.Timers.ElapsedEventHandler(WorkerFunction);
            servicetimer.Enabled = true;


            IOPath = ConfigurationManager.AppSettings["IOPath"];
            urlOne = ConfigurationManager.AppSettings["urlOne"];
            urlTwo = ConfigurationManager.AppSettings["urlTwo"];

            logfile = Path.Combine(IOPath ,"Q2-NewsCrawler.txt");
            outputxmlpath = Path.Combine(IOPath, "News.xml");

            writetoLog("\nService Started!");
            writetoLog("IO Path:" + IOPath + " ");
            writetoLog("Output File Path:" + outputxmlpath + " ");
        }

        private void WorkerFunction(object sender, ElapsedEventArgs e)
        {
            CrawlandStore(urlOne);
            CrawlandStore(urlTwo);
            writetoLog("Sorting News Items");
            NewsList.Sort();
            WriteNewsToXml();

        }

        private void WriteNewsToXml()
        {
            writetoLog("Wrting To Xml");
            XmlSerializer serializer = new XmlSerializer(NewsList.GetType());
            StreamWriter writer = new StreamWriter(outputxmlpath);
            serializer.Serialize(writer.BaseStream, NewsList);
            NewsList.Clear();
            writer.Close();
        }

        private void CrawlandStore(string url)
        {
            
            string xmlStr;
            using (var wc = new WebClient())
            {
                xmlStr = wc.DownloadString(url);
            }
            var xmlDoc = new XmlDocument();
            writetoLog("Crawling News from: " + url);
            rssXmlDoc.Load(url);
            XmlNodeList channel = (rssXmlDoc.GetElementsByTagName("title"));
            XmlNodeList ItemList = rssXmlDoc.GetElementsByTagName("item");
            for (int i = 0; i < ItemList.Count; i++)
            {
                NewsItem temp=((NewsItem)serializer.Deserialize(new XmlNodeReader(ItemList[i]))).FurnishNews();
                temp.setChannel(channel[0].InnerText);
                NewsList.Add(temp);

            }
        }

        protected override void OnStop()
        {
            writetoLog("\nService Stopped!");
        }
    }
}

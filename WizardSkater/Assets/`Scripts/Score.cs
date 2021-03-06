﻿using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.IO;
using System.Linq;
using JsonFx.Json;

public class Score : MonoBehaviour
{
    public float m_scoreAmtTrick = 50f;
    public float m_scoreMultiplierTime;

    public float m_levelScore;

    public List<float> m_levelTopScores;

    private JsonReader jsonReader;
    private JsonWriter jsonWriter;
    private System.Text.StringBuilder jsonOutput;

    public string m_fileName = "LevelScores.json";
    public string m_filePath;

    void Awake()
    {
        jsonOutput = new System.Text.StringBuilder();

        JsonReaderSettings readSettings = new JsonReaderSettings();
        readSettings.AddTypeConverter(new RecordConverter());
        jsonReader = new JsonReader(jsonOutput, readSettings);

        JsonWriterSettings writeSettings = new JsonWriterSettings();
        writeSettings.PrettyPrint = true;
        writeSettings.AddTypeConverter(new RecordConverter());
        jsonWriter = new JsonWriter(jsonOutput, writeSettings);

        m_filePath = Application.dataPath;
        if (Application.platform == RuntimePlatform.OSXDashboardPlayer || Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.OSXPlayer || Application.platform == RuntimePlatform.OSXWebPlayer)
        {
            m_filePath += "/Score/";
        }
        else
        {
            m_filePath += "\\Score\\";
        }

        m_levelScore = 0f;
    }

    public void Init()
    {
        var levels = 4;

        if (!File.Exists(m_filePath + m_fileName))
        {
            Hashtable hash = new Hashtable();
            string data = JsonWriter.Serialize(hash);
            StreamWriter sw = new StreamWriter(m_filePath + m_fileName);
            sw.Write(data);
            sw.Close();
        }
        else
        {
            StreamReader stream = new StreamReader(m_filePath + m_fileName);
            var str = stream.ReadToEnd();

            Dictionary<string, object> scores;
            scores = JsonReader.Deserialize(str, typeof(Dictionary<string, object>)) as Dictionary<string, object>;
            int[] scoreThings = scores.Values.First() as int[];
            List<float> levelScores = new List<float>();
            foreach (object f in scoreThings)
            {
                string sf = f.ToString();
                float newF = float.Parse(sf);
                levelScores.Add(newF);
            }

            for (int i = 0; i < levels; i++)
            {
                if (levelScores[i] != -1f)
                    m_levelTopScores.Add(levelScores[i]);
                else
                    m_levelTopScores.Add(-1f);
            }
        }
    }

    void LateUpdate()
    {
        if (m_levelScore < 0)
            m_levelScore = 0;
    }

    public IEnumerator ScoreTrick()
    {
        m_levelScore += m_scoreAmtTrick;
        yield return new WaitForEndOfFrame();
    }

    public IEnumerator FailTrick()
    {
        m_levelScore -= m_scoreAmtTrick/2;
        yield return new WaitForEndOfFrame();
    }

    public void EvaluateLevelHighScore(int level)
    {
        if (level != -1)
        {
            float time = SystemsManager.m_Timer.GetTime();

            int TimeScoreMultiplied = (int) (555 - (0.4f*(time*time)));
            if (TimeScoreMultiplied < 0)
                TimeScoreMultiplied = 0;

            m_levelScore += TimeScoreMultiplied;

            Debug.Log("hudScore is " + m_levelScore + ", top score is " + m_levelTopScores[level]);

            if (m_levelTopScores[level] == -1f || m_levelScore > m_levelTopScores[level])
            {
                //new record
                Debug.Log("Saving new record...");
                m_levelTopScores[level] = m_levelScore;
                SaveNewRecord(new Record(m_levelTopScores));
            }
        }
    }

    public void SaveNewRecord(Record record)
    {
        if (!Directory.Exists(m_filePath))
        {
            Debug.Log("can't find directory, creating...");
            Directory.CreateDirectory(m_filePath);
        }
        else
        {
            Debug.Log("found directory, all good for file");
        }
        if (!File.Exists(m_filePath + m_fileName))
        {
            Debug.Log("saving file... creating file");
            Hashtable hash = new Hashtable();
            hash.Add("LevelScores", record.LevelScores);
            string data = JsonWriter.Serialize(hash);
            StreamWriter sw = new StreamWriter(m_filePath + m_fileName);
            Debug.Log("saving out " + data);
            sw.Write(data);
            sw.Close();
        }
        else
        {
            Debug.Log("saving file... found file");
            Hashtable hash = new Hashtable();
            hash.Add("LevelScores", record.LevelScores);
            string data = JsonWriter.Serialize(hash);
            StreamWriter sw = new StreamWriter(m_filePath + m_fileName);
            Debug.Log("saving out " + data);
            sw.Write(data);
            sw.Close();
        }
    }
}

public class Record
{
    public List<float> LevelScores;

    public Record(List<float> levelScores = null)
    {
        LevelScores = levelScores;
    }
}

public class RecordConverter : JsonConverter
{
    public override bool CanConvert(Type t)
    {
        return t == typeof(Record);
    }

    public override Dictionary<string, object> WriteJson(Type type, object value)
    {
        Record val = (Record)value;
        Dictionary<string, object> dict = new Dictionary<string, object>();
        dict.Add("LevelScores", val.LevelScores);
        return dict;
    }

    public override object ReadJson(Type type, Dictionary<string, object> value)
    {
        Record rec = new Record();
        return rec;
    }
}
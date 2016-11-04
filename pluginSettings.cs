
using UnityEngine;
using System;

namespace img_viewer
{
    public class ImgVwrSettings
    {
        ConfigNode configFile;
        ConfigNode configFileNode;

        public void Create()
        {
            if (configFileNode == null)
            {
                configFile = new ConfigNode();
            }
            if (!configFile.HasNode("IMAGEVIEWER"))
            {
                configFileNode = new ConfigNode("IMAGEVIEWER");
                configFile.SetNode("IMAGEVIEWER", configFileNode, true);
            }
            else
            {
                if (configFileNode == null)
                {
                    configFileNode = configFile.GetNode("IMAGEVIEWER");

                }
            }
        }

        public bool Load()
        {
            configFile = ConfigNode.Load(KSPUtil.ApplicationRootPath + "GameData/img_viewer/PluginData/img_viewer.cfg");
            if (configFile != null)
                configFileNode = configFile.GetNode("IMAGEVIEWER");
            return configFile != null;
        }

        public void Save()
        {
            configFile.Save(KSPUtil.ApplicationRootPath + "GameData/img_viewer/PluginData/img_viewer.cfg");
        }

        public void SetValue(string name, string value)
        {            
            configFileNode.SetValue(name, value, true);
        }

        public void SetValue(string name, Rect rect)
        {
            SetValue(name + ".x", (int)rect.x);
            SetValue(name + ".y", (int)rect.y);
            SetValue(name + ".width", (int)rect.width);
            SetValue(name + ".height", (int)rect.height);
        }
        public void SetValue(string name, int i)
        {
            SetValue(name, i.ToString());
        }
        public void SetValue(string name, bool b)
        {
            SetValue(name, b.ToString());
        }

        public string GetValue(string name, string d = "")
        {
            string s = "";
            if (name == null)
                return "";

            if (configFileNode.HasValue(name))
                s = configFileNode.GetValue(name);

            return s;
        }
        public Rect GetValue(string name, Rect rect)
        {
            try
            {
                rect.x = Convert.ToSingle(GetValue(name + ".x", rect.x.ToString()));
                rect.y = Convert.ToSingle(GetValue(name + ".y", rect.y.ToString()));
                rect.width = Convert.ToSingle(GetValue(name + ".width", rect.width.ToString()));
                rect.height = Convert.ToSingle(GetValue(name + ".height", rect.height.ToString()));
            }
            catch (Exception e)
            {
                Debug.Log("Exception converting: " + name + " -   " + e.Message);
            }
            return rect;
        }
        public int GetValue(string name, int i = 0)
        {
            int r;
            try
            {
                r = Convert.ToInt32(GetValue(name, i.ToString()));
            }
            catch (Exception e)
            {
                r = i;
                Debug.Log("Exception converting: " + name + " -   " + e.Message);
            }
            return r;
        }
        public bool GetValue(string name, bool b = false)
        {
            bool r;
            try {
                
                r = Convert.ToBoolean(GetValue(name, b.ToString()));
            }
            catch (Exception e)
            {
                r = b;
                Debug.Log("Exception converting: " + name + " -   " + e.Message);
            }
            return r;
        }
    }
}
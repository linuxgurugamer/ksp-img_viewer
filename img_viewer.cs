// -------------------------------------------------------------------------------------------------
// img_viewer.cs 0.3
//
// Simple KSP plugin to view images ingame.
// Copyright (C) 2015 Iván Atienza
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see http://www.gnu.org/licenses/.
//
// Email: mecagoenbush at gmail dot com
// Freenode: hashashin
//
// -------------------------------------------------------------------------------------------------

using KSP.IO;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using File = KSP.IO.File;

namespace img_viewer
{
    [KSPAddon(KSPAddon.Startup.EveryScene, true)]
    public class ImgViewer : MonoBehaviour
    {
        private Rect _windowRect;
        private Rect _windowRect2 = new Rect(Screen.width / 2 - 150f, Screen.height / 2 - 75f, 260f, 390f);

        private string _keybind = "i";

        private bool _visible = false;

        private IButton _button;
        private const string _tooltip = "Image Viewer Menu";
        private const string _btextureOn = "ImageViewer/Textures/icon_on";
        private const string _btextureOff = "ImageViewer/Textures/icon_off";

        //private string _version;
        private string _versionlastrun;
        private Texture2D _image;

        private WWW _imagetex;
        private string _imagefile;


        public ImgVwrSettings ImgVwrSettings = new ImgVwrSettings();
        private readonly string kspPluginDataFldr =
            KSPUtil.ApplicationRootPath + "GameData/ImageViewer/PluginData/";
        private static  string _imagedir = KSPUtil.ApplicationRootPath.Replace("\\", "/") +
                                   "/GameData/ImageViewer/PluginData/Images/";
        private readonly string _imageurl = "file://" + _imagedir;

        private List<string> _imageList;
        private Vector2 _scrollViewVector = Vector2.zero;
        private int _selectionGridInt = 0;
        private bool _showList;
        private bool _useKSPskin;
        private int _lastimg = -1;

        private void Awake()
        {
            //LoadVersion();
            //VersionCheck();
            LoadSettings();
            GameEvents.onGameSceneLoadRequested.Add(onSceneChange);

        }
        public void onSceneChange(GameScenes scene)
        {
            //if (_showList && !keepShowlist)
            {
                _showList = false;
            }
        }

        private void Start()
        {
            //populate the list of images
            if (_imageList == null)
            {
                GetImages();
            }
            DontDestroyOnLoad(this);
            // toolbar stuff
            if (!ToolbarManager.ToolbarAvailable) return;
            _button = ToolbarManager.Instance.add("ImageViewer", "toggle");
            _button.TexturePath = _btextureOff;
            _button.ToolTip = _tooltip;
            _button.OnClick += (e => TogglePopupMenu(_button));
           
        }
        bool resetSize = false;
        private void OnGUI()
        {
            // Saves the current Gui.skin for later restore
            GUISkin _defGuiSkin = GUI.skin;
            if (_visible)
            {
                if (resetSize)
                {
                    ImageOrig();
                    resetSize = false;
                }
                GUI.skin = _useKSPskin ? HighLogic.Skin : _defGuiSkin;
                _windowRect = GUI.Window(GUIUtility.GetControlID(0, FocusType.Passive), _windowRect, IvWindow,
                    "Image viewer");
            }
            if (_showList)
            {
                GUI.skin = _useKSPskin ? HighLogic.Skin : _defGuiSkin;
                _windowRect2 = GUI.Window(GUIUtility.GetControlID(FocusType.Passive), _windowRect2, ListWindow,
                    "Image list");
            }
            //Restore the skin
            GUI.skin = _defGuiSkin;
           
        }

        private void IvWindow(int windowID)
        {
            if (_image != null)
            {
                _windowRect = new Rect(_windowRect.xMin, _windowRect.yMin, _image.width, _image.height + 20f);
                GUI.DrawTexture(new Rect(0f, 20f, _image.width, _image.height), _image, ScaleMode.ScaleToFit, true, 0f);
            }
            else
            {
                _windowRect = new Rect(Screen.width / 2f, Screen.height / 2f, 100f, 100f);
            }
            if (GUI.Button(new Rect(2f, 2f, 13f, 13f), "X"))
            {
                Toggle();
            }
            GUI.DragWindow();
        }

        private void ListWindow(int windowId)
        {
            if (_imageList != null)
            {
                // Notes list gui.
                _scrollViewVector = GUILayout.BeginScrollView(_scrollViewVector);
                var _options = new[] { GUILayout.Width(225f), GUILayout.ExpandWidth(false) };
                _selectionGridInt = GUILayout.SelectionGrid(_selectionGridInt, _imageList.ToArray(), 1,_options);
                GUILayout.EndScrollView();

                // Refresh images list.
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("-10% size"))
                {
                    ImageZm();
                }
                if (GUILayout.Button("Original size"))
                {
                    ImageOrig();
                }
                if (GUILayout.Button("+10% size"))
                {
                    ImageZp();
                }
                GUILayout.EndHorizontal();
            }
            GUI.contentColor = Color.green;
            if (GUILayout.Button("Refresh list"))
            {
                GetImages();
            }
            GUI.contentColor = Color.white;
            // Close the list window.
            if (GUI.Button(new Rect(2f, 2f, 13f, 13f), "X"))
            {
                _showList = !_showList;
            }
            // Makes the window dragable.
            GUI.DragWindow();
        }

        
        private void Update()
        {
            if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(_keybind))
            {
                Toggle();
            }
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(_keybind))
            {
                _showList = !_showList;
                return;
            }
            
            if (_imageList == null) return;
            
            if (_lastimg != _selectionGridInt)
            {
                Destroy(_image);
                resetSize = true;
                _lastimg = _selectionGridInt;
                if (_selectionGridInt > 0)
                {
                    _imagefile = _imageList[_selectionGridInt];
                    _imagetex = new WWW(_imageurl + _imagefile);
                    _image = _imagetex.texture;
                    _imagetex.Dispose();
                  
                    // Let's be sure it isn't bigger than the screen size
                    if (_image.width > Screen.width || _image.height > Screen.height)
                    {

                        float finalRatio = Mathf.Min((float)Screen.width / (float)_image.width, (float)Screen.width / (float)_image.height);

                        float finalWidth = (float)_image.width * finalRatio;
                        float finalHeight = (float)_image.height * finalRatio;

                        TextureScale.Bilinear(_image, (int)finalWidth, (int)finalHeight);

                    }
                   // _lastimg = _selectionGridInt;
                    if (!_visible)
                        Toggle();
                }
                else
                {
                  //  _lastimg = _selectionGridInt;
                    if (_visible)
                        Toggle(true);
                }
            }
        }

        private void GetImages()
        {

            if (Directory.GetFiles(_imagedir, "*").Any())
            {

                _imageList = new List<string>();
                _imageList.Add("Hide Image Window");
                var f  = new List<string>(Directory.GetFiles(_imagedir, "*"));
                _imageList.AddRange(f);
                for (int i = 1; i < _imageList.Count; i++)
                {
                    _imageList[i] = Path.GetFileName(_imageList[i]);
                }
            }
        }

        private void OnDestroy()
        {
            SaveSettings();
            if (_button != null)
            {
                _button.Destroy();
            }
            GameEvents.onGameSceneLoadRequested.Remove(onSceneChange);
        }

        private void createSettings()
        {
            ImgVwrSettings.Create();
            ImgVwrSettings.Save();
        }
        private void LoadSettings()
        {
            print("[ImageViewer.dll] Loading Config...");
            if (!System.IO.File.Exists(kspPluginDataFldr + "ImageViewer.cfg"))
                createSettings();
            ImgVwrSettings.Load();

            _windowRect = ImgVwrSettings.GetValue("windowpos", new Rect(1,1,2,2));
            _windowRect2 = ImgVwrSettings.GetValue("windowpos2", new Rect(Screen.width / 2 - 150f, Screen.height / 2 - 75f, 270f, 390f));
            _keybind = ImgVwrSettings.GetValue("keybind", "i");
            if (_keybind == "")
                _keybind = "i";
            _versionlastrun = ImgVwrSettings.GetValue("version", "");
            _useKSPskin = ImgVwrSettings.GetValue("kspskin", false);
            //_visible = ImgVwrSettings.GetValue("visible", false);
            _selectionGridInt = ImgVwrSettings.GetValue("lastimage", 0);

            print("[ImageViewer.dll] Config Loaded Successfully");
        }

        private void SaveSettings()
        {
            print("[ImageViewer.dll] Saving Config...");
            

            ImgVwrSettings.SetValue("windowpos", _windowRect);
            ImgVwrSettings.SetValue("windowpos2", _windowRect2);
            ImgVwrSettings.SetValue("keybind", _keybind);
           // ImgVwrSettings.SetValue("version", _version);
            ImgVwrSettings.SetValue("kspskin", _useKSPskin);
            //ImgVwrSettings.SetValue("visible", _visible);
            ImgVwrSettings.SetValue("lastimage", _selectionGridInt);

            ImgVwrSettings.Save();
            print("[ImageViewer.dll] Config Saved ");
        }

        private void Toggle(bool keepShowlist = false)
        {
            if (_visible)
            {
                _visible = false;
                if (_button != null)
                    _button.TexturePath = _btextureOff;
                if (_showList && !keepShowlist)
                {
                    _showList = false;
                }
            }
            else
            {
                _visible = true;
                if (_button != null)
                    _button.TexturePath = _btextureOn;
            }
        }

#if false
        private void VersionCheck()
        {
            _version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            print("ImageViewer.dll version: " + _version);
            if ((_version != _versionlastrun) && (System.IO.File.Exists(kspPluginDataFldr + "ImageViewer.cfg")))
            {
                
                System.IO.File.Delete(kspPluginDataFldr + "ImageViewer.cfg");
            }
#if DEBUG
           // File.Delete(kspPluginDataFldr + "config.xml");
#endif
        }


        private void LoadVersion()
        {
            if (!System.IO.File.Exists(kspPluginDataFldr + "ImageViewer.cfg"))
                createSettings();
            ImgVwrSettings.Load();
            _versionlastrun = ImgVwrSettings.GetValue("version", "");
        }
#endif

        private void TogglePopupMenu(IButton button)
        {
            if (button.Drawable == null)
            {
                createPopupMenu(button);
            }
            else
            {
                destroyPopupMenu(button);
            }
        }

        private void createPopupMenu(IButton button)
        {
            // create menu drawable
            PopupMenuDrawable _menu = new PopupMenuDrawable();

            // create menu options
            //IButton _option1 = _menu.AddOption("Show/Hide image");
            //_option1.OnClick += e => Toggle();
            IButton _option2 = _menu.AddOption("Show/hide image list");
            _option2.OnClick += e => _showList = !_showList;
            IButton _option3 = _menu.AddOption("Change skin");
            _option3.OnClick += e => _useKSPskin = !_useKSPskin;
            IButton _option4 = _menu.AddOption("Next image");
            _option4.OnClick += e => ImageNext();
            IButton _option5 = _menu.AddOption("Prev image");
            _option5.OnClick += e => ImagePrev();
            IButton _option6 = _menu.AddOption("-10% size");
            _option6.OnClick += e => ImageZm();
            IButton _option7 = _menu.AddOption("Original");
            _option7.OnClick += e => ImageOrig();
            IButton _option8 = _menu.AddOption("+10% size");
            _option8.OnClick += e => ImageZp();
            // auto-close popup menu when any option is clicked
            _menu.OnAnyOptionClicked += () => destroyPopupMenu(button);

            // hook drawable to button
            button.Drawable = _menu;
        }

        private void ImagePrev()
        {
            if (_selectionGridInt == 0) return;
            _selectionGridInt--;
        }

        private void ImageNext()
        {
            if (_selectionGridInt == _imageList.Count - 1) return;
            _selectionGridInt++;
        }

        private void ImageZm()
        {
            TextureScale.Bilinear(_image, _image.width - ((_image.width * 10) / 100), _image.height - ((_image.height * 10) / 100));
            GUI.DrawTexture(new Rect(0f, 20f, _image.width, _image.height), _image, ScaleMode.ScaleToFit, true, 0f);
        }

        private void ImageOrig()
        {
            _imagetex = new WWW(_imageurl + _imagefile);
            _image = _imagetex.texture;
            _imagetex.Dispose();
            GUI.DrawTexture(new Rect(0f, 20f, _image.width, _image.height), _image, ScaleMode.ScaleToFit, true, 0f);
        }

        private void ImageZp()
        {
            TextureScale.Bilinear(_image, _image.width + ((_image.width * 10) / 100), _image.height + ((_image.height * 10) / 100));
            GUI.DrawTexture(new Rect(0f, 20f, _image.width, _image.height), _image, ScaleMode.ScaleToFit, true, 0f);
        }

        private void destroyPopupMenu(IButton button)
        {
            // PopupMenuDrawable must be destroyed explicitly
            ((PopupMenuDrawable)button.Drawable).Destroy();

            // unhook drawable
            button.Drawable = null;
        }
    }
}
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

using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using KSP.UI.Screens;

using ClickThroughFix;
using ToolbarControl_NS;


namespace img_viewer
{
    [KSPAddon(KSPAddon.Startup.AllGameScenes, true)]
    public class ImgViewer : MonoBehaviour
    {
        private Rect _windowRect;
        private Rect _windowRect2 = new Rect(Screen.width / 2 - 150f, Screen.height / 2 - 75f, 260f, 390f);

        private string _keybind = "i";

        private bool _visible = false;
        
        ToolbarControl toolbarControl;

        internal const string MODID = "ImageViewer_NS";
        internal const string MODNAME = "Image Viewer";

        private const string _tooltip = "Image Viewer Menu";
        private const string _btextureOn = "ImageViewer/Textures/icon_on";
        private const string _btextureOff = "ImageViewer/Textures/icon_off";
        
        private string _versionlastrun;
        private Texture2D _image;
        
        private string _imagefile;


        public ImgVwrSettings ImgVwrSettings = new ImgVwrSettings();
        private string kspPluginDataFldr;

        internal string _imagedir = "";
        internal string imagedirDefault;

        private List<string> _imageList;
        private Vector2 _scrollViewVector = Vector2.zero;
        private int _selectionGridInt = -1;
        private bool _showList = false;
        private bool _useKSPskin;
        private int _lastimg = -1;
        
        private void Awake()
        {
            kspPluginDataFldr = KSPUtil.ApplicationRootPath.Replace("\\", "/") + "GameData/ImageViewer/PluginData/";
            imagedirDefault = kspPluginDataFldr + "Images/";
            _imagedir = imagedirDefault;
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

            toolbarControl = gameObject.AddComponent<ToolbarControl>();

            toolbarControl.AddToAllToolbars(TogglePopupMenu, TogglePopupMenu,
                ApplicationLauncher.AppScenes.SPACECENTER |
                ApplicationLauncher.AppScenes.VAB |
                ApplicationLauncher.AppScenes.SPH |
                ApplicationLauncher.AppScenes.TRACKSTATION |
                ApplicationLauncher.AppScenes.FLIGHT |
                ApplicationLauncher.AppScenes.MAPVIEW,
                MODID,
                "imageViewerButtonButton",
                _btextureOff,
                _btextureOff,
                MODNAME
            );
        }

        bool resetSize = false;
        private void OnGUI()
        {
            // The GUI.skin has been moved inside the two "if" sections to minimize the performance impact when not being used            
            if (_visible)
            {
                // Saves the current Gui.skin for later restore
                GUISkin _defGuiSkin = GUI.skin;
                if (resetSize)
                {
                    ImageOrig();
                    resetSize = false;
                }
                GUI.skin = _useKSPskin ? HighLogic.Skin : _defGuiSkin;
                _windowRect = ClickThruBlocker.GUIWindow(GUIUtility.GetControlID(0, FocusType.Passive), _windowRect, IvWindow,
                    "Image viewer");
                //Restore the skin
                GUI.skin = _defGuiSkin;
            }
            if (_showList)
            {
                // Saves the current Gui.skin for later restore
                GUISkin _defGuiSkin = GUI.skin;
                GUI.skin = _useKSPskin ? HighLogic.Skin : _defGuiSkin;
                _windowRect2 = ClickThruBlocker.GUIWindow(GUIUtility.GetControlID(FocusType.Passive), _windowRect2, ListWindow,
                    "Image list");
                //Restore the skin
                GUI.skin = _defGuiSkin;
            }

            if (Event.current.type == EventType.Layout)
            {
                if (_showMenu)
                    _menuRect = ClickThruBlocker.GUILayoutWindow(this.GetInstanceID(), _menuRect, MenuContent, "Image Viewer");
                else
                    _menuRect = new Rect();
            }
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
                Toggle();

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

        void LoadImageFromFile(int idx, bool showIfVisible = true)
        {
            _imagefile = _imageList[idx];
            _image = new Texture2D(2, 2);
            ToolbarControl.LoadImageFromFile(ref _image, _imagedir +_imagefile);

            // Let's be sure it isn't bigger than the screen size
            if (_image.width > Screen.width || _image.height > Screen.height)
            {

                float finalRatio = Mathf.Min((float)Screen.width / (float)_image.width, (float)Screen.width / (float)_image.height);

                float finalWidth = (float)_image.width * finalRatio;
                float finalHeight = (float)_image.height * finalRatio;

                TextureScale.Bilinear(_image, (int)finalWidth, (int)finalHeight);

            }
            // _lastimg = _selectionGridInt;

            if (showIfVisible && !_visible)
                Toggle(true);
  
        }

        private void Update()
        {
            if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(_keybind))
                Toggle();

            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(_keybind))
            {
                _showList = !_showList;
                return;
            }
            
            if (_imageList == null) return;
            
            if (_lastimg != _selectionGridInt )
            {
                Destroy(_image);
                resetSize = true;
                _lastimg = _selectionGridInt;
                if (_selectionGridInt > 0)
                {
                    LoadImageFromFile(_selectionGridInt);
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
                if (_selectionGridInt > 0)
                    LoadImageFromFile(_selectionGridInt, false);
            }
        }

        private void OnDestroy()
        {
            SaveSettings();

            if (toolbarControl != null)
            {
                toolbarControl.OnDestroy();
                Destroy(toolbarControl);
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

            _imagedir = ImgVwrSettings.GetValue("imagedir", imagedirDefault);
            if (!Directory.Exists(_imagedir))
            {
                Debug.Log("Specified image directory doesn't exist: [" + _imagedir + "]");
                _imagedir = imagedirDefault;
            }
            _selectionGridInt = ImgVwrSettings.GetValue("lastimage", 0);
            _lastimg = _selectionGridInt; // Needed to keep last image from being shown on game start            
            print("[ImageViewer.dll] Config Loaded Successfully");
        }

        private void SaveSettings()
        {
            if (HighLogic.CurrentGame == null)
                return;
            print("[ImageViewer.dll] Saving Config...");
            

            ImgVwrSettings.SetValue("windowpos", _windowRect);
            ImgVwrSettings.SetValue("windowpos2", _windowRect2);
            ImgVwrSettings.SetValue("keybind", _keybind);
            ImgVwrSettings.SetValue("kspskin", _useKSPskin);
            ImgVwrSettings.SetValue("imagedir", _imagedir);            
            ImgVwrSettings.SetValue("lastimage", _selectionGridInt);

            ImgVwrSettings.Save();
            print("[ImageViewer.dll] Config Saved ");
        }

        private void Toggle(bool keepShowlist = false)
        {
            if (_visible)
            {
                _visible = false;
#if false
                if (_button != null)
                    _button.TexturePath = _btextureOff;
#endif
                if (toolbarControl != null)
                    toolbarControl.SetTexture(_btextureOff, _btextureOff);
                if (_showList && !keepShowlist)
                {
                    _showList = false;
                }
            }
            else
            {
                _visible = true;
#if false
                if (_button != null)
                    _button.TexturePath = _btextureOn;
#endif  
                if (toolbarControl != null)
                    toolbarControl.SetTexture(_btextureOn, _btextureOn);
            }
        }

        bool _showMenu = false;
        private void TogglePopupMenu()
        {
            if (!_showMenu)
            {
                InitShowMenu();
            }
  
            _showMenu = !_showMenu;
        }

        void MenuContent(int WindowID)
        {
            if (GUILayout.Button("Show/hide image list"))
            {
                _showList = !_showList;
            }

            if (GUILayout.Button("Change skin"))
            {
                _useKSPskin = !_useKSPskin;
            }
            if (GUILayout.Button("Next image"))
            {
                ImageNext();
            }

            if (GUILayout.Button("Prev image"))
            {
                ImagePrev();
            }

            if (GUILayout.Button("-10% size"))
            {
                ImageZm();
            }

            if (GUILayout.Button("Original"))
            {
                ImageOrig();
            }

            if (GUILayout.Button("+10% size"))
            {
                ImageZp();
            }

        }

        Rect _menuRect = new Rect();
        Vector2 pos;
        const float _menuWidth = 100.0f;
        const float menuHeight = 230f;
        const int _toolbarHeight = 42;
        Vector2 SetButtonPos()
        {
            Vector2 pos = Input.mousePosition;
            pos.y = Screen.height - pos.y;
            return  pos;
        }

        void InitShowMenu(bool firstTime = true)
        {
            pos = SetButtonPos();
            int toolbarHeight = (int)(_toolbarHeight * GameSettings.UI_SCALE);
            if (firstTime)
            {
                float ym = pos.y - menuHeight;
                ym = Mathf.Max(ym, 0);
                ym = Mathf.Min(ym, Screen.height - toolbarHeight - menuHeight);
                _menuRect = new Rect()
                {
                    xMin = pos.x - _menuWidth / 2,
                    xMax = pos.x + _menuWidth / 2,
                    yMin = ym,
                    yMax =Mathf.Min( Screen.height - toolbarHeight, ym + menuHeight)
                };

            }
            else
            {
                _menuRect.Set(
                    _menuRect.x,
                    Screen.height - toolbarHeight - menuHeight,
                     _menuRect.width,
                     menuHeight
                );
            }
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
            _image = new Texture2D(2, 2);
            ToolbarControl.LoadImageFromFile(ref _image, _imagedir + _imagefile);

            GUI.DrawTexture(new Rect(0f, 20f, _image.width, _image.height), _image, ScaleMode.ScaleToFit, true, 0f);
        }

        private void ImageZp()
        {
            TextureScale.Bilinear(_image, _image.width + ((_image.width * 10) / 100), _image.height + ((_image.height * 10) / 100));
            GUI.DrawTexture(new Rect(0f, 20f, _image.width, _image.height), _image, ScaleMode.ScaleToFit, true, 0f);
        }
    }
}
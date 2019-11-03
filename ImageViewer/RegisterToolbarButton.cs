using UnityEngine;
using ToolbarControl_NS;

namespace img_viewer
{
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class RegisterToolbar : MonoBehaviour
    {
        void Start()
        {
            ToolbarControl.RegisterMod(ImgViewer.MODID, ImgViewer.MODNAME);
        }
    }
}
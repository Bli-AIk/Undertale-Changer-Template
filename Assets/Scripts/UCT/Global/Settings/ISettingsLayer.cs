using System;
using UCT.Control;
using UCT.Global.Audio;
using UCT.Global.Core;
using UCT.Global.UI;
using UCT.Service;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UCT.Global.Settings
{
    public interface ISettingsLayer
    {
        void UpdateLayer();
    }
    
    public class HomeLayer : ISettingsLayer
    {

        public void UpdateLayer()
        {
            
        }
    }

    public class KeyConfigLayer : ISettingsLayer
    {
        public void UpdateLayer()
        {
            
        }
    }

    public class LanguagePacksConfigLayer : ISettingsLayer
    {
        public void UpdateLayer()
        {
            
        }
    }
}
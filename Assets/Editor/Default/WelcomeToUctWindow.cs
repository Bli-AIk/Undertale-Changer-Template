using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.Default
{
    public class WelcomeToUctWindow : EditorWindow
    {
        private enum UctWelcomeLayers
        {
            Introduction,
            Dependencies,
            About
        }

        private UctWelcomeLayers _uctWelcomeLayer;
        
        public void CreateGUI()
        {
            var root = rootVisualElement;
            var styleSheet = (StyleSheet)EditorGUIUtility.Load("Default/WelcomeToUctStyle.uss");
            root.styleSheets.Add(styleSheet);

            var titleZone = new VisualElement();
            titleZone.AddToClassList("TitleZone");
            root.Add(titleZone);

            var titleLabel = new Label("Welcome To Undertale Changer Template!");
            titleLabel.AddToClassList("TitleLabel");
            titleZone.Add(titleLabel);


            var mainZone = new VisualElement();
            mainZone.AddToClassList("MainZone");
            root.Add(mainZone);

            var buttonZone = new VisualElement();
            buttonZone.AddToClassList("ButtonZone");
            
            mainZone.Add(buttonZone);

            var pageZone = new VisualElement();
            pageZone.AddToClassList("ButtonZone");
            pageZone.style.flexDirection = FlexDirection.Column;
            mainZone.Add(pageZone);
            
            foreach (var enumName in Enum.GetNames(typeof(UctWelcomeLayers)))
            {
                var button = new Button
                {
                    text = enumName
                };
                button.clicked += () =>
                {
                    var layer = (UctWelcomeLayers)Enum.Parse(typeof(UctWelcomeLayers), enumName);
                    _uctWelcomeLayer = layer;
                    RenderPage(pageZone);
                };
                buttonZone.Add(button);
            }
            

            RenderPage(pageZone);
            
        }

        private void RenderPage(VisualElement pageZone)
        {
            pageZone.Clear();
            switch (_uctWelcomeLayer)
            {
                case UctWelcomeLayers.Introduction:
                    IntroductionPage(pageZone);
                    break;
                case UctWelcomeLayers.Dependencies:
                    DependenciesPage(pageZone);
                    break;
                case UctWelcomeLayers.About:
                    AboutPage(pageZone);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static void IntroductionPage(VisualElement pageZone)
        {
            var uctImage = new Image
            {
                sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Logos/logo.png")
            };
            uctImage.AddToClassList("UctImage");
            pageZone.Add(uctImage);
            var introductionInformation = new Label
            {
                text = "Undertale Changer Template由Bli_AIk制作。\n" +
                       "该项目使用了通用渲染管线（URP）。\n" +
                       "目前，该模板包括以下内容：\n" +
                       "标题场景，重新命名场景，世界场景，战斗场景，游戏结束场景。\n" +
                       "包含外部语言包系统。\n" +
                       "您可以使用此模板创建自己的游戏或修改原始代码。\n" +
                       "但请确保在显眼的位置醒目显示“Undertale-Changer-Template”。\n" +
                       "得了我也懒得写了反正这块先这么写吧之后再改（",
                style =
                {
                    unityTextAlign = TextAnchor.UpperLeft
                }
            };
            pageZone.Add(introductionInformation);
        }
        private static void DependenciesPage(VisualElement pageZone)
        {
            var introductionInformation = new Label
            {
                text = "<size=20><b>本模板依赖项</b></size>\n\n" +
                       "Clipper2\n" +
                       "DOTween\n" +
                       "LibTessDotNet\n" +
                       "Live2D\n" +
                       "TextMesh Pro\n" +
                       "More Effective Coroutines\n" +
                       "Fusion Pixel Font\n" +
                       "RiderFlow\n" +
                       "Pinyin4NET\n" +
                       "NuGetForUnity\n" +
                       "chineseStroke\n" +
                       "Alchemy\n"
                ,
                style =
                {
                    unityTextAlign = TextAnchor.UpperCenter
                }
            };
            pageZone.Add(introductionInformation);
        }    
        private static void AboutPage(VisualElement pageZone)
        {
            var introductionInformation = new Label
            {
                text = "制作人员名单：AIk、AIk、AIk、AIk、AIk、AIk、AIk、AIk、AIk、AIk、AIk、AIk、AIk、AIk、AIk、AIk、AIk、AIk",
                style =
                {
                    unityTextAlign = TextAnchor.UpperLeft
                }
            };
            pageZone.Add(introductionInformation);
        }

        [MenuItem("Window/UCT/Welcome")]
        public static void ShowExample()
        {
            var window = GetWindow<WelcomeToUctWindow>();
            window.titleContent = new GUIContent
            {
                text = "Welcome",
                image = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Sprites/Logos/logo2.png")
            };
            window.minSize = new Vector2(600, 400);
            
        }
    }
}
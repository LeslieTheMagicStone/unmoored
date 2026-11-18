using System.Collections.Generic;
using System.Xml;
using UnityEngine;

namespace CGJ2026
{
    public class LocalizationManager : MonoBehaviour
    {
        public static LocalizationManager Instance { get; private set; }

        Dictionary<string, string> en = new();
        Dictionary<string, string> zh = new();

        public enum Lang { EN, ZH }
        [SerializeField] Lang currentLang = Lang.ZH;

        void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadXML();
        }

        void LoadXML()
        {
            var xml = Resources.Load<TextAsset>("Localization");
            if (xml == null)
                throw new System.Exception("Localization.xml not found in Resources");

            var doc = new XmlDocument();
            doc.LoadXml(xml.text);

            foreach (XmlNode node in doc.SelectNodes("strings/string"))
            {
                var key = node.Attributes["key"].Value;
                var parts = node.InnerText.Split(" / ", 2, System.StringSplitOptions.None);
                if (parts.Length != 2)
                    throw new System.Exception($"Localization: missing ' / ' separator for key '{key}'");

                en[key] = parts[0].Trim();
                zh[key] = parts[1].Trim();
            }
        }

        public string Get(string key, params object[] args)
        {
            var dict = currentLang == Lang.EN ? en : zh;
            if (!dict.TryGetValue(key, out var value))
                throw new System.Collections.Generic.KeyNotFoundException(
                    $"Localization key '{key}' not found for language {currentLang}");

            return args.Length > 0 ? string.Format(value, args) : value;
        }
    }
}

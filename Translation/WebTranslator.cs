// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

using Translation.Baidu;
using Translation.Deepl;
using Translation.Papago;
using Translation.Google;
using Translation.Yandex;
using Translation.Utils;
using System.Net;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Translation
{
    public class WebTranslator
    {
        public ReadOnlyCollection<TranslationEngine> TranslationEngines
        {
            get { return _TranslationEngines; }
        }

        private ReadOnlyCollection<TranslationEngine> _TranslationEngines;

        GoogleTranslator _GoogleTranslator;

        YandexTranslator _YandexTranslator;

        BaiduTranslater _BaiduTranslator;

        DeepLTranslator _DeepLTranslator;

        PapagoTranslator _PapagoTranslator;

        List<KeyValuePair<TranslationRequest, string>> transaltionCache;
        KeyValuePair<TranslationRequest, string> defaultCachedResult = default(KeyValuePair<TranslationRequest, string>);

        LanguageDetector _LanguageDetector;

        ILog _Logger;

        string _TransaltionSettingsPath = "TranslationSysSettings.json";

        public WebTranslator(ILog logger)
        {

            _Logger = logger;
            //azaza
            /*if (!Helper.LoadStaticFromJson(typeof(GlobalTranslationSettings), _TransaltionSettingsPath))
            {
                Helper.SaveStaticToJson(typeof(GlobalTranslationSettings), _TransaltionSettingsPath);
                Helper.LoadStaticFromJson(typeof(GlobalTranslationSettings), _TransaltionSettingsPath);
            }*/

            transaltionCache = new List<KeyValuePair<TranslationRequest, string>>(GlobalTranslationSettings.TranslationCacheSize);

            _GoogleTranslator = new GoogleTranslator(_Logger);

            _YandexTranslator = new YandexTranslator(_Logger);

            _DeepLTranslator = new DeepLTranslator(_Logger);

            _PapagoTranslator = new PapagoTranslator(_Logger);

            _BaiduTranslator = new BaiduTranslater(_Logger);

            _LanguageDetector = new LanguageDetector(GlobalTranslationSettings.MaxSameLanguagePercent,
                GlobalTranslationSettings.NTextCatLanguageModelsPath, _Logger);
        }

        public void LoadLanguages()
        {
            LoadLanguages(GlobalTranslationSettings.GoogleTranslateLanguages, GlobalTranslationSettings.MultillectTranslateLanguages,
                GlobalTranslationSettings.DeeplLanguages, GlobalTranslationSettings.YandexLanguages,
                GlobalTranslationSettings.PapagoLanguages, GlobalTranslationSettings.BaiduLanguages);
        }

        public async Task<string> TranslateAsync(string inSentence, TranslationEngine translationEngine, TranslatorLanguague fromLang, TranslatorLanguague toLang)
        {
            string result = String.Empty;

            await Task.Run(() =>
            {
                result = Translate(inSentence, translationEngine, fromLang, toLang);
            });

            return result;
        }
        public static Dictionary<string, string> tenguwords = new Dictionary<string, string>();
        /*{
           { "Alphinaud", "Альфинауд" },
           { "Merlwyb", "Мелвиб" },
            {"daedalus stoneworks", "Даедалус Стоунворкс" },
            {"<sniffle>", "<хнык>" },
            {"venture","adventure" },
            {"oh", "ах" }
        };*/

        public string Translate(string inSentence, TranslationEngine translationEngine, TranslatorLanguague fromLang, TranslatorLanguague toLang)
        {
            string data = String.Empty;
            string name = String.Empty;
            string text = inSentence;
            
            if (text.IndexOf(":") > 0)
            {
                name = text.Split(':')[0];
                text = text.Split(new string[] { ":" }, 2, StringSplitOptions.None)[1];
            }
            if(name.Length>0 && text.Length<=1)
            { name = ""; text= inSentence; }

            //azazaza
            text = text.Trim();
            if (text.Contains("...") && text.Length > 3)
                text = text.TrimStart('.');
            if (text.IndexOf(":") == 0)
                text.Substring(1,text.Length);


            /*foreach (var v in tenguwords.Keys)
            { 
                if (text.IndexOf(v, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    string v0 = v;
                    if (v.Contains(@"?"))
                        v0 = v0.Replace("?", @"\?");
                    else if (v.Contains(@"."))
                        v0 = v0.Replace(".", @"\.");
                    //text = text.Replace(v, tenguwords[v]);
                    text = Regex.Replace(text, v0, tenguwords[v], RegexOptions.IgnoreCase);
                }
            }*/
            //char[] patt = new char[] { ' ', ',', '.', ':', ';', '?', '!' };
            //==            
            /*foreach (var v in tenguwords.Keys)
            {
                //if (text.IndexOf(v, StringComparison.OrdinalIgnoreCase) >= 0 && v.IndexOfAny(patt) >=0)
                //{ }
                string v0 = v;
                if (v.Contains(@"?"))
                    v0 = v0.Replace("?", @"\?");
                else if (v.Contains(@"."))
                    v0 = v0.Replace(".", @"\.");
                //text = text.Replace(v, tenguwords[v]);
                text = Regex.Replace(text, v0, tenguwords[v], RegexOptions.IgnoreCase);

            }*/
            
            string chtext = text;            
            foreach (var v in tenguwords.Keys)
            {
                string v0 = v;
                if (v.Contains(@"?"))
                    v0 = v0.Replace("?", @"\?");
                else if (v.Contains(@"."))
                    v0 = v0.Replace(".", @"\.");

                if (v0.IndexOf("@") >= 0)
                    v0 = v0.Substring(1, v0.Length - 1);

                if (chtext.IndexOf(v0, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    string optext = "";
                    Regex rxcheck = new Regex(@"[\s]");
                    if (rxcheck.IsMatch(v) == true || v.IndexOf("@") >= 0)                    
                    {
                        optext = Regex.Replace(chtext, v0, tenguwords[v], RegexOptions.IgnoreCase);
                    }
                    else
                    {
                        Regex rx = new Regex(@"[a-zA-Z0-9]");
                        var split = Regex.Split(chtext, @"(?<=[\s,.:;!?])"); //@"(?<=[\s,.:;])"

                        foreach (var s in split)
                        {
                            string word = s;
                            int sres = 0;
                            int baseind = s.IndexOf(v, StringComparison.OrdinalIgnoreCase);
                            if (baseind >= 0)
                            {
                                int beforind = baseind - 1;
                                int afterind = baseind + v.Length;
                                sres = (beforind >= 0 && rx.IsMatch(s.Substring(beforind, 1)) == true) ? ++sres : sres;
                                sres = (afterind > 0 && afterind < s.Length && rx.IsMatch(s.Substring(afterind, 1)) == true) ? ++sres : sres;
                                if (sres == 0) word = Regex.Replace(s, v0, tenguwords[v], RegexOptions.IgnoreCase);
                            }
                            optext = optext + word;

                        }
                    }
                    chtext = optext;
                }
            }
            text = chtext;
            if (text.IndexOf("─") >= 0) text = Regex.Replace(chtext, "─", " ─ ");
            //==
            /*string ss = "";
            int start = 0, index;
            if (text.IndexOfAny(patt) != -1)
            {
                while ((index = text.IndexOfAny(patt, start)) != -1)
                {
                    string ts = "";
                    if (index - start > 0)
                    {
                        ts = text.Substring(start, index - start);
                        foreach (var v in tenguwords.Keys)
                        {
                            if (ts.IndexOf(v, StringComparison.OrdinalIgnoreCase) >= 0 && ts.Trim().Length == v.Length)
                            {
                                ts = Regex.Replace(ts, v, tenguwords[v], RegexOptions.IgnoreCase);
                            }
                        }
                    }
                    ts = ts + text.Substring(index, 1);
                    start = index + 1;

                    ss = ss + ts;
                }
            }
            else
            {
                if(tenguwords.ContainsKey(text))
                    ss = Regex.Replace(text, text, tenguwords[text], RegexOptions.IgnoreCase);
            }
            text = ss.Length==0 ? text : ss;*/
            //
            //using (StreamWriter outputFile = new StreamWriter(@"D:\tete.txt", true)) { outputFile.WriteLine(name+": "+text); }//azazaza


            /*HttpWebRequest request = (HttpWebRequest)WebRequest.Create($"https://translate.google.com/m?&ie=UTF-8&prev=m&q={text}&sl=en&tl=ru");
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            if (response.StatusCode == HttpStatusCode.OK)
            {
                Stream receiveStream = response.GetResponseStream();
                StreamReader readStream = null;

                if (String.IsNullOrWhiteSpace(response.CharacterSet))
                    readStream = new StreamReader(receiveStream);
                else
                    readStream = new StreamReader(receiveStream, Encoding.GetEncoding(response.CharacterSet));

                string tempdata = readStream.ReadToEnd();
                //data = Regex.Match(tempdata, @"English</a></div><br><div dir=""ltr"" class=""t0"">(.+?)</div>").Groups[1].Value;
                //Match m = Regex.Match(tempdata, @"English</a></div><br><div dir=""ltr"" class=""t0"">(.+?)</div>");
                //for (int i = 1; i <= m.Groups.Count; i++)
                //{
                //    data = data + m.Groups[i].Value; //
                //}
                tempdata = System.Net.WebUtility.HtmlDecode(tempdata);
                if (tempdata.Contains(@"English</a></div><br><div dir=""ltr"" class=""t0"">"))
                    data = Regex.Match(tempdata, @"English</a></div><br><div dir=""ltr"" class=""t0"">(.+?)</div>").Groups[1].Value;
                else if (tempdata.Contains(@"""result-container"">"))
                    data = Regex.Match(tempdata, @"""result-container"">(.+?)</div>").Groups[1].Value;
                else
                    data = tempdata;
                response.Close();
                readStream.Close();
            }
            return name+":"+data;*/

            if (text != "...")
            {
                string tempdata = GetTranZZ($"https://translate.google.com/m?&ie=UTF-8&prev=m&q={text}&sl=en&tl=ru");
                
                if (tempdata.IndexOf("Error") < 1)
                {
                    tempdata = System.Net.WebUtility.HtmlDecode(tempdata);                   
                    if (tempdata.Contains(@"English</a></div><br><div dir=""ltr"" class=""t0"">"))
                        data = Regex.Match(tempdata, @"English</a></div><br><div dir=""ltr"" class=""t0"">(.+?)</div>").Groups[1].Value;
                    else if (tempdata.Contains(@"""result-container"">"))
                        data = Regex.Match(tempdata, @"""result-container"">(.+?)</div>").Groups[1].Value;
                    //else
                    //    data = tempdata;
                }
            }
            else data = "...";
            if (data.Contains("<"))
                data = data.Replace("<", "\"");
            if (data.Contains(">"))
                data = data.Replace(">", "\"");

            if (data.Length > 71)
            {
                string substr = data.Substring(0, 36);
                var fr = Regex.Split(data, substr);
                if(fr.Count()>2)
                    data=substr + fr[1];
            }
            //using (StreamWriter sw = File.CreateText(@"D:\z.txt")) { sw.WriteLine(tempdata); }
            return name.Length>1 ? name + ":"+data : data;            
        }
        public static async Task<string> PostWebAsync(string url, string idata)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            var data = Encoding.UTF8.GetBytes(idata);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = data.Length;
            using (var stream = await request.GetRequestStreamAsync())
            {
                await stream.WriteAsync(data, 0, data.Length);
            }

            var response = (HttpWebResponse)await request.GetResponseAsync();

            var r = new StreamReader(response.GetResponseStream());
            //    System.Windows.MessageBox.Show(await r.ReadToEndAsync());
            return await r.ReadToEndAsync();
        }
        public string GetTranZZ(string _url)
        {
            string result = "Error";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(_url);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            if (response.StatusCode == HttpStatusCode.OK)
            {
                Stream receiveStream = response.GetResponseStream();
                StreamReader readStream = null;

                if (String.IsNullOrWhiteSpace(response.CharacterSet))
                    readStream = new StreamReader(receiveStream);
                else
                    readStream = new StreamReader(receiveStream, Encoding.GetEncoding(response.CharacterSet));

                result = readStream.ReadToEnd();
                response.Close();
                readStream.Close();
            }
                return result;
        }
        public string Translate22(string inSentence, TranslationEngine translationEngine, TranslatorLanguague fromLang, TranslatorLanguague toLang)
        {

            if (fromLang.SystemName == "Auto")
            {
                if (translationEngine.EngineName != TranslationEngineName.GoogleTranslate)
                {
                    var dLang = _LanguageDetector.TryDetectLanguague(inSentence);
                    if (dLang.Length > 1)
                    {
                        var nLang = translationEngine.SupportedLanguages.FirstOrDefault(x => x.SystemName == dLang);
                        if (nLang != null)
                            fromLang = nLang;
                    }
                }
            }

            if (fromLang.SystemName == toLang.SystemName)
                return inSentence;

            if (inSentence.All(x => !char.IsLetter(x)))
                return inSentence;

            switch (toLang.SystemName)
            {
                case "Korean":
                    if (_LanguageDetector.HasKorean(inSentence))
                        return inSentence;
                    break;
                case "Japanese":
                    if (_LanguageDetector.HasJapanese(inSentence))
                        return inSentence;
                    break;
            }

            TranslationRequest translationRequest = new TranslationRequest(inSentence, translationEngine.EngineName, fromLang.LanguageCode, toLang.LanguageCode);
            var cachedResult = transaltionCache.FirstOrDefault(x => x.Key == translationRequest);

            if (!cachedResult.Equals(defaultCachedResult))
            {
                return cachedResult.Value;
            }

            string result = String.Empty;

            inSentence = PreprocessSentence(inSentence);

            var fromLangCode = fromLang.LanguageCode;
            var toLangCode = toLang.LanguageCode;

            switch (translationEngine.EngineName)
            {
                case TranslationEngineName.GoogleTranslate:
                    {
                        result = GoogleTranslate(inSentence, fromLangCode, toLangCode);
                        break;
                    }

                case TranslationEngineName.DeepL:
                    {
                        result = DeeplTranslate(inSentence, fromLangCode, toLangCode);
                        break;
                    }
                case TranslationEngineName.Yandex:
                    {
                        result = YandexTranslate(inSentence, fromLangCode, toLangCode);
                        break;
                    }
                case TranslationEngineName.Papago:
                    {
                        result = PapagoTranslate(inSentence, fromLangCode, toLangCode);
                        break;
                    }
                case TranslationEngineName.Baidu:
                    {
                        result = BaiduTranslate(inSentence, fromLangCode, toLangCode);
                        break;
                    }
                default:
                    {
                        result = String.Empty;
                        break;
                    }
            }

            if (result.Length > 1)
            {
                cachedResult = transaltionCache.FirstOrDefault(x => x.Key == translationRequest);

                if (cachedResult.Equals(defaultCachedResult))
                    transaltionCache.Add(new KeyValuePair<TranslationRequest, string>(translationRequest, result));

                if (transaltionCache.Count > GlobalTranslationSettings.TranslationCacheSize - 10)
                    transaltionCache.RemoveRange(0, GlobalTranslationSettings.TranslationCacheSize / 2);

            }

            return result;
        }

        private void LoadLanguages(string glTrPath, string MultTrPath, string deepPath, string YaTrPath, string PapagoTrPath, string baiduTrPath)
        {
            try
            {
                List<TranslationEngine> tmptranslationEngines = new List<TranslationEngine>();
                var tmpList = Helper.LoadJsonData<List<TranslatorLanguague>>(glTrPath, _Logger);
                tmptranslationEngines.Add(new TranslationEngine(TranslationEngineName.GoogleTranslate, tmpList, 9));

                /*
                tmpList = Helper.LoadJsonData<List<TranslatorLanguague>>(MultTrPath, _Logger);
                tmptranslationEngines.Add(new TranslationEngine(TranslationEngineName.Multillect, tmpList, 1));//*/

                tmpList = Helper.LoadJsonData<List<TranslatorLanguague>>(deepPath, _Logger);
                tmptranslationEngines.Add(new TranslationEngine(TranslationEngineName.DeepL, tmpList, 10));

                /*
                tmpList = Helper.LoadJsonData<List<TranslatorLanguague>>(YaTrPath, _Logger);
                tmptranslationEngines.Add(new TranslationEngine(TranslationEngineName.Yandex, tmpList, 8));//*/

                tmpList = Helper.LoadJsonData<List<TranslatorLanguague>>(PapagoTrPath, _Logger);
                tmptranslationEngines.Add(new TranslationEngine(TranslationEngineName.Papago, tmpList, 6));

                tmpList = Helper.LoadJsonData<List<TranslatorLanguague>>(baiduTrPath, _Logger);
                tmptranslationEngines.Add(new TranslationEngine(TranslationEngineName.Baidu, tmpList, 3));

                tmptranslationEngines = tmptranslationEngines.OrderByDescending(x => x.Quality).ToList();


                _TranslationEngines = new ReadOnlyCollection<TranslationEngine>(tmptranslationEngines);
            }
            catch (Exception e)
            {
                _Logger.WriteLog(Convert.ToString(e));
            }
        }

        private string GoogleTranslate(string sentence, string inLang, string outLang)
        {
            string result = String.Empty;

            try
            {
                result = _GoogleTranslator.Translate(sentence, inLang, outLang);
            }
            catch (Exception e)
            {
                _Logger.WriteLog(Convert.ToString(e));
            }

            return result;
        }

        private string DeeplTranslate(string sentence, string inLang, string outLang)
        {
            return _DeepLTranslator.Translate(sentence, inLang, outLang);
        }

        private string YandexTranslate(string sentence, string inLang, string outLang)
        {
            string result = String.Empty;
            try
            {
                result = _YandexTranslator.Translate(sentence, inLang, outLang);
            }
            catch (Exception e)
            {
                _Logger.WriteLog(Convert.ToString(e));
            }

            return result;
        }

        private string PapagoTranslate(string sentence, string inLang, string outLang)
        {
            string result = string.Empty;

            try
            {
                result = _PapagoTranslator.Translate(sentence, inLang, outLang);
            }
            catch (Exception e)
            {
                _Logger.WriteLog(e.ToString());
            }

            return result;
        }

        private string BaiduTranslate(string sentence, string inLang, string outLang)
        {
            string result = string.Empty;

            try
            {
                result = _BaiduTranslator.Translate(sentence, inLang, outLang);
            }
            catch (Exception e)
            {
                _Logger.WriteLog(e.ToString());
            }

            return result;
        }

        private string PreprocessSentence(string sentence)
        {
            return sentence.Replace("&", " and ");
        }
    }
}

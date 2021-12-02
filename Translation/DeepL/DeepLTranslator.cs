﻿// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Translation.Deepl
{
    class DeepLTranslator
    {
        int _DeepLId;

        Random Rnd;

        WebApi.DeepLWebReader DeeplWebReader;

        bool _IsFirstTime;

        bool _ServerSplit;

        ILog _Logger;

        public DeepLTranslator(ILog logger)
        {
            _Logger = logger;

            DeeplWebReader = new WebApi.DeepLWebReader(_Logger);

            Rnd = new Random(DateTime.Now.Millisecond);

            _DeepLId = Rnd.Next(33931525, 53931525);

            _IsFirstTime = true;

            _ServerSplit = false;
        }

        public string Translate(string sentence, string inLang, string outLang)
        {
            string result = String.Empty;

            if (inLang == outLang)
                return sentence;

            try
            {
                string _outLang = outLang;
                string _inLang = inLang;

                if (_DeepLId > 83931525)
                {
                    _DeepLId = Rnd.Next(33931525, 53931525);
                    DeeplWebReader = new WebApi.DeepLWebReader(_Logger);

                    _IsFirstTime = true;
                }

                if (_IsFirstTime)
                {
                    _IsFirstTime = false;

                    DeepLRequest.DeepLHandshakeRequest deepLHandshakeRequest = new DeepLRequest.DeepLHandshakeRequest(_DeepLId);

                    string handShakeUrl = @"https://www.deepl.com/PHP/backend/clientState.php?request_type=jsonrpc&il=RU";
                    string strDeepLHandshakeRequest = JsonConvert.SerializeObject(deepLHandshakeRequest);

                    var strDeepLHandshakeResp = DeeplWebReader.GetWebData(handShakeUrl, WebApi.DeepLWebReader.WebMethods.POST, strDeepLHandshakeRequest);

                    var DeepLHandshakeResp = JsonConvert.DeserializeObject<DeepLResponse.DeepLHandshakeResponse>(strDeepLHandshakeResp);

                    _Logger?.WriteLog(strDeepLHandshakeResp);

                    _DeepLId++;

                    _IsFirstTime = false;
                }

                string url = @"https://www2.deepl.com/jsonrpc";

                if (_ServerSplit || inLang == "auto")
                {
                    DeepLRequest.DeepLCookieRequest deepLSentenceRequest = new DeepLRequest.DeepLCookieRequest(_DeepLId, sentence);
                    string strDeepLsentenceRequest = deepLSentenceRequest.ToJsonString();

                    var strDeepLSentencetResp = DeeplWebReader.GetWebData(url, WebApi.DeepLWebReader.WebMethods.POST, strDeepLsentenceRequest);
                    var deepLSentenceResp = JsonConvert.DeserializeObject<DeepLResponse.DeepLSentencePreprocessResponse>(strDeepLSentencetResp);

                    _DeepLId++;

                    inLang = deepLSentenceResp.result.lang;
                    if (inLang.Length == 0)
                        return result;
                }

                DeepLRequest.DeepLTranslatorRequest deepLTranslationRequest = new DeepLRequest.DeepLTranslatorRequest(_DeepLId, sentence, inLang, outLang);
                string strDeepLTranslationRequest = deepLTranslationRequest.ToJsonString();

                var strDeepLTranslationResponse = DeeplWebReader.GetWebData(url, WebApi.DeepLWebReader.WebMethods.POST, strDeepLTranslationRequest);
                _DeepLId++;

                var DeepLTranslationResponse = JsonConvert.DeserializeObject<DeepLResponse.DeepLTranslationResponse>(strDeepLTranslationResponse);


                StringBuilder temporaryResult = new StringBuilder();
                if (DeepLTranslationResponse?.ResponseResult?.Translations != null)
                {
                    List<DeepLResponse.DeepLTranslationResponse.Translation> translations = DeepLTranslationResponse.ResponseResult.Translations;
                    foreach (DeepLResponse.DeepLTranslationResponse.Translation transaltion in translations)
                    {
                        var beam = transaltion.Beams.FirstOrDefault();
                        if (beam?.PostprocessedSentence != null)
                        {
                            temporaryResult.Append(beam.PostprocessedSentence);
                            temporaryResult.Append(" ");
                        }
                    }
                }//*/

                result = temporaryResult.ToString();

            }
            catch (Exception e)
            {
                _Logger?.WriteLog(Convert.ToString(e));
            }

            return result;
        }
    }
}

﻿using System;
using System.Text;
using Doods.StdLibSsh.Base.Queries;
using Doods.StdLibSsh.Interfaces;

namespace Doods.StdLibSsh.Queries
{
    /// <summary>
    /// 
    /// </summary>
    /// <example>
    ///  vcgencmd version
    ///  Apr  5 2017 11:49:52
    ///  Copyright(c) 2012 Broadcom
    ///  version 3ca4cf4a663c5351eaec08b29d50d6e8324981b4(clean) (release)
    /// </example>
    public class FirmwareQuery : GenericQuery<string>
    {

        private int SHORTENED_HASH_LENGTH = 8;
        private char BLANK = ' ';

        private string _vcgencmdPath;
        public FirmwareQuery(IClientSsh client, String vcgencmdPath) : base(client)
        {
            _vcgencmdPath = vcgencmdPath;
            CmdString = _vcgencmdPath + " version";
        }

        protected override string PaseResult(string result)
        {
            return ParseFirmwareVersion(result);
        }
        private string ParseFirmwareVersion(string output)
        {
            var splitted = output.Split('\n');
            if (splitted.Length >= 3)
            {
                if (splitted[2].StartsWith("version "))
                {
                    return checkAndFormatVersionHash(splitted[2].Replace("version ", ""));
                    //return splitted[2].replace("version ", "");
                }
                else
                {
                    return splitted[2];
                }
            }
            else
            {
                Logger.Instance.Error("Could not parse firmware. Maybe the output of 'vcgencmd version' changed.");
                Logger.Instance.Error($"Output of 'vcgencmd version': {Environment.NewLine}{output}");
                return "n/a";
            }
        }

        private string checkAndFormatVersionHash(string versionString)
        {
            var sb = new StringBuilder();
            var splitted = versionString.Split(BLANK);
            if (splitted.Length == 3)
            {
                string hash = splitted[0];
                if (hash.Length > SHORTENED_HASH_LENGTH)
                {
                    sb.Append(hash.Substring(0, SHORTENED_HASH_LENGTH));
                }
                else
                {
                    sb.Append(hash);
                }
                return sb.Append(BLANK).Append(splitted[1]).Append(BLANK).Append(splitted[2]).ToString();
            }
            return versionString;
        }
    }
}

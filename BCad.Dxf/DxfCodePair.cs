﻿using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using BCad.Dxf.Entities;
using BCad.Dxf.Sections;

namespace BCad.Dxf
{
    public partial class DxfCodePair
    {
        public const int CommentCode = 999;

        private KeyValuePair<int, object> data;

        public int Code
        {
            get { return data.Key; }
            set { data = new KeyValuePair<int, object>(value, data.Value); }
        }

        public object Value
        {
            get { return data.Value; }
            set { data = new KeyValuePair<int, object>(data.Key, value); }
        }

        public string StringValue
        {
            get { return (string)Value; }
        }

        public double DoubleValue
        {
            get { return (double)Value; }
        }

        public short ShortValue
        {
            get { return (short)Value; }
        }

        public int IntegerValue
        {
            get { return (int)Value; }
        }

        public long LongValue
        {
            get { return (long)Value; }
        }

        public string HandleValue
        {
            get
            {
                if (IsHandle)
                    return StringValue;
                else
                    throw new DxfReadException("Value was not a valid handle");
            }
        }

        public DxfCodePair(int code, object value)
        {
            data = new KeyValuePair<int, object>(code, value);
        }

        private bool IsHandle
        {
            get
            {
                return handleRegex.IsMatch(StringValue);
            }
        }

        private static Regex handleRegex = new Regex("([a-fA-F0-9]){1,16}", RegexOptions.Compiled);

        public override string ToString()
        {
            return string.Format("[{0}: {1}]", Code, Value);
        }

        public static bool IsSectionStart(DxfCodePair pair)
        {
            return pair.Code == 0 && pair.StringValue == DxfSection.SectionText;
        }

        public static bool IsSectionEnd(DxfCodePair pair)
        {
            return pair.Code == 0 && pair.StringValue == DxfSection.EndSectionText;
        }

        public static bool IsEof(DxfCodePair pair)
        {
            return pair.Code == 0 && pair.StringValue == DxfFile.EofText;
        }

        public static bool IsComment(DxfCodePair pair)
        {
            return pair.Code == CommentCode;
        }
    }
}

﻿using System;

namespace BCad.Dxf.Sections
{
    public enum DxfSectionType
    {
        None,
        Header,
        Classes,
        Tables,
        Blocks,
        Entities,
        Objects
    }

    internal static class DxfSectionTypeHelper
    {
        public static string ToSectionName(this DxfSectionType section)
        {
            string name = null;
            switch (section)
            {
                case DxfSectionType.None:
                    throw new NotImplementedException("Section type NONE not supported.");
                case DxfSectionType.Header:
                    name = DxfSection.HeaderSectionText;
                    break;
                case DxfSectionType.Classes:
                    name = DxfSection.ClassesSectionText;
                    break;
                case DxfSectionType.Tables:
                    name = DxfSection.TablesSectionText;
                    break;
                case DxfSectionType.Blocks:
                    name = DxfSection.BlocksSectionText;
                    break;
                case DxfSectionType.Entities:
                    name = DxfSection.EntitiesSectionText;
                    break;
                case DxfSectionType.Objects:
                    name = DxfSection.ObjectsSectionText;
                    break;
            }
            return name;
        }

        public static DxfSectionType ToDxfSection(this string sectionName)
        {
            var sec = DxfSectionType.None;
            switch (sectionName.ToUpperInvariant())
            {
                case DxfSection.HeaderSectionText:
                    sec = DxfSectionType.Header;
                    break;
                case DxfSection.ClassesSectionText:
                    sec = DxfSectionType.Classes;
                    break;
                case DxfSection.TablesSectionText:
                    sec = DxfSectionType.Tables;
                    break;
                case DxfSection.BlocksSectionText:
                    sec = DxfSectionType.Blocks;
                    break;
                case DxfSection.EntitiesSectionText:
                    sec = DxfSectionType.Entities;
                    break;
                case DxfSection.ObjectsSectionText:
                    sec = DxfSectionType.Objects;
                    break;
            }
            return sec;
        }
    }
}

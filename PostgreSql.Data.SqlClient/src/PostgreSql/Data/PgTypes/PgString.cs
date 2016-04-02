// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using PostgreSql.Data.Frontend;
using System;
using System.Globalization;

namespace PostgreSql.Data.PgTypes
{
    //
    // Summary:
    //     Specifies the compare option values for a System.Data.SqlTypes.PgString structure.
    [Flags]
    public enum PgCompareOptions
    {
        //     Specifies the default option settings for System.Data.SqlTypes.PgString comparisons.
        None = 0,
        //     Specifies that System.Data.SqlTypes.PgString comparisons must ignore case.
        IgnoreCase = 1,
        //     Specifies that System.Data.SqlTypes.PgString comparisons must ignore nonspace
        //     combining characters, such as diacritics. The Unicode Standard defines combining
        //     characters as characters that are combined with base characters to produce a
        //     new character. Non-space combining characters do not use character space by themselves
        //     when rendered. For more information about non-space combining characters, see
        //     the Unicode Standard at http://www.unicode.org.
        IgnoreNonSpace = 2,
        //     Specifies that System.Data.SqlTypes.PgString comparisons must ignore the Kana
        //     type. Kana type refers to Japanese hiragana and katakana characters that represent
        //     phonetic sounds in the Japanese language. Hiragana is used for native Japanese
        //     expressions and words, while katakana is used for words borrowed from other languages,
        //     such as "computer" or "Internet". A phonetic sound can be expressed in both hiragana
        //     and katakana. If this value is selected, the hiragana character for one sound
        //     is considered equal to the katakana character for the same sound.
        IgnoreKanaType = 8,
        //     Specifies that System.Data.SqlTypes.PgString comparisons must ignore the character
        //     width. For example, Japanese katakana characters can be written as full-width
        //     or half-width and, if this value is selected, the katakana characters written
        //     as full-width are considered equal to the same characters written in half-width.
        IgnoreWidth = 16,
        //     Performs a binary sort.
        BinarySort2 = 16384,
        //     Specifies that sorts should be based on a characters numeric value instead of
        //     its alphabetical value.
        BinarySort = 32768
    }

    public struct PgString
        : INullable, IComparable<PgString>, IComparable, IEquatable<PgString>
    {
        public static readonly int BinarySort       = (int)PgCompareOptions.BinarySort;
        public static readonly int BinarySort2      = (int)PgCompareOptions.BinarySort2;
        public static readonly int IgnoreCase       = (int)PgCompareOptions.IgnoreCase;
        public static readonly int IgnoreKanaType   = (int)PgCompareOptions.IgnoreKanaType;
        public static readonly int IgnoreNonSpace   = (int)PgCompareOptions.IgnoreNonSpace;
        public static readonly int IgnoreWidth      = (int)PgCompareOptions.IgnoreWidth;
        
        public static readonly PgString Null = PgTypeInfoProvider.NullString;

        public PgString(string data)
        {
            throw new NotImplementedException();
        }

        public PgString(string data, int lcid)
        {
            throw new NotImplementedException();
        }

        public PgString(int lcid, PgCompareOptions compareOptions, byte[] data)
        {
            throw new NotImplementedException();
        }

        public PgString(string data, int lcid, PgCompareOptions compareOptions)
        {
            throw new NotImplementedException();
        }

        public PgString(int lcid, PgCompareOptions compareOptions, byte[] data, bool fUnicode)
        {
            throw new NotImplementedException();
        }

        public PgString(int lcid, PgCompareOptions compareOptions, byte[] data, int index, int count)
        {
            throw new NotImplementedException();
        }

        public PgString(int lcid, PgCompareOptions compareOptions, byte[] data, int index, int count, bool fUnicode)
        {
            throw new NotImplementedException();
        }

        public static PgBoolean operator !=(PgString x, PgString y)
        {
            throw new NotImplementedException();
        }

        public static PgString operator +(PgString x, PgString y)
        {
            throw new NotImplementedException();
        }

        public static PgBoolean operator <(PgString x, PgString y)
        {
            throw new NotImplementedException();
        }

        public static PgBoolean operator <=(PgString x, PgString y)
        {
            throw new NotImplementedException();
        }

        public static PgBoolean operator ==(PgString x, PgString y)
        {
            throw new NotImplementedException();
        }

        public static PgBoolean operator >(PgString x, PgString y)
        {
            throw new NotImplementedException();
        }

        public static PgBoolean operator >=(PgString x, PgString y)
        {
            throw new NotImplementedException();
        }

        public static explicit operator PgString(PgBit x)
        {
            throw new NotImplementedException();
        }

        public static explicit operator PgString(PgBoolean x)
        {
            throw new NotImplementedException();
        }

        public static explicit operator PgString(PgByte x)
        {
            throw new NotImplementedException();
        }

        public static explicit operator PgString(PgTimestamp x)
        {
            throw new NotImplementedException();
        }

        public static explicit operator PgString(PgDecimal x)
        {
            throw new NotImplementedException();
        }

        public static explicit operator PgString(PgDouble x)
        {
            throw new NotImplementedException();
        }

        public static explicit operator PgString(PgInt16 x)
        {
            throw new NotImplementedException();
        }

        public static explicit operator PgString(PgInt32 x)
        {
            throw new NotImplementedException();
        }

        public static explicit operator PgString(PgInt64 x)
        {
            throw new NotImplementedException();
        }

        public static explicit operator PgString(PgMoney x)
        {
            throw new NotImplementedException();
        }

        public static explicit operator PgString(PgReal x)
        {
            throw new NotImplementedException();
        }

        public static explicit operator string(PgString x)
        {
            throw new NotImplementedException();
        }

        public static implicit operator PgString(string x)
        {
            throw new NotImplementedException();
        }

        public CompareInfo CompareInfo 
        { 
            get { throw new NotImplementedException(); } 
        }

        public CultureInfo CultureInfo
        { 
            get { throw new NotImplementedException(); } 
        }

        public bool IsNull
        { 
            get { throw new NotImplementedException(); } 
        }

        public int LCID
        { 
            get { throw new NotImplementedException(); } 
        }

        public PgCompareOptions PgCompareOptions
        { 
            get { throw new NotImplementedException(); } 
        }

        public string Value
        { 
            get { throw new NotImplementedException(); } 
        }

        public static PgString Add(PgString x, PgString y)
        { 
            throw new NotImplementedException(); 
        }

        public PgString Clone()
        { 
            throw new NotImplementedException(); 
        }

        public static CompareOptions CompareOptionsFromPgCompareOptions(PgCompareOptions compareOptions)
        { 
            throw new NotImplementedException(); 
        }

        public int CompareTo(object value)
        { 
            throw new NotImplementedException(); 
        }

        public int CompareTo(PgString value)
        { 
            throw new NotImplementedException(); 
        }

        public static PgString Concat(PgString x, PgString y)
        { 
            throw new NotImplementedException(); 
        }

        public bool Equals(PgString other)
        {
            return (this == other).Value;
        }

        public override bool Equals(object value)
        { 
            throw new NotImplementedException(); 
        }

        public static PgBoolean Equals(PgString x, PgString y)
        { 
            throw new NotImplementedException(); 
        }

        public override int GetHashCode()
        { 
            throw new NotImplementedException(); 
        }

        public byte[] GetNonUnicodeBytes()
        { 
            throw new NotImplementedException(); 
        }

        public byte[] GetUnicodeBytes()
        { 
            throw new NotImplementedException(); 
        }

        public static PgBoolean GreaterThan(PgString x, PgString y)
        {
            throw new NotImplementedException();
        }

        public static PgBoolean GreaterThanOrEqual(PgString x, PgString y)
        {
            throw new NotImplementedException();
        }

        public static PgBoolean LessThan(PgString x, PgString y)
        {
            throw new NotImplementedException();
        }

        public static PgBoolean LessThanOrEqual(PgString x, PgString y)
        {
            throw new NotImplementedException();
        }

        public static PgBoolean NotEquals(PgString x, PgString y)
        {
            throw new NotImplementedException();
        }

        public PgBit ToPgBit()
        {
            throw new NotImplementedException();
        }

        public PgBoolean ToPgBoolean()
        {
            throw new NotImplementedException();
        }

        public PgByte ToPgByte()
        {
            throw new NotImplementedException();
        }

        public PgTimestamp ToPgTimestamp()
        {
            throw new NotImplementedException();
        }

        public PgDecimal ToPgDecimal()
        {
            throw new NotImplementedException();
        }

        public PgDouble ToPgDouble()
        {
            throw new NotImplementedException();
        }

        public PgInt16 ToPgInt16()
        {
            throw new NotImplementedException();
        }

        public PgInt32 ToPgInt32()
        {
            throw new NotImplementedException();
        }

        public PgInt64 ToPgInt64()
        {
            throw new NotImplementedException();
        }

        public PgMoney ToPgMoney()
        {
            throw new NotImplementedException();
        }

        public PgReal ToPgReal()
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            throw new NotImplementedException();
        }
    }
}

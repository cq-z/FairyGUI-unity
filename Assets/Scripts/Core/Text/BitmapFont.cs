﻿using System.Collections.Generic;
using UnityEngine;

namespace FairyGUI
{
    /// <summary>
    /// 
    /// </summary>
    public class BitmapFont : BaseFont
    {
        /// <summary>
        /// 
        /// </summary>
        public class BMGlyph
        {
            public float x;
            public float y;
            public float width;
            public float height;
            public int advance;
            public int lineHeight;
            public Vector2[] uv = new Vector2[4];
            public int channel;//0-n/a, 1-r,2-g,3-b,4-alpha
        }

        /// <summary>
        /// 
        /// </summary>
        public int size;

        /// <summary>
        /// 
        /// </summary>
        public bool resizable;

        /// <summary>
        /// Font generated by BMFont use channels.
        /// </summary>
        public bool hasChannel;

        protected Dictionary<int, BMGlyph> _dict;
        protected BMGlyph _glyph;
        float _scale;

        public BitmapFont()
        {
            this.canTint = true;
            this.hasChannel = false;
            this.customOutline = true;
            this.shader = ShaderConfig.bmFontShader;

            _dict = new Dictionary<int, BMGlyph>();
            _scale = 1;
        }

        public void AddChar(char ch, BMGlyph glyph)
        {
            _dict[ch] = glyph;
        }

        override public void SetFormat(TextFormat format, float fontSizeScale)
        {
            if (resizable)
                _scale = (float)format.size / size * fontSizeScale;
            else
                _scale = fontSizeScale;

            if (canTint)
                format.FillVertexColors(vertexColors);
        }

        override public bool GetGlyph(char ch, out float width, out float height, out float baseline)
        {
            if (ch == ' ')
            {
                width = Mathf.RoundToInt(size * _scale / 2);
                height = Mathf.RoundToInt(size * _scale);
                baseline = height;
                _glyph = null;
                return true;
            }
            else if (_dict.TryGetValue((int)ch, out _glyph))
            {
                width = Mathf.RoundToInt(_glyph.advance * _scale);
                height = Mathf.RoundToInt(_glyph.lineHeight * _scale);
                baseline = height;
                return true;
            }
            else
            {
                width = 0;
                height = 0;
                baseline = 0;
                return false;
            }
        }

        static Vector3 bottomLeft;
        static Vector3 topLeft;
        static Vector3 topRight;
        static Vector3 bottomRight;

        static Color32[] vertexColors = new Color32[4];

        override public void DrawGlyph(VertexBuffer vb, float x, float y)
        {
            if (_glyph == null) //space
                return;

            topLeft.x = x + _glyph.x * _scale;
            topLeft.y = y + (_glyph.lineHeight - _glyph.y) * _scale;
            bottomRight.x = x + (_glyph.x + _glyph.width) * _scale;
            bottomRight.y = topLeft.y - _glyph.height * _scale;

            topRight.x = bottomRight.x;
            topRight.y = topLeft.y;
            bottomLeft.x = topLeft.x;
            bottomLeft.y = bottomRight.y;

            vb.vertices.Add(bottomLeft);
            vb.vertices.Add(topLeft);
            vb.vertices.Add(topRight);
            vb.vertices.Add(bottomRight);

            vb.uvs.AddRange(_glyph.uv);

            if (hasChannel)
            {
                Vector2 channel = new Vector2(_glyph.channel, 0);
                vb.uvs2.Add(channel);
                vb.uvs2.Add(channel);
                vb.uvs2.Add(channel);
                vb.uvs2.Add(channel);
            }

            if (canTint)
            {
                vb.colors.Add(vertexColors[0]);
                vb.colors.Add(vertexColors[1]);
                vb.colors.Add(vertexColors[2]);
                vb.colors.Add(vertexColors[3]);
            }
            else
            {
                vb.colors.Add(Color.white);
                vb.colors.Add(Color.white);
                vb.colors.Add(Color.white);
                vb.colors.Add(Color.white);
            }
        }

        override public bool HasCharacter(char ch)
        {
            return ch == ' ' || _dict.ContainsKey((int)ch);
        }

        override public int GetLineHeight(int size)
        {
            if (_dict.Count > 0)
            {
                using (var et = _dict.GetEnumerator())
                {
                    et.MoveNext();
                    if (resizable)
                        return Mathf.RoundToInt((float)et.Current.Value.lineHeight * size / this.size);
                    else
                        return et.Current.Value.lineHeight;
                }
            }
            else
                return 0;
        }
    }
}

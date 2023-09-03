using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Threading.Tasks;
using System.Reflection;

namespace RandomGains.Frame.Display
{
    internal static class FLabelHooks
    {
        public static void HookOn()
        {
            On.FLabel.UpdateLocalPosition += FLabel_UpdateLocalPosition;
        }

        private static void FLabel_UpdateLocalPosition(On.FLabel.orig_UpdateLocalPosition orig, FLabel self)
        {
            if (self is FCustomLabel)
                (self as FCustomLabel).UpdateLocalPositionReplace();
            else
                orig.Invoke(self);
        }
    }
    internal class FCustomLabel : FLabel
    {
        //23
        //14
        public Vector2[] vertices = new Vector2[4];
        bool _isCustomMeshDirty;

        public FCustomLabel(string fontName, string text = "") : base(fontName, text)
        {
            
        }

        public void SetVertice(int i, Vector2 v)
        {
            if (vertices[i] == v)
                return;

            vertices[i] = v;
            _doesLocalPositionNeedUpdate = true;
            _isMeshDirty = true;
        }

        public void UpdateLocalPositionReplace()
        {
            _doesLocalPositionNeedUpdate = false;
            float rectButtom = float.MaxValue;
            float rectTop = float.MinValue;
            float rectLeft = float.MaxValue;
            float rectRight = float.MinValue;
            int lineNum = _letterQuadLines.Length;

            for (int i = 0; i < lineNum; i++)
            {
                FLetterQuadLine fletterQuadLine = _letterQuadLines[i];
                rectButtom = Math.Min(fletterQuadLine.bounds.yMin, rectButtom);
                rectTop = Math.Max(fletterQuadLine.bounds.yMax, rectTop);
            }
            float num6 = -(rectButtom + (rectTop - rectButtom) * _anchorY);
            for (int j = 0; j < lineNum; j++)
            {
                FLetterQuadLine fletterQuadLine = _letterQuadLines[j];
                float lineLeft = -fletterQuadLine.bounds.width * _anchorX;
                rectLeft = Math.Min(lineLeft, rectLeft);
                rectRight = Math.Max(lineLeft + fletterQuadLine.bounds.width, rectRight);
            }
            _textRect.x = rectLeft;
            _textRect.y = rectButtom + num6;
            _textRect.width = rectRight - rectLeft;
            _textRect.height = rectTop - rectButtom;

            for(int i = 0; i < lineNum; i++)
            {
                FLetterQuadLine fletterQuadLine = _letterQuadLines[i];
                int quadNum = fletterQuadLine.quads.Length;
                for (int k = 0; k < quadNum; k++)
                {
                    CalculateVectorsReplace(ref fletterQuadLine.quads[k], _font.offsetX, _font.offsetY);
                }
            }

            //_isMeshDirty = true;
            //Redraw(false, false);
        }

        void CalculateVectorsReplace(ref FLetterQuad quad, float offsetX, float offsetY)
        {
            quad.topLeft = GetVector2InGrid(new Vector2(quad.rect.xMin, quad.rect.yMax));
            quad.topRight = GetVector2InGrid(new Vector2(quad.rect.xMax, quad.rect.yMax));
            quad.bottomRight = GetVector2InGrid(new Vector2(quad.rect.xMax, quad.rect.yMin));
            quad.bottomLeft = GetVector2InGrid(new Vector2(quad.rect.xMin, quad.rect.yMin));
        }

        Vector2 GetVector2InGrid(Vector2 v)
        {
            float tX = v.x / _textRect.width;
            float tY = 1f + v.y / _textRect.height;

            Vector2 left = Vector2.Lerp(vertices[0], vertices[1], tY);
            Vector2 right = Vector2.Lerp(vertices[3], vertices[2], tY);

            return Vector2.Lerp(left, right, tX);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using FairyGUI.Utils;

namespace FairyGUI
{
    public partial class TextField {
        public void OnPopulateMesh_Outline(VertexBuffer vb) {
            if (_textWidth == 0 && _lines.Count == 1) {
                if (_charPositions != null) {
                    _charPositions.Clear();
                    _charPositions.Add(new CharPosition());
                }

                if (_richTextField != null)
                    _richTextField.RefreshObjects();

                return;
            }

            float letterSpacing = _textFormat.letterSpacing * _fontSizeScale;
            TextFormat format = _textFormat;
            _font.SetFormat(format, _fontSizeScale);
            _font.UpdateGraphics(graphics);

            float rectWidth = _contentRect.width > 0 ? (_contentRect.width - GUTTER_X * 2) : 0;
            float rectHeight = _contentRect.height > 0
                ? Mathf.Max(_contentRect.height, _font.GetLineHeight(format.size))
                : 0;

            if (_charPositions != null)
                _charPositions.Clear();

            List<Vector3> vertList = vb.vertices;
            List<Vector2> uvList = vb.uvs;
            List<Vector2> uv2List = vb.uvs2;
            List<Color32> colList = vb.colors;

            HtmlLink currentLink = null;
            float linkStartX = 0;
            int linkStartLine = 0;

            float posx = 0;
            float indent_x;
            bool clipping = !_input && _autoSize == AutoSizeType.None;
            bool lineClipped;
            AlignType lineAlign;
            float glyphWidth, glyphHeight, baseline;
            short vertCount;
            float underlineStart;
            float strikethroughStart;
            int minFontSize;
            int maxFontSize;
            string rtlLine = null;

            int elementIndex = 0;
            int elementCount = _elements.Count;
            HtmlElement element = null;
            if (elementCount > 0)
                element = _elements[elementIndex];

            int lineCount = _lines.Count;
            for (int i = 0; i < lineCount; ++i) {
                LineInfo line = _lines[i];
                if (line.charCount == 0)
                    continue;

                lineClipped = clipping && i != 0 && line.y + line.height > rectHeight;
                lineAlign = format.align;
                if (element != null && element.charIndex == line.charIndex)
                    lineAlign = element.format.align;
                else
                    lineAlign = format.align;

                if (_textDirection == RTLSupport.DirectionType.RTL) {
                    if (lineAlign == AlignType.Center)
                        indent_x = (int) ((rectWidth + line.width) / 2);
                    else if (lineAlign == AlignType.Right)
                        indent_x = rectWidth;
                    else
                        indent_x = line.width + GUTTER_X * 2;

                    if (indent_x > rectWidth)
                        indent_x = rectWidth;

                    posx = indent_x - GUTTER_X;
                }
                else {
                    if (lineAlign == AlignType.Center)
                        indent_x = (int) ((rectWidth - line.width) / 2);
                    else if (lineAlign == AlignType.Right)
                        indent_x = rectWidth - line.width;
                    else
                        indent_x = 0;

                    if (indent_x < 0)
                        indent_x = 0;

                    posx = GUTTER_X + indent_x;
                }

                int lineCharCount = line.charCount;
                underlineStart = posx;
                strikethroughStart = posx;
                minFontSize = maxFontSize = format.size;

                if (_textDirection != RTLSupport.DirectionType.UNKNOW) {
                    rtlLine = _parsedText.Substring(line.charIndex, lineCharCount);
                    if (_textDirection == RTLSupport.DirectionType.RTL)
                        rtlLine = RTLSupport.ConvertLineR(rtlLine);
                    else
                        rtlLine = RTLSupport.ConvertLineL(rtlLine);
                    lineCharCount = rtlLine.Length;
                }

                for (int j = 0; j < lineCharCount; j++) {
                    int charIndex = line.charIndex + j;
                    char ch = rtlLine != null ? rtlLine[j] : _parsedText[charIndex];

                    while (element != null && charIndex == element.charIndex) {
                        if (element.type == HtmlElementType.Text) {
                            vertCount = 0;
                            if (format.underline != element.format.underline) {
                                if (format.underline) {
                                    if (!lineClipped) {
                                        float lineWidth;
                                        if (_textDirection == RTLSupport.DirectionType.UNKNOW)
                                            lineWidth = (clipping
                                                ? Mathf.Clamp(posx, GUTTER_X, GUTTER_X + rectWidth)
                                                : posx) - underlineStart;
                                        else
                                            lineWidth = underlineStart - (clipping
                                                ? Mathf.Clamp(posx, GUTTER_X, GUTTER_X + rectWidth)
                                                : posx);
                                        if (lineWidth > 0)
                                            vertCount += (short) _font.DrawLine(
                                                underlineStart < posx ? underlineStart : posx,
                                                -(line.y + line.baseline), lineWidth,
                                                maxFontSize, 0, vertList, uvList, uv2List, colList);
                                    }

                                    maxFontSize = 0;
                                }
                                else
                                    underlineStart = posx;
                            }

                            if (format.strikethrough != element.format.strikethrough) {
                                if (format.strikethrough) {
                                    if (!lineClipped) {
                                        float lineWidth;
                                        if (_textDirection == RTLSupport.DirectionType.UNKNOW)
                                            lineWidth = (clipping
                                                ? Mathf.Clamp(posx, GUTTER_X, GUTTER_X + rectWidth)
                                                : posx) - strikethroughStart;
                                        else
                                            lineWidth = strikethroughStart - (clipping
                                                ? Mathf.Clamp(posx, GUTTER_X, GUTTER_X + rectWidth)
                                                : posx);
                                        if (lineWidth > 0)
                                            vertCount += (short) _font.DrawLine(
                                                strikethroughStart < posx ? strikethroughStart : posx,
                                                -(line.y + line.baseline), lineWidth,
                                                minFontSize, 1, vertList, uvList, uv2List, colList);
                                    }

                                    minFontSize = int.MaxValue;
                                }
                                else
                                    strikethroughStart = posx;
                            }

                            if (vertCount > 0 && _charPositions != null) {
                                CharPosition cp = _charPositions[_charPositions.Count - 1];
                                cp.vertCount += vertCount;
                                _charPositions[_charPositions.Count - 1] = cp;
                            }

                            format = element.format;
                            minFontSize = Math.Min(minFontSize, format.size);
                            maxFontSize = Math.Max(maxFontSize, format.size);
                            _font.SetFormat(format, _fontSizeScale);
                        }
                        else if (element.type == HtmlElementType.Link) {
                            currentLink = (HtmlLink) element.htmlObject;
                            if (currentLink != null) {
                                element.position = Vector2.zero;
                                currentLink.SetPosition(0, 0);
                                linkStartX = posx;
                                linkStartLine = i;
                            }
                        }
                        else if (element.type == HtmlElementType.LinkEnd) {
                            if (currentLink != null) {
                                currentLink.SetArea(linkStartLine, linkStartX, i, posx);
                                currentLink = null;
                            }
                        }
                        else {
                            IHtmlObject htmlObj = element.htmlObject;
                            if (htmlObj != null) {
                                if (_textDirection == RTLSupport.DirectionType.RTL)
                                    posx -= htmlObj.width - 2;

                                if (_charPositions != null) {
                                    CharPosition cp = new CharPosition();
                                    cp.lineIndex = (short) i;
                                    cp.charIndex = _charPositions.Count;
                                    cp.imgIndex = (short) (elementIndex + 1);
                                    cp.offsetX = posx;
                                    cp.width = (short) htmlObj.width;
                                    _charPositions.Add(cp);
                                }

                                if (lineClipped || clipping && (posx < GUTTER_X ||
                                                                posx > GUTTER_X && posx + htmlObj.width >
                                                                _contentRect.width - GUTTER_X))
                                    element.status |= 1;
                                else
                                    element.status &= 254;

                                element.position = new Vector2(posx + 1,
                                    line.y + line.baseline - htmlObj.height * IMAGE_BASELINE);
                                htmlObj.SetPosition(element.position.x, element.position.y);

                                if (_textDirection == RTLSupport.DirectionType.RTL)
                                    posx -= letterSpacing;
                                else
                                    posx += htmlObj.width + letterSpacing + 2;
                            }
                        }

                        if (element.isEntity)
                            ch = '\0';

                        elementIndex++;
                        if (elementIndex < elementCount)
                            element = _elements[elementIndex];
                        else
                            element = null;
                    }

                    if (ch == '\0')
                        continue;

                    if (_font.GetGlyph(ch == '\t' ? ' ' : ch, out glyphWidth, out glyphHeight, out baseline)) {
                        if (ch == '\t')
                            glyphWidth *= 4;

                        if (_textDirection == RTLSupport.DirectionType.RTL) {
                            if (lineClipped || clipping && (rectWidth < 7 || posx != (indent_x - GUTTER_X)) &&
                                posx < GUTTER_X - 0.5f) //超出区域，剪裁
                            {
                                posx -= (letterSpacing + glyphWidth);
                                continue;
                            }

                            posx -= glyphWidth;
                        }
                        else {
                            if (lineClipped || clipping && (rectWidth < 7 || posx != (GUTTER_X + indent_x)) &&
                                posx + glyphWidth > _contentRect.width - GUTTER_X + 0.5f) //超出区域，剪裁
                            {
                                posx += letterSpacing + glyphWidth;
                                continue;
                            }
                        }

                        vertCount = (short) _font.DrawGlyph(posx, -(line.y + line.baseline), vertList, uvList, uv2List,
                            colList);

                        if (_charPositions != null) {
                            CharPosition cp = new CharPosition();
                            cp.lineIndex = (short) i;
                            cp.charIndex = _charPositions.Count;
                            cp.vertCount = vertCount;
                            cp.offsetX = posx;
                            cp.width = (short) glyphWidth;
                            _charPositions.Add(cp);
                        }

                        if (_textDirection == RTLSupport.DirectionType.RTL)
                            posx -= letterSpacing;
                        else
                            posx += letterSpacing + glyphWidth;
                    }
                    else //if GetGlyph failed
                    {
                        if (_charPositions != null) {
                            CharPosition cp = new CharPosition();
                            cp.lineIndex = (short) i;
                            cp.charIndex = _charPositions.Count;
                            cp.offsetX = posx;
                            _charPositions.Add(cp);
                        }

                        if (_textDirection == RTLSupport.DirectionType.RTL)
                            posx -= letterSpacing;
                        else
                            posx += letterSpacing;
                    }
                } //text loop

                if (!lineClipped) {
                    vertCount = 0;
                    if (format.underline) {
                        float lineWidth;
                        if (_textDirection == RTLSupport.DirectionType.UNKNOW)
                            lineWidth = (clipping ? Mathf.Clamp(posx, GUTTER_X, GUTTER_X + rectWidth) : posx) -
                                        underlineStart;
                        else
                            lineWidth = underlineStart -
                                        (clipping ? Mathf.Clamp(posx, GUTTER_X, GUTTER_X + rectWidth) : posx);
                        if (lineWidth > 0)
                            vertCount += (short) _font.DrawLine(underlineStart < posx ? underlineStart : posx,
                                -(line.y + line.baseline), lineWidth,
                                maxFontSize, 0, vertList, uvList, uv2List, colList);
                    }

                    if (format.strikethrough) {
                        float lineWidth;
                        if (_textDirection == RTLSupport.DirectionType.UNKNOW)
                            lineWidth = (clipping ? Mathf.Clamp(posx, GUTTER_X, GUTTER_X + rectWidth) : posx) -
                                        strikethroughStart;
                        else
                            lineWidth = strikethroughStart -
                                        (clipping ? Mathf.Clamp(posx, GUTTER_X, GUTTER_X + rectWidth) : posx);
                        if (lineWidth > 0)
                            vertCount += (short) _font.DrawLine(strikethroughStart < posx ? strikethroughStart : posx,
                                -(line.y + line.baseline), lineWidth,
                                minFontSize, 1, vertList, uvList, uv2List, colList);
                    }

                    if (vertCount > 0 && _charPositions != null) {
                        CharPosition cp = _charPositions[_charPositions.Count - 1];
                        cp.vertCount += vertCount;
                        _charPositions[_charPositions.Count - 1] = cp;
                    }
                }

            } //line loop

            if (element != null && element.type == HtmlElementType.LinkEnd && currentLink != null)
                currentLink.SetArea(linkStartLine, linkStartX, lineCount - 1, posx);

            if (_charPositions != null) {
                CharPosition cp = new CharPosition();
                cp.lineIndex = (short) (lineCount - 1);
                cp.charIndex = _charPositions.Count;
                cp.offsetX = posx;
                _charPositions.Add(cp);
            }

            int count = vertList.Count;
            if (count > 65000) {
                Debug.LogWarning("Text is too large. A mesh may not have more than 65000 vertices.");
                vertList.RemoveRange(65000, count - 65000);
                colList.RemoveRange(65000, count - 65000);
                uvList.RemoveRange(65000, count - 65000);
                if (uv2List.Count > 0)
                    uv2List.RemoveRange(65000, count - 65000);
                count = 65000;
            }

            bool hasShadow = _textFormat.shadowOffset.x != 0 || _textFormat.shadowOffset.y != 0;
            int allocCount = count;
            // int drawDirs = 0;
            // if (_textFormat.outline != 0) {
            //     drawDirs = UIConfig.enhancedTextOutlineEffect ? 8 : 4;
            //     allocCount += count * drawDirs;
            // }

            if (hasShadow)
                allocCount += count;
            if (allocCount > 65000) {
                Debug.LogWarning("Text is too large. Outline/shadow effect cannot be completed.");
                allocCount = count;
            }

            if (graphics.Tangents == null) {
                graphics.Tangents = new Vector4[graphics.MaxCount];
                graphics.UV2 = new Vector2[graphics.MaxCount];
            }

            if (graphics.MaxCount < allocCount) {
                graphics.MaxCount = allocCount;
                graphics.Tangents = new Vector4[graphics.MaxCount];
                graphics.UV2 = new Vector2[graphics.MaxCount];
            }

            graphics.CurCount = allocCount;

            // if (allocCount != count) {
            if (hasShadow || stroke != 0) {
                VertexBuffer vb2 = VertexBuffer.Begin();
                List<Vector3> vertList2 = vb2.vertices;
                List<Color32> colList2 = vb2.colors;

                Color32 col = _textFormat.outlineColor;
                graphics.OutlineColor = col;
                graphics.OutlineWidth = stroke;

                if (stroke != 0)
                {
                    Vector3 sum = Vector3.zero;
                    Vector2 uvSum = Vector2.zero;

                    Vector2 leftBottom = new Vector2(1, 1);
                    Vector2 rightTop = new Vector2(0, 0);
                    Vector3[] posNormals = new Vector3[count];
                    Vector3[] uvNormals = new Vector3[count];

                    Vector2 uv = Vector2.zero;
                    for (int i = 0, j = 1; i < count; i++, j++)
                    {
                        uv = uvList[i];

                        sum += vertList[i];
                        uvSum += uv;
                        if (uv.x < leftBottom.x)
                        {
                            leftBottom.x = uv.x;
                        }
                        else if (uv.x > rightTop.x)
                        {
                            rightTop.x = uv.x;
                        }
                        if (uv.y < leftBottom.y)
                        {
                            leftBottom.y = uv.y;
                        }
                        else if (uv.y > rightTop.y)
                        {
                            rightTop.y = uv.y;
                        }

                        if (j % 4 == 0) {
                            sum /= 4;
                            posNormals[i] = vertList[i] - sum;
                            posNormals[i - 1] = vertList[i - 1] - sum;
                            posNormals[i - 2] = vertList[i - 2] - sum;
                            posNormals[i - 3] = vertList[i - 3] - sum;
                            //uv
                            uvSum /= 4;
                            uvNormals[i] = new Vector2(uvList[i].x, uvList[i].y) - uvSum;
                            uvNormals[i - 1] = new Vector2(uvList[i - 1].x, uvList[i - 1].y) - uvSum;
                            uvNormals[i - 2] = new Vector2(uvList[i - 2].x, uvList[i - 2].y) - uvSum;
                            uvNormals[i - 3] = new Vector2(uvList[i - 3].x, uvList[i - 3].y) - uvSum;

                            sum = Vector3.zero;
                            uvSum = Vector2.zero;

                            Vector4 uvBounds = new Vector4(leftBottom.x, leftBottom.y, rightTop.x, rightTop.y);
                            graphics.Tangents[i] = uvBounds;
                            graphics.Tangents[i - 1] = uvBounds;
                            graphics.Tangents[i - 2] = uvBounds;
                            graphics.Tangents[i - 3] = uvBounds;

                            leftBottom = new Vector2(1, 1);
                            rightTop = new Vector2(0, 0);
                        }
                    }

                    Vector3 pos;
                    for (int i = 0; i < count; i++)
                    {
                        pos = vertList[i];
                        pos.x += Mathf.Sign(posNormals[i].x) * stroke;
                        pos.y += Mathf.Sign(posNormals[i].y) * stroke;

                        uv.x = (Mathf.Sign(uvNormals[i].x) + 1) * 0.5f;
                        uv.y = (Mathf.Sign(uvNormals[i].y) + 1) * 0.5f;

                        graphics.UV2[i] = uv;
                        vertList[i] = pos;
                    }
                }

                if (hasShadow)
                {
                    col = _textFormat.shadowColor;
                    Vector2 offset = _textFormat.shadowOffset;
                    int start = allocCount - count;
                    for (int i = 0; i < count; i++)
                    {
                        Vector3 vert = vertList[i];
                        vertList2.Add(new Vector3(vert.x + offset.x, vert.y - offset.y, 0));
                        colList2.Add(col);
                        //影子的tangents和uv2
                        graphics.Tangents[i + start] = graphics.Tangents[i];
                        graphics.UV2[i + start] = graphics.UV2[i];
                    }

                    vb2.uvs.AddRange(uvList);
                    if (uv2List.Count > 0)
                        vb2.uvs2.AddRange(uv2List);
                }

                vb.Insert(vb2);
                vb2.End();
            }


            vb.AddTriangles();

            if (_richTextField != null)
                _richTextField.RefreshObjects();
        }

        void BuildLines2() {
            float letterSpacing = _textFormat.letterSpacing * _fontSizeScale;
            float lineSpacing = (_textFormat.lineSpacing - 1) * _fontSizeScale;
            float rectWidth = _contentRect.width - GUTTER_X * 2;
            float glyphWidth = 0, glyphHeight = 0, baseline = 0;
            short wordLen = 0;
            bool wordPossible = false;
            float posx = 0;

            TextFormat format = _textFormat;
            _font.SetFormat(format, _fontSizeScale);
            bool wrap = _wordWrap && !_singleLine;
            if (_maxWidth > 0) {
                wrap = true;
                rectWidth = _maxWidth - GUTTER_X * 2;
            }
            _textWidth = _textHeight = 0;

            RequestText();

            int elementCount = _elements.Count;
            int elementIndex = 0;
            HtmlElement element = null;
            if (elementCount > 0)
                element = _elements[elementIndex];
            int textLength = _parsedText.Length;

            LineInfo line = LineInfo.Borrow();
            _lines.Add(line);
            line.y = line.y2 = GUTTER_Y;
            sLineChars.Clear();

            for (int charIndex = 0; charIndex < textLength; charIndex++) {
                char ch = _parsedText[charIndex];

                glyphWidth = glyphHeight = baseline = 0;

                while (element != null && element.charIndex == charIndex) {
                    if (element.type == HtmlElementType.Text) {
                        format = element.format;
                        _font.SetFormat(format, _fontSizeScale);
                    }
                    else {
                        IHtmlObject htmlObject = element.htmlObject;
                        if (_richTextField != null && htmlObject == null) {
                            element.space = (int)(rectWidth - line.width - 4);
                            htmlObject = _richTextField.htmlPageContext.CreateObject(_richTextField, element);
                            element.htmlObject = htmlObject;
                        }
                        if (htmlObject != null) {
                            glyphWidth = htmlObject.width + 2;
                            glyphHeight = htmlObject.height;
                            baseline = glyphHeight * IMAGE_BASELINE;
                        }

                        if (element.isEntity)
                            ch = '\0'; //indicate it is a place holder
                    }

                    elementIndex++;
                    if (elementIndex < elementCount)
                        element = _elements[elementIndex];
                    else
                        element = null;
                }

                if (ch == '\0' || ch == '\n') {
                    wordPossible = false;
                }
                else if (_font.GetGlyph(ch == '\t' ? ' ' : ch, out glyphWidth, out glyphHeight, out baseline)) {
                    if (ch == '\t')
                        glyphWidth *= 4;

                    if (wordPossible) {
                        if (char.IsWhiteSpace(ch)) {
                            wordLen = 0;
                        }
                        else if (ch >= 'a' && ch <= 'z' || ch >= 'A' && ch <= 'Z'
                            || ch >= '0' && ch <= '9'
                            || ch == '.' || ch == '"' || ch == '\''
                            || format.specialStyle == TextFormat.SpecialStyle.Subscript
                            || format.specialStyle == TextFormat.SpecialStyle.Superscript
                            || _textDirection != RTLSupport.DirectionType.UNKNOW && RTLSupport.IsArabicLetter(ch)) {
                            wordLen++;
                        }
                        else
                            wordPossible = false;
                    }
                    else if (char.IsWhiteSpace(ch)) {
                        wordLen = 0;
                        wordPossible = true;
                    }
                    else if (format.specialStyle == TextFormat.SpecialStyle.Subscript
                        || format.specialStyle == TextFormat.SpecialStyle.Superscript) {
                        if (sLineChars.Count > 0) {
                            wordLen = 2; //避免上标和下标折到下一行
                            wordPossible = true;
                        }
                    }
                    else
                        wordPossible = false;
                }
                else
                    wordPossible = false;

                sLineChars.Add(new LineCharInfo() { width = glyphWidth, height = glyphHeight, baseline = baseline });
                if (glyphWidth != 0) {
                    if (posx != 0)
                        posx += letterSpacing;
                    posx += glyphWidth;
                }

                #region 处理中文行首为标点

                bool needNewline = false;
                if (wrap && format.specialStyle == TextFormat.SpecialStyle.None) {
                    int nextIndex = charIndex + 1;
                    if (nextIndex < parsedText.Length) {
                        var nextCh = _parsedText[nextIndex];
                        if (char.IsNumber(nextCh)) {
                            // 这里判断是否有连续数字+符号的情况
                            // 比如"100(200)。"

                            var accumulatePosX = posx;
                            for (int i = nextIndex; i < parsedText.Length; i++) {
                                var checkCh = _parsedText[i];
                                if (!char.IsNumber(checkCh) &&
                                    !UIConfig.LineBreakingWithNumberCharacters.Contains(checkCh) &&
                                    !UIConfig.LineBreakingFollowingCharacters.Contains(checkCh)) {
                                    break;
                                }

                                if (_font.GetGlyph(checkCh, out var checkGlyphWidth, out _, out _)) {
                                    if ((accumulatePosX += checkGlyphWidth) > rectWidth) {
                                        needNewline = true;
                                        break;
                                    }
                                }
                            }
                        }
                        else if (ch >= 0x4e00 && ch <= 0x9fbb) {
                            // 这里判断是否有汉字+符号的情况（包括连续符号）
                            // 比如"好的。"以及"（好的）。"

                            // bool print = ch == '害' || ch == '力';

                            var accumulatePosX = posx;
                            for (int i = nextIndex; i < parsedText.Length; i++) {
                                var checkCh = _parsedText[i];

                                if (!UIConfig.LineBreakingFollowingCharacters.Contains(checkCh) &&
                                    !char.IsNumber(checkCh)) {
                                    break;
                                }

                                // if (print) {
                                //     DebugUtils.LogError(() => checkCh.ToString());
                                // }

                                if (_font.GetGlyph(checkCh, out var checkGlyphWidth, out _, out _)) {
                                    if ((accumulatePosX += checkGlyphWidth) > rectWidth) {
                                        needNewline = true;
                                        break;
                                    }
                                }
                            }
                        }

                    }
                }

                #endregion

                if (ch == '\n' && !_singleLine) {
                    UpdateLineInfo(line, letterSpacing, sLineChars.Count);

                    LineInfo newLine = LineInfo.Borrow();
                    _lines.Add(newLine);
                    newLine.y = line.y + (line.height + lineSpacing);
                    if (newLine.y < GUTTER_Y) //lineSpacing maybe negative
                        newLine.y = GUTTER_Y;
                    newLine.y2 = newLine.y;
                    newLine.charIndex = line.charIndex + line.charCount;

                    sLineChars.Clear();
                    wordPossible = false;
                    posx = 0;
                    line = newLine;
                }
                else if (needNewline || (wrap && posx > rectWidth)) {
                    int lineCharCount = sLineChars.Count;
                    int toMoveChars;

                    if (wordPossible && wordLen < 20 && lineCharCount > 2) //if word had broken, move word to new line
                        toMoveChars = wordLen;
                    else if (lineCharCount != 1) //only one char here, we cant move it to new line
                        toMoveChars = 1;
                    else
                        toMoveChars = 0;

                    UpdateLineInfo(line, letterSpacing, lineCharCount - toMoveChars);

                    LineInfo newLine = LineInfo.Borrow();
                    _lines.Add(newLine);
                    newLine.y = line.y + (line.height + lineSpacing);
                    if (newLine.y < GUTTER_Y)
                        newLine.y = GUTTER_Y;
                    newLine.y2 = newLine.y;
                    newLine.charIndex = line.charIndex + line.charCount;

                    posx = 0;
                    if (toMoveChars != 0) {
                        for (int i = line.charCount; i < lineCharCount; i++) {
                            LineCharInfo ci = sLineChars[i];
                            if (posx != 0)
                                posx += letterSpacing;
                            posx += ci.width;
                        }

                        sLineChars.RemoveRange(0, line.charCount);
                    }
                    else
                        sLineChars.Clear();

                    wordPossible = false;
                    line = newLine;
                }
            }

            UpdateLineInfo(line, letterSpacing, sLineChars.Count);

            if (_textWidth > 0)
                _textWidth += GUTTER_X * 2;
            _textHeight = line.y + line.height + GUTTER_Y;

            _textWidth = Mathf.RoundToInt(_textWidth);
            _textHeight = Mathf.RoundToInt(_textHeight);
        }
    }
}
